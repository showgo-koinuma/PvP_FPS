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
    protected override void OnDamageTakenShare(int damage, int collierIndex, Vector3 objVectorDiff, int playerID)
    {
        ChangeMaterial();
        // id���猂����obj���Q�Ƃ��A���O�������Ă������ʂ�̒e����\������
        StartCoroutine(InGameManager.Instance.ViewGameObjects[playerID].GetComponent<PlayerManager>().ActiveGun.DrawBallistic(transform.position + objVectorDiff));
    }

    void ChangeMaterial()
    {
        _meshRenderer.material = _materials[_index];
        _index++;
        _index %= _materials.Length;
    }
}
