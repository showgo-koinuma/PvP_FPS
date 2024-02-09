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
    [SerializeField] PostProcessVolume _healPostProcess;
    [SerializeField] PostProcessVolume _damagePostProcess;
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
    float _armor;
    float _hp;
    bool _isArmor; // �A�[�}�[�����邩

    // ���ʂɂ��_���[�W���[�g
    float _headDmgRate = 2f; // ��
    float _limbsDmgRate = 1; // �葫

    // damage timer
    float _timeLimit = 1f; // 1�b���Action
    float _damageTimer = 0;

    // heal timer
    float _healthTimer = 0;
    float _healSpeedSec = 100; // 1sec��100�񕜂���

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _armor = _maxArmor;
        _hp = _maxHp;
        _healPostProcess.weight = 0;
        _damagePostProcess.weight = 0;
    }

    private void Update()
    {
        if (_damageTimer < _timeLimit)
        {
            _damageTimer += Time.deltaTime;
        }

        if (_damagePostProcess.weight > 0)
        {
            _damagePostProcess.weight -= Time.deltaTime / 0.3f; // 0.3s�Ńt�F�[�h�A�E�g
        }

        if (_healPostProcess.weight > 0)
        {
            _healPostProcess.weight -= Time.deltaTime / 0.3f;
        }

        ReflectHealing();
        ReflectHitGauge();

        // is armor reflect
        if (_isArmor != _armor > 0)
        {
            photonView.RPC(nameof(ReflectIsArmor), RpcTarget.All, _armor > 0);
        }
    }

    [PunRPC]
    void ReflectIsArmor(bool isArmor)
    {
        _isArmor = isArmor;
    }

    protected override HitData OnDamageTaken(int dmg, int colliderIndex)
    {
        int calcDmg = dmg;

        if (colliderIndex == 8) calcDmg = (int)(calcDmg * _headDmgRate); // ��
        else if (colliderIndex != 4) calcDmg = (int)(calcDmg * _limbsDmgRate); // �葫

        return new HitData(calcDmg, colliderIndex == 8, _armor > 0, _armor <= calcDmg);
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int dmg, int colliderIndex)
    {
        float calcDmg = dmg; // ���ʂɂ��_���[�W�v�Z�p

        if (colliderIndex == 8) calcDmg = calcDmg * _headDmgRate; // ��
        else if (colliderIndex != 4) calcDmg = calcDmg * _limbsDmgRate; // �葫

        if (_armor >= calcDmg)
        {
            _armor -= calcDmg; // �A�[�}�[�Ŏ󂯂����
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
            _damageCounter.DamageUpdate((int)calcDmg, _isArmor);
        }
    }

    /// <summary>�������_���[�W���󂯂��Ƃ��̏���</summary>
    private void OnDamageTakenIsMine()
    {
        _damageTimer = 0; // �_���[�W�^�C�}�[�J�n
        ReflectHPUI();
        _damagePostProcess.weight = 1; // damage effect�J�n
    }

    public void SetHeal(float sec)
    {
        _healthTimer += sec;
    }

    /// <summary>healTimer���c���Ă�����heal����</summary>
    void ReflectHealing()
    {
        if (_healthTimer > 0)
        { // heal
            // system
            _healthTimer -= Time.deltaTime;

            if (_armor >= _maxArmor)
            { // hp armor max dattara heal sinai
                _armor = _maxArmor;
                return;
            }

            // heal
            _healPostProcess.weight = 1;

            if (_hp < _maxHp)
            { // hp max denai
                if (_hp + _healSpeedSec * Time.deltaTime > _maxHp)
                { // hp max ni naru
                    _armor += _hp + _healSpeedSec * Time.deltaTime - _maxHp;
                    _hp = _maxHp;
                }
                else
                { // hp max ni naran
                    _hp += _healSpeedSec * Time.deltaTime;
                }
            }
            else
            { // hp max
                _armor += _healSpeedSec * Time.deltaTime;

                if (_armor > _maxArmor)
                { // top clamp
                    _armor = _maxArmor;
                }
            }

            ReflectHPUI();
        }
        else
        {
            _healthTimer = 0;
        }
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

        _armor = _maxArmor;
        _hp = _maxHp;

        _pManager.OnDead();
        _pManager.RespawnPosShare();
    }
}

public struct HitData
{
    public HitData(int damage, bool isHead, bool isArmor, bool exceedsArmor)
    {
        Damage = damage;
        IsHead = isHead;
        IsArmor = isArmor;
        ExceedsArmor = exceedsArmor;
    }

    public int Damage;
    public bool IsHead;
    public bool IsArmor;
    public bool ExceedsArmor;
}