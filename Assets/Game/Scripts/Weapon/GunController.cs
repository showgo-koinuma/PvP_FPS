using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] GameObject[] _gunModelObjs;
    [SerializeField] GunStatus _gunStatus;
    [SerializeField] Transform _muzzlePos;
    [SerializeField, Tooltip("弾道のLine")] LineRenderer _ballisticLine;
    /// <summary>isMineでコールバックを登録しているか</summary>
    bool _setedAction = false;
    GunState _currentGunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;

    static int _hitLayer;
    int _currentMagazine;
    float _ballisticFadeOutTime = 0.01f;
    /// <summary>現在の弾の拡散</summary>
    float _currentDiffusion = 0;

    private void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();
        _currentMagazine = _gunStatus.FullMagazineSize;
        if (!_playerManager.photonView.IsMine) foreach(var obj in _gunModelObjs) obj.layer = 8; // 相手の銃モデルを見えないように
    }

    /// <summary>射撃時にどのような処理をするか計算する</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (_currentDiffusion > 0.1f) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
            else _currentDiffusion = 0;
            return; // nomalでないと撃てない
        }

        if (_currentMagazine <= 0) // 弾がなければreload
        {
            Reload();
            return;
        }

        Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion),
            UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 0) * Camera.main.transform.forward;

        if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
        {
            Debug.Log(hit.collider.name);

            // 親オブジェクトにTryGetComponent
            if (hit.collider.gameObject.transform.root.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.point - hit.transform.position, _playerManager.photonView.ViewID);
            }
            else
            {
                _playerManager.FireActionCall(hit.point); // 動くobjでなければ単純な処理となる
            }
        }

        // ads中は拡散しない
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion = 0;

        _headCntler.Recoil(UnityEngine.Random.Range(0, -_gunStatus.RecoilY), UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX)); // 反動を画面に反映
        _currentMagazine--;
        _currentGunState = GunState.interval; // インターバルに入れて
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // 指定時間で戻す
    }

    /// <summary>計算結果の処理を反映する(no Action)</summary>
    public void FireAction(Vector3 hitPos)
    {
        StartCoroutine(DrawBallistic(hitPos));
    }

    void Reload()
    {
        if (_currentGunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize) return;
        Debug.Log("reload");
        _currentGunState = GunState.reloading;
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // 強引すぎるか
    }

    void ADS()
    {
        _headCntler.OnADSCamera(PlayerInput.Instance.IsADS, _gunStatus.ADSFov, _gunStatus.ADSSpeed);
    }

    /// <summary>gun stateをnomalに戻す</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOutする弾道を描画する</summary>
    public IEnumerator DrawBallistic(Vector3 target)
    {
        _ballisticLine.SetPosition(0, _muzzlePos.position);
        _ballisticLine.SetPosition(1, target);
        Vector3 pos = _muzzlePos.position;
        yield return new WaitForSeconds(_ballisticFadeOutTime);
        _ballisticLine.SetPosition(1, pos);
    }

    /// <summary>弾が当たるレイヤーをセットする 自分には当たらないようにする</summary>
    public void SetHitlayer(bool isMaster)
    {
        if (isMaster) _hitLayer = ~(1 << 6);
        else _hitLayer = ~(1 << 7);
        Debug.Log("setHitLayer");
    }

    private void OnEnable()
    {
        //InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // オブジェクト共有
        transform.root.GetComponent<PlayerManager>().ActiveGun = this; // げんざいのActiveを設定
        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
    }

    private void OnDisable()
    {
        if (!_setedAction) return; // Actionをセットしていなければ実行しない
        PlayerInput.Instance.DelInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.DelInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction -= FireCalculation;
    }
}

enum GunState
{
    nomal,
    interval,
    reloading
}