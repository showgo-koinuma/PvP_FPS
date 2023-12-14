using Photon.Pun.Demo.SlotRacer.Utils;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] GunStatus _gunStatus;
    [Header("外観関連")]
    [SerializeField, Tooltip("PlayerModelが持っている武器Model")] GameObject _holdGunModel;
    [SerializeField, Tooltip("ADSしたときのモデルのlocalPosition")] Vector3 _ADSPos;
    [SerializeField, Tooltip("弾道オブジェクトの親になるマズルオブジェクト [0] = view, [1] = model")] GameObject[] _muzzles;
    [SerializeField, Tooltip("弾道TrailRendererプレハブ")] TrailRenderer _ballisticTrailPrefab;
    [Header("Crosshair")]
    [SerializeField] CrosshairCntlr _crosshairCntlr;
    /// <summary>isMineでコールバックを登録しているか</summary>
    bool _setedAction = false;
    GunState _currentGunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;
    PlayerAnimationManager _playerAnimManager;

    static int _hitLayer = ~(1 << 7);
    int _currentMagazine;
    /// <summary>弾道が消えるまでの時間</summary>
    float _ballisticFadeOutTime = 0.01f;
    /// <summary>現在の弾の拡散</summary>
    float _currentDiffusion = 0;
    /// <summary>現在のリコイルインデックス</summary>
    int _recoilIndex;

    private void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();
        _playerAnimManager = transform.root.GetComponent<PlayerAnimationManager>();

        _currentMagazine = _gunStatus.FullMagazineSize; // 弾数初期化
    }

    /// <summary>射撃時にどのような処理をするか計算する</summary>
    void FireCalculation()
    {
        _crosshairCntlr.SetSize(_currentDiffusion);

        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (!PlayerInput.Instance.InputOnFire) _recoilIndex = 0;

            // 非射撃時に拡散をもとに戻す
            if (PlayerInput.Instance.IsADS)
            {
                if (_currentDiffusion > _gunStatus.ADSDefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime; // 1秒で元に戻る
                else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;
            }
            else
            {
                if (_currentDiffusion > _gunStatus.DefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
                else _currentDiffusion = _gunStatus.DefaultDiffusion;
            }
            return; // nomalでないと撃てない
        }

        if (_currentMagazine <= 0) // 弾がなければreload
        {
            _recoilIndex = 0;
            Reload();
            return;
        }

        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            // ランダムな拡散弾道を生成
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
        }

        // 拡散しないを増加させる
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion += _gunStatus.ADSDiffusion;

        // リコイル
        if (_recoilIndex >= _gunStatus.RecoilPattern.Length) // ランダムな反動
        {
            _headCntler.Recoil(new Vector2(UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX),
                UnityEngine.Random.Range(0, _gunStatus.RecoilY)));
        }
        else _headCntler.Recoil(_gunStatus.RecoilPattern[_recoilIndex]); // パターンの反動
        
        _currentMagazine--;
        _recoilIndex++;

        _playerAnimManager.SetFireTrigger();

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
        _crosshairCntlr.SwitchDisplay(!PlayerInput.Instance.IsADS);

        if (PlayerInput.Instance.IsADS) transform.localPosition = _ADSPos;
        else transform.localPosition = Vector3.zero;
    }

    /// <summary>gun stateをnomalに戻す</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOutする弾道を描画する</summary>
    public IEnumerator DrawBallistic(Vector3 target)
    {
        for (int i= 0; i < _muzzles.Length; i++)
        {
            var ballisticTrail = Instantiate(_ballisticTrailPrefab, _muzzles[i].transform.position, Quaternion.identity);
            if (_playerManager.photonView.IsMine ^ i == 0) ballisticTrail.gameObject.layer = 7; // invisible layer
            ballisticTrail.AddPosition(_muzzles[i].transform.position); // 初期pos
            ballisticTrail.transform.position = target; // 着弾pos
            Destroy(ballisticTrail.gameObject, 0.1f); // 着弾したら消す(0.1s)
        }

        yield break;
    }

    private void OnEnable()
    {
        _holdGunModel.SetActive(true); // モデル可視化
        _playerManager.ActiveGun = this; // 現在のActiveをPlayerに設定

        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
    }

    private void OnDisable()
    {
        _holdGunModel.SetActive(false); // モデル不可視化

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

/// <summary>Dictionaryをinspecterで使える</summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class KeyAndValuePair<TKey, TValue>
{
    [SerializeField] private TKey key;
    [SerializeField] private TValue value;

    public TKey Key => key;
    public TValue Value => value;
}