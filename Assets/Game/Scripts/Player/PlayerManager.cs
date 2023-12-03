using Photon.Pun;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
public class PlayerManager : MonoBehaviourPun
{
    [SerializeField, Tooltip("�e�̓�����I�u�W�F�N�g����")] GameObject[] _hitBodyObjects;
    [SerializeField, Tooltip("[0]:IsMaster, [1]:NotMaster")] int[] _playerLayer;

    /// <summary>����Active��GunController</summary>
    GunController _activeGun;
    public GunController ActiveGun { get => _activeGun;  set => _activeGun = value; }

    int _score = 0;
    int _clearScore = 1; // inGameManager�����

    private void Awake()
    {
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // �I�u�W�F�N�g���L

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
        GetComponentInChildren<GunController>().SetHitlayer(isMaster);
        foreach (GameObject body in _hitBodyObjects) body.layer = layer;
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << layer | 1 << 8);
    }

    public void FireActionCall(Vector3 pos)
    {
        photonView.RPC(nameof(FireAction), RpcTarget.All, pos);
    }
    /// <summary>photonView��1�ɂ��邽��Gun��Action��Manager�ŌĂяo��</summary>
    [PunRPC]
    void FireAction(Vector3 pos)
    {
        _activeGun.FireAction(pos);
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

        // TO:DO �����f�[�^�̏�����
    }
}
