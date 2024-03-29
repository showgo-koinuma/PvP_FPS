using UnityEngine;

[CreateAssetMenu(menuName = "Status/WeaponStatus")]
public class GunStatus : ScriptableObject
{
    [SerializeField, Tooltip("1shotÅ½­oé©")] int _oneShotNum;
    [Space(10)]
    [SerializeField] int _damage;
    [SerializeField] int _fullMagazineSize;
    [SerializeField] float _fireInterval;
    [SerializeField] float _reloadTime;
    [Header("RC")]
    [SerializeField, Tooltip("ÜÁ½RCp^[")] Vector2[] _RecoilPattern;
    [SerializeField] float _randomRecoilY;
    [SerializeField] float _randomRecoilX;
    [Header("gU")]
    [SerializeField, Tooltip("ftHgÌgU")] float _defaultDiffusion;
    [SerializeField, Tooltip("ADSÌftHgÌgU")] float _adsDefaultDiffusion;
    [SerializeField, Tooltip("ËÌgUÊ")] float _diffusionValue;
    [SerializeField, Tooltip("ADSËÌgUÊ")] float _adsDiffusionValue;
    [SerializeField, Tooltip("gU¸­ÌXs[h")] float _reducedDiffusionRate;
    [Space(10)]
    [SerializeField] int _adsFov;
    [SerializeField] float _timeADSTake;

    public int OneShotNum { get => _oneShotNum; }
    public int Damage { get => _damage; }
    public int FullMagazineSize { get => _fullMagazineSize; }
    public float FireInterval { get => _fireInterval; }
    public float ReloadTime { get => _reloadTime; }
    public Vector2[] RecoilPattern { get => _RecoilPattern; }
    public float RecoilY {  get => _randomRecoilY; }
    public float RecoilX { get => _randomRecoilX; }
    public float DefaultDiffusion { get => _defaultDiffusion; }
    public float ADSDefaultDiffusion { get => _adsDefaultDiffusion; }
    public float Diffusion {  get => _diffusionValue; }
    public float ADSDiffusion { get => _adsDiffusionValue; }
    public float ReducedDiffusionRate { get => _reducedDiffusionRate; }
    public int ADSFov {  get => _adsFov; }
    public float ADSSpeed { get => _timeADSTake; }
}
/* íÉKvÈXe[^X
* damage
* e
* AË¬x
* [h¬x
* RC
* úgU
*/