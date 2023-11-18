using Photon.Pun;
using UnityEngine;

/// <summary>撃たれたときにActionがある</summary>
public abstract class Damageable : MonoBehaviourPunCallbacks
{
    Collider[] _colliders;
    public Collider[] Colliders { get => _colliders; }

    private void Start()
    {
        _colliders = GetComponents<Collider>();
    }

    /// <summary>自身の被ダメ時処理を呼び出し、共有する</summary>
    public void OnDamageTakenInvoker(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        photonView.RPC(nameof(OnDamageTakenShare), RpcTarget.All, damage, collierIndex, objVectorDiff, playerID);
    }

    /// <summary>共有するダメージ処理、弾道表示</summary>
    [PunRPC]
    protected abstract void OnDamageTakenShare(int damage, int collierIndex, Vector3 objVectorDiff, int playerID);
}