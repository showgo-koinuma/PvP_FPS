using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

/// <summary>SelectRoomButtonÇä«óùÇ∑ÇÈ</summary>
public class SelectRoomButtonManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _roomNameText;
    [SerializeField] TextMeshProUGUI _roomNumOfPeopleText;
    RoomInfo _thisRoomInfo;

    public void Initialization(RoomInfo roomInfo)
    {
        _roomNameText.text = roomInfo.Name + " Room";
        _roomNumOfPeopleText.text = roomInfo.PlayerCount.ToString() + " / 2"; // maxêlêîÇÕ2
        _thisRoomInfo = roomInfo;
        GetComponent<CustomButton>().ButtonAction += JoinRoom;
    }

    void JoinRoom()
    {
        LobbySceneUIManager.Instance.JoinRoom(_thisRoomInfo);
    }
}
