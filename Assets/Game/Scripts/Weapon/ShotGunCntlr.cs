using Photon.Pun;
using UnityEngine;

// ショットガンとアサルトの差分はリロードとコッキング
/// <summary>Assaultとの処理の違いをoverrideしていく</summary>
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
        Invoke(nameof(PlayCockingSound), 0.15f); // コッキング音
    }

    // アニメーションからコッキングが終わったことを受け取る
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

    // shellが入ったときにanimCntlrから呼び出す
    void InsertShellAction()
    {
        _currentMagazine++;
        _curretnMagText.text = _currentMagazine.ToString(); // 弾数UI更新
        _gunAudioManager.PlayInsertShell(); // sound

        if (_currentMagazine >= _gunStatus.FullMagazineSize)
        {
            _weaponModelAnimator.SetTrigger("FinishReload");
            _weaponModelAnimator.SetBool("Reloading", _reloading = false);
            Invoke(nameof(PlayCockingSound), 0.1f); // リロード終わりコッキング音
        }
        else
        {
            _playerAnimManager.SetContInsertTrig();
        }
    }

    // タイムラグ上、コッキング音をInvokeするためにかませる
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
