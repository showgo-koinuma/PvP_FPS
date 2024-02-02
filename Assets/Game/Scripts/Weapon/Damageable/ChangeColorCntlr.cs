using Photon.Pun;
using UnityEngine;

public class ChangeColorCntlr : Damageable
{
    [SerializeField] Material[] _materials;
    MeshRenderer _meshRenderer;
    int _index = 0;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        ChangeMaterial();
    }

    [PunRPC]
    protected override void OnDamageTakenShare(int damage, int collierIndex)
    {
        ChangeMaterial();
    }

    void ChangeMaterial()
    {
        _meshRenderer.material = _materials[_index];
        _index++;
        _index %= _materials.Length;
    }

    protected override HitData OnDamageTaken(int dmg, int colliderIndex)
    {
        return new HitData(dmg, false, false, false);
    }
}
