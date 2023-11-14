using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform _playerCam;
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
    [SerializeField] float _slideCounterMovement = 0.2f;
    [SerializeField, Tooltip("���Ⴊ�݂ł̃X�P�[��")] Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 _playerScale;

    // Jumping
    private bool _readyToJump = true;
    [SerializeField] float _jumpCooldown = 0.25f;
    [SerializeField] float _jumpForce = 550f;

    // Input
    float _moveInputX, _moveInputY;
    bool _jumping, _sprinting, _crouching;

    // Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector; // ���Ɏg���H �ǃW�����v��

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _orientation = GetComponent<Transform>();
    }

    void Start()
    {
        _playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        MyInput();
        if (_jumping) Jump();
    }

    private void MyInput()
    {
        _moveInputX = Input.GetAxisRaw("Horizontal");
        _moveInputY = Input.GetAxisRaw("Vertical");
        _jumping = Input.GetButton("Jump") || Input.mouseScrollDelta.y < 0;
        _crouching = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void StartCrouch()
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
    }

    private void StopCrouch()
    {
        transform.localScale = _playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z);
    }

    private void Movement()
    {
        _rb.AddForce(Vector3.down * Time.deltaTime * 10); // ��������AddForce

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(_moveInputX, _moveInputY, mag);

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (_crouching && _grounded && _readyToJump)
        {
            _rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        float maxSpeed = _maxSpeed;
        if (_moveInputX > 0 && xMag > maxSpeed) _moveInputX = 0;
        if (_moveInputX < 0 && xMag < -maxSpeed) _moveInputX = 0;
        if (_moveInputY > 0 && yMag > maxSpeed) _moveInputY = 0;
        if (_moveInputY < 0 && yMag < -maxSpeed) _moveInputY = 0;

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
        _rb.AddForce(_orientation.transform.forward * _moveInputY * _moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rb.AddForce(_orientation.transform.right * _moveInputX * _moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (_grounded && _readyToJump)
        {
            _readyToJump = false;

            _rb.AddForce(Vector2.up * _jumpForce * 1.5f);
            _rb.AddForce(_normalVector * _jumpForce * 0.5f); // �⓹�̉e���������󂯂�

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

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!_grounded || _jumping) return;

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
}