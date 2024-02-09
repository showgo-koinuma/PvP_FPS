using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// ���j���[�V�[���̃}�l�[�W���[�R���|�[�l���g
/// �l�b�g���[�N�n�̏������s��
/// </summary>
public class LobbySceneUIManager : MonoBehaviourPunCallbacks
{
    [Header("Default")]
    [SerializeField] GameObject _defaultButtonsObj;

    [Header("Room�쐬")]
    [SerializeField] GameObject _createRoomObj;
    [SerializeField] TextMeshProUGUI _inputRoomName;

    [Header("Room�Q��")]
    [SerializeField] GameObject _joinRoomObj;
    [SerializeField, Tooltip("scroll view��content")] GameObject _contentRoom;
    [SerializeField] GameObject _SelectRoomButton;

    [Header("PlayerName�ݒ�")]
    [SerializeField] GameObject _InputPlayerNameObj;
    [SerializeField] TextMeshProUGUI _inputPlayerNameText;
    [SerializeField] TextMeshProUGUI _playerNameText;

    [Header("�Q�[���X�^�[�g�ҋ@")]
    [SerializeField] GameObject _waitingStartGameObj;
    [SerializeField] TextMeshProUGUI _roomNameText;
    [SerializeField, Tooltip("scroll view��content")] GameObject _contentPlayerName;
    [SerializeField, Tooltip("game start button")] GameObject _gameStartButton;
    [SerializeField, Tooltip("scroll view�ɕ\�����邽�߂�TextObj")] GameObject _nameTextObj;

    [Header("Loading")]
    [SerializeField] GameObject _loadingObj;
    [SerializeField] TextMeshProUGUI _loadingText;

    static LobbySceneUIManager _instance;
    public static LobbySceneUIManager Instance { get => _instance; }
    /// <summary>���݃A�N�e�B�u�ɂȂ��Ă���UI�I�u�W�F�N�g</summary>
    GameObject _activeObj;
    /// <summary>Room�̍ő�l��</summary>
    int _maxRoomPlayer = 2;
    /// <summary>InGame��scene in build�̐���</summary>
    int _inGameSceneInBuildNum = 1;

    private void Start()
    {
        if (!_instance) _instance = this;
        //�}�E�X�̃��b�N�����������
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //�p�l���ƃe�L�X�g���X�V
        ChangeUIObj(_loadingObj);
        _loadingText.text = "Connecting Network...";
        ConnectNetwork(); // network�ɐڑ�

        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    /// <summary>���r�[�ɐڑ��A�܂��̓��r�[�ڑ����̏��������s</summary>
    void ConnectNetwork()
    {
        if (PhotonNetwork.IsConnected) // �����̐ڑ���Ԃŏ�������
        {
            if (PhotonNetwork.InRoom) ChangeUIObj(_waitingStartGameObj); // in room �̂Ƃ�
            else if (PhotonNetwork.InLobby) ChangeUIObj(_defaultButtonsObj); // in lobby �̂Ƃ�
            else OnConnectedToMaster();
        }
        else PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>�w�肵��UIObj�ɑJ�ڂ���</summary>
    void ChangeUIObj(GameObject toUIObj)
    {
        CloseAllUI();
        (_activeObj = toUIObj).SetActive(true);
        if (_activeObj == _waitingStartGameObj)
        {
            _roomNameText.text = PhotonNetwork.CurrentRoom.Name + " Room"; // ���[���̖��O�𔽉f
            GeneratePlayerNameTextObj(); // player list��UI�X�V
            // �}�X�^�[�����肵�ă{�^���\��
            if (PhotonNetwork.IsMasterClient) _gameStartButton.SetActive(true);
            else _gameStartButton.SetActive(false);
        }
    }

    /// <summary>�S�Ă�UI���\���ɂ���</summary>
    void CloseAllUI()
    {
        _defaultButtonsObj.SetActive(false);
        _createRoomObj.SetActive(false);
        _joinRoomObj.SetActive(false);
        _InputPlayerNameObj.SetActive(false);
        _waitingStartGameObj.SetActive(false);
        _loadingObj.SetActive(false);
    }

    /// <summary>PlayerName��scroll view���X�V����</summary>
    void GeneratePlayerNameTextObj() // join, left call
    {
        // content�̎q�I�u�W�F�N�g��S�č폜����
        foreach (Transform child in _contentPlayerName.transform) Destroy(child.gameObject);
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Instantiate(_nameTextObj, _contentPlayerName.transform).GetComponent<TextMeshProUGUI>().text = player.NickName;
        }
    }

    /// <summary>RoomList�ɍX�V���������Ƃ�room�ꗗ��ScrollView���X�V����</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // content�̎q�I�u�W�F�N�g��S�č폜����
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
            PhotonNetwork.CreateRoom(_inputRoomName.text, options); // Room�쐬
            ChangeUIObj(_loadingObj); // Loading
            _loadingText.text = "Making Room...";
        } // to:do ���͂��Ȃ��G���[
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

    /// <summary>BackButton���������Ƃ��̏���</summary>
    public void OnBackButton()
    {
        if (_activeObj == _waitingStartGameObj) LeaveRoom();
        else if (_activeObj != _defaultButtonsObj) ChangeUIObj(_defaultButtonsObj);
    }
    /// <summary>room����ޏo����</summary>
    void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        ChangeUIObj(_loadingObj); // UI�X�V
        _loadingText.text = "Leaving Room...";
    }
    #endregion

    #region Photon Call Back ==============================================================
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); // ���r�[�ɐڑ�
        _loadingText.text = "Joining Room..."; // �e�L�X�g�X�V
        PhotonNetwork.AutomaticallySyncScene = true; // Master Client�Ɠ������x�������[�h
    }
    public override void OnJoinedLobby()
    {
        ChangeUIObj(_defaultButtonsObj); // UI�X�V
    }
    public override void OnJoinedRoom()
    {
        ChangeUIObj(_waitingStartGameObj);
    }
    public override void OnLeftRoom()
    {
        ChangeUIObj(_defaultButtonsObj); // default UI�ɖ߂�
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GeneratePlayerNameTextObj();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GeneratePlayerNameTextObj(); // player list��UI�X�V
    }
    public override void OnMasterClientSwitched(Player newMasterClient) // Master�ɐ؂�ւ������Button�\��
    {
        if (PhotonNetwork.IsMasterClient) _gameStartButton.SetActive(true);
    }
    #endregion
}