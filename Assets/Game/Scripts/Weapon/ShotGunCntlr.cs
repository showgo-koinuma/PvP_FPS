using Photon.Pun;
using UnityEngine;

// �V���b�g�K���ƃA�T���g�̍����̓����[�h�ƃR�b�L���O
/// <summary>Assault�Ƃ̏����̈Ⴂ��override���Ă���</summary>
public class ShotGunCntlr : GunController
{
    [Header("only shotgun")]
    [SerializeField] ShotGunAnimCntlr _animCntlr;

    bool _reloading;

    protected override void Awake()
    {
        base.Awake();
        _animCntlr._finishCookingAction += FinishCocking;
        _animCntlr._insertShellAction += InsertShellAction;
    }

    protected override void ShootInterval()
    {
        _weaponModelAnimator.SetBool("Reloading", _reloading = false);
        _weaponModelAnimator.SetTrigger("Shot");
    }

    // �A�j���[�V��������R�b�L���O���I��������Ƃ��󂯎��
    void FinishCocking()
    {
        ReturnGunState();
    }

    protected override void FireCalculation()
    {
        base.FireCalculation();
    }


    /// <summary>�p�������photon���������Ȃ� ****</summary>
    [PunRPC]
    protected override void ShareFireAction(Vector3 hitPoint)
    {
        base.ShareFireAction(hitPoint);
    }

    protected override void Reload()
    {
        if (_gunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize || _reloading) return;
        _weaponModelAnimator.SetBool("Reloading", _reloading = true);
        Debug.Log("reload");
    }

    // shell���������Ƃ���animCntlr����Ăяo��
    void InsertShellAction()
    {
        Debug.Log("insert shell");
        _currentMagazine++;
        if (_currentMagazine >= _gunStatus.FullMagazineSize)
        {
            _weaponModelAnimator.SetTrigger("FinishReload");
            _weaponModelAnimator.SetBool("Reloading", _reloading = false);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_gunState == GunState.interval) _weaponModelAnimator.SetTrigger("Shot");
    }
}
