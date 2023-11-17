using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] GunStatus _gunStatus;
    [SerializeField] Transform _muzzlePos;
    GunState _currentGunState = GunState.nomal;
    int _currentMagazine;
    LineRenderer _ballisticLine;
    float _ballisticFadeOutTime = 0.02f;

    private void Awake()
    {
        _ballisticLine = GetComponent<LineRenderer>();
        _currentMagazine = _gunStatus.FullMagazineSize;
    }

    void Fire()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire)) return; // nomalでないと撃てない
        if (_currentMagazine <= 0) // 弾がなければreload
        {
            Reload();
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamageTaken(_gunStatus.Damage, hit.collider);
            }
        }

        _currentMagazine--;
        _currentGunState = GunState.interval;
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval);
        _ballisticLine.SetPosition(0, _muzzlePos.position);
        _ballisticLine.SetPosition(1, hit.point);
        StartCoroutine(FadeBallistic());
    }

    void Reload()
    {
        if (_currentGunState != GunState.nomal) return;
        Debug.Log("reload");
        _currentGunState = GunState.reloading;
        Invoke(nameof(ReturnGunState), _gunStatus.ReloadTime);
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // 強引すぎるか
    }

    /// <summary>gun stateをnomalに戻す</summary>
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
        InGameManager.Instance.UpdateAction += Fire;
    }
}

enum GunState
{
    nomal,
    interval,
    reloading
}