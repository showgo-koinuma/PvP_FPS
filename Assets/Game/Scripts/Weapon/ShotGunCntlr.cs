using UnityEngine;

// ショットガンとアサルトの差分はリロードとコッキング
/// <summary>Assaultとの処理の違いをoverrideしていく</summary>
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

    // アニメーションからコッキングが終わったことを受け取る
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

    // shellが入ったときにanimCntlrから呼び出す
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
