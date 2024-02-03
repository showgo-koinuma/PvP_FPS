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

    public void Initialization(RoomInfo roomInfo, AudioSource audioSource)
    {
        _roomNameText.text = roomInfo.Name + " Room";
        _roomNumOfPeopleText.text = roomInfo.PlayerCount.ToString() + " / 2"; // maxêlêîÇÕ2
        _thisRoomInfo = roomInfo;
        CustomButton button =  GetComponent<CustomButton>();
        button.ButtonAction = JoinRoom;
        button.AudioSource = audioSource;
    }

    void JoinRoom()
    {
        LobbyManager.Instance.JoinRoom(_thisRoomInfo);
    }
}
