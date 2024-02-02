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
    [SerializeField] PlayableDirector _openingTimeline;
    [SerializeField, Tooltip("player�̃X�|�[���n�_ [0]:Master, [1]:not Master")] Vector3[] _playerSpawnPoints;
    [SerializeField] GameObject _masterRespawnWall;
    [SerializeField] GameObject _otherRespawnWall;
    [SerializeField] TMP_Text _gameStartCountText;
    [SerializeField] GameObject _gameStartWall;

    [Header("Result")]
    [SerializeField] ResultManager _resultManager;
    [SerializeField] CustomButton _continueButton;

    static InGameManager _instance;
    public static InGameManager Instance { get => _instance; }
    public event Action UpdateAction;

    public GameState GameState = GameState.Ready;

    public Vector3[] PlayerSpawnPoints { get => _playerSpawnPoints; }
    bool _otherContinue = false;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;

        Cursor.lockState = CursorLockMode.Locked; // �J�[�\��
        Cursor.visible = false;
    }

    private void Start()
    {
        _openingTimeline.Play();
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
        yield return new WaitForSeconds(2); // count down �J�n�܂�
        _gameStartCountText.text = "5";
        yield return new WaitForSeconds(0.5f);
        _gameStartCountText.GetComponent<Animator>().Play("CountDownTextScale");
        yield return new WaitForSeconds(0.5f);
        _gameStartCountText.text = "4";
        yield return new WaitForSeconds(1);
        _gameStartCountText.text = "3";
        yield return new WaitForSeconds(1);
        _gameStartCountText.text = "2";
        yield return new WaitForSeconds(1);
        _gameStartCountText.text = "1";
        yield return new WaitForSeconds(1);
        _gameStartCountText.text = "Go";
        _gameStartCountText.DOFade(0, 0.5f).OnComplete(() => _gameStartCountText.gameObject.SetActive(false));
        _gameStartCountText.rectTransform.DOScale(4, 0.5f);
        yield return new WaitForSeconds(0.5f);
        // game start
        _gameStartWall.transform.DOMove(Vector3.up * 3, 1f).OnComplete(() => _gameStartWall.SetActive(false));

        foreach (Collider col in PhotonNetwork.IsMasterClient ? _masterRespawnWall.GetComponentsInChildren<Collider>() : _otherRespawnWall.GetComponentsInChildren<Collider>())
        { // �����̃��X��collider�͏���
            col.enabled = false;
        }
    }

    /// <summary>�Q�[���𑱂����I��</summary>
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

    /// <summary>�Q�[���I����I��</summary>
    public void SelectEndGame() // button call
    {
        photonView.RPC(nameof(GameEnded), RpcTarget.Others); // �I�������L
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    /// <summary>���肪�Q�[���I����I������</summary>
    [PunRPC]
    void GameEnded()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}

public enum GameState
{
    Ready,
    InGame,
    Result
}