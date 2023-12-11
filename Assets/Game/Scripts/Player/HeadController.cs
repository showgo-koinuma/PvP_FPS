using Cinemachine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine;

public class HeadController : MonoBehaviourPun
{
    [SerializeField, Tooltip("自分のカメラ")] GameObject _myCinemachine;
    /// <summary>Fov変更用</summary>
    CinemachineVirtualCamera _myVirtualCam;
    [SerializeField, Tooltip("動く頭")] Transform _head;
    [SerializeField, Tooltip("body 体の向きを取得するため")] Transform _orientation;
    [SerializeField, Tooltip("IKのLookTargetを回転させるもの")] Transform _rotationLookTarget;
    float _yRotation, _xRotation, _headYrotation;
    Vector3 _headRotation;
    [SerializeField] float _XSensitivity = 50f;
    [SerializeField] float _YSensitivity = 50f;
    [SerializeField] float _adsSensRate = 0.8f;

    PlayerAnimationManager _animManager;

    // ADS
    /// <summary>ADS Sens Rate</summary>
    float _sensMultiplier = 1f; // ads時感度変更用
    float _currentFov = 90f;

    // リコイル
    Vector3 _targetRotation;
    Vector3 _currentRotation;
    Vector3 _returnTarget = Vector3.zero;
    float _returnSpeed = 1;
    float _snappiness = 6;

    private void Awake()
    {
        _animManager = GetComponent<PlayerAnimationManager>();

        if (!photonView.IsMine) ;// this.enabled = false;
        else
        {
            _myVirtualCam = Instantiate(_myCinemachine, transform).GetComponent<CinemachineVirtualCamera>();
            _myVirtualCam.Follow = _head;
        }
    }

    void Look()
    {
        Vector2 lookRotation = new Vector2(PlayerInput.Instance.LookRotation.x * _XSensitivity * Time.fixedDeltaTime * _sensMultiplier,
            PlayerInput.Instance.LookRotation.y * _YSensitivity * Time.fixedDeltaTime * _sensMultiplier);

        if (lookRotation.magnitude != 0) _returnTarget = _currentRotation;

        //Find current look rotation
        Vector3 rot = _orientation.localRotation.eulerAngles;
        _yRotation = rot.y + lookRotation.x;

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= lookRotation.y;
        //_xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        Vector3 currentRot = _currentRotation;
        if (_xRotation + currentRot.x > 90f) // 手動Clamp
        {
            currentRot.x -= _xRotation + currentRot.x - 90;
        }
        else if (_xRotation + currentRot.x < -90f)
        {
            currentRot.x += -90f - (_xRotation + currentRot.x);
        }

        //Perform the rotations
        //_orientation.transform.localRotation = Quaternion.Euler(0, _desiredX, 0);
        _head.transform.localRotation = Quaternion.Euler(_xRotation + currentRot.x, currentRot.y, 0);
        Debug.Log(_currentRotation);
        //_rotationLookTarget.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }

    /// <summary>指定したリコイルを設定する</summary>
    public void Recoil(float recoilY, float recoilX)
    {
        _targetRotation += new Vector3(recoilY, recoilX, 0);
    }

    /// <summary>リコイルを反映させる</summary>
    void ReflectsRecoil()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, _returnTarget, _returnSpeed * Time.deltaTime / (_returnTarget - _targetRotation).magnitude * 20);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.deltaTime);
        //_head.transform.localRotation = Quaternion.Euler(_currentRotation + _head.transform.localRotation.eulerAngles);
    }

    /// <summary>同期を呼び出すもの</summary>
    void ReflectsLookRotate()
    {
        photonView.RPC(nameof(RotationLookTarget), RpcTarget.All, _yRotation, _xRotation, _currentRotation);
    }

    /// <summary>LookTargetを同期する</summary>
    [PunRPC]
    void RotationLookTarget(float yRotation, float xRotatino, Vector3 currentRotation)
    {
        // y軸回転とIKを同期する
        _orientation.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        _rotationLookTarget.transform.localRotation = Quaternion.Euler(xRotatino, 0, 0);
        _rotationLookTarget.transform.localRotation = Quaternion.Euler(currentRotation + _rotationLookTarget.transform.localRotation.eulerAngles);
    }

    /// <summary>ADS時のカメラ関連の処理</summary>
    public void OnADSCamera(bool on, float fov, float adsSpeed)
    {
        if (on) // fovの遷移と感度の変更
        {
            DOTween.To(() => _currentFov, x => _currentFov = x, fov, adsSpeed * (_currentFov - fov) / (90 - fov));
            _sensMultiplier = _adsSensRate;
        }
        else
        {
            DOTween.To(() => _currentFov, x => _currentFov = x, 90f, adsSpeed * (90 - _currentFov) / (90 - fov));
            _sensMultiplier = 1;
        }
    }

    /// <summary>fovを遷移させる</summary>
    void ReflectsADS()
    {
        _myVirtualCam.m_Lens.FieldOfView = _currentFov;
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;
        InGameManager.Instance.UpdateAction += Look;
        InGameManager.Instance.UpdateAction += ReflectsRecoil;
        InGameManager.Instance.UpdateAction += ReflectsLookRotate;
        InGameManager.Instance.UpdateAction += ReflectsADS;
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;
        InGameManager.Instance.UpdateAction -= Look;
        InGameManager.Instance.UpdateAction -= ReflectsRecoil;
        InGameManager.Instance.UpdateAction -= ReflectsLookRotate;
        InGameManager.Instance.UpdateAction -= ReflectsADS;
    }
}
