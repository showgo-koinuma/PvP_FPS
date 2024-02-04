using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPun
{
    [Header("Opening")]
    [SerializeField] PlayableDirector _openingTimeline;
    [SerializeField, Tooltip("0:mine, 1:other")] TMP_Text[] _playerNameTexts;

    [Header("InGame")]
    [SerializeField, Tooltip("playerのスポーン地点 [0]:Master, [1]:not Master")] Vector3[] _playerSpawnPoints;
    [SerializeField] GameObject _masterRespawnWall;
    [SerializeField] GameObject _otherRespawnWall;
    [SerializeField] TMP_Text _gameStartCountText;
    [SerializeField, Tooltip("0:count, 1:start")] AudioClip[] _gameStartSounds;
    [SerializeField] GameObject _gameStartWall;

    [Header("Result")]
    [SerializeField] ResultManager _resultManager;
    [SerializeField] CustomButton _continueButton;

    static InGameManager _instance;
    public static InGameManager Instance { get => _instance; }
    public event Action UpdateAction;
    AudioSource _audioSource;

    public GameState GameState = GameState.Ready;

    public Vector3[] PlayerSpawnPoints { get => _playerSpawnPoints; }
    bool _otherContinue = false;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;

        Cursor.lockState = CursorLockMode.Locked; // カーソル
        Cursor.visible = false;

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // opening timeline player name set
        _playerNameTexts[0].text = PhotonNetwork.NickName;
        _playerNameTexts[1].text = PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2 ? PhotonNetwork.PlayerList[1].NickName : PhotonNetwork.MasterClient.NickName;

        _openingTimeline.Play(); // opening time line 再生
    }

    private void Update()
    {
        switch (GameState)
        {
            case GameState.Ready:
                if (_openingTimeline.state != PlayState.Playing)
                {
                    PlayerInitialSpawn();
                    if (PhotonNetwork.IsMasterClient) photonView.RPC(nameof(StartGameStartCountDown), RpcTarget.AllViaServer);
                    _gameStartCountText.text = "";
                    GameState = GameState.InGame;
                }
                break;
            case GameState.InGame:
                UpdateAction?.Invoke();
                break;
            case GameState.Result:
                break;
        }
    }

    void PlayerInitialSpawn()
    {
        Vector3 position;

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            position = InGameManager.Instance.PlayerSpawnPoints[0];
        }
        else
        {
            position = InGameManager.Instance.PlayerSpawnPoints[1];
        }

        PhotonNetwork.Instantiate("Player", position, Quaternion.identity);
    }

    [PunRPC]
    void StartGameStartCountDown()
    {
        StartCoroutine(nameof(GameStartCountDown));
    }
    IEnumerator GameStartCountDown()
    {
        yield return new WaitForSeconds(3); // count down 開始まで
        _gameStartCountText.text = "5";
        _audioSource.PlayOneShot(_gameStartSounds[0]);
        yield return new WaitForSeconds(0.5f);
        _gameStartCountText.GetComponent<Animator>().Play("CountDownTextScale");
        yield return new WaitForSeconds(0.5f);
        _gameStartCountText.text = "4";
        yield return new WaitForSeconds(0.4f);
        _audioSource.PlayOneShot(_gameStartSounds[0]);
        yield return new WaitForSeconds(0.6f);
        _gameStartCountText.text = "3";
        yield return new WaitForSeconds(0.4f);
        _audioSource.PlayOneShot(_gameStartSounds[0]);
        yield return new WaitForSeconds(0.6f);
        _gameStartCountText.text = "2";
        yield return new WaitForSeconds(0.4f);
        _audioSource.PlayOneShot(_gameStartSounds[0]);
        yield return new WaitForSeconds(0.6f);
        _gameStartCountText.text = "1";
        yield return new WaitForSeconds(0.4f);
        _audioSource.PlayOneShot(_gameStartSounds[0]);
        yield return new WaitForSeconds(0.6f);
        _gameStartCountText.rectTransform.localScale = Vector3.one * 1.4f;
        _gameStartCountText.text = "GO";
        yield return new WaitForSeconds(0.4f);
        _audioSource.PlayOneShot(_gameStartSounds[1]);
        _gameStartCountText.DOFade(0, 1f).OnComplete(() => _gameStartCountText.gameObject.SetActive(false));
        _gameStartCountText.rectTransform.DOScale(4, 1f);
        yield return new WaitForSeconds(0.5f);
        // game start
        _gameStartWall.transform.DOMove(Vector3.up * 3, 1f).OnComplete(() => _gameStartWall.SetActive(false));

        foreach (Collider col in PhotonNetwork.IsMasterClient ? _masterRespawnWall.GetComponentsInChildren<Collider>() : _otherRespawnWall.GetComponentsInChildren<Collider>())
        { // 自分のリスのcolliderは消す
            col.enabled = false;
        }
    }

    /// <summary>ゲームを続けるを選択</summary>
    public void SelectContinueGame() // button call
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        _continueButton.ChangeButtonState(false, "wait\ncontinue");

        if (_otherContinue)
        {
            photonView.RPC(nameof(RestartGame), RpcTarget.MasterClient);
        }
        else
        {
            photonView.RPC(nameof(ShareContinueGame), RpcTarget.Others);
        }
    }

    [PunRPC]
    void ShareContinueGame()
    {
        _otherContinue = true;
        _resultManager.OtherIsContinue();
    }

    [PunRPC]
    void RestartGame()
    {
        PhotonNetwork.LoadLevel(2);
    }

    /// <summary>ゲーム終了を選択</summary>
    public void SelectEndGame() // button call
    {
        photonView.RPC(nameof(GameEnded), RpcTarget.Others); // 終了を共有
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    /// <summary>相手がゲーム終了を選択した</summary>
    [PunRPC]
    void GameEnded()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    private void OnEnable()
    {
        SettingManager.Instance.QuitButton.ChangeButtonState(true, "Leave Match", GameEnded);
    }
}

public enum GameState
{
    Ready,
    InGame,
    Result
}