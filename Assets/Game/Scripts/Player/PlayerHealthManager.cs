using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;

/// <summary>playerのHPを管理する</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxArmor = 100;
    [SerializeField] int _maxHp = 100;
    [Header("UI")]
    [SerializeField] PostProcessVolume _damagePostProcessV;
    [SerializeField] DamageCounter _damageCounter;

    PlayerManager _pManager;

    // collider
    // 0 MoveBody, 1 mixamorig:LeftUpLeg, 2 mixamorig:LeftLeg, 3 mixamorig:RightUpLeg, 4 mixamorig:RightLeg,
    // 5 mixamorig:Spine, 6 mixamorig:LeftArm, 7 mixamorig:LeftForeArm, 8 mixamorig:Head, 9 mixamorig:RightArm, 10mixamorig:RightForeArm, 
    // 頭 : 8
    // 胴 : 5
    // 手足 : その他(MoveBodyのcolliderには当たらない)

    // hp status HPはintなのか
    int _armor;
    int _hp;

    // 部位によるダメージレート
    float _headDmgRate = 2f; // 頭
    float _limbsDmgRate = 0.8f; // 手足

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _armor = _maxArmor;
        _hp = _maxHp;
        _damagePostProcessV.weight = 0;
    }

    private void Update()
    {
        if (_damagePostProcessV.weight > 0)
        {
            _damagePostProcessV.weight -= Time.deltaTime / 0.3f; // 0.1sでフェードアウト
        }
    }

    protected override HitData OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg;

        if (colliderIndex == 8) calcDmg = (int)(calcDmg * _headDmgRate); // 頭
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // 手足

        return new HitData(colliderIndex == 8, _armor > 0, _armor <= calcDmg);
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // 部位によるダメージ計算
        bool isArmour = false;

        if (colliderIndex == 8) calcDmg = (int)(calcDmg * _headDmgRate); // 頭
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // 手足

        if (_armor >= calcDmg)
        {
            _armor -= calcDmg; // アーマーで受けきれる
            isArmour = true;
        }
        else if (_armor > 0) // アーマーで受けきれない
        {
            _hp -= calcDmg - _armor; // 超過分hpを減らす
            _armor = 0;
        }
        else // アーマーがない
        {
            _hp -= calcDmg;

            if (_hp <= 0)
            {
                _hp = 0; // 下限clamp
                OnDead();
            }
        }

        if (photonView.IsMine)
        {
            OnDamageTakenIsMine();
        }
        else
        {
            _damageCounter.DamageUpdate(calcDmg, isArmour);
        }
    }

    /// <summary>自分がダメージを受けたときの処理</summary>
    private void OnDamageTakenIsMine()
    {
        Debug.Log("dame-ji");

        _damagePostProcessV.weight = 1; // damage effect開始
    }

    void OnDead()
    {
        if (!photonView.IsMine) return;

        Debug.Log("sinnda");
        _armor = _maxArmor;
        _hp = _maxHp;

        _pManager.OnDead();
        _pManager.RespawnPosShare();
    }
}

public struct HitData
{
    public HitData(bool isHead, bool isArmor, bool exceedsArmor)
    {
        IsHead = isHead;
        IsArmor = isArmor;
        ExceedsArmor = exceedsArmor;
    }

    public bool IsHead;
    public bool IsArmor;
    public bool ExceedsArmor;
}