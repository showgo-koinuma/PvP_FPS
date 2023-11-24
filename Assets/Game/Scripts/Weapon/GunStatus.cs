using UnityEngine;

[CreateAssetMenu(menuName = "Status/WeaponStatus")]
public class GunStatus : ScriptableObject
{
    [SerializeField] int _damage;
    [SerializeField] int _fullMagazineSize;
    [SerializeField] float _fireInterval;
    [SerializeField] float _reloadTime;
    [SerializeField] float _randomRecoilY;
    [SerializeField] float _randomRecoilX;
    [SerializeField] float _diffusionValue;
    [SerializeField] int _adsFov;
    [SerializeField] float _timeADSTake;

    public int Damage { get => _damage; }
    public int FullMagazineSize { get => _fullMagazineSize; }
    public float FireInterval { get => _fireInterval; }
    public float ReloadTime { get => _reloadTime; }
    public float RecoilY {  get => _randomRecoilY; }
    public float RecoilX { get => _randomRecoilX; }
    public float Diffusion {  get => _diffusionValue; }
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