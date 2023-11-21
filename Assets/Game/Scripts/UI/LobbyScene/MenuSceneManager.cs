using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// ���j���[�V�[���̃}�l�[�W���[�R���|�[�l���g
/// �l�b�g���[�N�n�̏������s��
/// </summary>
public class MenuSceneManager : MonoBehaviourPunCallbacks
{
    static MenuSceneManager _instance;
    public static MenuSceneManager Instance => _instance;

    [Header("���[�h�p�l��")]
    [SerializeField] GameObject _loadingPanel;
    [SerializeField] Text _loadingText;

    /// <summary>�S�Ẵ��r�[�ɕ\�������{�^�����������e�I�u�W�F�N�g</summary>
    [SerializeField] GameObject _buttons;

    [Header("���[���p�l��")]
    [SerializeField] GameObject _createRoomPanel;
    [SerializeField] GameObject _roomPanel;
    [SerializeField] Text _roomName;
    [SerializeField] Text _enterRoomName;

    [Header("�G���[�p�l��")]
    [SerializeField] GameObject _errorPanel;
    [SerializeField] Text _errorText;

    [Header("���[���p�l��")]
    [SerializeField] GameObject _roomListPanel;

    /// <summary>��������{�^����Prefab���̃X�N���v�g</summary>
    [SerializeField] Room _originalRoomButton;

    [SerializeField] GameObject _roomButtonContent;

    /// <summary>���[���̏�����������</summary>
    Dictionary<string, RoomInfo> _roomList = new Dictionary<string, RoomInfo>();

    /// <summary>���[���{�^�����������X�g</summary>
    List<Room> _allRoomButtons = new List<Room>();

    /// <summary>���O�e�L�X�g</summary>
    [SerializeField] Text _playerNameText;

    /// <summary>���O�e�L�X�g�i�[���X�g</summary>
    List<Text> _allPlayerNames = new List<Text>();

    /// <summary>���O�e�L�X�g�̐e�I�u�W�F�N�g</summary>
    [SerializeField] GameObject _playerNameContent;

    /// <summary>���O���̓p�l��</summary>
    [SerializeField] GameObject _nameInputPanel;

    /// <summary>���O���͕\���e�L�X�g</summary>
    [SerializeField] Text _placeHolderText;

    /// <summary>���̓t�B�[���h</summary>
    [SerializeField] InputField _nameInput;

    /// <summary>�ꕔ���̍ő�l��</summary>
    byte _maxPlayerNum = 2;

    /// <summary>���O����͂���������</summary>
    static bool _setName;

    /// <summary>�X�^�[�g�{�^��</summary>
    [SerializeField] GameObject _startButton;

    /// <summary>�J�ڃV�[����</summary>
    [SerializeField] string _battleMode;

    /// <summary>�J�ڃV�[����</summary>
    [SerializeField] GameObject _battleStatsPanel;

    //[SerializeField] MenuUIManager _menuUIManager;

    private void Awake()
    {
        //static�ϐ��Ɋi�[
        _instance = this;
    }

    private void Start()
    {
        //UI�����ׂĕ���֐����Ă�
        CloseMenuUI();

        //�}�E�X�̃��b�N�����������
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //�p�l���ƃe�L�X�g���X�V
        _loadingPanel.SetActive(true);
        _loadingText.text = "�l�b�g���[�N�ɐڑ����c";

        IsConnected();
    }

