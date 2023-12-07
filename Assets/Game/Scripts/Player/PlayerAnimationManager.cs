using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMovement))]
/// <summary>プレイヤーのアニメーションを管理する</summary>
public class PlayerAnimationManager : MonoBehaviour
{
    [Header("Look")]
    /// <summary>見るターゲット</summary>
    [SerializeField] Transform _lookTarget = default;
    /// <summary>どれくらい見るか</summary>
    float _weight = 1;
    /// <summary>身体をどれくらい向けるか</summary>
    float _bodyWeight = 1;
    /// <summary>頭をどれくらい向けるか</summary>
    float _headWeight = 1;
    /// <summary>目をどれくらい向けるか</summary>
    float _eyesWeight = 1;
    /// <summary>関節の動きをどれくらい制限するか</summary>
    float _clampWeight = 0;

    [Header("Hand")]
    /// <summary>右手のターゲット</summary>
    [SerializeField] Transform _rightTarget = default;
    /// <summary>左手のターゲット</summary>
    [SerializeField] Transform _leftTarget = default;
    /// <summary>Position に対するウェイト</summary>
    float _positionWeight = 1;
    /// <summary>Rotation に対するウェイト</summary>
    float _rotationWeight = 1;

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
        // look
        _animator.SetLookAtWeight(_weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight);
        _animator.SetLookAtPosition(_lookTarget.position);

        //return;
        //hand
        // 右手に対して IK を設定する
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _positionWeight);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _rotationWeight);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, _rightTarget.position);
        _animator.SetIKRotation(AvatarIKGoal.RightHand, _rightTarget.rotation);
        // 左手に対して IK を設定する
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _positionWeight);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _rotationWeight);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftTarget.position);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftTarget.rotation);
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