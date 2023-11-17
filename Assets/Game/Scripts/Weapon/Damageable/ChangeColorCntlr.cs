using Photon.Pun;
using UnityEngine;

public class ChangeColorCntlr : Damageable
{
    [SerializeField] Material[] _materials;
    MeshRenderer _meshRenderer;
    int _index = 0;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        ChangeMaterial();
    }

    [PunRPC]
    protected override void OnDamageTaken(int damage, int collierIndex, Vector3 objVectorDiff)
    {
        ChangeMaterial();
    }

    void ChangeMaterial()
    {
        _meshRenderer.material = _materials[_index];
        _index++;
        _index %= _materials.Length;
    }
}
