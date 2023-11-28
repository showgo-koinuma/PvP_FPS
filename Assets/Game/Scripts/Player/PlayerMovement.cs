using System;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun
{
    Transform _orientation; // �Ⴄ�I�u�W�F�N�g�������ꍇSerializeField�ɂ���
    private Rigidbody _rb;

    // Movement
    [Header("�ړ�")]
    [SerializeField] float _moveSpeed = 4500;
    [SerializeField] float _maxSpeed = 20;
    bool _grounded;
    [SerializeField, Tooltip("�n�ʂ̃��C���[")] LayerMask _whatIsGround;
    [SerializeField] float _counterMovement = 0.175f;
    private float _threshold = 0.01f;
    [SerializeField, Tooltip("�n�ʂƔ��肷��ő�̌X�Ίp�x")] float _maxSlopeAngle = 35f;
    private bool _cancellingGrounded;

    // ���Ⴊ��
    [Header("���Ⴊ�݁A�W�����v")]
    [SerializeField] float _slideForce = 400;
    [SerializeField, Tooltip("���Ⴊ�݂ɂ�鑬�x�ቺ����")] float _crouchMoveSpeedRate;
    // ���݂̃X�s�[�h����
    float _currentMoveSpeedRate = 1f;
    [SerializeField] float _slideCounterMovement = 0.2f;
    [SerializeField, Tooltip("���Ⴊ�݂ł̃X�P�[��")] Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 _playerScale;

    // Jumping
    private bool _readyToJump = true;
    [SerializeField] float _jumpCooldown = 0.1f;
    [SerializeField] float _jumpForce = 550f;
    [SerializeField, Tooltip("�󒆂ł̃^�[���X�s�[�h")] float _turnSpeed = 0.1f;

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

    #region �g���邱�Ƃ̂Ȃ��Ȃ����c�[�@�Q�l�p
    private void FixedUpdate()
    {
        //Movement();
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y < 0) Jump();
        //MyInput();
        //if (_jumping) Jump();
        //_jumping = Input.GetButton("Jump") || Input.mouseScrollDelta.y < 0;
    }

    private void MyInput() // �g���邱�Ƃ̂Ȃ��Ȃ����c�[�@�Q�l�p
    {
        //_moveInputX = Input.GetAxisRaw("Horizontal");
        //_moveInputY = Input.GetAxisRaw("Vertical");
        _jumping = Input.GetButton("Jump") || Input.mouseScrollDelta.y < 0;
        _crouching = Input.GetKey(KeyCode.LeftControl);

        //if (Input.GetKeyDown(KeyCode.LeftControl)) StartCrouch();
        //if (Input.GetKeyUp(KeyCode.LeftControl)) StopCrouch();
    }
    #endregion

    void Movement()
    {
        Vector3 dir = new Vector3(PlayerInput.Instance.InputMoveVector.x, 0, PlayerInput.Instance.InputMoveVector.y);
        dir = transform.TransformDirection(dir); // �̂̌����ɍ��킹��
        float maxXDiff = 5f; // ��������velo�̌����̂���̍ő�l
        if (_grounded && _readyToJump) // �ڒn��
        {
            if (_rb.velocity.magnitude > _moveSpeed) // �������Ă����ꍇ�A���X�Ɍ���������
            {
                float mag = _rb.velocity.magnitude - 10 * Time.deltaTime;
                _rb.velocity = dir * mag * _currentMoveSpeedRate;
            }
            else _rb.velocity = dir * _moveSpeed * _currentMoveSpeedRate;
        }
        else // ��
        {
            if (dir.magnitude == 0) return; // ���͂��Ȃ���ΏI��
            float acceleRate = 10;
            Vector3 currentVelo = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // ���݂̐���Velocity
            float currentSpeed = Vector3.Dot(currentVelo, dir);
            float speedDiff = _moveSpeed - currentSpeed;
            if (speedDiff <= 0) return;
            float acceleSpeed = _turnSpeed * Time.deltaTime;
            if (acceleSpeed > speedDiff) acceleSpeed = speedDiff;
            currentVelo += dir * acceleSpeed;
            if (currentVelo.magnitude > _maxSpeed) currentVelo = currentVelo.normalized * 20;
            currentVelo.y = _rb.velocity.y;
            _rb.velocity = currentVelo;
            return;

            // ���݂̐���direction�Ɠ��͕�����Slerp���ċ󒆂ŕ����]��(�X�g���C�t)����
            Vector3 moveVelo = Vector3.Lerp(currentVelo, dir * _moveSpeed,
                _turnSpeed * Time.deltaTime * (currentVelo - dir * _moveSpeed).magnitude);
            //Debug.Log((Vector3.Dot(moveVelo, transform.forward) * transform.forward - moveVelo).magnitude);
            if ((Vector3.Dot(moveVelo, transform.forward) * transform.forward - moveVelo).magnitude <= maxXDiff)
            {
                moveVelo = Vector3.Lerp(currentVelo.normalized, dir,
                    _turnSpeed * Time.deltaTime * (currentVelo - dir * _moveSpeed).magnitude * 2) * currentVelo.magnitude * 1.2f;
                if (moveVelo.magnitude > _moveSpeed * 1.2f)
                {
                    moveVelo = moveVelo.normalized * 1.2f;
                }
            }
            moveVelo.y = _rb.velocity.y; // �������x�͕ێ�����
            _rb.velocity = moveVelo;
        }
    }

    private void MovementAddForce()
    {
        Vector2 moveInput = PlayerInput.Instance.InputMoveVector;
        _rb.AddForce(Vector3.down * Time.deltaTime * 10); // ��������AddForce

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(moveInput.x, moveInput.y, mag); //��U�Ȃ���
        _jumping = false;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (_crouching && _grounded && _readyToJump)
        {
            _rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        float maxSpeed = _maxSpeed;
        if (moveInput.x > 0 && xMag > maxSpeed) moveInput.x = 0;
        if (moveInput.x < 0 && xMag < -maxSpeed) moveInput.x = 0;
        if (moveInput.y > 0 && yMag > maxSpeed) moveInput.y = 0;
        if (moveInput.y < 0 && yMag < -maxSpeed) moveInput.y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!_grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (_grounded && _crouching) multiplierV = 0f;

        //Apply forces to move player
        _rb.AddForce(_orientation.transform.forward * moveInput.y * _moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rb.AddForce(_orientation.transform.right * moveInput.x * _moveSpeed * Time.deltaTime * multiplier);
    }

    void Jump()
    {
        if (_grounded && _readyToJump)
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
            if (_rb.velocity.magnitude > 0.5f)
            {
                if (_grounded)
                {
                    _rb.AddForce(_orientation.transform.forward * _slideForce);
                }
            }
            _crouching = true;
            return;
        } // return�ؑւ��Ă��Ⴊ�݉����̏���
        transform.localScale = _playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
        _crouching = false;
    }

    /// <summary>�ȁ@�Ɂ@���@��</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="mag"></param>
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!_grounded || !_readyToJump) return;

        //Slow down sliding
        if (_crouching)
        {
            _rb.AddForce(_moveSpeed * Time.deltaTime * -_rb.velocity.normalized * _slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > _threshold && Math.Abs(x) < 0.05f || (mag.x < -_threshold && x > 0) || (mag.x > _threshold && x < 0))
        {
            _rb.AddForce(_moveSpeed * _orientation.transform.right * Time.deltaTime * -mag.x * _counterMovement);
        }
        if (Math.Abs(mag.y) > _threshold && Math.Abs(y) < 0.05f || (mag.y < -_threshold && y > 0) || (mag.y > _threshold && y < 0))
        {
            _rb.AddForce(_moveSpeed * _orientation.transform.forward * Time.deltaTime * -mag.y * _counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(_rb.velocity.x, 2) + Mathf.Pow(_rb.velocity.z, 2))) > _maxSpeed)
        {
            float fallspeed = _rb.velocity.y;
            Vector3 n = _rb.velocity.normalized * _maxSpeed;
            _rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = _orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
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
                _grounded = true;
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
        _grounded = false;
    }

    private void OnEnable()
    {
        InGameManager.Instance.UpdateAction += Movement;
        //InGameManager.Instance.UpdateAction += MovementAddForce;
        PlayerInput.Instance.SetInputAction(InputType.Jump, Jump);
        PlayerInput.Instance.SetInputAction(InputType.Crouch, SwitchCrouch);
    }

    private void OnDisable()
    {
        InGameManager.Instance.UpdateAction -= Movement;
        //InGameManager.Instance.UpdateAction -= MovementAddForce;
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