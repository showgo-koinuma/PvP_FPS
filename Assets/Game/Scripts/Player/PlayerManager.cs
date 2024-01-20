using DG.Tweening;
using Photon.Pun;
using UnityEngine;

/// <summary>Player全てを管理する</summary>
public class PlayerManager : MonoBehaviourPun
{
    [Header("looks")]
    [SerializeField, Tooltip("当たり判定オブジェクト")] GameObject[] _hitObjects;
    [SerializeField, Tooltip("自分で見えなくなる(相手に映る自分のモデル)")] GameObject[] _invisibleToMyselfObj;
    [SerializeField, Tooltip("自分で見えなくなる(相手に映る自分のモデル)の親")] GameObject[] _invisibleToMyselfObjs;
    [SerializeField, Tooltip("相手から見えなくなる(自分の画面に映る自分のモデル)の親")] GameObject[] _invisibleToEnemeyObjs;
    [Header("weapon [0] : AR, [1] : SG")]
    [SerializeField, Tooltip("[0] : AR, [1] : SG")] GameObject[] _weapons;
    [SerializeField] RectTransform[] _weaponIconPivots;
    [SerializeField] GameObject[] _weaponIconOutLines;

    /// <summary>現在ActiveのGunController</summary>
    //GunController _activeGun;
    //public GunController ActiveGun { get => _activeGun;  set => _activeGun = value; }
    PlayerAnimationManager _pAnimMg;

    int _hitLayer = 6;
    int _invisibleLayer = 7;

    // result data
    int _shootCount = 0;
    public int ShootCount { get => _shootCount; }
    int _hitCount = 0;
    public int HitCount { get => _hitCount; }
    int _deadCount = 0;
    public int DeadCount { get => _deadCount; }

    // weapon switch
    int _weaponIndex = 0;
    bool _canSwitch = true;
    float _switchInterval = 0.5f;
    float _selectWeaponPivotScale = 0.8f;

    private void Awake()
    {
        if (MatchManager.Instance) MatchManager.Instance.SetPlayer(this, photonView.IsMine);

        InitializationLayer();
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << _invisibleLayer); // 見えないレイヤー設定
        _pAnimMg = GetComponent<PlayerAnimationManager>();
    }

    /// <summary>IsMaster別のLayer設定</summary>
    void InitializationLayer()
    {
        if (photonView.IsMine)
        {
            foreach (var obj in _hitObjects) obj.layer = _invisibleLayer;
            foreach (var obj in _invisibleToMyselfObj) obj.layer = _invisibleLayer;
            foreach (var obj in _invisibleToMyselfObjs)
            {
                foreach (Transform child in obj.transform) child.gameObject.layer = _invisibleLayer;
            }
        }
        else
        {
            foreach (var obj in _hitObjects) obj.layer = _hitLayer;
            foreach (var obj in _invisibleToEnemeyObjs)
            {
                foreach (Transform child in obj.transform) child.gameObject.layer = _invisibleLayer;
            }
        }
    }

    #region Helth -------------------------------------------------------
    public void OnDead()
    {
        photonView.RPC(nameof(ShareDead), RpcTarget.All);
    }

    [PunRPC]
    void ShareDead()
    {
        _deadCount++;
    }

    public void RespawnPosShare()
    {
        photonView.RPC(nameof(RespawnPosition), RpcTarget.All);
    }

    [PunRPC]
    void RespawnPosition()
    {
        // 位置、向きの初期化
        Vector3 position;
        if (PhotonNetwork.IsMasterClient ^ !photonView.IsMine)
        {
            position = InGameManager.Instance.PlayerSpawnPoints[0];
            transform.forward = Vector3.forward;
        }
        else
        {
            position = InGameManager.Instance.PlayerSpawnPoints[1];
            transform.forward = Vector3.back;
        }
        transform.position = position;
        Debug.Log("respawn");
        // TO:DO 内部データの初期化
    }
    #endregion

    #region Weapon ----------------------------------------------
    public void OnShoot(bool isHit)
    {
        _shootCount++;
        if (isHit) _hitCount++;
    }

    void SwitchWeapon()
    {
        if (!_canSwitch) return;
        // Switch
        photonView.RPC(nameof(SwitchWeaponActive), RpcTarget.All, _weaponIndex);
        SwitchWeaponUI(_weaponIndex);
        _weaponIndex++;
        _weaponIndex %= _weapons.Length;
        _pAnimMg.SetWeaponIndex(_weaponIndex == 1);
        // Interval
        _canSwitch = false;
        Invoke(nameof(SwitchInterval), _switchInterval);
    }

    [PunRPC]
    void SwitchWeaponActive(int index)
    {
        _weapons[index].SetActive(false);
        _weapons[(index + 1) % _weapons.Length].SetActive(true);
    }

    /// <summary>武器変更時のWeaponIconの動的処理</summary>
    void SwitchWeaponUI(int index)
    {
        // icon scale
        _weaponIconPivots[index].DOScale(Vector3.one * _selectWeaponPivotScale, 0.2f);
        _weaponIconPivots[(index + 1) % _weapons.Length].DOScale(Vector3.one, 0.2f);
        //_weaponIconPivots[index].localScale = Vector3.one * _selectWeaponPivotScale;
        //_weaponIconPivots[(index + 1) % _weapons.Length].localScale = Vector3.one;
        // out line
        _weaponIconOutLines[index].SetActive(false);
        _weaponIconOutLines[(index + 1) % _weapons.Length].SetActive(true);
    }

    void SwitchInterval()
    {
        _canSwitch = true;
    }
    #endregion

    private void OnEnable()
    {
        if (photonView.IsMine) PlayerInput.Instance.SetInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }

    private void OnDisable()
    {
        if (photonView.IsMine) PlayerInput.Instance.DelInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }
}
