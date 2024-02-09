using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// メニューシーンのマネージャーコンポーネント
/// ネットワーク系の処理を行う
/// </summary>
public class LobbySceneUIManager : MonoBehaviourPunCallbacks
{
    [Header("Default")]
    [SerializeField] GameObject _defaultButtonsObj;

    [Header("Room作成")]
    [SerializeField] GameObject _createRoomObj;
    [SerializeField] TextMeshProUGUI _inputRoomName;

    [Header("Room参加")]
    [SerializeField] GameObject _joinRoomObj;
    [SerializeField, Tooltip("scroll viewのcontent")] GameObject _contentRoom;
    [SerializeField] GameObject _SelectRoomButton;

    [Header("PlayerName設定")]
    [SerializeField] GameObject _InputPlayerNameObj;
    [SerializeField] TextMeshProUGUI _inputPlayerNameText;
    [SerializeField] TextMeshProUGUI _playerNameText;

    [Header("ゲームスタート待機")]
    [SerializeField] GameObject _waitingStartGameObj;
    [SerializeField] TextMeshProUGUI _roomNameText;
    [SerializeField, Tooltip("scroll viewのcontent")] GameObject _contentPlayerName;
    [SerializeField, Tooltip("game start button")] GameObject _gameStartButton;
    [SerializeField, Tooltip("scroll viewに表示するためのTextObj")] GameObject _nameTextObj;

    [Header("Loading")]
    [SerializeField] GameObject _loadingObj;
    [SerializeField] TextMeshProUGUI _loadingText;

    static LobbySceneUIManager _instance;
    public static LobbySceneUIManager Instance { get => _instance; }
    /// <summary>現在アクティブになっているUIオブジェクト</summary>
    GameObject _activeObj;
    /// <summary>Roomの最大人数</summary>
    int _maxRoomPlayer = 2;
    /// <summary>InGameのscene in buildの数字</summary>
    int _inGameSceneInBuildNum = 1;

    private void Start()
    {
        if (!_instance) _instance = this;
        //マウスのロックが解除される
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //パネルとテキストを更新
        ChangeUIObj(_loadingObj);
        _loadingText.text = "Connecting Network...";
        ConnectNetwork(); // networkに接続

        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    /// <summary>ロビーに接続、またはロビー接続時の処理を実行</summary>
    void ConnectNetwork()
    {
        if (PhotonNetwork.IsConnected) // 自分の接続状態で条件分岐
        {
            if (PhotonNetwork.InRoom) ChangeUIObj(_waitingStartGameObj); // in room のとき
            else if (PhotonNetwork.InLobby) ChangeUIObj(_defaultButtonsObj); // in lobby のとき
            else OnConnectedToMaster();
        }
        else PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>指定したUIObjに遷移する</summary>
    void ChangeUIObj(GameObject toUIObj)
    {
        CloseAllUI();
        (_activeObj = toUIObj).SetActive(true);
        if (_activeObj == _waitingStartGameObj)
        {
            _roomNameText.text = PhotonNetwork.CurrentRoom.Name + " Room"; // ルームの名前を反映
            GeneratePlayerNameTextObj(); // player listのUI更新
            // マスターか判定してボタン表示
            if (PhotonNetwork.IsMasterClient) _gameStartButton.SetActive(true);
            else _gameStartButton.SetActive(false);
        }
    }

    /// <summary>全てのUIを非表示にする</summary>
    void CloseAllUI()
    {
        _defaultButtonsObj.SetActive(false);
        _createRoomObj.SetActive(false);
        _joinRoomObj.SetActive(false);
        _InputPlayerNameObj.SetActive(false);
        _waitingStartGameObj.SetActive(false);
        _loadingObj.SetActive(false);
    }

    /// <summary>PlayerNameのscroll viewを更新する</summary>
    void GeneratePlayerNameTextObj() // join, left call
    {
        // contentの子オブジェクトを全て削除する
        foreach (Transform child in _contentPlayerName.transform) Destroy(child.gameObject);
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Instantiate(_nameTextObj, _contentPlayerName.transform).GetComponent<TextMeshProUGUI>().text = player.NickName;
        }
    }

    /// <summary>RoomListに更新があったときroom一覧のScrollViewを更新する</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // contentの子オブジェクトを全て削除する
        foreach (Transform child in _contentRoom.transform) Destroy(child.gameObject);
        foreach (RoomInfo roomInfo in roomList)
        {
            //Instantiate(_SelectRoomButton, _contentRoom.transform).
                //GetComponent<SelectRoomButtonManager>().Initialization(roomInfo);
        }
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers) return;
        PhotonNetwork.JoinRoom(roomInfo.Name);
        ChangeUIObj(_loadingObj);
        _loadingText.text = "Joining Room...";
    }

    public void ReturnLobby()
    {
        ChangeUIObj(_waitingStartGameObj);
    }

    #region Button Action ===================================================================
    public void OnCreateRoomButton()
    {
        if (!string.IsNullOrEmpty(_inputRoomName.text))
        {
            RoomOptions options = new RoomOptions { MaxPlayers = _maxRoomPlayer };
            PhotonNetwork.CreateRoom(_inputRoomName.text, options); // Room作成
            ChangeUIObj(_loadingObj); // Loading
            _loadingText.text = "Making Room...";
        } // to:do 入力がないエラー
    }

    public void OnStartGame() // start game button
    {
        PhotonNetwork.LoadLevel(_inGameSceneInBuildNum);
    }

    public void OnChangeNameButton()
    {
        PhotonNetwork.NickName = _inputPlayerNameText.text;
        _playerNameText.text = "Player Name : " + PhotonNetwork.NickName;
    }

    /// <summary>BackButtonを押したときの処理</summary>
    public void OnBackButton()
    {
        if (_activeObj == _waitingStartGameObj) LeaveRoom();
        else if (_activeObj != _defaultButtonsObj) ChangeUIObj(_defaultButtonsObj);
    }
    /// <summary>roomから退出する</summary>
    void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        ChangeUIObj(_loadingObj); // UI更新
        _loadingText.text = "Leaving Room...";
    }
    #endregion

    #region Photon Call Back ==============================================================
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // ロビーに接続
        _loadingText.text = "Joining Room..."; // テキスト更新
        PhotonNetwork.AutomaticallySyncScene = true; // Master Clientと同じレベルをロード
    }
    public override void OnJoinedLobby()
    {
        ChangeUIObj(_defaultButtonsObj); // UI更新
    }
    public override void OnJoinedRoom()
    {
        ChangeUIObj(_waitingStartGameObj);
    }
    public override void OnLeftRoom()
    {
        ChangeUIObj(_defaultButtonsObj); // default UIに戻る
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GeneratePlayerNameTextObj();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GeneratePlayerNameTextObj(); // player listのUI更新
    }
    public override void OnMasterClientSwitched(Player newMasterClient) // Masterに切り替わったらButton表示
    {
        if (PhotonNetwork.IsMasterClient) _gameStartButton.SetActive(true);
    }
    #endregion
}