using Photon.Pun;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
public class PlayerManager : MonoBehaviourPun
{
    [Header("looks")]
    [SerializeField, Tooltip("�����蔻��I�u�W�F�N�g")] GameObject[] _hitObjects;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)")] GameObject[] _invisibleToMyselfObj;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToMyselfObjs;
    [SerializeField, Tooltip("���肩�猩���Ȃ��Ȃ�(�����̉�ʂɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToEnemeyObjs;
    [Header("weapon")]
    [SerializeField, Tooltip("[0] : AR, [1] : SG")] GameObject[] _weapons;

    /// <summary>����Active��GunController</summary>
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

    int _weaponIndex = 0;
    bool _canSwitch = true;
    float _switchInterval = 0.5f;

    private void Awake()
    {
        if (MatchManager.Instance) MatchManager.Instance.SetPlayer(this, photonView.IsMine);

        InitializationLayer();
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << _invisibleLayer); // �����Ȃ����C���[�ݒ�
        _pAnimMg = GetComponent<PlayerAnimationManager>();
    }

    /// <summary>IsMaster�ʂ�Layer�ݒ�</summary>
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
        // �ʒu�A�����̏�����
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
        // TO:DO �����f�[�^�̏�����
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
        photonView.RPC(nameof(SwitchOtherWeapon), RpcTarget.All, _weaponIndex);
        _weaponIndex++;
        _weaponIndex %= _weapons.Length;
        _pAnimMg.SetWeaponIndex(_weaponIndex == 1);
        // Interval
        _canSwitch = false;
        Invoke(nameof(SwitchInterval), _switchInterval);
    }

    [PunRPC]
    void SwitchOtherWeapon(int index)
    {
        _weapons[index].SetActive(false);
        _weapons[(index + 1) % _weapons.Length].SetActive(true);
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
