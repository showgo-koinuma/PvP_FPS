using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GunController : MonoBehaviourPun
{
    [SerializeField] GunStatus _gunStatus;
    [Header("�O�ϊ֘A")]
    [SerializeField, Tooltip("PlayerModel�������Ă��镐��Model")] GameObject _holdGunModel;
    [SerializeField, Tooltip("ADS�����Ƃ��̃��f����localPosition")] Vector3 _ADSPos;
    [SerializeField, Tooltip("�e���I�u�W�F�N�g�̐e�ɂȂ�}�Y���I�u�W�F�N�g [0] = view, [1] = model")] GameObject[] _muzzles;
    [SerializeField, Tooltip("�e��TrailRenderer�v���n�u")] TrailRenderer _ballisticTrailPrefab;
    [SerializeField, Tooltip("�}�Y���t���b�V��particle")] ParticleSystem _muzzleFlash;
    [SerializeField] ParticleSystem _hitParticleEffect;
    [Header("Crosshair")]
    [SerializeField] CrosshairCntlr _crosshairCntlr;
    /// <summary>isMine�ŃR�[���o�b�N��o�^���Ă��邩</summary>
    bool _setedAction = false;
    GunState _currentGunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;
    PlayerAnimationManager _playerAnimManager;

    static int _hitLayer = ~(1 << 7);
    int _currentMagazine;
    /// <summary>�e����������܂ł̎���</summary>
    float _ballisticFadeOutTime = 0.01f;
    /// <summary>���݂̒e�̊g�U</summary>
    float _currentDiffusion = 0;
    /// <summary>���݂̃��R�C���C���f�b�N�X</summary>
    int _recoilIndex;

    private void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();
        _playerAnimManager = transform.root.GetComponent<PlayerAnimationManager>();

        if (!_playerManager.photonView.IsMine) _crosshairCntlr.gameObject.SetActive(false); // �����łȂ��Ȃ����
        _currentMagazine = _gunStatus.FullMagazineSize; // �e��������
    }

    /// <summary>�ˌ����ɂǂ̂悤�ȏ��������邩�v�Z����</summary>
    void FireCalculation()
    {
        _crosshairCntlr.SetSize(_currentDiffusion);

        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
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

        // Hit�v�Z
        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            // �����_���Ȋg�U�e���𐶐�
            Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion),
                UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 0) * Camera.main.transform.forward;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
            {
                Debug.Log(hit.collider.name);
                photonView.RPC(nameof(ShareFireAction), RpcTarget.All, hit.point);

                // �e�I�u�W�F�N�g��TryGetComponent
                if (hit.collider.gameObject.transform.root.gameObject.TryGetComponent(out Damageable damageable))
                {
                    damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.point - hit.transform.position, _playerManager.photonView.ViewID);
                }
                else
                {
                    PlayHitEffect(hit);
                }
            }
        }

        // �g�U���Ȃ��𑝉�������
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion += _gunStatus.ADSDiffusion;

        // ���R�C��
        if (_recoilIndex >= _gunStatus.RecoilPattern.Length) // �����_���Ȕ���
        {
            _headCntler.Recoil(new Vector2(UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX),
               _gunStatus.RecoilY));
        }
        else _headCntler.Recoil(_gunStatus.RecoilPattern[_recoilIndex]); // �p�^�[���̔���
        
        _currentMagazine--;
        _recoilIndex++;

        _playerAnimManager.SetFireTrigger(); // play model animation
        _muzzleFlash.Emit(1); // play muzzle flash


        _currentGunState = GunState.interval; // �C���^�[�o���ɓ����
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // �w�莞�ԂŖ߂�
    }

    /// <summary>���ʂ̎ˌ��A�N�V����</summary>
    [PunRPC]
    void ShareFireAction(Vector3 hitPoint)
    {
        DrawBallistic(hitPoint);
    }

    void Reload()
    {
        if (_currentGunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize) return;
        Debug.Log("reload");
        _currentGunState = GunState.reloading;
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // ���������邩
    }

    void ADS()
    {
        _headCntler.OnADSCamera(PlayerInput.Instance.IsADS, _gunStatus.ADSFov, _gunStatus.ADSSpeed);
        _crosshairCntlr.SwitchDisplay(!PlayerInput.Instance.IsADS);

        if (PlayerInput.Instance.IsADS) transform.localPosition = _ADSPos;
        else transform.localPosition = Vector3.zero;
    }

    /// <summary>gun state��nomal�ɖ߂�</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOut����e����`�悷��</summary>
    void DrawBallistic(Vector3 target)
    {
        for (int i= 0; i < _muzzles.Length; i++)
        {
            var ballisticTrail = Instantiate(_ballisticTrailPrefab, _muzzles[i].transform.position, Quaternion.identity);
            if (_playerManager.photonView.IsMine ^ i == 0) ballisticTrail.gameObject.layer = 7; // setting invisible layer
            ballisticTrail.AddPosition(_muzzles[i].transform.position); // ����pos
            ballisticTrail.transform.position = target; // ���epos
            Destroy(ballisticTrail.gameObject, 0.1f); // ���e���������(0.1s)
        }
    }

    /// <summary>obj�ɓ��������Ƃ��̃G�t�F�N�g�Đ�</summary>
    void PlayHitEffect(RaycastHit hit)
    {
        var effect = Instantiate(_hitParticleEffect, hit.point, Quaternion.identity);
        effect.transform.forward = hit.normal;
        effect.Emit(1);
    }

    private void OnEnable()
    {
        _holdGunModel.SetActive(true); // ���f������
        _playerManager.ActiveGun = this; // ���݂�Active��Player�ɐݒ�

        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
    }

    private void OnDisable()
    {
        _holdGunModel.SetActive(false); // ���f���s����

        if (!_setedAction) return; // Action���Z�b�g���Ă��Ȃ���Ύ��s���Ȃ�
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

/// <summary>Dictionary��inspecter�Ŏg����</summary>
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