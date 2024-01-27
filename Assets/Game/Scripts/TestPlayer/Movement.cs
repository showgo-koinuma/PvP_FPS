using UnityEngine;

/// <summary>
/// Player�𓮂����R���|�[�l���g
/// </summary>
// �R���|�[�l���g�������邩
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    Rigidbody _rb;
    LineRenderer _lineRenderer;

    [Header("�ړ�")]
    // �ڒn��
    [SerializeField] float _moveSpeed;
    [SerializeField, Tooltip("�ڒn���̉����x")] float _groundAcceleration;
    [SerializeField, Tooltip("���C")] float _friction;
    [SerializeField] float _adsMoveSpeedRate = 0.5f;
    [Space(5)]
    // ��
    [SerializeField] float _airMoveSpeed;
    [SerializeField, Tooltip("�󒆂ł̉����x")] float _airAcceleration;
    [SerializeField, Tooltip("�󒆖��C")] float _airFriction;
    [SerializeField] float _maxSpeed;
    [Space(5)]
    // ���̑�
    [SerializeField, Tooltip("�n�ʂƔ��肷��ő�̌X�Ίp�x")] float _maxSlopeAngle;
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
    float _slidingTimer;

    // ���݂̏�Ԃ��Ǘ��@enum�ł��K�v���łĂ��邩��
    bool _jumping, _crouching, _isSliding;

    void Start()
    {
        _playerScale = transform.localScale;
        _rb = GetComponent<Rigidbody>();
        _lineRenderer = GetComponent<LineRenderer>();
        PlayerInput.Instance.SetInputAction(InputType.Crouch, SwitchCrouch);

        // �J�[�\�����b�N
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _jumping = false;
        if (Input.mouseScrollDelta.y < 0 || PlayerInput.Instance.OnJumpButton) { Jump(); WallJump(); } // �}�E�X�z�C�[�����{�^���݂����Ɏg�������񂾂��ǂ�
        //Move();
        CrouchTransition();
        SlidingTimerCounter();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (_isGround && !PlayerInput.Instance.OnJumpButton) GroundMove();
        else AirMove();

        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, transform.position + _playerVelocity * 2);

        _playerVelocity.y = _rb.velocity.y;
        _rb.velocity = _playerVelocity;
        _playerVelocity.y = 0;

        //Debug.Log(_playerVelocity.magnitude);
    }

    /// <summary>�n��ł̓���</summary>
    public void GroundMove()
    {
        Vector3 vec = _playerVelocity; // Equivalent to: VectorCopy();
        vec.y = 0f;
        float speed = vec.magnitude;

        float control = speed < _groundAcceleration ? _groundAcceleration : speed;
        float drop = control * _friction * 1 * Time.fixedDeltaTime;//Time.deltaTime;
        if (_jumping) drop = 0f;
        if (_isSliding) drop *= _slidingFriction;

        float newspeed = speed - drop;
        if (newspeed < 0) newspeed = 0;
        if (speed > 0) newspeed /= speed;

        _playerVelocity.x *= newspeed;
        _playerVelocity.z *= newspeed;

        //Debug.Log(newspeed);

        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;
        wishdir = transform.TransformDirection(wishdir);

        if (_isSliding)
        {
            //Accelerate(wishdir, _moveSpeed, _groundAcceleration);
            Debug.Log("isSliding");
            if (_playerVelocity.magnitude <= 5) _isSliding = false;
        }
        else if (_crouching)
        {
            Accelerate(wishdir, _moveSpeed * _crouchMoveSpeedRate, _groundAcceleration);
            Sliding(_playerVelocity.magnitude);
        }
        else if (PlayerInput.Instance.IsADS) // ads���͒x���Ȃ�
        {
            Accelerate(wishdir, _moveSpeed * _adsMoveSpeedRate, _groundAcceleration * _adsMoveSpeedRate);
        }
        else
        {
            Accelerate(wishdir, _moveSpeed, _groundAcceleration);
        }
    }

    /// <summary>�󒆂ł̓���</summary>
    public void AirMove()
    {
        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;

        float speed = _playerVelocity.magnitude;
        float control = speed < _airAcceleration ? _airAcceleration : speed;
        float drop = control * _airFriction * 1 * Time.fixedDeltaTime;//Time.deltaTime;

        float newspeed = speed - drop;
        if (newspeed < 0) newspeed = 0;
        if (speed > 0) newspeed /= speed;

        _playerVelocity.x *= newspeed;
        _playerVelocity.z *= newspeed;

        wishdir = transform.TransformDirection(wishdir);
        Accelerate(wishdir, _airMoveSpeed, _airAcceleration);
    }

    /// <summary>�x�N�g�����v�Z����</summary>
    void Accelerate(Vector3 wishdir, float wishSpeed, float accel)
    {
        float currentspeed = Vector3.Dot(_playerVelocity, wishdir);
        float addspeed = wishSpeed - currentspeed;
        float accelspeed = Mathf.Clamp(accel * Time.fixedDeltaTime * wishSpeed, 0, addspeed);

        //CurrentSpeed = Math.Pow(Vector3.Dot(Vel, inputVector), 5f); �N���̂�@�Ȃ񂩂����Ⴄ
        //addSpeed = Math.Clamp(Vel.magnitude - CurrentSpeed, 0, StrafeAmount);

        _playerVelocity.x += accelspeed * wishdir.x;
        _playerVelocity.z += accelspeed * wishdir.z;

        if (_playerVelocity.magnitude > _maxSpeed)
        {
            _playerVelocity = _playerVelocity.normalized * _maxSpeed;
        }
    }

    void Jump()
    {
        if (_isGround && _readyToJump)
        {
            _isGround = false;
            _readyToJump = false;
            _jumping = true;
            _isSliding = false;

            _rb.AddForce(Vector2.up * _jumpForce * 1.5f);
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

        // vector�̑��
        _playerVelocity = wallLookJumpVec; // �v�Z�p�ϐ��ɑ��
        _rb.velocity = wallLookJumpVec; // velocity�ɒ��ڑ�� y���͒��ړ���Ȃ��Ɩʓ|
    }

    /// <summary>���Ⴊ�ݏ�Ԃ�؂�ւ���</summary>
    void SwitchCrouch()
    {
        if (PlayerInput.Instance.IsCrouching) // ���Ⴊ�݊J�n����
        {
            _crouchDir = -1;
            _crouching = true;
            return;
        } // return�ؑւ��Ă��Ⴊ�݉����̏���

        _crouchDir = 1;
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
        _playerVelocity = _playerVelocity.normalized * _slidingSpeed;
        _playerVelocity.y = _rb.velocity.y;
        _readyToSliding = false;
        _isSliding = true;
        Invoke(nameof(FinishSliding), 1f);
        _slidingTimer = _slidingCooldown;
    }

    void FinishSliding()
    {
        _isSliding = false;
    }

    /// <summary>sliding timer�̌v�Z</summary>
    void SlidingTimerCounter()
    {
        if (_slidingTimer > 0 && _readyToJump && !_crouching)
        {
            _slidingTimer -= Time.deltaTime;
        }
        else if (_slidingTimer <= 0)
        {
            _readyToSliding = true;
        }
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
}
