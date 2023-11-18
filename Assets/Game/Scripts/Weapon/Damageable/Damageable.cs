using Photon.Pun;
using UnityEngine;

/// <summary>�����ꂽ�Ƃ���Action������</summary>
public abstract class Damageable : MonoBehaviourPunCallbacks
{
    Collider[] _colliders;
    public Collider[] Colliders { get => _colliders; }

    private void Start()
    {
        _colliders = GetComponents<Collider>();
    }

    /// <summary>���g�̔�_�����������Ăяo���A���L����</summary>
    public void OnDamageTakenInvoker(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        photonView.RPC(nameof(OnDamageTakenShare), RpcTarget.All, damage, collierIndex, objVectorDiff, playerID);
    }

    /// <summary>���L����_���[�W�����A�e���\��</summary>
    [PunRPC]
    protected abstract void OnDamageTakenShare(int damage, int collierIndex, Vector3 objVectorDiff, int playerID);
}