    /// <summary>
    /// �l�b�g���[�N�Ɍq�����Ă��邩����
    /// </summary>
    private void IsConnected()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //�l�b�g���[�N�ɐڑ�
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //���r�[�ɐڑ�
            OnConnectedToMaster();
        }
    }

    /// <summary>
    /// ���j���[UI�����ׂĕ���֐�
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
    /// ���r�[UI��\������֐�
    /// �{�^���Ŋ֐����g�p����s����public�ɂȂ��Ă��܂��Ă���B�v����
    /// </summary>
    public void LobbyMenuDisplay()
    {
        CloseMenuUI();
        _buttons.SetActive(true);
    }

    /// <summary>
    /// ���r�[�ɐڑ�����֐�
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //���r�[�ɐڑ�
        PhotonNetwork.JoinLobby();

        //�e�L�X�g�X�V
        _loadingText.text = "���r�[�ɎQ�����c";

        //Master Client�Ɠ������x�������[�h
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// ���r�[�ڑ����ɌĂ΂��֐�
    /// </summary>
    public override void OnJoinedLobby()
    {
        LobbyMenuDisplay();

        //�����̏�����
        _roomList.Clear();

        //���O�����͍ς݂��m�F����UI�X�V
        ConfirmationName();
    }

    /// <summary>
    /// ���[�������{�^���p�̊֐��쐬
    /// </summary>
    public void OpenCreateRoomPanel()
    {
        CloseMenuUI();

        _createRoomPanel.SetActive(true);
    }

    /// <summary>
    /// ���[�����쐬�{�^���p�̊֐� 
    /// �{�^���Ŋ֐����g�p����
    /// </summary>
    public void CreateRoomButton()
    {
        if (!string.IsNullOrEmpty(_enterRoomName.text))
        {
            RoomOptions options = new RoomOptions { MaxPlayers = _maxPlayerNum };

            //���[���쐬
            PhotonNetwork.CreateRoom(_enterRoomName.text, options);

            CloseMenuUI();

            //���[�h�p�l����\��
            _loadingText.text = "���[�����쐬��...";
            _loadingPanel.SetActive(true);
        }

    }

    /// <summary>
    /// ���[���Q�����ɌĂ΂��֐�
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenuUI();

        _roomPanel.SetActive(true);

        //���[���̖��O�𔽉f
        _roomName.text = PhotonNetwork.CurrentRoom.Name;

        //���[���ɂ���v���C���[�����擾����
        GetAllPlayer();

        //�}�X�^�[�����肵�ă{�^���\��
        CheckRoomMaster();
    }

    /// <summary>
    /// ���[����ޏo����֐�
    //<<<<<<< HEAD
    //=======
    /// �{�^���Ŋ֐����g�p����
    //>>>>>>> 99b3ca560314c0a9aeb4453129390950705fe98d
    /// </summary>
    public void LeaveRoom()
    {
        //���[������ޏo
        PhotonNetwork.LeaveRoom();

        CloseMenuUI();

        _loadingText.text = "�ޏo���c";
        _loadingPanel.SetActive(true);
    }

    /// <summary>
    /// ���[���ޏo���ɌĂ΂��֐�
    /// </summary>
    public override void OnLeftRoom()
    {
        //���r�[UI�\��
        LobbyMenuDisplay();
    }

    /// <summary>
    /// ���[���쐬�Ɏ��s�����Ƃ��ɌĂ΂��֐�
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //UI�̕\����ς���
        CloseMenuUI();

        _errorText.text = "���[���̍쐬�Ɏ��s���܂���" + message;

        _errorPanel.SetActive(true);
    }

    /// <summary>
    /// ���[���ꗗ�p�l�����J���֐��쐬
    /// �{�^���Ŋ֐����g�p����
    /// </summary>
    public void FindRoom()
    {
        CloseMenuUI();
        _roomListPanel.SetActive(true);
    }

    /// <summary>
    /// ���[�����X�g�ɍX�V�����������ɌĂ΂��֐�
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //���[��������U������
        RoomUIInitialize();

       �@//�����ɓo�^
        UpdateRoomList(roomList);
    }

    /// <summary>
    /// ���[�����������ɓo�^
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

        //���[���{�^���\���֐�
        RoomListDisplay(_roomList);
    }

    /// <summary>
    /// ���[���{�^�����쐬���ĕ\��
    /// </summary>
    /// <param name="cachedRoomList"></param>
    void RoomListDisplay(Dictionary<string, RoomInfo> cachedRoomList)
    {
        foreach (var roomInfo in cachedRoomList)
        {
            //�{�^�����쐬
            //Room newButton = Instantiate(_originalRoomButton);

            //���������{�^���Ƀ��[�����ݒ�
            //newButton.SetRoomInfo(roomInfo.Value);

            //�e�̐ݒ�
            //newButton.transform.SetParent(_roomButtonContent.transform);

            //_allRoomButtons.Add(newButton);
        }
    }

    /// <summary>
    /// ���[���{�^����UI�������֐�
    /// </summary>
    void RoomUIInitialize()
    {
        foreach (Room rm in _allRoomButtons)
        {
            //�폜
            //Destroy(rm.gameObject);
        }

        //���X�g�̏�����
        _allRoomButtons.Clear();
    }

    /// <summary>
    /// �����̃��[���ɓ���֐�
    /// </summary>
    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers)
        {
            return;
        }

        //���[���ɎQ��
        PhotonNetwork.JoinRoom(roomInfo.Name);

        //UI�����
        CloseMenuUI();

        _loadingText.text = "���[���ɎQ����";
        _loadingPanel.SetActive(true);
    }

    /// <summary>
    /// ���[���ɂ���v���C���[�����擾����
    /// </summary>
    void GetAllPlayer()
    {
        //���O�e�L�X�g��������
        InitializePlayerList();
        Debug.Log("���[���v���C���[�擾");
        //�v���C���[�\���֐�
        PlayerDisplay();
    }

    /// <summary>
    /// ���O�e�L�X�g��������
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
    /// �v���C���[��\������֐�
    /// </summary>
    void PlayerDisplay()
    {
        //���[���ɎQ�����Ă���l����UI�\��
        foreach (var players in PhotonNetwork.PlayerList)
        {
            //UI�쐬�֐�
            PlayerTextGeneration(players);
        }
    }

    /// <summary>
    /// UI�𐶐�����֐�
    /// </summary>
    void PlayerTextGeneration(Player players)
    {
        //UI����
        Text newPlayerText = Instantiate(_playerNameText);

        //�e�L�X�g�ɖ��O�𔽉f
        newPlayerText.text = players.NickName;

        //�e�I�u�W�F�N�g�̐ݒ�
        newPlayerText.transform.SetParent(_playerNameContent.transform);

        //���X�g�ɓo�^
        _allPlayerNames.Add(newPlayerText);
    }

    /// <summary>
    /// ���O�����͍ς݂��m�F����UI�X�V
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
    /// ���O���莞�̃{�^���p�֐�
    /// </summary>
    public void SetName()
    {
        //���̓t�B�[���h�ɕ��������͂���Ă��邩�ǂ���
        if (!string.IsNullOrEmpty(_nameInput.text))
        {
            //���[�U�[���o�^
            PhotonNetwork.NickName = _nameInput.text;

            //�ۑ�
            PlayerPrefs.SetString("playerName", _nameInput.text);

            //UI�\��
            LobbyMenuDisplay();

            _setName = true;

            //_menuUIManager.SetName = true;
        }
    }

    /// <summary>
    /// �v���C���[�����[���ɓ��������ɌĂяo�����֐�
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerTextGeneration(newPlayer);
    }

    /// <summary>
    /// �v���C���[�����[�����痣��邩�A��A�N�e�B�u�ɂȂ������ɌĂяo�����֐�
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayer();
    }

    /// <summary>
    /// �}�X�^�[�����肵�ă{�^���\��
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
    /// �}�X�^�[���؂�ւ�������ɌĂ΂��֐�
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
    /// �J�ڊ֐�
    /// �{�^������ݒ�
    /// </summary>
    public void PlayGame()
    {
        PhotonNetwork.LoadLevel(_battleMode);
    }

    /// <summary>
    /// �ˌ����K��ɓ���B�{�^������s��
    /// </summary>
    public void EnterPracticeRange()
    {
        SceneManager.LoadScene("PracticeRange");
    }

    /// <summary>
    /// �Q�[���I��
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
    /// ���O��ύX����B�{�^������s��
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
