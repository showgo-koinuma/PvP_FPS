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
        Invoke(nameof(PlayCockingSound), 0.15f); // �R�b�L���O��
    }

    // �A�j���[�V��������R�b�L���O���I��������Ƃ��󂯎��
    void FinishCocking()
    {
        ReturnGunState();
    }

    [PunRPC]
    protected override void ShareFireAction(Vector3 hitPoint)
    {
        base.ShareFireAction(hitPoint);
    }

    protected override void Reload()
    {
        if (_gunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize || _reloading) return;
        _weaponModelAnimator.SetBool("Reloading", _reloading = true);
        _playerAnimManager.SetReloadTrigger();
        Debug.Log("reload");
    }

    // shell���������Ƃ���animCntlr����Ăяo��
    void InsertShellAction()
    {
        _currentMagazine++;
        _curretnMagText.text = _currentMagazine.ToString(); // �e��UI�X�V
        _gunAudioManager.PlayInsertShell(); // sound

        if (_currentMagazine >= _gunStatus.FullMagazineSize)
        {
            _weaponModelAnimator.SetTrigger("FinishReload");
            _weaponModelAnimator.SetBool("Reloading", _reloading = false);
            Invoke(nameof(PlayCockingSound), 0.1f); // �����[�h�I���R�b�L���O��
        }
        else
        {
            _playerAnimManager.SetContInsertTrig();
        }
    }

    // �^�C�����O��A�R�b�L���O����Invoke���邽�߂ɂ��܂���
    void PlayCockingSound()
    {
        _gunAudioManager.PlayCocking();
    }

    protected override void ReturnLastState()
    {
        base.ReturnLastState();

        if (_gunState == GunState.interval)
        {
            _gunState = GunState.interval;
            ShootInterval();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_gunState == GunState.interval) _weaponModelAnimator.SetTrigger("Shot");
    }
}
