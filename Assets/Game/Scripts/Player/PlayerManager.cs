using Photon.Pun;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
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

        // �e�̃��C���[�ƃI�u�W�F�N�g���C���[�̐ݒ�
        if (PhotonNetwork.IsMasterClient) Initialization(true, _playerLayer[0]);
        else Initialization(false, _playerLayer[1]);
    }

    /// <summary>IsMaster�ʂ̏����ݒ�</summary>
    void Initialization(bool isMaster, int layer)
    {
        GetComponent<GunController>().SetHitlayer(isMaster);
        foreach (GameObject body in _bodyObjects) body.layer = layer;
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << layer);
    }

    public void AddScore()
    {
        _score++;
        if (_score >= _clearScore) // �Q�[���I������
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
