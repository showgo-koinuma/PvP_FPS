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
    protected override void OnDamageTaken(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        ChangeMaterial();
        // idから撃ったobjを参照し、ラグがあっても処理通りの弾道を表示する
        StartCoroutine(InGameManager.Instance.ViewGameObjects[playerID].GetComponent<GunController>().DrawBallistic(transform.position + objVectorDiff));
    }

    void ChangeMaterial()
    {
        _meshRenderer.material = _materials[_index];
        _index++;
        _index %= _materials.Length;
    }
}
