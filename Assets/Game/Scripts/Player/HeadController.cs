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
    private float _xRotation;
    [SerializeField] float _XSensitivity = 50f;
    [SerializeField] float _YSensitivity = 50f;
    [SerializeField] float _adsSensRate = 0.8f;

    // ADS
    /// <summary>ADS Sens Rate</summary>
    float _sensMultiplier = 1f; // ads時感度変更用
    float _currentFov = 90f;

    // リコイル
    Vector3 _targetRotation;
    Vector3 _currentRotation;
    float _returnSpeed = 1;
    float _snappiness = 3;

    private void Awake()
    {
        if (!photonView.IsMine) this.enabled = false;
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

        //Find current look rotation
        Vector3 rot = _orientation.localRotation.eulerAngles;
        float desiredX = rot.y + lookRotation.x;

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= lookRotation.y;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        //Perform the rotations
        _orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
        _head.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }

    /// <summary>指定したリコイルを設定する</summary>
    public void Recoil(float recoilY, float recoilX)
    {
        _targetRotation += new Vector3(recoilY, recoilX, 0);
    }

    /// <summary>リコイルを反映させる</summary>
    void ReflectsRecoil()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.deltaTime);
        _head.transform.localRotation = Quaternion.Euler(_currentRotation + _head.transform.localRotation.eulerAngles);
    }

    /// <summary>ADS時のカメラ関連の処理</summary>
    public void OnADSCamera(bool on, float fov, float adsSpeed)
    {
        if (on)
        {
            DOTween.To(() => _currentFov, x => _currentFov = x, fov, adsSpeed * (_currentFov - fov) / (90 - fov));
            _sensMultiplier = _adsSensRate;
            Debug.Log(_myVirtualCam.gameObject.name);
        }
        else
        {
            DOTween.To(() => _currentFov, x => _currentFov = x, 90f, adsSpeed * (90 - _currentFov) / (90 - fov));
            _sensMultiplier = 1;
        }
    }

    void ReflectsADS()
    {
        _myVirtualCam.m_Lens.FieldOfView = _currentFov;
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += Look;
        InGameManager.Instance.UpdateAction += ReflectsRecoil;
        InGameManager.Instance.UpdateAction += ReflectsADS;
    }
}
