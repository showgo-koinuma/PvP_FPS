using Photon.Pun;
using UnityEngine;

/// <summary>Player全てを管理する</summary>
public class PlayerManager : MonoBehaviourPun
{
    int _score = 0;

    public void AddScore()
    {
        _score++;
        if (_score >= 10) // ゲーム終了条件
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
