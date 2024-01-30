using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] TMP_Text _loadingText;

    [Header("Default UI")]
    [SerializeField] GameObject _defaultPlayButton;
    [SerializeField] CustomButton _playButton;

    [Header("Select Mode")]
    [SerializeField] GameObject _selectModeCanvas;

    [Header("Create Room")]
    [SerializeField] GameObject _createRoomCanvas;
    [SerializeField] TMP_Text _inputRoomName;

    [Header("Join Room")]
    [SerializeField] GameObject _joinRoomCanvas;
    [SerializeField, Tooltip("scroll viewのcontent")] GameObject _roomContent;
    [SerializeField, Tooltip("contentに並べるroomボタン")] GameObject _SelectRoomButton;

    [Header("Player Name")]
    [SerializeField] TMP_Text _mineNameText;
    [SerializeField] TMP_Text _otherNameText;

    static LobbyManager _instance;
    public static LobbyManager Instance { get => _instance; }

    /// <summary>現在アクティブになっているUIオブジェクト</summary>
    GameObject _activeCanvas;
    /// <summary>プレイボタンのAction</summary>
    UnityEvent _playButtonAction;
    /// <summary>Roomの最大人数</summary>
    int _maxRoomPlayer = 2;
    /// <summary>otherがreadyを出しているか</summary>
    static bool _otherIsReady = false;
    /// <summary>InGameのscene in buildの数字</summary>
    int _inGameSceneInBuildNum = 1;

    private void Awake()
    {
        // しんぐるとん
        if (_instance) Destroy(gameObject);
        else _instance = this;

        // マウスのロックが解除される
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Canvasの初期設定
        _selectModeCanvas.SetActive(false);
        _createRoomCanvas.SetActive(false);
        _joinRoomCanvas.SetActive(false);

        // load textを表示し接続処理に入る
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Connecting Network...";
        ConnectNetwork(); // networkに接続

        // 30だったけこれ
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    /// <summary>ロビーに接続、またはロビー接続時の処理を実行</summary>
    void ConnectNetwork()
    {
        if (PhotonNetwork.IsConnected) // 自分の接続状態で条件分岐
        {
            if (PhotonNetwork.InRoom) Debug.Log("in room"); // in room のとき
            else if (PhotonNetwork.InLobby) Debug.Log("in lobby"); // in lobby のとき
            else OnConnectedToMaster();
        }
        else PhotonNetwork.ConnectUsingSettings();
        Debug.Log("接続処理");
    }

    /// <summary>指定したUIObjに遷移する</summary>
    void ChangeCanvas(GameObject openCanvas = null)
    {
        _activeCanvas?.SetActive(false);
        _activeCanvas = openCanvas;
    }

    /// <summary>PlayerNameを更新する</summary>
    void ReflectPlayerName() // on join, left call
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _mineNameText.text = PhotonNetwork.PlayerList[0].NickName;
            if (PhotonNetwork.PlayerList.Length == 2) _otherNameText.text = PhotonNetwork.PlayerList[1].NickName;
        }
        else
        {
            _mineNameText.text = PhotonNetwork.PlayerList[1].NickName;
            _otherNameText.text = PhotonNetwork.PlayerList[0].NickName;
        }
    }

    /// <summary>RoomListに更新があったときroom一覧のScrollViewを更新する</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // contentの子オブジェクトを全て削除する
        foreach (Transform child in _roomContent.transform) Destroy(child.gameObject);
        foreach (RoomInfo roomInfo in roomList)
        {
            Instantiate(_SelectRoomButton, _roomContent.transform).
                GetComponent<SelectRoomButtonManager>().Initialization(roomInfo);
        }
    }

    /// <summary>RoomListのボタンから部屋に入るための処理</summary>
    /// <param name="roomInfo"></param>
    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers) return;
        PhotonNetwork.JoinRoom(roomInfo.Name);

        // join roomのロードに入る
        ChangeCanvas(); // canvasを閉じる
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Joining Room...";
    }

    void IOnEventCallback.OnEvent(EventData e)
    {
        if ((int) e.Code == 0) // other readyが更新された
        {
            _otherIsReady = (bool)e.CustomData;

            if (_otherIsReady) // 受け取った情報に対する処理
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    _playButton.ChangeButtonState(true, "Game Start", OnPlayButtonToStartGame);
                }
                else
                {
                    _playButton.ChangeButtonState(true, "Cancel Ready", OnPlayButtonToNotReady);
                }
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    _playButton.ChangeButtonState(false, "waiting ready...");
                }
                else
                {
                    _playButton.ChangeButtonState(true, "Ready", OnPlayButtonToReady);
                }
            }
        }
        else if ((int)e.Code == 1) // ニックネームが変更された
        {

        }
    }

    #region Button Action ===================================================================
    void OnPlayButtonToSelect()
    {
        _selectModeCanvas.SetActive(true);
    }

    void OnPlayButtonToStartGame()
    {
        PhotonNetwork.LoadLevel(_inGameSceneInBuildNum);
    }

    void OnPlayButtonToReady()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(0, true, raiseEventOptions, new SendOptions());
    }

    void OnPlayButtonToNotReady()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(0, false, raiseEventOptions, new SendOptions());
    }

    public void OnCreateRoomButton()
    {
        if (!string.IsNullOrEmpty(_inputRoomName.text))
        {
            RoomOptions options = new RoomOptions { MaxPlayers = _maxRoomPlayer };
            PhotonNetwork.CreateRoom(_inputRoomName.text, options); // Room作成
            ChangeCanvas();
            _loadingText.gameObject.SetActive(true);
            _loadingText.text = "Making Room...";
        }
        else Debug.Log("roomの名前が設定されていません"); // to:do 入力がないエラー
    }

    public void OnChangeNameButton()
    {
        //PhotonNetwork.NickName = _inputPlayerNameText.text;
        //_playerNameText.text = "Player Name : " + PhotonNetwork.NickName;
    }

    /// <summary>BackButtonを押したときの処理</summary>
    public void OnBackButton()
    {
        if (PhotonNetwork.InRoom)
        {
            LeaveRoom();
        }

        if (_activeCanvas == _createRoomCanvas || _activeCanvas == _joinRoomCanvas)
        {
            ChangeCanvas(_selectModeCanvas);
        }
        else
        {
            ChangeCanvas();
        }
    }
    /// <summary>roomから退出する</summary>
    void LeaveRoom()
    {
        Debug.Log("leave room");
        PhotonNetwork.LeaveRoom();
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Leaving Room...";
    }
    #endregion

    #region Photon Call Back ==============================================================
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // ロビーに接続
        _loadingText.text = "Joining Room..."; // テキスト更新
        // Master Clientと同じレベルをロード Masterがシーン遷移すると同じシーンに遷移する(遅延あり)
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        _loadingText.gameObject.SetActive(false);
        _playButton.ChangeButtonState(true, "Play", OnPlayButtonToSelect);
    }
    public override void OnJoinedRoom()
    {
        _loadingText.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            _playButton.ChangeButtonState(false, "waiting player...");
        }
        else
        {
            _playButton.ChangeButtonState(true, "Ready", OnPlayButtonToReady);
        }
    }
    public override void OnLeftRoom()
    {
        _loadingText.gameObject.SetActive(false);
        ChangeCanvas();
        _playButton.ChangeButtonState(true, "Play", OnPlayButtonToSelect);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _playButton.ChangeButtonState(false, "waiting ready...");
        ReflectPlayerName();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ReflectPlayerName(); // player listのUI更新
    }
    public override void OnMasterClientSwitched(Player newMasterClient) // Masterに切り替わったらButton変更
    {
        _playButton.ChangeButtonState(false, "waiting player...");
    }
    #endregion
}
