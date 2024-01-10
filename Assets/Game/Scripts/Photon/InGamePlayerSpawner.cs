using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
/// <summary>マッチングシステムのない仮のconnect</summary>
public class InGamePlayerSpawner : MonoBehaviourPunCallbacks
{
    /* マスターサーバー -> ロビー -> 指定名のJoinOrCreateRoom
     * 成功 -> onCreateRoomかonJoinedRoomで１人目２人目を判定
     * 失敗 -> 失敗コールバック -> roomが満員またはその他のエラー
    */

    private void Start()
    {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        if (PhotonNetwork.IsConnected == false) PhotonNetwork.ConnectUsingSettings();
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", roomOptions, TypedLobby.Default);
    }

    // roomに参加したとき
    public override void OnJoinedRoom()
    {
        // Playerを生成し、RespawnでTrasformを初期化する
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>().RespawnPosition();
    }

    /// <summary>roomへの参加に失敗した</summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed: " + message);
    }
}