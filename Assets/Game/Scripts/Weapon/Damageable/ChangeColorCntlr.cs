using UnityEngine;

public class ChangeColorCntlr : MonoBehaviour , Damageable
{
    [SerializeField] Material[] _materials;
    MeshRenderer _meshRenderer;
    int _index = 0;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        ChangeMaterial();
    }

    void Damageable.OnDamageTaken(int damage, Collider hitCollider)
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
