using UnityEngine;

[CreateAssetMenu(menuName = "Status/WeaponStatus")]
public class GunStatus : ScriptableObject
{
    [SerializeField, Tooltip("1shot�ŉ����o�邩")] int _oneShotNum;
    [Space(10)]
    [SerializeField] int _damage;
    [SerializeField] int _fullMagazineSize;
    [SerializeField] float _fireInterval;
    [SerializeField] float _reloadTime;
    [Header("���R�C��")]
    [SerializeField] float _randomRecoilY;
    [SerializeField] float _randomRecoilX;
    [Header("�g�U")]
    [SerializeField, Tooltip("�f�t�H���g�̊g�U")] float _defaultDiffusion;
    [SerializeField, Tooltip("ADS���̃f�t�H���g�̊g�U")] float _adsDefaultDiffusion;
    [SerializeField, Tooltip("�ˌ����̊g�U��")] float _diffusionValue;
    [SerializeField, Tooltip("ADS�ˌ����̊g�U��")] float _adsDiffusionValue;
    [Space(10)]
    [SerializeField] int _adsFov;
    [SerializeField] float _timeADSTake;

    public int OneShotNum { get => _oneShotNum; }
    public int Damage { get => _damage; }
    public int FullMagazineSize { get => _fullMagazineSize; }
    public float FireInterval { get => _fireInterval; }
    public float ReloadTime { get => _reloadTime; }
    public float RecoilY {  get => _randomRecoilY; }
    public float RecoilX { get => _randomRecoilX; }
    public float DefaultDiffusion { get => _defaultDiffusion; }
    public float ADSDefaultDiffusion { get => _adsDefaultDiffusion; }
    public float Diffusion {  get => _diffusionValue; }
    public float ADSDiffusion { get => _adsDiffusionValue; }
    public int ADSFov {  get => _adsFov; }
    public float ADSSpeed { get => _timeADSTake; }
}
/* ����ɕK�v�ȃX�e�[�^�X
* damage
* ���e��
* �A�ˑ��x
* �����[�h���x
* ���R�C��
* �����g�U
*/