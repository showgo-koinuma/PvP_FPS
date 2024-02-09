using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
public class PlayerManager : MonoBehaviourPun
{
    [Header("looks")]
    [SerializeField, Tooltip("�����蔻��I�u�W�F�N�g")] GameObject[] _hitObjects;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)")] GameObject[] _invisibleToMyselfObj;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToMyselfObjs;
    [SerializeField, Tooltip("���肩�猩���Ȃ��Ȃ�(�����̉�ʂɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToEnemeyObjs;

    [Header("weapon [0] : AR, [1] : SG")]
    [SerializeField, Tooltip("[0] : AR, [1] : SG")] GameObject[] _weapons;

    [Header("UI")]
    [SerializeField] GameObject[] _playerCanvas;
    [SerializeField] RectTransform[] _weaponIconPivots;
    [SerializeField] GameObject[] _weaponIconOutLines;
    [SerializeField] GameObject _respawnUI;
    [SerializeField] TMP_Text _respawnCountText;

    [Header("ScatteredPlayer")]
    [SerializeField] GameObject _scatteredPlayer;

    [Header("Audio")]
    [SerializeField] AudioSource _playerSystemAudioSource;
    [SerializeField] AudioClip _killSound;

    PlayerAnimationManager _pAnimMg;
    HeadController _headController;
    Rigidbody _rb;

    PlayerState _playerState = PlayerState.Nomal;
    public PlayerState PlayerState { get => _playerState; }
    int _hitLayer = 6;
    int _invisibleLayer = 7;

    // result data
    int _shootCount = 0;
    public int ShootCount { get => _shootCount;}
    int _hitCount = 0;
    public int HitCount { get => _hitCount;}
    int _headShotCount = 0;
    public int HeadShotCount { get => _headShotCount;}
    int _totalDamage = 0;
    public int TotalDamage { get => _totalDamage;}
    int _deadCount = 0;
    public int DeadCount { get => _deadCount; }

    // weapon switch
    int _weaponIndex = 0;
    bool _canSwitch = true;
    float _switchInterval = 0.6f;
    float _selectWeaponPivotScale = 0.8f;

    // respawn
    float _respawnTimer;

    private void Awake()
    {
        if (MatchManager.Instance) MatchManager.Instance.SetPlayer(this, photonView.IsMine);

        InitializationLayer();
        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << _invisibleLayer); // �����Ȃ����C���[�ݒ�
        _pAnimMg = GetComponent<PlayerAnimationManager>();
        _headController = GetComponent<HeadController>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_playerState == PlayerState.DuringRespawn)
        { // respawn timer�̏���
            _respawnTimer -= Time.deltaTime;
            _respawnCountText.text = _respawnTimer.ToString("0.0");

            if (_respawnTimer > 1)
            {
                _respawnCountText.text = ((int)_respawnTimer + 1).ToString();
            }
        }
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

            foreach (GameObject canvas in _playerCanvas) // �G�̃L�����o�X�͌����Ȃ�
            {
                canvas.SetActive(false);
            }
        }

        _respawnUI.SetActive(false);
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

        if (!photonView.IsMine)
        {
            _playerSystemAudioSource.PlayOneShot(_killSound);
        }
    }

    public void RespawnPosShare()
    {
        photonView.RPC(nameof(RespawnPosition), RpcTarget.All);
    }

    [PunRPC]
    void RespawnPosition()
    {
        ScatteredModel scatteredModel = Instantiate(_scatteredPlayer, transform.position, transform.rotation)
                                            .GetComponent<ScatteredModel>();
        scatteredModel.Initialize(photonView.IsMine, MatchManager.Instance.RespawnTime, _rb.velocity);

        if (photonView.IsMine)
        {
            foreach (GameObject canvas in _playerCanvas)
            {
                canvas.SetActive(false);
            }

            _respawnUI.SetActive(true); // respawn ui 
            _respawnTimer = MatchManager.Instance.RespawnTime;
            _playerState = PlayerState.DuringRespawn;
            _headController.ResetRotationYonMine();

            Invoke(nameof(EndRespawn), MatchManager.Instance.RespawnTime);
        }

        // �ʒu�A�����̏�����
        if (PhotonNetwork.IsMasterClient ^ !photonView.IsMine)
        {
            transform.position = InGameManager.Instance.PlayerSpawnPoints[0];
        }
        else
        {
            transform.position = InGameManager.Instance.PlayerSpawnPoints[1];
        }
    }

    void EndRespawn()
    {
        foreach (GameObject canvas in _playerCanvas)
        {
            canvas.SetActive(true);
        }

        _respawnUI.SetActive(false);
        _playerState = PlayerState.Nomal;
    }
    #endregion

    #region Weapon ----------------------------------------------
    public void AddResultData(int damage, bool isHit, bool isHead)
    {
        _totalDamage += damage;
        _shootCount++;

        if (isHit)
        {
            _hitCount++;

            if (isHead) _headShotCount++;
        }
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

    /// <summary>����ύX����WeaponIcon�̓��I����</summary>
    void SwitchWeaponUI(int index)
    {
        // icon scale
        _weaponIconPivots[index].DOScale(Vector3.one * _selectWeaponPivotScale, 0.2f);
        _weaponIconPivots[(index + 1) % _weapons.Length].DOScale(Vector3.one, 0.2f);
        // out line
        _weaponIconOutLines[index].SetActive(false);
        _weaponIconOutLines[(index + 1) % _weapons.Length].SetActive(true);
    }

    void SwitchInterval()
    {
        _canSwitch = true;
    }
    #endregion

    /// <summary>�Q�[���I�����Ƀv���C���[��UI�����ׂď���</summary>
    public void UIInvisibleOnGameOver()
    {
        foreach (GameObject canvas in _playerCanvas)
        {
            canvas.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (photonView.IsMine) PlayerInput.Instance.SetInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }

    private void OnDisable()
    {
        if (photonView.IsMine) PlayerInput.Instance.DelInputAction(InputType.SwitchWeapon, SwitchWeapon);
    }
}

public enum PlayerState
{
    Nomal,
    DuringRespawn
}