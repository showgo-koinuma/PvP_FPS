using Photon.Pun;
using UnityEngine;

/// <summary>Player全てを管理する</summary>
public class PlayerManager : MonoBehaviourPun
{
    [SerializeField, Tooltip("弾の当たるオブジェクトたち")] GameObject[] _hitBodyObjects;
    [SerializeField, Tooltip("[0]:IsMaster, [1]:NotMaster")] int[] _playerLayer;

    /// <summary>現在ActiveのGunController</summary>
    GunController _activeGun;
    public GunController ActiveGun { get => _activeGun;  set => _activeGun = value; }

    int _score = 0;
    int _clearScore = 1; // inGameManagerが無難か

    private void Awake()
    {
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // オブジェクト共有

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
        GetComponentInChildren<GunController>().SetHitlayer(isMaster);
        foreach (GameObject body in _hitBodyObjects) body.layer = layer;
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << layer | 1 << 8);
    }

    public void FireActionCall(Vector3 pos)
    {
        photonView.RPC(nameof(FireAction), RpcTarget.All, pos);
    }
    /// <summary>photonViewを1つにするためGunのActionをManagerで呼び出す</summary>
    [PunRPC]
    void FireAction(Vector3 pos)
    {
        _activeGun.FireAction(pos);
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

        // TO:DO 内部データの初期化
    }
}
