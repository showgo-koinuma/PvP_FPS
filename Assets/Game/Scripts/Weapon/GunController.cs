using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviourPun
{
    [Header("�X�e�[�^�X")]
    [SerializeField] protected GunStatus _gunStatus;
    [SerializeField, Tooltip("���탂�f���p��Animator")] protected Animator _weaponModelAnimator;

    [Header("���f��")]
    [SerializeField, Tooltip("Ricoil�p�I�u�W�F�N�g")] GameObject _recoilObj; // view model��recoil�p
    [SerializeField, Tooltip("PlayerModel�������Ă��镐��Model")] GameObject _holdGunModel;
    [SerializeField, Tooltip("ADS�����Ƃ��̃��f����localPosition")] Vector3 _ADSPos;

    [Header("�G�t�F�N�g")]
    [SerializeField, Tooltip("�e���I�u�W�F�N�g�̐e�ɂȂ�}�Y���I�u�W�F�N�g [0] = view, [1] = model")] GameObject[] _muzzles;
    [SerializeField, Tooltip("�e��TrailRenderer�v���n�u")] TrailRenderer _ballisticTrailPrefab;
    TrailRenderer[] _ballisticTrailObjs;
    int _ballisticTrailIndex = 0;
    [SerializeField, Tooltip("�}�Y���t���b�V��particle")] ParticleSystem _muzzleFlash;
    [SerializeField] ParticleSystem _hitParticleEffect;

    [Header("�N���X�w�A")]
    [SerializeField] CrosshairCntlr _crosshairCntlr;

    /// <summary>isMine�ŃR�[���o�b�N��o�^���Ă��邩</summary>
    bool _setedAction = false;
    protected GunState _gunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;
    protected PlayerAnimationManager _playerAnimManager;

    static int _hitLayer = ~(1 << 7 | 1 << 3);
    protected int _currentMagazine;
    /// <summary>���݂̒e�̊g�U</summary>
    float _currentDiffusion = 0;
    /// <summary>���݂̃��R�C���C���f�b�N�X</summary>
    int _recoilIndex;

    // ���f���̃��R�C�� �A�T���g�̏ꍇ
    Vector3 _defaultPos;
    float _currentZpos = -0.07f;
    float _targetZpos;
    float _modelRecoilSize = -0.03f;
    float _reflectSpeed = 0.05f;
    float _returnSpeed = 0.3f;

    // weapon switch
    GunState _lastState = GunState.nomal; // switch�O�̍Ō��state
    float _switchTimer = 0;
    Vector3 _startPos = new Vector3(0.065f, 0.284f, -0.303f);
    float _startRotX = -90;

    protected virtual void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();
        _playerAnimManager = transform.root.GetComponent<PlayerAnimationManager>();

        _ballisticTrailObjs = new TrailRenderer[_gunStatus.OneShotNum];

        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            _ballisticTrailObjs[i] = Instantiate(_ballisticTrailPrefab);
        }

        if (!_playerManager.photonView.IsMine) _crosshairCntlr.gameObject.SetActive(false); // �����łȂ��Ȃ����

        _currentMagazine = _gunStatus.FullMagazineSize; // �e��������
        _defaultPos = _recoilObj.transform.localPosition;
    }

    /// <summary>�ˌ����ɂǂ̂悤�ȏ��������邩�v�Z����</summary>
    protected virtual void FireCalculation()
    {
        _crosshairCntlr.SetSize(_currentDiffusion);

        if (!(_gunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (!PlayerInput.Instance.InputOnFire) _recoilIndex = 0;

            // ��ˌ����Ɋg�U�����Ƃɖ߂�
            if (PlayerInput.Instance.IsADS)
            {
                if (_currentDiffusion > _gunStatus.ADSDefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime; // 1�b�Ō��ɖ߂�
                else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;
            }
            else
            {
                if (_currentDiffusion > _gunStatus.DefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
                else _currentDiffusion = _gunStatus.DefaultDiffusion;
            }
            return; // nomal�łȂ��ƌ��ĂȂ�
        }

        if (_currentMagazine <= 0) // �e���Ȃ����reload
        {
            _recoilIndex = 0;
            Reload();
            return;
        }

        bool isHit = false;

        // Hit�v�Z
        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            // �����_���Ȋg�U�e���𐶐�
            Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 
                UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion)) * Camera.main.transform.forward;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
            {
                Debug.Log(hit.collider.name);
                photonView.RPC(nameof(ShareFireAction), RpcTarget.All, hit.point);

                // �e�I�u�W�F�N�g��TryGetComponent
                if (hit.collider.gameObject.transform.root.gameObject.TryGetComponent(out Damageable damageable))
                {
                    DamageableHitEffect(damageable.OnDamageTakenInvoker(_gunStatus.Damage, hit.collider));
                    isHit = true;
                }
                else
                {
                    PlayHitEffect(hit);
                }
            }
        }

        // �g�U���Ȃ��𑝉�������
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;

        // ���R�C��
        if (_recoilIndex >= _gunStatus.RecoilPattern.Length) // �����_���Ȕ���
        {
            _headCntler.Recoil(new Vector2(UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX),
               _gunStatus.RecoilY));
        }
        else _headCntler.Recoil(_gunStatus.RecoilPattern[_recoilIndex]); // �p�^�[���̔���
        
        _currentMagazine--;
        _recoilIndex++;

        StartRecoil(); // view model recoil animation
        _playerAnimManager.SetFireTrigger(); // play model animation
        _muzzleFlash.Emit(1); // play muzzle flash

        _playerManager.OnShoot(isHit);

        _gunState = GunState.interval; // �C���^�[�o���ɓ����
        ShootInterval();
    }

    protected virtual void ShootInterval()
    {
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // �w�莞�ԂŖ߂�
    }

    /// <summary>���ʂ̎ˌ��A�N�V����</summary>
    [PunRPC]
    protected virtual void ShareFireAction(Vector3 hitPoint)
    {
        DrawBallistic(hitPoint);
    }

    protected virtual void Reload()
    {
        if ((_gunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize) && _lastState != GunState.reloading) return;
        Debug.Log("reload");
        _gunState = GunState.reloading;
        _weaponModelAnimator.SetTrigger("Reload");
        _playerAnimManager.SetReloadTrigger();
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // ���������邩
    }

    /// <summary>gun state��nomal�ɖ߂�</summary>
    protected void ReturnGunState()
    {
        _gunState = GunState.nomal;
    }

    /// <summary>input��isADS���Q�Ƃ��Ĕ��f</summary>
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

    /// <summary>�e����`�悷��</summary>
    void DrawBallistic(Vector3 target)
    {
        if (photonView.IsMine)
        {
            _ballisticTrailObjs[_ballisticTrailIndex].Clear();
            _ballisticTrailObjs[_ballisticTrailIndex].AddPosition(_muzzles[0].transform.position); // ����pos
            _ballisticTrailObjs[_ballisticTrailIndex].transform.position = target; // ����pos
        }
        else
        {
            _ballisticTrailObjs[_ballisticTrailIndex].Clear();
            _ballisticTrailObjs[_ballisticTrailIndex].AddPosition(_muzzles[1].transform.position); // ����pos
            _ballisticTrailObjs[_ballisticTrailIndex].transform.position = target; // ����pos
        }

        _ballisticTrailIndex = (_ballisticTrailIndex + 1) % _gunStatus.OneShotNum;
    }

    void DamageableHitEffect(HitData hitData)
    {

    }

    /// <summary>obj�ɓ��������Ƃ��̃G�t�F�N�g�Đ�</summary>
    void PlayHitEffect(RaycastHit hit)
    {
        var effect = Instantiate(_hitParticleEffect, hit.point, Quaternion.identity);
        effect.transform.forward = hit.normal;
        effect.Emit(1);
    }

    /// <summary>view model��recoil animation</summary>
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

    IEnumerator SwitchWeaponAnimation()
    {
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
        _holdGunModel.SetActive(true); // ���f������

        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;

        ADS();
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
        InGameManager.Instance.UpdateAction += ReflectRecoil;

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
        _holdGunModel.SetActive(false); // ���f���s����

        if (!_setedAction) return; // Action���Z�b�g���Ă��Ȃ���Ύ��s���Ȃ�
        PlayerInput.Instance.DelInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.DelInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction -= FireCalculation;
        InGameManager.Instance.UpdateAction -= ReflectRecoil;

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