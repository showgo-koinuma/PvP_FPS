using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviourPun
{
    [Header("ステータス")]
    [SerializeField] protected GunStatus _gunStatus;
    [SerializeField, Tooltip("武器モデル用のAnimator")] protected Animator _weaponModelAnimator;

    [Header("モデル")]
    [SerializeField, Tooltip("Ricoil用オブジェクト")] GameObject _recoilObj; // view modelのrecoil用
    [SerializeField, Tooltip("PlayerModelが持っている武器Model")] GameObject _holdGunModel;
    [SerializeField, Tooltip("ADSしたときのモデルのlocalPosition")] Vector3 _ADSPos;

    [Header("エフェクト")]
    [SerializeField, Tooltip("弾道オブジェクトの親になるマズルオブジェクト [0] = view, [1] = model")] GameObject[] _muzzles;
    [SerializeField, Tooltip("弾道TrailRendererプレハブ")] TrailRenderer _ballisticTrailPrefab;
    TrailRenderer[] _ballisticTrailObjs;
    int _ballisticTrailIndex = 0;
    [SerializeField, Tooltip("マズルフラッシュparticle")] ParticleSystem _muzzleFlash;
    [SerializeField] ParticleSystem _hitParticleEffect;

    [Header("UI")]
    [SerializeField] CrosshairCntlr _crosshairCntlr;
    [Space(5)]
    [SerializeField] protected TMP_Text _curretnMagText;
    [SerializeField] TMP_Text _maxMagText;
    [SerializeField] Image _iconBackImage;

    /// <summary>isMineでコールバックを登録しているか</summary>
    bool _setedAction = false;
    protected GunState _gunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;
    protected PlayerAnimationManager _playerAnimManager;
    protected GunAudioManager _gunAudioManager;

    static int _hitLayer = ~(1 << 7 | 1 << 3);
    protected int _currentMagazine;
    /// <summary>現在の弾の拡散</summary>
    float _currentDiffusion = 0;
    /// <summary>現在のリコイルインデックス</summary>
    int _recoilIndex;

    // モデルのリコイル アサルトの場合
    Vector3 _defaultPos;
    float _currentZpos = -0.07f;
    float _targetZpos;
    float _modelRecoilSize = -0.03f;
    float _reflectSpeed = 0.05f;
    float _returnSpeed = 0.3f;

    // weapon switch
    GunState _lastState = GunState.nomal; // switch前の最後のstate
    float _switchTimer = 0;
    Vector3 _startPos = new Vector3(0.065f, 0.284f, -0.303f);
    float _startRotX = -90;

    protected virtual void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();
        _playerAnimManager = transform.root.GetComponent<PlayerAnimationManager>();
        _gunAudioManager = GetComponent<GunAudioManager>();

        _ballisticTrailObjs = new TrailRenderer[_gunStatus.OneShotNum];

        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            _ballisticTrailObjs[i] = Instantiate(_ballisticTrailPrefab);
        }

        //if (!_playerManager.photonView.IsMine) _crosshairCntlr.gameObject.SetActive(false); // 自分でないなら消す

        _currentMagazine = _gunStatus.FullMagazineSize; // 弾数初期化
        _defaultPos = _recoilObj.transform.localPosition;
    }

    /// <summary>射撃時にどのような処理をするか計算する</summary>
    protected virtual void FireCalculation()
    {
        _crosshairCntlr.SetSize(_currentDiffusion);

        if (!(_gunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (!PlayerInput.Instance.InputOnFire) _recoilIndex = 0;

            // 非射撃時に拡散をもとに戻す
            if (PlayerInput.Instance.IsADS)
            {
                if (_currentDiffusion > _gunStatus.ADSDefaultDiffusion)
                {
                    _currentDiffusion -= _currentDiffusion * _gunStatus.ReducedDiffusionRate * Time.deltaTime; // 1秒で元に戻る
                }
                else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;
            }
            else
            {
                if (_currentDiffusion > _gunStatus.DefaultDiffusion)
                {
                    _currentDiffusion -= _currentDiffusion * _gunStatus.ReducedDiffusionRate * Time.deltaTime;
                }
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

        if (_playerManager.PlayerState != PlayerState.Nomal)
        {
            return;
        }

        int damage = 0;
        bool isHit = false;
        bool isHead = false;

        // Hit計算
        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            // ランダムな拡散弾道を生成
            Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 
                UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion)) * Camera.main.transform.forward;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
            {
                photonView.RPC(nameof(ShareFireAction), RpcTarget.All, hit.point);

                // 親オブジェクトにTryGetComponent
                if (hit.collider.gameObject.transform.root.gameObject.TryGetComponent(out Damageable damageable))
                {
                    HitData hitData = damageable.OnDamageTakenInvoker(_gunStatus.Damage, hit.collider);
                    DamageableHitEffect(hitData);
                    isHit = true;
                    damage += hitData.Damage;

                    if (hitData.IsHead)
                    {
                        isHead = true;
                    }
                }
                else
                {
                    PlayHitEffect(hit);
                }
            }
        }

        // 拡散しないを増加させる
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;

        // リコイル
        if (_recoilIndex >= _gunStatus.RecoilPattern.Length) // ランダムな反動
        {
            _headCntler.Recoil(new Vector2(UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX),
               _gunStatus.RecoilY));
        }
        else _headCntler.Recoil(_gunStatus.RecoilPattern[_recoilIndex]); // パターンの反動
        
        _currentMagazine--;
        _recoilIndex++;
        CurrentMagReflectUI();

        StartRecoil(); // view model recoil animation
        _playerAnimManager.SetFireTrigger(); // play model animation
        _muzzleFlash.Emit(1); // play muzzle flash

        // shot sound
        if (isHit)
        {
            if (!isHead)
            {
                _gunAudioManager.PlayHitSound();
            }
            else
            {
                _gunAudioManager.PlayHeadSound();
            }
        }

        _playerManager.AddResultData(damage, isHit, isHead);

        _gunState = GunState.interval; // インターバルに入れて
        ShootInterval();
    }

    protected virtual void ShootInterval()
    {
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // 指定時間で戻す
    }

    /// <summary>共通の射撃アクション</summary>
    [PunRPC]
    protected virtual void ShareFireAction(Vector3 hitPoint)
    {
        DrawBallistic(hitPoint);
        _gunAudioManager.PlayShotSound();
    }

    protected virtual void Reload()
    {
        if ((_gunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize) && _lastState != GunState.reloading) return;
        _gunState = GunState.reloading;
        _weaponModelAnimator.SetTrigger("Reload");
        _playerAnimManager.SetReloadTrigger();
        _gunAudioManager.PlayReloadSound();

        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate 
        { // リロード完了したときの処理
            _currentMagazine = _gunStatus.FullMagazineSize;
            CurrentMagReflectUI();
        }
        )).Method.Name, _gunStatus.ReloadTime); // 強引すぎるか
    }

    /// <summary>gun stateをnomalに戻す</summary>
    protected void ReturnGunState()
    {
        _gunState = GunState.nomal;
    }

    /// <summary>inputのisADSを参照して反映</summary>
    void ADS()
    {
        _headCntler.OnADSCamera(PlayerInput.Instance.IsADS, _gunStatus.ADSFov, _gunStatus.ADSSpeed);
        _crosshairCntlr.SwitchDisplay(!PlayerInput.Instance.IsADS);

        if (PlayerInput.Instance.IsADS)
        {
            transform.localPosition = _ADSPos;
            _modelRecoilSize = -0.01f;
        }
        else
        {
            transform.localPosition = Vector3.zero;
            _modelRecoilSize = -0.03f;
        }
    }

    /// <summary>弾道を描画する</summary>
    void DrawBallistic(Vector3 target)
    {
        if (photonView.IsMine)
        {
            _ballisticTrailObjs[_ballisticTrailIndex].Clear();
            _ballisticTrailObjs[_ballisticTrailIndex].AddPosition(_muzzles[0].transform.position); // 初期pos
            _ballisticTrailObjs[_ballisticTrailIndex].transform.position = target; // 初期pos
        }
        else
        {
            _ballisticTrailObjs[_ballisticTrailIndex].Clear();
            _ballisticTrailObjs[_ballisticTrailIndex].AddPosition(_muzzles[1].transform.position); // 初期pos
            _ballisticTrailObjs[_ballisticTrailIndex].transform.position = target; // 初期pos
        }

        _ballisticTrailIndex = (_ballisticTrailIndex + 1) % _gunStatus.OneShotNum;
    }

    void DamageableHitEffect(HitData hitData)
    {
        _crosshairCntlr.OnHit(hitData.IsHead);
    }

    /// <summary>objに当たったときのエフェクト再生</summary>
    void PlayHitEffect(RaycastHit hit)
    {
        var effect = Instantiate(_hitParticleEffect, hit.point, Quaternion.identity);
        effect.transform.forward = hit.normal;
        effect.Emit(1);
    }

    /// <summary>view modelのrecoil animation</summary>
    void StartRecoil()
    {
        _targetZpos = _defaultPos.z + _modelRecoilSize;
    }

    void ReflectRecoil()
    {
        _currentZpos += (_targetZpos - _currentZpos) * Time.deltaTime / _reflectSpeed;

        if (_defaultPos.z > _targetZpos)
        {
            _targetZpos -= _modelRecoilSize * Time.deltaTime / _returnSpeed;
        }
        else
        {
            _targetZpos = _defaultPos.z;
        }

        _recoilObj.transform.localPosition = new Vector3(_defaultPos.x, _defaultPos.y, _currentZpos);
    }

    /// <summary>弾数情報をUIへ反映させる</summary>
    void CurrentMagReflectUI()
    {
        _curretnMagText.text = _currentMagazine.ToString(); // 弾数UI更新
        _iconBackImage.fillAmount = (float)_currentMagazine / _gunStatus.FullMagazineSize;
    }

    IEnumerator SwitchWeaponAnimation()
    {
        _gunAudioManager.PlaySwitchSound();
        transform.localPosition = _startPos;
        transform.localRotation = Quaternion.Euler(_startRotX, 0, 0);

        while (_gunState == GunState.switching)
        {
            transform.localPosition = _startPos * (0.4f - _switchTimer) / 0.4f;
            transform.localRotation = Quaternion.Euler(_startRotX * (0.4f - _switchTimer) / 0.4f, 0, 0);

            _switchTimer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        ADS();
    }

    protected virtual void ReturnLastState()
    {
        _gunState = _lastState;
        if (_lastState == GunState.reloading) Reload();
        else if (_lastState == GunState.interval) _gunState = GunState.nomal;
        _lastState = GunState.nomal;
    }

    protected virtual void OnEnable()
    {
        _holdGunModel.SetActive(true); // モデル可視化

        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;

        ADS();
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
        InGameManager.Instance.UpdateAction += ReflectRecoil;

        // ui
        CurrentMagReflectUI();
        _maxMagText.text = "/" + _gunStatus.FullMagazineSize.ToString();

        // weapon switch
        _gunState = GunState.switching;
        Invoke(nameof(ReturnLastState), 0.4f);
        _switchTimer = 0;
        StartCoroutine(SwitchWeaponAnimation());
    }

    /// <summary>for only debug</summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Debug.Log(_gunState);
    }

    protected virtual void OnDisable()
    {
        _holdGunModel.SetActive(false); // モデル不可視化

        if (!_setedAction) return; // Actionをセットしていなければ実行しない
        PlayerInput.Instance.DelInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.DelInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction -= FireCalculation;
        InGameManager.Instance.UpdateAction -= ReflectRecoil;

        CancelInvoke(nameof(ReturnGunState));
        _lastState = _gunState;
    }
}

public enum GunState
{
    nomal,
    interval,
    reloading,
    switching
}