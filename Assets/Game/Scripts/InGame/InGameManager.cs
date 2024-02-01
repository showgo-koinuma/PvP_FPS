using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPun
{
    [SerializeField] PlayableDirector _openingTimeline;
    [SerializeField, Tooltip("playerのスポーン地点 [0]:Master, [1]:not Master")] Vector3[] _playerSpawnPoints;

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

        Cursor.lockState = CursorLockMode.Locked; // カーソル
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
}

public enum GameState
{
    Ready,
    InGame,
    Result
}