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
    [SerializeField, Tooltip("scroll view��content")] GameObject _roomContent;
    [SerializeField, Tooltip("content�ɕ��ׂ�room�{�^��")] GameObject _SelectRoomButton;

    [Header("Player Name")]
    [SerializeField] TMP_Text _mineNameText;
    [SerializeField] TMP_Text _otherNameText;

    static LobbyManager _instance;
    public static LobbyManager Instance { get => _instance; }

    /// <summary>���݃A�N�e�B�u�ɂȂ��Ă���UI�I�u�W�F�N�g</summary>
    GameObject _activeCanvas;
    /// <summary>�v���C�{�^����Action</summary>
    UnityEvent _playButtonAction;
    /// <summary>Room�̍ő�l��</summary>
    int _maxRoomPlayer = 2;
    /// <summary>other��ready���o���Ă��邩</summary>
    static bool _otherIsReady = false;
    /// <summary>InGame��scene in build�̐���</summary>
    int _inGameSceneInBuildNum = 1;

    private void Awake()
    {
        // ���񂮂�Ƃ�
        if (_instance) Destroy(gameObject);
        else _instance = this;

        // �}�E�X�̃��b�N�����������
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Canvas�̏����ݒ�
        _selectModeCanvas.SetActive(false);
        _createRoomCanvas.SetActive(false);
        _joinRoomCanvas.SetActive(false);

        // load text��\�����ڑ������ɓ���
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Connecting Network...";
        ConnectNetwork(); // network�ɐڑ�

        // 30������������
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
    }

    /// <summary>���r�[�ɐڑ��A�܂��̓��r�[�ڑ����̏��������s</summary>
    void ConnectNetwork()
    {
        if (PhotonNetwork.IsConnected) // �����̐ڑ���Ԃŏ�������
        {
            if (PhotonNetwork.InRoom) Debug.Log("in room"); // in room �̂Ƃ�
            else if (PhotonNetwork.InLobby) Debug.Log("in lobby"); // in lobby �̂Ƃ�
            else OnConnectedToMaster();
        }
        else PhotonNetwork.ConnectUsingSettings();
        Debug.Log("�ڑ�����");
    }

    /// <summary>�w�肵��UIObj�ɑJ�ڂ���</summary>
    void ChangeCanvas(GameObject openCanvas = null)
    {
        _activeCanvas?.SetActive(false);
        _activeCanvas = openCanvas;
    }

    /// <summary>PlayerName���X�V����</summary>
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

    /// <summary>RoomList�ɍX�V���������Ƃ�room�ꗗ��ScrollView���X�V����</summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // content�̎q�I�u�W�F�N�g��S�č폜����
        foreach (Transform child in _roomContent.transform) Destroy(child.gameObject);
        foreach (RoomInfo roomInfo in roomList)
        {
            Instantiate(_SelectRoomButton, _roomContent.transform).
                GetComponent<SelectRoomButtonManager>().Initialization(roomInfo);
        }
    }

    /// <summary>RoomList�̃{�^�����畔���ɓ��邽�߂̏���</summary>
    /// <param name="roomInfo"></param>
    public void JoinRoom(RoomInfo roomInfo)
    {
        if (roomInfo.PlayerCount == roomInfo.MaxPlayers) return;
        PhotonNetwork.JoinRoom(roomInfo.Name);

        // join room�̃��[�h�ɓ���
        ChangeCanvas(); // canvas�����
        _loadingText.gameObject.SetActive(true);
        _loadingText.text = "Joining Room...";
    }

    void IOnEventCallback.OnEvent(EventData e)
    {
        if ((int) e.Code == 0) // other ready���X�V���ꂽ
        {
            _otherIsReady = (bool)e.CustomData;

            if (_otherIsReady) // �󂯎�������ɑ΂��鏈��
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
        else if ((int)e.Code == 1) // �j�b�N�l�[�����ύX���ꂽ
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
            PhotonNetwork.CreateRoom(_inputRoomName.text, options); // Room�쐬
            ChangeCanvas();
            _loadingText.gameObject.SetActive(true);
            _loadingText.text = "Making Room...";
        }
        else Debug.Log("room�̖��O���ݒ肳��Ă��܂���"); // to:do ���͂��Ȃ��G���[
    }

    public void OnChangeNameButton()
    {
        //PhotonNetwork.NickName = _inputPlayerNameText.text;
        //_playerNameText.text = "Player Name : " + PhotonNetwork.NickName;
    }

    /// <summary>BackButton���������Ƃ��̏���</summary>
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
    /// <summary>room����ޏo����</summary>
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
        PhotonNetwork.JoinLobby(); // ���r�[�ɐڑ�
        _loadingText.text = "Joining Room..."; // �e�L�X�g�X�V
        // Master Client�Ɠ������x�������[�h Master���V�[���J�ڂ���Ɠ����V�[���ɑJ�ڂ���(�x������)
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
        ReflectPlayerName(); // player list��UI�X�V
    }
    public override void OnMasterClientSwitched(Player newMasterClient) // Master�ɐ؂�ւ������Button�ύX
    {
        _playButton.ChangeButtonState(false, "waiting player...");
    }
    #endregion
}
