using System;
using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField, Tooltip("PlayerModel�������Ă��镐��Model")] GameObject _holdGunModel;
    //[SerializeField, Tooltip("����ɂ͌����Ȃ��I�u�W�F�N�g")] GameObject[] _gunModelObjs;
    [SerializeField] GunStatus _gunStatus;
    [SerializeField, Tooltip("ADS�����Ƃ��̃��f����localPosition")] Vector3 _ADSPos;
    [SerializeField, Tooltip("�e���I�u�W�F�N�g�̐e�ɂȂ�}�Y���I�u�W�F�N�g [0] = view, [1] = model")] GameObject[] _muzzles;
    [SerializeField, Tooltip("�e��LineRenderer�v���n�u")] GameObject _bllisticPrefab;
    /// <summary>isMine�ŃR�[���o�b�N��o�^���Ă��邩</summary>
    bool _setedAction = false;
    GunState _currentGunState = GunState.nomal;

    PlayerManager _playerManager;
    HeadController _headCntler;

    static int _hitLayer = ~(1 << 7);
    int _currentMagazine;
    /// <summary>�e����������܂ł̎���</summary>
    float _ballisticFadeOutTime = 0.01f;
    /// <summary>���݂̒e�̊g�U</summary>
    float _currentDiffusion = 0;
    LineRenderer[][] _ballisticLines;
    int _bulletIndex;

    private void Awake()
    {
        _playerManager = transform.root.GetComponent<PlayerManager>();
        _headCntler = transform.root.GetComponent<HeadController>();

        _currentMagazine = _gunStatus.FullMagazineSize; // �e��������
        BallisticInitialization(); // �e��������

        //if (!_playerManager.photonView.IsMine) foreach(var obj in _gunModelObjs) obj.layer = 8; // ����̏e���f���������Ȃ��悤��
    }

    /// <summary>�e��LineRenderer�̏����ݒ������</summary>
    void BallisticInitialization()
    {
        _ballisticLines = new LineRenderer[_muzzles.Length][];

        // �e����\������muzzle���ꂼ���1���ɏo��e�̐������e���I�u�W�F�N�g�𐶐�����
        for (int i = 0; i < _muzzles.Length; i++)
        {
            _ballisticLines[i] = new LineRenderer[_gunStatus.OneShotNum];

            for (int j = 0; j < _gunStatus.OneShotNum; j++)
            {
                GameObject Line = Instantiate(_bllisticPrefab, _muzzles[i].transform);
                _ballisticLines[i][j] = Line.GetComponent<LineRenderer>();

                if (_playerManager.photonView.IsMine ^ i == 0) Line.layer = 7; // invisible layer
            }
        }
    }

    /// <summary>�ˌ����ɂǂ̂悤�ȏ��������邩�v�Z����</summary>
    void FireCalculation()
    {
        if (!(_currentGunState == GunState.nomal && PlayerInput.Instance.InputOnFire))
        {
            // ��ˌ����Ɋg�U�����Ƃɖ߂�
            if (PlayerInput.Instance.IsADS)
            {
                if (_currentDiffusion > _gunStatus.ADSDefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime; // 1�b�Ō��ɖ߂�
                else _currentDiffusion = _gunStatus.ADSDefaultDiffusion;
            }
            else
            {
                if (_currentDiffusion > _gunStatus.DefaultDiffusion) _currentDiffusion -= _currentDiffusion * Time.deltaTime;
                else _currentDiffusion = _gunStatus.DefaultDiffusion;
            }
            return; // nomal�łȂ��ƌ��ĂȂ�
        }

        if (_currentMagazine <= 0) // �e���Ȃ����reload
        {
            Reload();
            return;
        }

        for (int i = 0; i < _gunStatus.OneShotNum; i++)
        {
            _bulletIndex = i;
            // �����_���Ȓe���𐶐�
            Vector3 dir = Quaternion.Euler(UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion),
                UnityEngine.Random.Range(_currentDiffusion, -_currentDiffusion), 0) * Camera.main.transform.forward;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, float.MaxValue, _hitLayer))
            {
                Debug.Log(hit.collider.name);

                // �e�I�u�W�F�N�g��TryGetComponent
                if (hit.collider.gameObject.transform.root.gameObject.TryGetComponent(out Damageable damageable))
                {
                    damageable.OnDamageTakenInvoker(_gunStatus.Damage, Array.IndexOf(damageable.Colliders, hit.collider), hit.point - hit.transform.position, _playerManager.photonView.ViewID);
                }
                else
                {
                    _playerManager.FireActionCall(hit.point); // ����obj�łȂ���ΒP���ȏ����ƂȂ�
                }
            }
        }

        // �g�U���Ȃ��𑝉�������
        if (!PlayerInput.Instance.IsADS) _currentDiffusion += _gunStatus.Diffusion;
        else _currentDiffusion += _gunStatus.ADSDiffusion;

        _headCntler.Recoil(UnityEngine.Random.Range(0, -_gunStatus.RecoilY), UnityEngine.Random.Range(_gunStatus.RecoilX, -_gunStatus.RecoilX)); // ��������ʂɔ��f
        _currentMagazine--;
        _currentGunState = GunState.interval; // �C���^�[�o���ɓ����
        Invoke(nameof(ReturnGunState), _gunStatus.FireInterval); // �w�莞�ԂŖ߂�
    }

    /// <summary>�v�Z���ʂ̏����𔽉f����(no Action)</summary>
    public void FireAction(Vector3 hitPos)
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
        if (PlayerInput.Instance.IsADS) transform.localPosition = _ADSPos;
        else transform.localPosition = Vector3.zero;
    }

    /// <summary>gun state��nomal�ɖ߂�</summary>
    void ReturnGunState()
    {
        _currentGunState = GunState.nomal;
    }

    /// <summary>FadeOut����e����`�悷��</summary>
    public IEnumerator DrawBallistic(Vector3 target)
    {
        // �قȂ�LineRenderer�ɑΉ������}�Y������̒e��������
        //foreach(var posLinePair in _muzzleAndLineDict)
        //{
        //    posLinePair.Value.SetPosition(0, posLinePair.Key.position);
        //    posLinePair.Value.SetPosition(1, target);

        //}
        int index = _bulletIndex;

        for (int i = 0; i < _muzzles.Length; i++)
        {
            _ballisticLines[i][index].SetPosition(0, _muzzles[i].transform.position);
            _ballisticLines[i][index].SetPosition(1, target);
        }

        yield return new WaitForSeconds(_ballisticFadeOutTime);

        // ���_�ɖ߂��Ēe��������
        //foreach (var posLinePair in _muzzleAndLineDict)
        //{
        //    posLinePair.Value.SetPosition(1, posLinePair.Value.GetPosition(0));
        //}

        for (int i = 0; i < _muzzles.Length; i++)
        {
            _ballisticLines[i][index].SetPosition(1, _ballisticLines[i][index].GetPosition(0));
        }
    }

    private void OnEnable()
    {
        _holdGunModel.SetActive(true); // ���f������
        _playerManager.ActiveGun = this; // ���݂�Active��Player�ɐݒ�

        if (!_playerManager.photonView.IsMine) return;
        _setedAction = true;
        PlayerInput.Instance.SetInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.SetInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction += FireCalculation;
    }

    private void OnDisable()
    {
        _holdGunModel.SetActive(false); // ���f���s����

        if (!_setedAction) return; // Action���Z�b�g���Ă��Ȃ���Ύ��s���Ȃ�
        PlayerInput.Instance.DelInputAction(InputType.Reload, Reload);
        PlayerInput.Instance.DelInputAction(InputType.ADS, ADS);
        InGameManager.Instance.UpdateAction -= FireCalculation;
    }
}

enum GunState
{
    nomal,
    interval,
    reloading
}

/// <summary>Dictionary��inspecter�Ŏg����</summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class KeyAndValuePair<TKey, TValue>
{
    [SerializeField] private TKey key;
    [SerializeField] private TValue value;

    public TKey Key => key;
    public TValue Value => value;
}