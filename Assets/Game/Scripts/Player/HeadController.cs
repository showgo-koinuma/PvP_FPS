using Cinemachine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class HeadController : MonoBehaviourPun
{
    [SerializeField, Tooltip("�����̃J����")] GameObject _myCinemachine;
    /// <summary>Fov�ύX�p</summary>
    CinemachineVirtualCamera _myVirtualCam;
    [SerializeField, Tooltip("������")] Transform _head;
    [SerializeField, Tooltip("body �̂̌������擾���邽��")] Transform _orientation;
    [SerializeField, Tooltip("IK��LookTarget����]���������")] Transform _rotationLookTarget;
    float _yRotation, _xRotation, _headYrotation;
    Vector3 _headRotation;
    [SerializeField] float _XSensitivity = 50f;
    [SerializeField] float _YSensitivity = 50f;
    [SerializeField] float _adsSensRate = 0.8f;

    PlayerManager _playerManager;
    PlayerAnimationManager _animManager;

    // ADS
    Tweener _adsTweener;
    /// <summary>ADS Sens Rate</summary>
    float _sensMultiplier = 1f; // ads�����x�ύX�p
    float _currentFov = 90f;

    // ���R�C��
    Vector3 _targetRotation;
    Vector3 _currentRotation;
    Vector3 _returnTarget = Vector3.zero;
    float _returnSpeed = 1;
    float _snappiness = 6;

    private void Awake()
    {
        _playerManager = GetComponent<PlayerManager>();
        _animManager = GetComponent<PlayerAnimationManager>();

        if (photonView.IsMine)
        {
            ResetRotationYonMine();
            _myVirtualCam = Instantiate(_myCinemachine, transform).GetComponent<CinemachineVirtualCamera>();
            _myVirtualCam.Follow = _head;
        }
    }

    void Look()
    {
        if (_playerManager.PlayerState != PlayerState.Nomal)
        { // nomal�łȂ������王�_�ړ����o���Ȃ�����
            return;
        }

        Vector2 lookRotation = new Vector2(PlayerInput.Instance.LookRotation.x * _XSensitivity * Time.fixedDeltaTime * _sensMultiplier,
            PlayerInput.Instance.LookRotation.y * _YSensitivity * Time.fixedDeltaTime * _sensMultiplier);

        if (lookRotation.magnitude != 0) _returnTarget = _currentRotation;

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= lookRotation.y;
        _yRotation += lookRotation.x;
        _yRotation %= 360; // ��Βl���傫���Ȃ肷���Ȃ��悤��

        Vector3 currentRot = _currentRotation;

        if (_xRotation + currentRot.x > 90f) // �蓮Clamp
        {
            currentRot.x -= _xRotation + currentRot.x - 90;
        }
        else if (_xRotation + currentRot.x < -90f)
        {
            currentRot.x += -90f - (_xRotation + currentRot.x);
        }

        //Perform the rotations
        _head.transform.localRotation = Quaternion.Euler(_xRotation + currentRot.x, 0, 0);

        photonView.RPC(nameof(RotationLookTarget), RpcTarget.All, _yRotation + currentRot.y, new Vector3(_xRotation + currentRot.x, currentRot.y, 0));
    }

    /// <summary>�w�肵�����R�C����ݒ肷��</summary>
    public void Recoil(Vector2 recoil)
    {
        _targetRotation += new Vector3(-recoil.y, recoil.x, 0);
    }

    /// <summary>���R�C���𔽉f������</summary>
    void ReflectsRecoil()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, _returnTarget, _returnSpeed * Time.deltaTime / (_returnTarget - _targetRotation).magnitude * 20);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.deltaTime);
    }

    /// <summary>LookTarget�𓯊�����</summary>
    [PunRPC]
    void RotationLookTarget(float yRotation, Vector3 currentRotation)
    {
        // y����]��IK�𓯊�����
        _orientation.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        _rotationLookTarget.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    /// <summary>isMaster�����ƂɃv���C���[�̌��������Z�b�g����</summary>
    /// <remarks>isMine�ł̂ݎ��s���邱��</remarks>
    public void ResetRotationYonMine()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _yRotation = 0;
        }
        else
        {
            _yRotation = 180;
        }
    }

    #region ADS
    /// <summary>ADS���̃J�����֘A�̏���</summary>
    public void OnADSCamera(bool on, float fov, float adsSpeed)
    {
        if (_adsTweener != null)
        {
            _adsTweener.Kill();
        }

        if (on) // fov�̑J�ڂƊ��x�̕ύX
        {
            _adsTweener = DOTween.To(() => _currentFov, x => _currentFov = x, fov, adsSpeed * (_currentFov - fov) / (90 - fov));
            _sensMultiplier = _adsSensRate;
        }
        else
        {
            _adsTweener = DOTween.To(() => _currentFov, x => _currentFov = x, 90f, adsSpeed * (90 - _currentFov) / (90 - fov));
            _sensMultiplier = 1;
        }
    }

    /// <summary>fov��J�ڂ�����</summary>
    void ReflectsADS()
    {
        _myVirtualCam.m_Lens.FieldOfView = _currentFov;
    }
    #endregion

    #region �ݒ�ύX�̔��f
    void OnHoriSensChanged(float value)
    {
        _XSensitivity = value;
    }
    void OnVerSensChanged(float value)
    {
        _YSensitivity = value;
    }
    void OnZoomSensChanged(float value)
    {
        _adsSensRate = value;
    }
    #endregion

    private void OnEnable()
    {
        if (!photonView.IsMine) return;
        InGameManager.Instance.UpdateAction += Look;
        InGameManager.Instance.UpdateAction += ReflectsRecoil;
        InGameManager.Instance.UpdateAction += ReflectsADS;

        SettingManager.Instance.OnHoriSensChanged += OnHoriSensChanged;
        SettingManager.Instance.OnVerSensChanged += OnVerSensChanged;
        SettingManager.Instance.OnZoomSensChanged += OnZoomSensChanged;
        SettingManager.Instance.ReflectCurrentSettings();
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;
        InGameManager.Instance.UpdateAction -= Look;
        InGameManager.Instance.UpdateAction -= ReflectsRecoil;
        InGameManager.Instance.UpdateAction -= ReflectsADS;

        SettingManager.Instance.OnHoriSensChanged -= OnHoriSensChanged;
        SettingManager.Instance.OnVerSensChanged -= OnVerSensChanged;
        SettingManager.Instance.OnZoomSensChanged -= OnZoomSensChanged;
    }
}
