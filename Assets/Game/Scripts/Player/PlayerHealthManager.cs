using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

/// <summary>player��HP���Ǘ�����</summary>
public class PlayerHealthManager : Damageable
{
    [SerializeField] int _maxArmor = 100;
    [SerializeField] int _maxHp = 100;
    [Header("UI")]
    [SerializeField] PostProcessVolume _damagePostProcessV;
    [SerializeField] DamageCounter _damageCounter;
    [Space(10)]
    [SerializeField] Image _armarGauge;
    [SerializeField] Image _hitHealthGauge;
    [SerializeField] Image _healthGauge;

    PlayerManager _pManager;

    // collider
    // 0 MoveBody, 1 mixamorig:LeftUpLeg, 2 mixamorig:LeftLeg, 3 mixamorig:RightUpLeg, 4 mixamorig:RightLeg,
    // 5 mixamorig:Spine, 6 mixamorig:LeftArm, 7 mixamorig:LeftForeArm, 8 mixamorig:Head, 9 mixamorig:RightArm, 10mixamorig:RightForeArm, 
    // �� : 8
    // �� : 5
    // �葫 : ���̑�(MoveBody��collider�ɂ͓�����Ȃ�)

    // hp status HP��int�Ȃ̂�
    int _armor;
    int _hp;

    // ���ʂɂ��_���[�W���[�g
    float _headDmgRate = 2f; // ��
    float _limbsDmgRate = 0.8f; // �葫

    // damage timer
    float _timeLimit = 1f; // 1�b���Action
    float _damageTimer = 0;

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _armor = _maxArmor;
        _hp = _maxHp;
        _damagePostProcessV.weight = 0;
    }

    private void Update()
    {
        if (_damageTimer < _timeLimit)
        {
            _damageTimer += Time.deltaTime;
        }

        if (_damagePostProcessV.weight > 0)
        {
            _damagePostProcessV.weight -= Time.deltaTime / 0.3f; // 0.3s�Ńt�F�[�h�A�E�g
        }

        ReflectHitGauge();
    }

    protected override HitData OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg;

        if (colliderIndex == 8) calcDmg = (int)(calcDmg * _headDmgRate); // ��
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // �葫

        return new HitData(colliderIndex == 8, _armor > 0, _armor <= calcDmg);
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // ���ʂɂ��_���[�W�v�Z
        bool isArmour = false;

        if (colliderIndex == 8) calcDmg = (int)(calcDmg * _headDmgRate); // ��
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // �葫

        if (_armor >= calcDmg)
        {
            _armor -= calcDmg; // �A�[�}�[�Ŏ󂯂����
            isArmour = true;
        }
        else if (_armor > 0) // �A�[�}�[�Ŏ󂯂���Ȃ�
        {
            _hp -= calcDmg - _armor; // ���ߕ�hp�����炷
            _armor = 0;
        }
        else // �A�[�}�[���Ȃ�
        {
            _hp -= calcDmg;

            if (_hp <= 0)
            {
                _hp = 0; // ����clamp
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

    /// <summary>�������_���[�W���󂯂��Ƃ��̏���</summary>
    private void OnDamageTakenIsMine()
    {
        Debug.Log("dame-ji");
        _damageTimer = 0; // �_���[�W�^�C�}�[�J�n
        ReflectHPUI();
        _damagePostProcessV.weight = 1; // damage effect�J�n
    }

    /// <summary>hp ui�𔽉f������</summary>
    void ReflectHPUI()
    {
        _armarGauge.fillAmount = (float)_armor / _maxArmor;
        _healthGauge.fillAmount = (float)_hp / _maxHp;
    }

    /// <summary>update��hitGauge�𓮂���</summary>
    void ReflectHitGauge()
    {
        if (_hitHealthGauge.fillAmount > (float)_hp / _maxHp && _damageTimer >= _timeLimit)
        {
            _hitHealthGauge.fillAmount -= Time.deltaTime * 0.5f;
        }
        else if (_damageTimer >= _timeLimit)
        {
            _hitHealthGauge.fillAmount = (float)_hp / _maxHp;
        }
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