using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviourPun
{
    [SerializeField] GunStatus _gunStatus;
    [SerializeField] Transform _muzzlePos;
    [SerializeField, Tooltip("弾道のLine")] LineRenderer _ballisticLine;
    GunState _currentGunState = GunState.nomal;
    int _hitLayer;
    int _currentMagazine;
    float _ballisticFadeOutTime = 0.02f;

    private void Awake()
    {
        _currentMagazine = _gunStatus.FullMagazineSize;
    }

    /// <summary>射撃時にどのような処理をするか計算する</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire)) return; // nomalでないと撃てない
        if (_currentMagazine <= 0) // 弾がなければreload
        {
            Reload();
            return;
        }

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, float.MaxValue, _hitLayer))
        {
            Debug.Log(hit.collider.name);

            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.point - hit.transform.position, photonView.ViewID);
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

    /// <summary>計算結果の処理を反映する(no Action)</summary>
    [PunRPC]
    void FireNoAction(Vector3 hitPos)
    {
        StartCoroutine(DrawBallistic(hitPos));
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

    /// <summary>FadeOutする弾道を描画する</summary>
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
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // オブジェクト共有
        if (!photonView.IsMine) return;
        InGameManager.Instance.UpdateAction += FireCalculation;
        if (PhotonNetwork.LocalPlayer.IsMasterClient) // hit layerを初期化
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