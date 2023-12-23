using UnityEngine;

public class PointAreaManager : MonoBehaviour
{
    [SerializeField] bool visible = true;
    [SerializeField] Color color = Color.yellow;
    [SerializeField] Vector3 center = Vector3.zero;
    [SerializeField] Vector3 size = Vector3.one; //x,y,z •ûŒü‚Ì’·‚³

    void OnDrawGizmos()
    {
        if (!visible)
        {
            return;
        }

        Gizmos.color = color;

        Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
        Gizmos.DrawWireCube(center, size);
    }
}
