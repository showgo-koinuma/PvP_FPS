using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
/// <summary>�v���C���[�̃A�j���[�V�������Ǘ�����</summary>
public class PlayerAnimationManager : MonoBehaviour
{
    [Header("Look")]
    /// <summary>����^�[�Q�b�g</summary>
    [SerializeField] Transform _lookTarget = default;
    /// <summary>�ǂꂭ�炢���邩</summary>
    float _weight = 1;
    /// <summary>�g�̂��ǂꂭ�炢�����邩</summary>
    float _bodyWeight = 1;
    /// <summary>�����ǂꂭ�炢�����邩</summary>
    float _headWeight = 1;
    /// <summary>�ڂ��ǂꂭ�炢�����邩</summary>
    float _eyesWeight = 1;
    /// <summary>�֐߂̓������ǂꂭ�炢�������邩</summary>
    float _clampWeight = 0;

    [Header("Hand")]
    /// <summary>�E��̃^�[�Q�b�g</summary>
    [SerializeField] Transform _rightTarget = default;
    /// <summary>����̃^�[�Q�b�g</summary>
    [SerializeField] Transform _leftTarget = default;
    /// <summary>Position �ɑ΂���E�F�C�g</summary>
    float _positionWeight = 1;
    /// <summary>Rotation �ɑ΂���E�F�C�g</summary>
    float _rotationWeight = 1;

    Animator _animator;
    PlayerMovement _playerMove;

    bool _onJump;
    bool _lastFrameOnJump;

    private void Awake()
    {
        // �R���|�[�l���g�̎擾
        _animator = GetComponent<Animator>();
        _playerMove = GetComponent<PlayerMovement>();

    }

    /// <summary>Animator�p�����[�^�ɓK�p������</summary>
    void Adoption()
    {
        _animator.SetBool("IsADS", PlayerInput.Instance.IsADS);
        Vector3 localVelo = transform.InverseTransformDirection(_playerMove.PlayerVelocity);
        _animator.SetFloat("Speed", localVelo.magnitude);
        localVelo.Normalize();
        _animator.SetFloat("DirX", localVelo.x);
        _animator.SetFloat("DirY", localVelo.z);
        _animator.SetBool("IsGround", _playerMove.IsGround);
        if (!_lastFrameOnJump && _playerMove.IsJumping)
        {
            _onJump = true;
        }
        _animator.SetBool("IsJump", _onJump);
        _animator.SetBool("IsCrouching", _playerMove.IsCrouching);
        if (!_onJump) _animator.SetBool("IsSliding", _playerMove.IsSliding);

        if (Input.GetKeyDown(KeyCode.Q)) _animator.SetTrigger("SwitchGun");

        _onJump = false;
        _lastFrameOnJump = _playerMove.IsJumping;
    }

    public void SetFireTrigger()
    {
        _animator.SetTrigger("Fire");
    }

    public void SetRiloadTrigger()
    {
        _animator.SetTrigger("Reload");
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // look IK�𓯊�
        _animator.SetLookAtWeight(_weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight);
        _playerMove.photonView.RPC(nameof(SetLookAtPosition), RpcTarget.All);
    }

    [PunRPC]
    void SetLookAtPosition()
    {
        _animator.SetLookAtPosition(_lookTarget.position);
    }

    private void OnEnable()
    {
        if (!_playerMove.photonView.IsMine) return;
        InGameManager.Instance.UpdateAction += Adoption;
    }
    private void OnDisable()
    {
        if (!_playerMove.photonView.IsMine) return;
        InGameManager.Instance.UpdateAction -= Adoption;
    }
}