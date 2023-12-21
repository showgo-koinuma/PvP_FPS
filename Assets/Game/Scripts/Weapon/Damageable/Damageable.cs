using Photon.Pun;
using System;
using UnityEngine;

/// <summary>撃たれたときにActionがある</summary>
public abstract class Damageable : MonoBehaviourPun
{
    protected Collider[] _colliders;

    private void Start()
    {
        _colliders = GetComponentsInChildren<Collider>();

        //string s = string.Empty;
        //foreach (Collider collider in _colliders)
        //{
        //    s += collider.name + ", ";
        //}
        //Debug.Log(s);
    }

    /// <summary>自身の被ダメ時処理を呼び出し、共有する</summary>
    public void OnDamageTakenInvoker(int dmg, Collider collider)
    {
        photonView.RPC(nameof(OnDamageTakenShare), RpcTarget.All, dmg, Array.IndexOf(_colliders, collider));
        OnDamageTaken(dmg, Array.IndexOf(_colliders, collider));
    }

    /// <summary>共有する被弾処理(must PunRPC)</summary>
    [PunRPC]
    protected abstract void OnDamageTakenShare(int dmg, int collierIndex);
    /// <summary>共有しない被弾処理</summary>
    protected virtual void OnDamageTaken(int dmg, int colliderIndex) { }
}