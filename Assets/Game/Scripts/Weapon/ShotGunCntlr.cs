using UnityEngine;

// �V���b�g�K���ƃA�T���g�̍����̓����[�h�ƃR�b�L���O
/// <summary>Assault�Ƃ̏����̈Ⴂ��override���Ă���</summary>
public class ShotGunCntlr : GunController
{
    [Header("only shotgun")]
    [SerializeField] ShotGunAnimCntlr _animCntlr;

    protected override void Awake()
    {
        base.Awake();
        _animCntlr._finishCookingAction += FinishCocking;
        _animCntlr._insertShellAction += InsertShellAction;
    }

    protected override void ShootInterval()
    {
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
        _weaponModelAnimator.SetBool("Reloading", false);
    }

    protected override void Reload()
    {
        _weaponModelAnimator.SetBool("Reloading", true);
    }

    // shell���������Ƃ���animCntlr����Ăяo��
    void InsertShellAction()
    {
        if (_currentMagazine < _gunStatus.FullMagazineSize) _currentMagazine++;
        else
        {
            _weaponModelAnimator.SetTrigger("FinishReload");
            _weaponModelAnimator.SetBool("Reloading", false);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_gunState == GunState.interval) _weaponModelAnimator.SetTrigger("Shot");
    }
}
