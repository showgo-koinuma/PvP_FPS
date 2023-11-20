using UnityEngine;

public class CanvasTransition : MonoBehaviour
{
    [SerializeField] GameObject _select;
    [SerializeField] GameObject _createRoom;
    [SerializeField] GameObject _joinRoom;
    GameObject _activeObj;

    private void Start()
    {
        (_activeObj = _select).SetActive(true);
        _createRoom.SetActive(false);
        _joinRoom.SetActive(false);
    }

    public void ToCreateRoom()
    {
        _activeObj.SetActive(false);
        (_activeObj = _createRoom).SetActive(true);
    }

    public void ToJoinRoom()
    {
        _activeObj.SetActive(false);
        (_activeObj = _joinRoom).SetActive(true);
    }

    public void BackButton()
    {
        if (_activeObj == _select) ; // ƒ^ƒCƒgƒ‹‚É–ß‚é
        else
        {
            _activeObj.SetActive(false);
            (_activeObj = _select).SetActive(true);
        }
    }
}