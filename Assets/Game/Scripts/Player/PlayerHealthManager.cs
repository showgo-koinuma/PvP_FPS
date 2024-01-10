using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>player��HP���Ǘ�����</summary>
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
    // �� : 8
    // �� : 5
    // �葫 : ���̑�(MoveBody��collider�ɂ͓�����Ȃ�)

    // hp status HP��int�Ȃ̂�
    int _armor;
    int _hp;

    // ���ʂɂ��_���[�W���[�g
    float _headDmgRate = 2f; // ��
    float _limbsDmgRate = 0.8f; // �葫

    private void Awake()
    {
        _pManager = GetComponent<PlayerManager>();
        _armor = _maxArmor;
        _hp = _maxHp;
    }

    protected override void OnDamageTaken(int dmg, int colliderIndex)
    {
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int dmg, int colliderIndex)
    {
        int calcDmg = dmg; // ���ʂɂ��_���[�W�v�Z
        bool isArmour = false;

        if (colliderIndex == 7) calcDmg = (int)(calcDmg * _headDmgRate); // ��
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

        // ��ʂ�Ԃ����鏈��
        Color color = _damagaeCanvasImage.color;
        color.a = 0.3f;
        _damagaeCanvasImage.color = color; // a��������
        _damagaeCanvasImage.DOFade(0, 0.1f); // 0.1�b�Ŗ߂�
    }

    void OnDead()
    {
        if (photonView.IsMine) return; // �|�������ŏ�������

        Debug.Log("sinnda");
        _armor = _maxArmor;
        _hp = _maxHp;

        _pManager.OnDead();
        _pManager.RespawnPosition();
        photonView.RPC(nameof(OnOtherKill), RpcTarget.Others);
    }

    [PunRPC]
    void OnOtherKill()
    {
        _pManager.OnKill();
    }
}
