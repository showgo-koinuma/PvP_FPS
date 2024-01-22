using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class ScatteredModel : MonoBehaviour
{
    [SerializeField] float _deadTime;
    [SerializeField, Tooltip("é©ï™ÇÃÉJÉÅÉâ")] GameObject _myCinemachine;
    [SerializeField] Transform _cameraPos;
    [SerializeField] Vector3 _endDis;
    /// <summary>FovïœçXóp</summary>
    CinemachineVirtualCamera _myVirtualCam;
    Rigidbody[] bodies;

    public void Initialize(bool isMine, Vector3 velo)
    {
        bodies = GetComponentsInChildren<Rigidbody>();

        if (isMine)
        {
            _myVirtualCam = Instantiate(_myCinemachine, _cameraPos).GetComponent<CinemachineVirtualCamera>();
            _myVirtualCam.Follow = _cameraPos;
            _myVirtualCam.MoveToTopOfPrioritySubqueue();
            _cameraPos.DOMove(_cameraPos.position + _endDis, 3);
            _cameraPos.DORotate(new Vector3(90, 0, 0), 3);
        }

        foreach (var body in bodies)
        {
            body.velocity = velo;
        }

        Destroy(gameObject, _deadTime);
    }

    private void Awake()
    {
    }
}
