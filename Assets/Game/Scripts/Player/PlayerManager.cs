using Photon.Pun;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
public class PlayerManager : MonoBehaviourPun
{
    int _score = 0;

    public void AddScore()
    {
        _score++;
        if (_score >= 10) // �Q�[���I������
        {
            InGameManager.Instance.FinishGame();
        } 
    }

    public void Respawn()
    {
        // �ʒu�A�����̏�����
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

        // TO:DO �����f�[�^�̏����� �ǂ��ł�邩
    }
}
