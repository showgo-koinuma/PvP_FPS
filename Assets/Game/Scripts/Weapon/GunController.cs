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
    HeadController _headCntler;
    int _hitLayer;
    int _currentMagazine;
    float _ballisticFadeOutTime = 0.02f;
    float _currentDiffusion = 0;

    private void Awake()
    {
        _headCntler = GetComponent<HeadController>();
        _currentMagazine = _gunStatus.FullMagazineSize;
    }

    /// <summary>ËŒ‚‚É‚Ç‚Ì‚æ‚¤‚Èˆ—‚ğ‚·‚é‚©ŒvZ‚·‚é</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (_currentDiffusion > 0.1f) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
            else _currentDiffusion = 0;
            return; // nomal‚Å‚È‚¢‚ÆŒ‚‚Ä‚È‚¢
        }

        if (_currentMagazine <= 0) // ’e‚ª‚È‚¯‚ê‚Îreload
        {
            Reload();
            return;
        }

        Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion),
            UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 0) * Camera.main.transform.forward;
        if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
        {
            Debug.Log(hit.collider.name);

            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.point - hit.transform.position, photonView.ViewID);
            }
            else
            {
                photonView.RPC(nameof(FireNoAction), RpcTarget.All, hit.point); // “®‚­obj‚Å‚È‚¯‚ê‚Î’Pƒ‚Èˆ—‚Æ‚È‚é
            }
        }

        // ads’†‚ÍŠgU‚µ‚È‚¢
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion = 0;

        _headCntler.Recoil(UnityEngine.Random.Range(0, -_gunStatus.RecoilY), UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX)); // ”½“®‚ğ‰æ–Ê‚É”½‰f
        _currentMagazine--;
        _currentGunState = GunState.interval; // ƒCƒ“ƒ^[ƒoƒ‹‚É“ü‚ê‚Ä
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // w’èŠÔ‚Å–ß‚·
    }

    /// <summary>ŒvZŒ‹‰Ê‚Ìˆ—‚ğ”½‰f‚·‚é(no Action)</summary>
    [PunRPC]
    void FireNoAction(Vector3 hitPos)
    {
        StartCoroutine(DrawBallistic(hitPos));
    }

    void Reload()
    {
        if (_currentGunState != GunState.nomal || _currentMagazine >= _gunStatus.FullMagazineSize) return;
        Debug.Log("reload");
        _currentGunState = GunState.reloading;
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // ‹­ˆø‚·‚¬‚é‚©
    }

    void ADS()
    {
        _headCntler.OnADSCamera(PlayerInput.Instance.IsADS, _gunStatus.ADSFov, _gunStatus.ADSSpeed);
    }

    /// <summary>gun state‚ğnomal‚É–ß‚·</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOut‚·‚é’e“¹‚ğ•`‰æ‚·‚é</summary>
    public IEnumerator DrawBallistic(Vector3 target)
    {
        _ballisticLine.SetPosition(0, _muzzlePos.position);
        _ballisticLine.SetPosition(1, target);
        Vector3 pos = _muzzlePos.position;
        yield return new WaitForSeconds(_ballisticFadeOutTime);
        _ballisticLine.SetPosition(1, pos);
    }

    private void OnEnable()
    {
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // ƒIƒuƒWƒFƒNƒg‹¤—L
        if (!photonView.IsMine) return;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
        if (PhotonNetwork.LocalPlayer.IsMasterClient) // hit layer‚ğ‰Šú‰»
        {
            gameObject.layer = 6;
            _hitLayer = ~(1 << 6);
        }
        else
        {
            gameObject.layer = 7;
            _hitLayer = ~(1 << 7);
        }
    }
}

enum GunState
{
    nomal,
    interval,
    reloading
}