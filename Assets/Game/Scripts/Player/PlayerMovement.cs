using UnityEngine;
using Photon.Pun;
using System;

/// <summary>
/// Player�𓮂����R���|�[�l���g
/// </summary>
// �R���|�[�l���g�������邩
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun
{
    Transform _orientation; // �Ⴄ�I�u�W�F�N�g�������ꍇSerializeField�ɂ���
    Rigidbody _rb;

    [Header("�ړ�")]
    // �ڒn��
    [SerializeField] float _moveSpeed;
    [SerializeField, Tooltip("�ڒn���̉����x")] float _groundAcceleration;
    [Space(5)]
    // ��
    [SerializeField, Tooltip("�󒆂ł̉����x")] float _airAcceleration;
    [SerializeField, Tooltip("�X�g���C�t�̉����x")] float _strafeAcceleration;
    [SerializeField, Tooltip("�X�g���C�t�̍ő呬�x")] float _maxStrafeSpeed;
    [SerializeField, Tooltip("�󒆂łǂ̒��x����ł��邩")] float _airControl;
    [Space(5)]
    // ���̑�
    [SerializeField, Tooltip("�n�ʂƔ��肷��ő�̌X�Ίp�x")] float _maxSlopeAngle;
    [SerializeField, Tooltip("���C")] float _friction;
    [SerializeField, Tooltip("�n�ʂ̃��C���[")] LayerMask _whatIsGround;
    /// <summary>rb�ɒ��ڑ������player��velocity</summary>
    Vector3 _playerVelocity;
    bool _isGround;
    /// <summary>�ڒn���̖ʂ�nomalVector</summary>
    Vector3 _groundNormalVector = Vector3.up;
    bool _cancellingGrounded;

    [Header("�W�����v")]
    [SerializeField] float _jumpCooldown;
    [SerializeField] float _jumpForce;
    bool _readyToJump = true;
    [Space(10)]
    // �ǃW����
    [SerializeField, Tooltip("�ǃW�����̋���")] float _wallJumpPower;
    [SerializeField, Tooltip("�ǃW�����o����ǂƂ��Ĉ�����ő�̊p�x")] float _maxWallAngle; // �����_�㉺�̊p�x���l���o���ĂȂ�
    bool _onWall;
    /// <summary>�ł����ʂɂ���ǂ�nomalVector</summary>
    Vector3 _wallNormalVector;
    bool _cancellingOnWall;

    [Header("���Ⴊ��")]
    [SerializeField, Tooltip("�L�����R���pcollider��scale�ύX�̂���")] GameObject _moveBodyObject;
    [SerializeField, Tooltip("���Ⴊ�ݎ��̎��_�ړ��̂���")] GameObject _headObjct;
    [SerializeField, Tooltip("���Ⴊ�݂ɂ�鑬�x�ቺ����")] float _crouchMoveSpeedRate;
    [SerializeField, Tooltip("���Ⴊ�݂ł̃X�P�[��")] Vector3 _crouchScale;
    float _crouchTransitionTime = 0.2f;
    Vector3 _playerScale;
    int _crouchDir = 1;

    [Header("�X���C�f�B���O")]
    [SerializeField, Tooltip("�X���C�f�B���O�̏���")] float _slidingSpeed;
    [SerializeField, Tooltip("�X���C�f�B���O���̊���₷��(���C)")] float _slidingFriction;
    [SerializeField, Tooltip("�X���C�f�B�C���O��������X�s�[�h�̍ŏ��l")] float _minCnaSlidingSpeed;
    [SerializeField, Tooltip("�X���C�f�B���O�̃N�[���_�E��")] float _slidingCooldown;
    bool _readyToSliding = true;

    // ���݂̏�Ԃ��Ǘ��@enum�ł��K�v���łĂ��邩��
    bool _jumping, _crouching, _isSliding;

    // AnimationManager�̂��߂̃v���p�e�B
    public Vector3 PlayerVelocity { get => _playerVelocity; }
    public bool IsGround { get => _isGround; }
    public bool IsJumping { get => _jumping; }
    public bool IsCrouching { get => _crouching; }
    public bool IsSliding { get => _isSliding; }

    void Awake()
    {
        if (!photonView.IsMine) this.enabled = false;
        _rb = GetComponent<Rigidbody>();
        _orientation = GetComponent<Transform>();
    }

    void Start()
    {
        _playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ThisUpdate()
    {
        _jumping = false;
        if (Input.mouseScrollDelta.y < 0 || PlayerInput.Instance.OnJumpButton) { Jump(); WallJump(); } // �}�E�X�z�C�[�����{�^���݂����Ɏg�������񂾂��ǂ�
        Movement();
        CrouchTransition();
    }

    void Movement()
    {
        if (_isGround) GroundMove();
        else AirMove();
        _playerVelocity.y = _rb.velocity.y;
        _rb.velocity = _playerVelocity;
        _playerVelocity.y = 0;
        if (_isSliding) Debug.Log("sliding now");
    }

    /// <summary>�n��ł̓���</summary>
    public void GroundMove()
    {
        Vector3 vec = _playerVelocity; // Equivalent to: VectorCopy();
        vec.y = 0f;
        float speed = vec.magnitude;

        float control = speed < _groundAcceleration ? _groundAcceleration : speed;
        float drop = control * _friction * Time.deltaTime;
        if (_jumping) drop = 0f;
        if (_isSliding) drop *= _slidingFriction;

        float newspeed = speed - drop;
        if (newspeed < 0) newspeed = 0;
        if (speed > 0) newspeed /= speed;

        _playerVelocity.x *= newspeed;
        _playerVelocity.z *= newspeed;

        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;
        wishdir = transform.TransformDirection(wishdir);
        if (_isSliding)
        {
            Accelerate(wishdir, _moveSpeed * _crouchMoveSpeedRate, _groundAcceleration);
            if (_playerVelocity.magnitude <= _moveSpeed * _crouchMoveSpeedRate + 0.5f) _isSliding = false;
        }
        else if (PlayerInput.Instance.IsCrouching)
        {
            Accelerate(wishdir, _moveSpeed * _crouchMoveSpeedRate, _groundAcceleration);
            Sliding(_playerVelocity.magnitude);
        }
        else Accelerate(wishdir, _moveSpeed, _groundAcceleration);
    }

    /// <summary>�󒆂ł̓���</summary>
    public void AirMove()
    {
        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;

        float wishspeed = 7f;

        // Aircontrol
        float wishspeed2 = wishspeed;
        float accel = _airAcceleration;

        // ���E�L�[�����ŃX�g���C�t���Ă����ꍇ
        if (wishdir.x != 0 && wishdir.z == 0)
        {
            if (wishspeed > _maxStrafeSpeed)
                wishspeed = _maxStrafeSpeed;
            accel = _strafeAcceleration;
        }

        wishdir = transform.TransformDirection(wishdir);
        Accelerate(wishdir, wishspeed, accel);

        //if (wishspeed2 != 0) AirControl(wishdir, wishspeed2);

        // Apply gravity
        //_playerVelocity.y += gravity * Time.deltaTime;

        // ���E�L�[�����̃X�g���C�t�͑����Ȃ�I��
        void AirControl(Vector3 wishdir, float wishspeed)
        {
            _playerVelocity.y = 0;
            float speed = _playerVelocity.magnitude;
            _playerVelocity.Normalize();

            float dot = Vector3.Dot(_playerVelocity, wishdir);
            float k = 32; // �Ȃ�ł��̐��l�ɂȂ����̂��������
            k *= _airControl * dot * dot * Time.deltaTime;

            // �������Ȃ��������ς���
            if (dot > 0)
            {
                _playerVelocity = _playerVelocity * speed + wishdir * k;
                _playerVelocity.Normalize();
            }

            _playerVelocity.x *= speed;
            _playerVelocity.z *= speed;

        }
    }

    /// <summary>�x�N�g�����v�Z����</summary>
    public void Accelerate(Vector3 wishdir, float wishSpeed, float accel)
    {
        float currentspeed = Vector3.Dot(_playerVelocity, wishdir);
        float addspeed = wishSpeed - currentspeed;
        if (addspeed <= 0)
            return;
        float accelspeed = accel * Time.deltaTime * wishSpeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        _playerVelocity.x += accelspeed * wishdir.x;
        _playerVelocity.z += accelspeed * wishdir.z;
    }

    void Jump()
    {
        if (_isGround && _readyToJump)
        {
            _readyToJump = false;
            _jumping = true;
            _isSliding = false;

            _rb.AddForce(Vector2.up * _jumpForce * 1.5f);
            //_rb.AddForce(_normalVector * _jumpForce * 0.5f); // �⓹�̉e���������󂯂�
            //If jumping while falling, reset y velocity. �悭�킩���@���Z�b�g�͕K�v������AddForce�̌�ɂ��̂�...
            Vector3 vel = _rb.velocity;
            if (_rb.velocity.y < 0.5f)
                _rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (_rb.velocity.y > 0)
                _rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    void ResetJump()
    {
        _readyToJump = true;
    }

    void WallJump()
    {
        if (!_onWall || _isGround) return; // �ǂɐڂ��Ă��Ȃ� || �ڒn��
        Debug.Log("wall jump");

        _jumping = true;
        // �p�x�̌v�Z
        Vector3 jumpVec = Vector3.Reflect(_wallNormalVector, transform.forward); // ���ˊp
        Quaternion wallQ = // �ǂ��猩��forward����W�����v�x�N�g���ւ̊p�x
            Quaternion.AngleAxis((Mathf.Atan2(jumpVec.x, jumpVec.z) - Mathf.Atan2(_wallNormalVector.x, _wallNormalVector.z))
            * Mathf.Rad2Deg, Vector3.up);
        Vector3 wallLookJumpVec = wallQ * Vector3.forward; // �ǂ��猩���W�����v�x�N�g��
        wallLookJumpVec = new Vector3(wallLookJumpVec.x, 1, 1) * _wallJumpPower; // * ���x
        wallLookJumpVec = Quaternion.AngleAxis(Mathf.Atan2(_wallNormalVector.x, _wallNormalVector.z) * Mathf.Rad2Deg, Vector3.up) 
            * wallLookJumpVec; // world vector�ɒ���
        //wallLookJumpVec.y = _wallJumpPower;

        // vector�̑��
        _playerVelocity = wallLookJumpVec; // �v�Z�p�ϐ��ɑ��
        _rb.velocity = wallLookJumpVec; // velocity�ɒ��ڑ�� y���͒��ړ���Ȃ��Ɩʓ|
        // cool time�͕K�v���낤��
    }

    /// <summary>���Ⴊ�ݏ�Ԃ�؂�ւ���</summary>
    void SwitchCrouch()
    {
        if (PlayerInput.Instance.IsCrouching) // ���Ⴊ�݊J�n����
        {
            _crouchDir = -1;
            //transform.localScale = _crouchScale;
            //transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
            _crouching = true;
            return;
        } // return�ؑւ��Ă��Ⴊ�݉����̏���
        _crouchDir = 1;
        //transform.localScale = _playerScale;
        //transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
        _crouching = false;
        _isSliding = false;
    }

    /// <summary>update�ł��Ⴊ�݂̑J�ڂ�����</summary>
    void CrouchTransition()
    {
        if ((_crouchDir == 1 && _moveBodyObject.transform.localScale.y < 0.9f) || 
            (_crouchDir == -1 && _moveBodyObject.transform.localScale.y > _crouchScale.y))
        {
            _moveBodyObject.transform.localScale += new Vector3(0, (1 - _crouchScale.y) * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
            _moveBodyObject.transform.position += new Vector3(0, 0.5f * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
            _headObjct.transform.position += new Vector3(0, 0.8f * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
        }
        else // �J�ڂ��I����������
        {
            if (_crouchDir == 1)
            {
                _moveBodyObject.transform.localScale = new Vector3(_moveBodyObject.transform.localScale.x, 0.9f
                    , _moveBodyObject.transform.localScale.z);
                _moveBodyObject.transform.localPosition = new Vector3(0, 0.9f, 0);
                _headObjct.transform.localPosition = new Vector3(0, 1.6f, 0);
            }
            else
            {
                _moveBodyObject.transform.localScale = new Vector3(_moveBodyObject.transform.localScale.x, _crouchScale.y
                    , _moveBodyObject.transform.localScale.z);
                _headObjct.transform.localPosition = new Vector3(0, 0.8f, 0);
            }
        }
    }

    void Sliding(float speed)
    {
        if (!_readyToSliding || speed < _minCnaSlidingSpeed || !_readyToJump) return;
        Debug.Log("sliding");
        _playerVelocity = _playerVelocity.normalized * _slidingSpeed;
        _playerVelocity.y = _rb.velocity.y;
        _readyToSliding = false;
        _isSliding = true;
        Invoke(nameof(ResetSliding), _slidingCooldown);
    }

    void ResetSliding()
    {
        _readyToSliding = true;
    }

    /// <summary>���̖@���x�N�g���̖ʂ͒n�ʂƂ��Ĉ����邩</summary>
    private bool IsFloor(Vector3 nomalVector)
    {
        float angle = Vector3.Angle(Vector3.up, nomalVector);
        return angle < _maxSlopeAngle;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (_whatIsGround != (_whatIsGround | (1 << layer))) return; // �n�ʂ̃��C���[�łȂ���Γ������Ă��Ă��n�ʂ��Ɣ��肵�Ȃ�
        float minAngle = 180;

        for (int i = 0; i < other.contactCount; i++) // ���ׂĂ̐ڐG�_�ɂ�����IsFloor��������
        {
            Vector3 normal = other.contacts[i].normal; // �ڂ��Ă���ʂ̖@���x�N�g��

            if (IsFloor(normal)) // �n�ʂƂ��Ĉ����邩
            {
                _isGround = true;
                _cancellingGrounded = false;
                _groundNormalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
            else
            {
                float angle = Vector3.Angle(transform.forward, normal);

                if (angle > 180 - _maxWallAngle && minAngle > angle) // �ǃW�����\�� && �ł����ʂ�
                {
                    minAngle = angle;
                    _wallNormalVector = normal;
                    _onWall = true;
                    _cancellingOnWall = false;
                    CancelInvoke(nameof(StopOnWall));
                }
            }
        }

        // CollisionExit�Ŗ@�����`�F�b�N�ł��Ȃ����߁A�n�ʁ^�ǂ̃L�����Z�����Ăяo��
        float delay = 3f; // ���ꂼ��false�ɂ���t���[����
        if (!_cancellingGrounded)
        {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
        if (!_cancellingOnWall)
        {
            _cancellingOnWall = true;
            Invoke(nameof(StopOnWall), Time.deltaTime * delay);
        }
    }
    void StopGrounded()
    {
        _isGround = false;
    }
    void StopOnWall()
    {
        _onWall = false;
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += ThisUpdate;
        //PlayerInput.Instance.SetInputAction(InputType.Jump, Jump);
        PlayerInput.Instance.SetInputAction(InputType.Crouch, SwitchCrouch);
    }

    private void OnDisable()
    {
        InGameManager.Instance.UpdateAction -= ThisUpdate;
        //PlayerInput.Instance.DelInputAction(InputType.Jump, Jump);
        PlayerInput.Instance.DelInputAction(InputType.Crouch, SwitchCrouch);
    }

#if false
    //-----------------------------------------------------------------------------
    // Purpose: 
    // Input  : wishdir - 
    //			accel - 
    //-----------------------------------------------------------------------------
    void CGameMovement::AirAccelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        int i;
        float addspeed, accelspeed, currentspeed;
        float wishspd;

        wishspd = wishspeed;

        if (player->pl.deadflag) // ����ł���߂�
            return;
        if (player->m_flWaterJumpTime) // ���H�W�����v�H���Ă���߂�
            return;

        // Cap speed �X�q�X�s�[�h�H�H�H�@�ő�ʂ����߂Ă鉽���� Cap = �Œ�ő�l�ȏ�ɍs���Ȃ��悤�ɂ��邱��
        if (wishspd > GetAirSpeedCap())
            wishspd = GetAirSpeedCap();

        // Determine veer amount�@�U�ꕝ�̌���@magnitude?
        currentspeed = mv->m_vecVelocity.Dot(wishdir);

        // See how much to add�@�ǉ��ʂ�����
        addspeed = wishspd - currentspeed;

        // If not adding any, done.
        if (addspeed <= 0)
            return;

        // Determine acceleration speed after acceleration ������̉������x�����肷��
        accelspeed = accel * wishspeed * gpGlobals->frametime * player->m_surfaceFriction;

        // Cap it
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        // Adjust pmove vel.
        for (i = 0; i < 3; i++)
        {
            mv->m_vecVelocity[i] += accelspeed * wishdir[i];
            mv->m_outWishVel[i] += accelspeed * wishdir[i];
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: �ړI
    //-----------------------------------------------------------------------------
    void CGameMovement::AirMove()
    {
        int i;
        Vector3 wishvel = new Vector3();
        float fmove, smove;
        Vector3 wishdir;
        float wishspeed;
        Vector3 forward, right, up;

        AngleVectors(mv->m_vecViewAngles, &forward, &right, &up);  // Determine movement angles ����p�x�̌���

        // Copy movement amounts �R�s�[�ړ���
        fmove = mv->m_flForwardMove;
        smove = mv->m_flSideMove;

        // Zero out z components of movement vectors �ړ��x�N�g����z�������[���ɂ��� ��������y�������
        forward[2] = 0;
        right[2] = 0;
        VectorNormalize(forward);  // Normalize remainder of vectors �x�N�g���̗]��𐳋K������
        VectorNormalize(right);    // 

        for (i = 0; i < 2; i++)       // Determine x and y parts of velocity ���x��x������y���������肷��
            wishvel[i] = forward[i] * fmove + right[i] * smove;
        wishvel[2] = 0;             // Zero out z part of velocity ���x��z�������[���ɂ���

        VectorCopy(wishvel, wishdir);   // Determine maginitude of speed of move �ړ����x�̃}�W�j�`���[�h�����肷��
        wishspeed = VectorNormalize(wishdir);

        //
        // clamp to server defined max speed �T�[�o�[����`�����ō����x�ɃN�����v
        //
        if (wishspeed != 0 && (wishspeed > mv->m_flMaxSpeed))
        {
            VectorScale(wishvel, mv->m_flMaxSpeed / wishspeed, wishvel);
            wishspeed = mv->m_flMaxSpeed;
        }

        AirAccelerate(wishdir, wishspeed, sv_airaccelerate.GetFloat());

        // Add in any base velocity to the current velocity. ���݂̑��x�ɔC�ӂ̊�{���x��������
        VectorAdd(mv->m_vecVelocity, player->GetBaseVelocity(), mv->m_vecVelocity);

        TryPlayerMove();

        // Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
        // ���x�̓x�[�X���x�������߂� ��{���x�́A�R���x�A�̂悤�ȓ������̂ɏ���Ă���ꍇ�ɐݒ肳���(����Ƃ��ʂ̃����X�^�[�H)
        VectorSubtract(mv->m_vecVelocity, player->GetBaseVelocity(), mv->m_vecVelocity);
    }

    void SV_Accelerate()
    {
        int i;
        float addspeed, accelspeed, currentspeed;

        currentspeed = DotProduct(velocity, wishdir);
        addspeed = 20 - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = sv_accelerate.value * host_frametime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        for (i = 0; i < 3; i++)
            velocity[i] += accelspeed * wishdir[i];
    }
#endif
}