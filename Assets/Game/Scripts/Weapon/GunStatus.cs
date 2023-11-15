using UnityEngine;

[CreateAssetMenu(menuName = "Status/WeaponStatus")]
public class GunStatus : ScriptableObject
{
    [SerializeField] int _damage;
    [SerializeField] int _fullMagazineSize;
    [SerializeField] float _fireInterval;
    [SerializeField] float _reloadTime;

    public int Damage { get => _damage; }
    public int FullMagazineSize { get => _fullMagazineSize; }
    public float FireInterval { get => _fireInterval; }
    public float ReloadTime { get => _reloadTime; }
}
/* ����ɕK�v�ȃX�e�[�^�X
* damage
* ���e��
* �A�ˑ��x
* �����[�h���x
* ���R�C��
* �����g�U
*/