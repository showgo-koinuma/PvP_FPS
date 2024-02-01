using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class FPSHeadController : MonoBehaviour
{
    /// <summary>Fov•ÏX—p</summary>
    CinemachineVirtualCamera _myVirtualCam;
    [SerializeField, Tooltip("“®‚­“ª")] Transform _head;
    [SerializeField, Tooltip("body ‘Ì‚ÌŒü‚«‚ğæ“¾‚·‚é‚½‚ß")] Transform _orientation;
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
        _yRotation %= 360; // â‘Î’l‚ª‘å‚«‚­‚È‚è‚·‚¬‚È‚¢‚æ‚¤‚É

        //Perform the rotations
        _head.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        _orientation.transform.localRotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
