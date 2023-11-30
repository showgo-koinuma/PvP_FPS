using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun
{
    Transform _orientation; // �Ⴄ�I�u�W�F�N�g�������ꍇSerializeField�ɂ���
    Rigidbody _rb;

    // Movement
    [Header("�ړ�")]
    Vector3 _playerVelocity;
    [SerializeField] float _moveSpeed = 4500;
    [SerializeField] float _maxSpeed = 20;
    [SerializeField, Tooltip("�ڒn���̉����x")] float _groundAcceleration;
    [SerializeField, Tooltip("�󒆂ł̉����x")] float _airAcceleration;
    [SerializeField, Tooltip("�X�g���C�t�̉����x")] float _strafeAcceleration = 50f;
    [SerializeField, Tooltip("�X�g���C�t�̍ő呬�x")] float _maxStrafeSpeed = 1f;
    [SerializeField, Tooltip("�󒆂łǂ̒��x����ł��邩")] float _airControl;
    [SerializeField, Tooltip("�n�ʂƔ��肷��ő�̌X�Ίp�x")] float _maxSlopeAngle = 35f;
    [SerializeField, Tooltip("���C")] float _friction = 6f;
    [SerializeField, Tooltip("�n�ʂ̃��C���[")] LayerMask _whatIsGround;
    bool _isGround;
    bool _cancellingGrounded;

    [Header("�W�����v�A���Ⴊ��")]

    // Jumping
    bool _readyToJump = true;
    [SerializeField] float _jumpCooldown = 0.1f;
    [SerializeField] float _jumpForce = 550f;

    // ���Ⴊ��
    [SerializeField] float _slideForce = 400;
    [SerializeField, Tooltip("���Ⴊ�݂ɂ�鑬�x�ቺ����")] float _crouchMoveSpeedRate;
    [SerializeField, Tooltip("���Ⴊ�݂ł̃X�P�[��")] Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    [SerializeField, Tooltip("�X���C�f�B���O�̃N�[���_�E��")] float _slidingCooldown;
    /// <summary>���݂̃X�s�[�h����</summary>
    float _currentMoveSpeedRate = 1f;
    private Vector3 _playerScale;
    bool _readyToSliding = true;

    // Input
    //float _moveInputX, _moveInputY;
    bool _jumping, _sprinting, _crouching;

    // Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector; // ���Ɏg���H �ǃW�����v��

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
        if (Input.mouseScrollDelta.y < 0) Jump(); // �}�E�X�z�C�[�����{�^���݂����Ɏg�������񂾂��ǂ�
        Movement();
        _jumping = false;
    }

    void Movement()
    {
        if (_isGround) GroundMove();
        else AirMove();
        _playerVelocity.y = _rb.velocity.y;
        _rb.velocity = _playerVelocity;

        //Vector3 dir = new Vector3(PlayerInput.Instance.InputMoveVector.x, 0, PlayerInput.Instance.InputMoveVector.y);
        //dir = transform.TransformDirection(dir); // �̂̌����ɍ��킹��
        //float maxXDiff = 5f; // ��������velo�̌����̂���̍ő�l
        //if (_isGround && _readyToJump) // �ڒn��
        //{
        //    if (_rb.velocity.magnitude > _moveSpeed) // �������Ă����ꍇ�A���X�Ɍ���������
        //    {
        //        float mag = _rb.velocity.magnitude - 10 * Time.deltaTime;
        //        _rb.velocity = dir * mag * _currentMoveSpeedRate;
        //    }
        //    else _rb.velocity = dir * _moveSpeed * _currentMoveSpeedRate;
        //}
        //else // ��
        //{
        //    if (dir.magnitude == 0) return; // ���͂��Ȃ���ΏI��
        //    Vector3 currentVelo = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // ���݂̐���Velocity
        //    float currentSpeed = Vector3.Dot(dir, currentVelo);
        //    float speedDiff = _moveSpeed - currentSpeed;
        //    if (speedDiff <= 0) return;
        //    float acceleSpeed = _turnSpeed * Time.deltaTime;
        //    if (acceleSpeed > speedDiff) acceleSpeed = speedDiff;
        //    currentVelo += dir * acceleSpeed;
        //    if (currentVelo.magnitude > _maxSpeed) currentVelo = currentVelo.normalized * _maxSpeed;
        //    currentVelo.y = _rb.velocity.y;
        //    _rb.velocity = currentVelo;
        //}
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

        float newspeed = speed - drop;
        if (newspeed < 0) newspeed = 0;
        if (speed > 0) newspeed /= speed;

        _playerVelocity.x *= newspeed;
        _playerVelocity.z *= newspeed;

        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;
        wishdir = transform.TransformDirection(wishdir);
        Accelerate(wishdir, _moveSpeed, _groundAcceleration);
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
        if (wishdir.x == 0 && wishdir.z != 0)
        {
            if (wishspeed > _maxStrafeSpeed)
                wishspeed = _maxStrafeSpeed;
            accel = _strafeAcceleration;
        }

        wishdir = transform.TransformDirection(wishdir);
        Accelerate(wishdir, wishspeed, accel);

        if (wishspeed2 != 0) AirControl(wishdir, wishspeed2);

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
                _playerVelocity.x = _playerVelocity.x * speed + wishdir.x * k;
                _playerVelocity.y = _playerVelocity.y * speed + wishdir.y * k;
                _playerVelocity.z = _playerVelocity.z * speed + wishdir.z * k;

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

    private void ResetJump()
    {
        _readyToJump = true;
    }

    /// <summary>���Ⴊ�ݏ�Ԃ�؂�ւ���</summary>
    void SwitchCrouch()
    {
        if (PlayerInput.Instance.IsCrouching) // ���Ⴊ�݊J�n����
        {
            transform.localScale = _crouchScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
            Sliding();
            _crouching = true;
            return;
        } // return�ؑւ��Ă��Ⴊ�݉����̏���
        transform.localScale = _playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
        _crouching = false;
    }

    void Sliding()
    {
        if (!_readyToSliding) return;
        _readyToSliding = false;
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

        for (int i = 0; i < other.contactCount; i++) // ���ׂĂ̐ڐG�_�ɂ�����IsFloor��������
        {
            Vector3 normal = other.contacts[i].normal; // �ڂ��Ă���ʂ̖@���x�N�g��

            if (IsFloor(normal))
            {
                _isGround = true;
                _cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!_cancellingGrounded)
        {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        _isGround = false;
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += ThisUpdate;
        PlayerInput.Instance.SetInputAction(InputType.Jump, Jump);
        PlayerInput.Instance.SetInputAction(InputType.Crouch, SwitchCrouch);
    }

    private void OnDisable()
    {
        InGameManager.Instance.UpdateAction -= ThisUpdate;
        PlayerInput.Instance.DelInputAction(InputType.Jump, Jump);
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