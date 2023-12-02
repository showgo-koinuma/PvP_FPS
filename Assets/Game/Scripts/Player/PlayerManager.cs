using Photon.Pun;
using UnityEngine;

/// <summary>Player全てを管理する</summary>
public class PlayerManager : MonoBehaviourPun
{
    [SerializeField] GameObject[] _bodyObjects;
    [SerializeField, Tooltip("[0]:IsMaster, [1]:NotMaster")] int[] _playerLayer;

    int _score = 0;
    int _clearScore = 1;

    private void Awake()
    {
        if (!photonView.IsMine)
        {
            this.enabled = false;
            return;
        }

        // 銃のレイヤーとオブジェクトレイヤーの設定
        if (PhotonNetwork.IsMasterClient) Initialization(true, _playerLayer[0]);
        else Initialization(false, _playerLayer[1]);
    }

    /// <summary>IsMaster別の初期設定</summary>
    void Initialization(bool isMaster, int layer)
    {
        GetComponent<GunController>().SetHitlayer(isMaster);
        foreach (GameObject body in _bodyObjects) body.layer = layer;
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << layer);
    }

    public void AddScore()
    {
        _score++;
        if (_score >= _clearScore) // ゲーム終了条件
        {
            InGameManager.Instance.FinishGame();
        } 
    }

    public void Respawn()
    {
        // 位置、向きの初期化
        Vector3 position;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            position = InGameManager.Instance.PlayerSpawnPoints[0];
            transform.forward = Vector3.forward;
        }
        else
        {
            position = InGameManager.Instance.PlayerSpawnPoints[1];
            transform.forward = Vector3.back;
        }
        transform.position = position;

        // TO:DO 内部データの初期化 どこでやるか
    }
}
