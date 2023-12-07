using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
/// <summary>プレイヤーのアニメーションを管理する</summary>
public class PlayerAnimationManager : MonoBehaviour
{
    Animator _animator;
    PlayerMovement _playerMove;

    bool _onJump;
    bool _lastFrameOnJump;

    private void Awake()
    {
        // コンポーネントの取得
        _animator = GetComponent<Animator>();
        _playerMove = GetComponent<PlayerMovement>();
    }

    /// <summary>Animatorパラメータに適用させる</summary>
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

        _onJump = false;
        _lastFrameOnJump = _playerMove.IsJumping;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        
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