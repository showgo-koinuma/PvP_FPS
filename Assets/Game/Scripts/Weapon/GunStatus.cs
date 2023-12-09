using UnityEngine;

[CreateAssetMenu(menuName = "Status/WeaponStatus")]
public class GunStatus : ScriptableObject
{
    [SerializeField, Tooltip("1shotで何発出るか")] int _oneShotNum;
    [Space(10)]
    [SerializeField] int _damage;
    [SerializeField] int _fullMagazineSize;
    [SerializeField] float _fireInterval;
    [SerializeField] float _reloadTime;
    [Header("リコイル")]
    [SerializeField] float _randomRecoilY;
    [SerializeField] float _randomRecoilX;
    [Header("拡散")]
    [SerializeField, Tooltip("デフォルトの拡散")] float _defaultDiffusion;
    [SerializeField, Tooltip("ADS時のデフォルトの拡散")] float _adsDefaultDiffusion;
    [SerializeField, Tooltip("射撃時の拡散量")] float _diffusionValue;
    [SerializeField, Tooltip("ADS射撃時の拡散量")] float _adsDiffusionValue;
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
/* 武器に必要なステータス
* damage
* 装弾数
* 連射速度
* リロード速度
* リコイル
* 初期拡散
*/