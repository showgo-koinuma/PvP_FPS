using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>playerのHPを管理する</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxArmor = 100;
    [SerializeField] int _maxHp = 100;
    [SerializeField] Image _damagaeCanvasImage;
    [SerializeField] DamageCounter _damageCounter;

    PlayerManager _pManager;

    // collider
    // 0 MoveBody, 1 mixamorig:LeftUpLeg, 2 mixamorig:LeftLeg, 3 mixamorig:RightUpLeg, 4 mixamorig:RightLeg,
    // 5 mixamorig:Spine, 6 mixamorig:LeftArm, 7 mixamorig:LeftForeArm, 8 mixamorig:Head, 9 mixamorig:RightArm, 10mixamorig:RightForeArm, 
    // 頭 : 8
    // 胴 : 5
    // 手足 : その他(MoveBodyのcolliderには当たらない)

    int _currentHp;
    int CurrentHp
    {
        get => _currentHp;
        set
        {
            _currentHp = value;
            if (_currentHp <= 0) OnDead();
        }
    }

    // hp status HPはintなのか
    int _armor;
    int _hp;

    // 部位によるダメージレート
    float _headDmgRate = 2f; // 頭
    float _limbsDmgRate = 0.8f; // 手足

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _currentHp = _maxHp;
        _armor = _maxArmor;
        _hp = _maxHp;
    }

    protected override void OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // 部位によるダメージ計算
        bool isArmour = false;

        if (colliderIndex == 7) calcDmg = (int)(calcDmg * _headDmgRate); // 頭
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

        _damageCounter.DamageUpdate(calcDmg, isArmour);
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int damage, int collierIndex)
    {
        CurrentHp -= damage;
        OnDamageTakenIsMine();
    }

    /// <summary>自分がダメージを受けたときの処理</summary>
    private void OnDamageTakenIsMine()
    {
        if (!photonView.IsMine) return;
        Debug.Log("dame-ji");

        // 画面を赤くする処理
        Color color = _damagaeCanvasImage.color;
        color.a = 0.3f;
        _damagaeCanvasImage.color = color; // aをあげて
        _damagaeCanvasImage.DOFade(0, 0.1f); // 0.1秒で戻す
    }

    void OnDead()
    {
        Debug.Log("sinnda");
        //_lastHitPlayer.AddScore();
        _currentHp = _maxHp;
        _pManager.Respawn();
    }
}
