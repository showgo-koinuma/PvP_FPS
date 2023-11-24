using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviourPun
{
    [SerializeField] GunStatus _gunStatus;
    [SerializeField] Transform _muzzlePos;
    [SerializeField, Tooltip("�e����Line")] LineRenderer _ballisticLine;
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

    /// <summary>�ˌ����ɂǂ̂悤�ȏ��������邩�v�Z����</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            if (_currentDiffusion > 0.1f) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
            else _currentDiffusion = 0;
            return; // nomal�łȂ��ƌ��ĂȂ�
        }

        if (_currentMagazine <= 0) // �e���Ȃ����reload
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
                photonView.RPC(nameof(FireNoAction), RpcTarget.All, hit.point); // ����obj�łȂ���ΒP���ȏ����ƂȂ�
            }
        }

        // ads���͊g�U���Ȃ�
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion = 0;

        _headCntler.Recoil(UnityEngine.Random.Range(0, -_gunStatus.RecoilY), UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX)); // ��������ʂɔ��f
        _currentMagazine--;
        _currentGunState = GunState.interval; // �C���^�[�o���ɓ����
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // �w�莞�ԂŖ߂�
    }

    /// <summary>�v�Z���ʂ̏����𔽉f����(no Action)</summary>
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
        Invoke((new Action(delegate { _currentMagazine = _gunStatus.FullMagazineSize; })).Method.Name, _gunStatus.ReloadTime); // ���������邩
    }

    void ADS()
    {
        _headCntler.OnADSCamera(PlayerInput.Instance.IsADS, _gunStatus.ADSFov, _gunStatus.ADSSpeed);
    }

    /// <summary>gun state��nomal�ɖ߂�</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOut����e����`�悷��</summary>
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
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // �I�u�W�F�N�g���L
        if (!photonView.IsMine) return;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
        if (PhotonNetwork.LocalPlayer.IsMasterClient) // hit layer��������
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