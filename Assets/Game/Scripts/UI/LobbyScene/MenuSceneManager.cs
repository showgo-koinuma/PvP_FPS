using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// メニューシーンのマネージャーコンポーネント
/// ネットワーク系の処理を行う
/// </summary>
public class MenuSceneManager : MonoBehaviourPunCallbacks
{
    static MenuSceneManager _instance;
    public static MenuSceneManager Instance => _instance;

    [Header("ロードパネル")]
    [SerializeField] GameObject _loadingPanel;
    [SerializeField] Text _loadingText;

    /// <summary>全てのロビーに表示されるボタンが入った親オブジェクト</summary>
    [SerializeField] GameObject _buttons;

    [Header("ルームパネル")]
    [SerializeField] GameObject _createRoomPanel;
    [SerializeField] GameObject _roomPanel;
    [SerializeField] Text _roomName;
    [SerializeField] Text _enterRoomName;

    [Header("エラーパネル")]
    [SerializeField] GameObject _errorPanel;
    [SerializeField] Text _errorText;

    [Header("ルームパネル")]
    [SerializeField] GameObject _roomListPanel;

    /// <summary>生成するボタンのPrefab内のスクリプト</summary>
    [SerializeField] Room _originalRoomButton;

    [SerializeField] GameObject _roomButtonContent;

    /// <summary>ルームの情報を扱う辞書</summary>
    Dictionary<string, RoomInfo> _roomList = new Dictionary<string, RoomInfo>();

    /// <summary>ルームボタンを扱うリスト</summary>
    List<Room> _allRoomButtons = new List<Room>();

    /// <summary>名前テキスト</summary>
    [SerializeField] Text _playerNameText;

    /// <summary>名前テキスト格納リスト</summary>
    List<Text> _allPlayerNames = new List<Text>();

    /// <summary>名前テキストの親オブジェクト</summary>
    [SerializeField] GameObject _playerNameContent;

    /// <summary>名前入力パネル</summary>
    [SerializeField] GameObject _nameInputPanel;

    /// <summary>名前入力表示テキスト</summary>
    [SerializeField] Text _placeHolderText;

    /// <summary>入力フィールド</summary>
    [SerializeField] InputField _nameInput;

    /// <summary>一部屋の最大人数</summary>
    byte _maxPlayerNum = 2;

    /// <summary>名前を入力したか判定</summary>
    static bool _setName;

    /// <summary>スタートボタン</summary>
    [SerializeField] GameObject _startButton;

    /// <summary>遷移シーン名</summary>
    [SerializeField] string _battleMode;

    /// <summary>遷移シーン名</summary>
    [SerializeField] GameObject _battleStatsPanel;

    //[SerializeField] MenuUIManager _menuUIManager;

    private void Awake()
    {
        //static変数に格納
        _instance = this;
    }

    private void Start()
    {
        //UIをすべて閉じる関数を呼ぶ
        CloseMenuUI();

        //マウスのロックが解除される
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //パネルとテキストを更新
        _loadingPanel.SetActive(true);
        _loadingText.text = "ネットワークに接続中…";

        IsConnected();
    }

