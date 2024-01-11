using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviourPun
{
    [SerializeField, Tooltip("playerのスポーン地点 [0]:Master, [1]:not Master")] Vector3[] _playerSpawnPoints;

    public Vector3[] PlayerSpawnPoints { get => _playerSpawnPoints; }
    static InGameManager _instance;
    public static InGameManager Instance { get => _instance; }
    public event Action UpdateAction;

    bool _otherContinue = false;

    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
    }

    private void Start()
    {
        Vector3 position;
        Quaternion forword;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            position = InGameManager.Instance.PlayerSpawnPoints[0];
            forword = Quaternion.Euler(Vector3.forward);
        }
        else
        {
            position = InGameManager.Instance.PlayerSpawnPoints[1];
            forword = Quaternion.AngleAxis(180, Vector3.up);
        }
        PhotonNetwork.Instantiate("Player", position, forword);
    }

    /// <summary>ゲームを続けるを選択</summary>
    public void SelectContinueGame() // button call
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (_otherContinue)
        {
            photonView.RPC(nameof(ContinueGame), RpcTarget.MasterClient);
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
    }

    [PunRPC]
    void ContinueGame()
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

    private void Update()
    {
        UpdateAction?.Invoke();
    }
}