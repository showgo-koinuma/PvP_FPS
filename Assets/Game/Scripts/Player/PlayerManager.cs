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
    [Header("weapon")]
    [SerializeField, Tooltip("[0] : AR, [1] : SG")] GameObject[] _weapons;

    /// <summary>現在ActiveのGunController</summary>
    //GunController _activeGun;
    //public GunController ActiveGun { get => _activeGun;  set => _activeGun = value; }
    PlayerAnimationManager _pAnimMg;

    int _hitLayer = 6;
    int _invisibleLayer = 7;

    int _score = 0;
    int _clearScore = 1; // inGameManagerが無難か

    int _weaponIndex = 0;
    bool _canSwitch = true;
    float _switchInterval = 0.5f;

    private void Awake()
    {
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // オブジェクト共有
        MatchManager.Instance.SetPlayerToArea(transform, photonView.IsMine, PhotonNetwork.IsMasterClient);

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

    public void AddScore()
    {
        _score++;
        if (_score >= _clearScore) // ゲーム終了条件
        {
            InGameManager.Instance.FinishGame();
        } 
    }

    public void Respawn()
    {
        // 位置、向きの初期化
        Vector3 position;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
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

        // TO:DO 内部データの初期化
    }

    void SwitchWeapon()
    {
        if (!_canSwitch) return;
        // Switch
        _weapons[_weaponIndex].SetActive(false);
        _weaponIndex++;
        _weaponIndex %= _weapons.Length;
        _weapons[_weaponIndex].SetActive(true);
        _pAnimMg.SetWeaponIndex(_weaponIndex == 1);
        // Interval
        _canSwitch = false;
        Invoke(nameof(SwitchInterval), _switchInterval);
    }

    void SwitchInterval()
    {
        _canSwitch = true;
    }

    private void OnEnable()
    {
        PlayerInput.Instance.SetInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }

    private void OnDisable()
    {
        PlayerInput.Instance.DelInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }
}
