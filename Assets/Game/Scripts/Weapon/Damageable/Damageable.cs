using Photon.Pun;
using System;
using UnityEngine;

/// <summary>�����ꂽ�Ƃ���Action������</summary>
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

    /// <summary>���g�̔�_�����������Ăяo���A���L����</summary>
    public void OnDamageTakenInvoker(int dmg, Collider collider)
    {
        photonView.RPC(nameof(OnDamageTakenShare), RpcTarget.All, dmg, Array.IndexOf(_colliders, collider));
        OnDamageTaken(dmg, Array.IndexOf(_colliders, collider));
    }

    /// <summary>���L�����e����(must PunRPC)</summary>
    [PunRPC]
    protected abstract void OnDamageTakenShare(int dmg, int collierIndex);
    /// <summary>���L���Ȃ���e����</summary>
    protected virtual void OnDamageTaken(int dmg, int colliderIndex) { }
}