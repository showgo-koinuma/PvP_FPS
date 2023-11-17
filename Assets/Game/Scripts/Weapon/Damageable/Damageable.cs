using Photon.Pun;
using UnityEngine;

public abstract class Damageable : MonoBehaviourPunCallbacks
{
    Collider[] _colliders;
    public Collider[] Colliders { get => _colliders; }

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    public void OnDamageTakenInvoker(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        photonView.RPC(nameof(OnDamageTaken), RpcTarget.All, damage, collierIndex, objVectorDiff, playerID);
    }

    [PunRPC]
    protected abstract void OnDamageTaken(int damage, int collierIndex, Vector3 objVectorDiff, int playerID);
}