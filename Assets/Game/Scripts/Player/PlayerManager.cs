using Photon.Pun;
using UnityEngine;

/// <summary>Player�S�Ă��Ǘ�����</summary>
public class PlayerManager : MonoBehaviourPun
{
    [Header("look")]
    [SerializeField, Tooltip("�����蔻��I�u�W�F�N�g")] GameObject[] _hitObjects;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)")] GameObject[] _invisibleToMyselfObj;
    [SerializeField, Tooltip("�����Ō����Ȃ��Ȃ�(����ɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToMyselfObjs;
    [SerializeField, Tooltip("���肩�猩���Ȃ��Ȃ�(�����̉�ʂɉf�鎩���̃��f��)�̐e")] GameObject[] _invisibleToEnemeyObjs;
    [Header("weapon")]
    [SerializeField] GameObject[] _weapons;
    //[SerializeField, Tooltip("[0]:IsMaster, [1]:NotMaster")] int[] _playerLayer;

    /// <summary>����Active��GunController</summary>
    GunController _activeGun;
    public GunController ActiveGun { get => _activeGun;  set => _activeGun = value; }

    int _hitLayer = 6;
    int _invisibleLayer = 7;

    int _score = 0;
    int _clearScore = 1; // inGameManager�����

    int _weaponIndex = 0;

    private void Awake()
    {
        InGameManager.Instance.ViewGameObjects.Add(photonView.ViewID, this.gameObject); // �I�u�W�F�N�g���L
        InitializationLayer();

        //if (!photonView.IsMine)
        //{
        //    this.enabled = false;
        //    return;
        //}

        Camera.main.GetComponent<Camera>().cullingMask = ~(1 << _invisibleLayer);
        // �e�̃��C���[�ƃI�u�W�F�N�g���C���[�̐ݒ�
        //if (PhotonNetwork.IsMasterClient) Initialization(true, _playerLayer[0]);
        //else Initialization(false, _playerLayer[1]);
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

    public void AddScore()
    {
        _score++;
        if (_score >= _clearScore) // �Q�[���I������
        {
            InGameManager.Instance.FinishGame();
        } 
    }

    public void Respawn()
    {
        // �ʒu�A�����̏�����
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

        // TO:DO �����f�[�^�̏�����
    }

    void SwitchWeapon()
    {
        _weapons[_weaponIndex].SetActive(false);
        _weaponIndex++;
        _weaponIndex %= _weapons.Length;
        _weapons[_weaponIndex].SetActive(true);
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
