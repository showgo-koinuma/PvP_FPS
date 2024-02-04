using Cinemachine;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks, IOnEventCallback
{

    [Header("Default UI")]
    [SerializeField] GameObject _defaultPlayButton;
    [SerializeField] CustomButton _playButton;
    [SerializeField] CustomButton _settingButton;
    [SerializeField] TMP_Text _loadingText;
    [SerializeField] GameObject _leaveRoomButton;

    [Header("Select Mode")]
    [SerializeField] GameObject _selectModeCanvas;

    [Header("Create Room")]
    [SerializeField] GameObject _createRoomCanvas;
    [SerializeField] TMP_Text _inputRoomName;

    [Header("Join Room")]
    [SerializeField] GameObject _joinRoomCanvas;
    [SerializeField, Tooltip("scroll view��content")] GameObject _roomContent;
    [SerializeField, Tooltip("content�ɕ��ׂ�room�{�^��")] GameObject _SelectRoomButton;
    [SerializeField] AudioSource _selectRoomButtonAudioSource;

    [Header("Player Name")]
    [SerializeField] GameObject _minePlayerCanvas;
    [SerializeField] TMP_Text _mineNameText;
    [SerializeField] GameObject _nameInputFieldObj;
    [SerializeField] TMP_Text _nameInputFieldText;
    [SerializeField] CustomButton _nameChangeButton;
    [SerializeField] GameObject _otherPlayerCanvas;
    [SerializeField] GameObject _otherPlayerModelObj;
    [SerializeField] TMP_Text _otherNameText;

    [Header("Camera, Start Event")]
    [SerializeField] GameObject _joinedCamera;
    [SerializeField] GameObject _startGameBeginCamera;
    [SerializeField] GameObject _startGameEndCamera;
    [SerializeField] Image _fadePanelImage;
    [SerializeField] AudioClip _startGameSound;

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

    private void FixedUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.RightAlt)) OnPlayButtonToStartGame();
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            _defaultPlayButton.SetActive(false);
            _minePlayerCanvas.SetActive(false);
            _otherPlayerCanvas.SetActive(false);
            _startGameBeginCamera.SetActive(true);
            _fadePanelImage.enabled = true;
            _fadePanelImage.DOFade(1, 2.4f);
            Invoke(nameof(StartBlend), 1.2f);
            _selectRoomButtonAudioSource.PlayOneShot(_startGameSound);
        }
    }

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

        _nameChangeButton.ButtonAction = ToNameInputMode;
        _mineNameText.gameObject.SetActive(true);
        _nameInputFieldObj.SetActive(false);
        _leaveRoomButton.SetActive(false);
        _joinedCamera.SetActive(false);
        _otherPlayerModelObj.SetActive(false);
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
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("�ڑ�����");
        }
    }

    /// <summary>�w�肵��UIObj�ɑJ�ڂ���</summary>
    public void ChangeCanvas(GameObject openCanvas = null)
    {
        _activeCanvas?.SetActive(false);
        (_activeCanvas = openCanvas)?.SetActive(true);
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
                GetComponent<SelectRoomButtonManager>().Initialization(roomInfo, _selectRoomButtonAudioSource);
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
                    _playButton.ChangeButtonState(false, "waiting\nready...");
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

    void StartBlend()
    {
        _startGameEndCamera.SetActive(true);
        Invoke(nameof(SenceLoadToStartGame), 1.2f);
    }

    void SenceLoadToStartGame()
    {
        PhotonNetwork.LoadLevel(_inGameSceneInBuildNum);
    }

    #region Button Action ===================================================================
    void OnPlayButtonToSelect()
    {
        (_activeCanvas = _selectModeCanvas).SetActive(true);
    }

    void OnPlayButtonToStartGame()
    {
        _defaultPlayButton.SetActive(false);
        _minePlayerCanvas.SetActive(false);
        _otherPlayerCanvas.SetActive(false);
        _startGameBeginCamera.SetActive(true);
        _fadePanelImage.enabled = true;
        _fadePanelImage.DOFade(1, 2.4f);
        Invoke(nameof(StartBlend), 1.2f);
        _selectRoomButtonAudioSource.PlayOneShot(_startGameSound);
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
        if (_inputRoomName.text != "")
        {
            RoomOptions options = new RoomOptions { MaxPlayers = _maxRoomPlayer };
            PhotonNetwork.CreateRoom(_inputRoomName.text, options); // Room�쐬
            ChangeCanvas();
            _loadingText.gameObject.SetActive(true);
            _loadingText.text = "Making Room...";
        }
    }

    void ToNameInputMode()
    {
        _nameChangeButton.ButtonAction = OnChangeNameButton;
        _mineNameText.gameObject.SetActive(false);
        _nameInputFieldObj.SetActive(true);
    }

    void OnChangeNameButton()
    {
        //PhotonNetwork.NickName = _inputPlayerNameTyext.text;
        //_playerNameText.text = "Player Name : " + PhotonNetwork.NickName;
        _nameChangeButton.ButtonAction = ToNameInputMode;
        PhotonNetwork.NickName = _nameInputFieldText.text;
        _mineNameText.text = PhotonNetwork.NickName;
        _mineNameText.gameObject.SetActive(true);
        _nameInputFieldObj.SetActive(false);
    }

    /// <summary>BackButton���������Ƃ��̏���</summary>
    public void OnBackButton()
    {
        if (_activeCanvas == _createRoomCanvas || _activeCanvas == _joinRoomCanvas)
        {
            ChangeCanvas(_selectModeCanvas);
        }
        else
        {
            ChangeCanvas();
        }
    }

    public void OnSelectTutorial()
    {
        ChangeCanvas();
        _playButton.ChangeButtonState(true, "Tutorial"); // todo set action
        _leaveRoomButton.SetActive(true);
    }

    /// <summary>room����ޏo����</summary>
    public void LeaveRoom()
    {
        Debug.Log("leave room");
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            _loadingText.gameObject.SetActive(true);
            _loadingText.text = "Leaving Room...";
        }
        else
        {
            _leaveRoomButton.SetActive(false);
            _playButton.ChangeButtonState(true, "Play", OnPlayButtonToSelect);
        }
    }
    #endregion

    #region Photon Call Back ==============================================================
    public override void OnConnectedToMaster()
    {
        _loadingText.text = "Joining Room..."; // �e�L�X�g�X�V
        PhotonNetwork.JoinLobby(); // ���r�[�ɐڑ�
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
        _leaveRoomButton.SetActive(true);
        ReflectPlayerName();

        if (PhotonNetwork.IsMasterClient)
        {
            _playButton.ChangeButtonState(false, "waiting\nplayer...");
        }
        else
        {
            _playButton.ChangeButtonState(true, "Ready", OnPlayButtonToReady);
            _joinedCamera.SetActive(true);
            _otherPlayerModelObj.SetActive(true);
        }
    }
    public override void OnLeftRoom()
    {
        _loadingText.gameObject.SetActive(false);
        ChangeCanvas();
        _playButton.ChangeButtonState(true, "Play", OnPlayButtonToSelect);
        _joinedCamera.SetActive(false);
        _otherPlayerModelObj.SetActive(false);
        _leaveRoomButton.SetActive(false);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _playButton.ChangeButtonState(false, "waiting\nready...");
        ReflectPlayerName();
        _joinedCamera.SetActive(true);
        _otherPlayerModelObj.SetActive(true);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ReflectPlayerName();
        _joinedCamera.SetActive(false);
        _otherPlayerModelObj.SetActive(false);
    }
    public override void OnMasterClientSwitched(Player newMasterClient) // Master�ɐ؂�ւ������Button�ύX
    {
        _playButton.ChangeButtonState(false, "waiting\nplayer...");
    }
    #endregion

    void ExitToDesctop()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
        Application.Quit();//�Q�[���v���C�I��
#endif
    }

    #region ActionSetting
    public override void OnEnable()
    {
        base.OnEnable();
        _settingButton.ButtonAction = () => SettingManager.Instance.SwitchCanvas();
        SettingManager.Instance.QuitButton.ChangeButtonState(true, "Exit Game", ExitToDesctop);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        _settingButton.ButtonAction = null;
    }
    #endregion
}
