using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviourPun
{
    [SerializeField] GunStatus _gunStatus;
    [SerializeField] Transform _muzzlePos;
    [SerializeField, Tooltip("’e“¹‚ÌLine")] LineRenderer _ballisticLine;
    GunState _currentGunState = GunState.nomal;
    int _currentMagazine;
    float _ballisticFadeOutTime = 0.02f;

    private void Awake()
    {
        if (!photonView.IsMine) this.enabled = false;
        _currentMagazine = _gunStatus.FullMagazineSize;
    }

    /// <summary>ËŒ‚‚É‚Ç‚Ì‚æ‚¤‚Èˆ—‚ğ‚·‚é‚©ŒvZ‚·‚é</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire)) return; // nomal‚Å‚È‚¢‚ÆŒ‚‚Ä‚È‚¢
        if (_currentMagazine <= 0) // ’e‚ª‚È‚¯‚ê‚Îreload
        {
            Reload();
            return;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit))
        {
            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.transform.position - hit.point);
                //photonView.RPC(nameof(), RpcTarget.All, hit.transform);
            }
            else
            {
                photonView.RPC(nameof(FireNoAction), RpcTarget.All, hit.point);
            }
        }

        _currentMagazine--;
        _currentGunState = GunState.interval;
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval);
    }

    /// <summary>ŒvZŒ‹‰Ê‚Ìˆ—‚ğ”½‰f‚·‚é(no Action)</summary>
    [PunRPC]
    void FireNoAction(Vector3 hitPos)
    {
        Debug.Log("no aciton");
        _ballisticLine.SetPosition(0, _muzzlePos.position);
        _ballisticLine.SetPosition(1, hitPos);
        StartCoroutine(FadeBallistic());
    }

    void Reload()
    {
        if (_currentGunState != GunState.nomal) return;
        Debug.Log("reload");
        _currentGunState = GunState.reloading;
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // ‹­ˆø‚·‚¬‚é‚©
    }

    /// <summary>gun state‚ğnomal‚É–ß‚·</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    IEnumerator FadeBallistic()
    {
        Vector3 pos = _muzzlePos.position;
        yield return new WaitForSeconds(_ballisticFadeOutTime);
        _ballisticLine.SetPosition(1, pos);
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += FireCalculation;
    }
}

enum GunState
{
    nomal,
    interval,
    reloading
}