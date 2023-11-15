using UnityEngine;

public class HeadController : MonoBehaviour
{
    [SerializeField, Tooltip("動く頭")] Transform _head;
    [SerializeField, Tooltip("body 体の向きを取得するため")] Transform _orientation;
    private float _xRotation;
    [SerializeField] float _XSensitivity = 50f;
    [SerializeField] float _YSensitivity = 50f;
    /// <summary>なにこれ</summary>
    private float _sensMultiplier = 1f; // 感度変更用？デバフ、スタンとかかな 割合で増減できる

    private void Update()
    {
        Look();
    }

    void Look()
    {
        //float mouseX = Input.GetAxis("Mouse X") * _XSensitivity * Time.deltaTime * _sensMultiplier;
        //float mouseY = Input.GetAxis("Mouse Y") * _YSensitivity * Time.deltaTime * _sensMultiplier;
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
}
