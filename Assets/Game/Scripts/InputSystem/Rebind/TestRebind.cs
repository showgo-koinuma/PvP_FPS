using UnityEngine;
using TMPro;

public class TestRebind : MonoBehaviour
{
    [SerializeField] TMP_Text _tmpText;
    float _timer = 0f;
    bool _onJump, _isCrouching;

    private void Update()
    {
        if (_onJump != PlayerInput.Instance.OnJumpButton || _isCrouching != PlayerInput.Instance.IsCrouching)
        {
            _onJump = PlayerInput.Instance.OnJumpButton;
            _isCrouching = PlayerInput.Instance.IsCrouching;
            Debug.Log($"Jump : {_onJump}, Crouch : {_isCrouching}");
        }

        _timer += Time.deltaTime;

        if (_timer > 1f)
        {
            _timer -= 1f;
            _tmpText.text = (1 / Time.deltaTime).ToString();
        }
    }
}
