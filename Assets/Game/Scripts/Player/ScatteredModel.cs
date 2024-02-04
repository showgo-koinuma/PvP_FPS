using Cinemachine;
using UnityEngine;

public class ScatteredModel : MonoBehaviour
{
    CinemachineBlendListCamera _myVirtualCam;
    Rigidbody[] bodies;

    public void Initialize(bool isMine, float respawnTime, Vector3 velo)
    {
        bodies = GetComponentsInChildren<Rigidbody>();

        if (isMine)
        {
            _myVirtualCam = GetComponentInChildren<CinemachineBlendListCamera>();
            _myVirtualCam.Priority = 11; // €‚ñ‚¾‚Ì‚ª©•ª‚¾‚Á‚½‚çƒJƒƒ‰‚Ì—Dæ“x‚ğã‚°‚é
        }

        foreach (var body in bodies)
        {
            body.velocity = velo;
        }

        Destroy(gameObject, respawnTime);
    }
}
