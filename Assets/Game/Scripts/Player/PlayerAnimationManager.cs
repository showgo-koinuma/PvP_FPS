using UnityEngine;

/// <summary>�v���C���[�̃A�j���[�V�������Ǘ�����</summary>
public class PlayerAnimationManager : MonoBehaviour
{
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
        _animator.SetBool("IsGround", _playerMove.IsGround);
        if (!_lastFrameOnJump && _playerMove.IsJumping)
        {
            _onJump = true;
        }
        _animator.SetBool("IsJump", _onJump);
        _animator.SetBool("IsCrouching", _playerMove.IsCrouching);
        if (!_onJump) _animator.SetBool("IsSliding", _playerMove.IsSliding);

        _onJump = false;
        _lastFrameOnJump = _playerMove.IsJumping;
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += Adoption;
    }
    private void OnDisable()
    {
        InGameManager.Instance.UpdateAction -= Adoption;
    }
}