using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class FPSHeadController : MonoBehaviour
{
    /// <summary>Fov変更用</summary>
    CinemachineVirtualCamera _myVirtualCam;
    [SerializeField, Tooltip("動く頭")] Transform _head;
    [SerializeField, Tooltip("body 体の向きを取得するため")] Transform _orientation;
    float _yRotation, _xRotation;
    [SerializeField] float _XSensitivity = 50f;
    [SerializeField] float _YSensitivity = 50f;

    private void Update()
    {
        Look();
    }

    void Look()
    {
        Vector2 lookRotation = new Vector2(PlayerInput.Instance.LookRotation.x * _XSensitivity * Time.fixedDeltaTime,
            PlayerInput.Instance.LookRotation.y * _YSensitivity * Time.fixedDeltaTime);

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= lookRotation.y;
        _yRotation += lookRotation.x;
        _yRotation %= 360; // 絶対値が大きくなりすぎないように

        //Perform the rotations
        _head.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        _orientation.transform.localRotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
