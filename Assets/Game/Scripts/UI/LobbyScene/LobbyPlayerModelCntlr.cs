using UnityEngine;

public class LobbyPlayerModelCntlr : MonoBehaviour
{
    [SerializeField] GameObject _playerUI;
    [SerializeField] Transform _uiPosition;

    private void FixedUpdate()
    {
        _playerUI.transform.position = Camera.main.WorldToScreenPoint(_uiPosition.position);
    }
}