    /// <summary>
    /// ネットワークに繋がっているか判定
    /// </summary>
    private void IsConnected()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //ネットワークに接続
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //ロビーに接続
            OnConnectedToMaster();
        }
    }

    /// <summary>
    /// メニューUIをすべて閉じる関数
    /// </summary>
    void CloseMenuUI()
    {
        _loadingPanel.SetActive(false);

        _buttons.SetActive(false);

        _createRoomPanel.SetActive(false);

        _roomPanel.SetActive(false);

        _errorPanel.SetActive(false);

        _roomListPanel.SetActive(false);

        _nameInputPanel.SetActive(false);

        _battleStatsPanel.SetActive(false);
    }

    /// <summary>
    /// ロビーUIを表示する関数
    /// ボタンで関数を使用する都合上publicになってしまっている。要検討
    /// </summary>
    public void LobbyMenuDisplay()
    {
        CloseMenuUI();
        _buttons.SetActive(true);
    }

    /// <summary>
    /// ロビーに接続する関数
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //ロビーに接続
        PhotonNetwork.JoinLobby();

        //テキスト更新
        _loadingText.text = "ロビーに参加中…";

        //Master Clientと同じレベルをロード
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// ロビー接続時に呼ばれる関数
    /// </summary>
    public override void OnJoinedLobby()
    {
        LobbyMenuDisplay();

        //辞書の初期化
        _roomList.Clear();

        //名前が入力済みか確認してUI更新
        ConfirmationName();
    }

    /// <summary>
    /// ルームを作るボタン用の関数作成
    /// </summary>
    public void OpenCreateRoomPanel()
    {
        CloseMenuUI();

        _createRoomPanel.SetActive(true);
    }

    /// <summary>
    /// ルームを作成ボタン用の関数 
    /// ボタンで関数を使用する
    /// </summary>
    public void CreateRoomButton()
    {
        if (!string.IsNullOrEmpty(_enterRoomName.text))
        {
            RoomOptions options = new RoomOptions { MaxPlayers = _maxPlayerNum };

            //ルーム作成
            PhotonNetwork.CreateRoom(_enterRoomName.text, options);

            CloseMenuUI();

            //ロードパネルを表示
            _loadingText.text = "ルームを作成中...";
            _loadingPanel.SetActive(true);
        }

    }

    /// <summary>
    /// ルーム参加時に呼ばれる関数
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenuUI();

        _roomPanel.SetActive(true);

        //ルームの名前を反映
        _roomName.text = PhotonNetwork.CurrentRoom.Name;

        //ルームにいるプレイヤー情報を取得する
        GetAllPlayer();

        //マスターか判定してボタン表示
        CheckRoomMaster();
    }

    /// <summary>
    /// ルームを退出する関数
    //<<<<<<< HEAD
    //=======
    /// ボタンで関数を使用する
    //>>>>>>> 99b3ca560314c0a9aeb4453129390950705fe98d
    /// </summary>
    public void LeaveRoom()
    {
        //ルームから退出
        PhotonNetwork.LeaveRoom();

        CloseMenuUI();

        _loadingText.text = "退出中…";
        _loadingPanel.SetActive(true);
    }

    /// <summary>
    /// ルーム退出時に呼ばれる関数
    /// </summary>
    public override void OnLeftRoom()
    {
        //ロビーUI表示
        LobbyMenuDisplay();
    }

    /// <summary>
    /// ルーム作成に失敗したときに呼ばれる関数
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //UIの表示を変える
        CloseMenuUI();

        _errorText.text = "ルームの作成に失敗しました" + message;

        _errorPanel.SetActive(true);
    }

    /// <summary>
    /// ルーム一覧パネルを開く関数作成
    /// ボタンで関数を使用する
    /// </summary>
    public void FindRoom()
    {
        CloseMenuUI();
        _roomListPanel.SetActive(true);
    }

    /// <summary>
    /// ルームリストに更新があった時に呼ばれる関数
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //ルーム情報を一旦初期化
        RoomUIInitialize();

       　//辞書に登録
        UpdateRoomList(roomList);
    }

    /// <summary>
    /// ルーム情報を辞書に登録
    /// </summary>
    /// <param name="roomList"></param>
    void UpdateRoomList(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList)
            {
                _roomList.Remove(info.Name);
            }
            else
            {
                _roomList[info.Name] = info;
            }
        }

        //ルームボタン表示関数
        RoomListDisplay(_roomList);
    }

    /// <summary>
    /// ルームボタンを作成して表示
    /// </summary>
    /// <param name="cachedRoomList"></param>
    void RoomListDisplay(Dictionary<string, RoomInfo> cachedRoomList)
    {
        foreach (var roomInfo in cachedRoomList)
        {
            //ボタンを作成
            //Room newButton = Instantiate(_originalRoomButton);

            //生成したボタンにルーム情報設定
            //newButton.SetRoomInfo(roomInfo.Value);

            //親の設定
            //newButton.transform.SetParent(_roomButtonContent.transform);

            //_allRoomButtons.Add(newButton);
        }
    }

    /// <summary>
    /// ルームボタンのUI初期化関数
    /// </summary>
    void RoomUIInitialize()
    {
        foreach (Room rm in _allRoomButtons)
        {
            //削除
            //Destroy(rm.gameObject);
        }

        //リストの初期化
        _allRoomButtons.Clear();
    }

    /// <summary>
    /// 引数のルームに入る関数
    /// </summary>
    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers)
        {
            return;
        }

        //ルームに参加
        PhotonNetwork.JoinRoom(roomInfo.Name);

        //UIを閉じる
        CloseMenuUI();

        _loadingText.text = "ルームに参加中";
        _loadingPanel.SetActive(true);
    }

    /// <summary>
    /// ルームにいるプレイヤー情報を取得する
    /// </summary>
    void GetAllPlayer()
    {
        //名前テキストを初期化
        InitializePlayerList();
        Debug.Log("ルームプレイヤー取得");
        //プレイヤー表示関数
        PlayerDisplay();
    }

    /// <summary>
    /// 名前テキストを初期化
    /// </summary>
    void InitializePlayerList()
    {
        foreach (var rm in _allPlayerNames)
        {
            Destroy(rm.gameObject);
        }

        _allPlayerNames.Clear();
    }

    /// <summary>
    /// プレイヤーを表示する関数
    /// </summary>
    void PlayerDisplay()
    {
        //ルームに参加している人数分UI表示
        foreach (var players in PhotonNetwork.PlayerList)
        {
            //UI作成関数
            PlayerTextGeneration(players);
        }
    }

    /// <summary>
    /// UIを生成する関数
    /// </summary>
    void PlayerTextGeneration(Player players)
    {
        //UI生成
        Text newPlayerText = Instantiate(_playerNameText);

        //テキストに名前を反映
        newPlayerText.text = players.NickName;

        //親オブジェクトの設定
        newPlayerText.transform.SetParent(_playerNameContent.transform);

        //リストに登録
        _allPlayerNames.Add(newPlayerText);
    }

    /// <summary>
    /// 名前が入力済みか確認してUI更新
    /// </summary>
    void ConfirmationName()
    {
        if (!_setName)
        {
            CloseMenuUI();
            _nameInputPanel.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                _placeHolderText.text = PlayerPrefs.GetString("playerName");
                _nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    /// <summary>
    /// 名前決定時のボタン用関数
    /// </summary>
    public void SetName()
    {
        //入力フィールドに文字が入力されているかどうか
        if (!string.IsNullOrEmpty(_nameInput.text))
        {
            //ユーザー名登録
            PhotonNetwork.NickName = _nameInput.text;

            //保存
            PlayerPrefs.SetString("playerName", _nameInput.text);

            //UI表示
            LobbyMenuDisplay();

            _setName = true;

            //_menuUIManager.SetName = true;
        }
    }

    /// <summary>
    /// プレイヤーがルームに入った時に呼び出される関数
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerTextGeneration(newPlayer);
    }

    /// <summary>
    /// プレイヤーがルームから離れるか、非アクティブになった時に呼び出される関数
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayer();
    }

    /// <summary>
    /// マスターか判定してボタン表示
    /// </summary>
    void CheckRoomMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _startButton.SetActive(true);
        }
        else
        {
            _startButton.SetActive(false);
        }
    }

    /// <summary>
    /// マスターが切り替わった時に呼ばれる関数
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _startButton.SetActive(true);
        }
    }

    /// <summary>
    /// 遷移関数
    /// ボタンから設定
    /// </summary>
    public void PlayGame()
    {
        PhotonNetwork.LoadLevel(_battleMode);
    }

    /// <summary>
    /// 射撃練習場に入る。ボタンから行う
    /// </summary>
    public void EnterPracticeRange()
    {
        SceneManager.LoadScene("PracticeRange");
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 名前を変更する。ボタンから行う
    /// </summary>
    public void ChangeName()
    {
        CloseMenuUI();
        _nameInputPanel.SetActive(true);

        if (PlayerPrefs.HasKey("playerName"))
        {
            _placeHolderText.text = PlayerPrefs.GetString("playerName");
            _nameInput.text = PlayerPrefs.GetString("playerName");
        }
    }
}
