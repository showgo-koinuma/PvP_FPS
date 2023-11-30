using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun
{
    Transform _orientation; // 違うオブジェクトだった場合SerializeFieldにする
    Rigidbody _rb;

    // Movement
    [Header("移動")]
    Vector3 _playerVelocity;
    [SerializeField] float _moveSpeed = 4500;
    [SerializeField] float _maxSpeed = 20;
    [SerializeField, Tooltip("接地中の加速度")] float _groundAcceleration;
    [SerializeField, Tooltip("空中での加速度")] float _airAcceleration;
    [SerializeField, Tooltip("ストレイフの加速度")] float _strafeAcceleration = 50f;
    [SerializeField, Tooltip("ストレイフの最大速度")] float _maxStrafeSpeed = 1f;
    [SerializeField, Tooltip("空中でどの程度制御できるか")] float _airControl;
    [SerializeField, Tooltip("地面と判定する最大の傾斜角度")] float _maxSlopeAngle = 35f;
    [SerializeField, Tooltip("摩擦")] float _friction = 6f;
    [SerializeField, Tooltip("地面のレイヤー")] LayerMask _whatIsGround;
    bool _isGround;
    bool _cancellingGrounded;

    [Header("ジャンプ、しゃがみ")]

    // Jumping
    bool _readyToJump = true;
    [SerializeField] float _jumpCooldown = 0.1f;
    [SerializeField] float _jumpForce = 550f;

    // しゃがみ
    [SerializeField] float _slideForce = 400;
    [SerializeField, Tooltip("しゃがみによる速度低下割合")] float _crouchMoveSpeedRate;
    [SerializeField, Tooltip("しゃがみでのスケール")] Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    [SerializeField, Tooltip("スライディングのクールダウン")] float _slidingCooldown;
    /// <summary>現在のスピード割合</summary>
    float _currentMoveSpeedRate = 1f;
    private Vector3 _playerScale;
    bool _readyToSliding = true;

    // Input
    //float _moveInputX, _moveInputY;
    bool _jumping, _sprinting, _crouching;

    // Sliding
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector; // 何に使う？ 壁ジャンプか

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
        if (Input.mouseScrollDelta.y < 0) Jump(); // マウスホイールをボタンみたいに使いたいんだけどな
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
        //dir = transform.TransformDirection(dir); // 体の向きに合わせる
        //float maxXDiff = 5f; // 加速するveloの向きのずれの最大値
        //if (_isGround && _readyToJump) // 接地中
        //{
        //    if (_rb.velocity.magnitude > _moveSpeed) // 加速していた場合、徐々に減速させる
        //    {
        //        float mag = _rb.velocity.magnitude - 10 * Time.deltaTime;
        //        _rb.velocity = dir * mag * _currentMoveSpeedRate;
        //    }
        //    else _rb.velocity = dir * _moveSpeed * _currentMoveSpeedRate;
        //}
        //else // 空中
        //{
        //    if (dir.magnitude == 0) return; // 入力がなければ終了
        //    Vector3 currentVelo = new Vector3(_rb.velocity.x, 0, _rb.velocity.z); // 現在の水平Velocity
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

    /// <summary>地上での動き</summary>
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

    /// <summary>空中での動き</summary>
    public void AirMove()
    {
        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;

        float wishspeed = 7f;

        // Aircontrol
        float wishspeed2 = wishspeed;
        float accel = _airAcceleration;

        // 左右キーだけでストレイフしていた場合
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

        // 左右キーだけのストレイフは速くなる的な
        void AirControl(Vector3 wishdir, float wishspeed)
        {
            _playerVelocity.y = 0;
            float speed = _playerVelocity.magnitude;
            _playerVelocity.Normalize();

            float dot = Vector3.Dot(_playerVelocity, wishdir);
            float k = 32; // なんでこの数値になったのか分からん
            k *= _airControl * dot * dot * Time.deltaTime;

            // 減速しながら方向を変える
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

    /// <summary>ベクトルを計算する</summary>
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
            //_rb.AddForce(_normalVector * _jumpForce * 0.5f); // 坂道の影響を少し受ける
            //If jumping while falling, reset y velocity. よくわからん　リセットは必要だけどAddForceの後にやるのか...
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

    /// <summary>しゃがみ状態を切り替える</summary>
    void SwitchCrouch()
    {
        if (PlayerInput.Instance.IsCrouching) // しゃがみ開始処理
        {
            transform.localScale = _crouchScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
            Sliding();
            _crouching = true;
            return;
        } // return切替してしゃがみ解除の処理
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

    /// <summary>その法線ベクトルの面は地面として扱えるか</summary>
    private bool IsFloor(Vector3 nomalVector)
    {
        float angle = Vector3.Angle(Vector3.up, nomalVector);
        return angle < _maxSlopeAngle;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (_whatIsGround != (_whatIsGround | (1 << layer))) return; // 地面のレイヤーでなければ当たっていても地面だと判定しない

        for (int i = 0; i < other.contactCount; i++) // すべての接触点においてIsFloorをかける
        {
            Vector3 normal = other.contacts[i].normal; // 接している面の法線ベクトル

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

        if (player->pl.deadflag) // 死んでたら戻る
            return;
        if (player->m_flWaterJumpTime) // 水？ジャンプ？してたら戻る
            return;

        // Cap speed 帽子スピード？？？　最大量を決めてる何かで Cap = 固定最大値以上に行かないようにすること
        if (wishspd > GetAirSpeedCap())
            wishspd = GetAirSpeedCap();

        // Determine veer amount　振れ幅の決定　magnitude?
        currentspeed = mv->m_vecVelocity.Dot(wishdir);

        // See how much to add　追加量を見る
        addspeed = wishspd - currentspeed;

        // If not adding any, done.
        if (addspeed <= 0)
            return;

        // Determine acceleration speed after acceleration 加速後の加速速度を決定する
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
    // Purpose: 目的
    //-----------------------------------------------------------------------------
    void CGameMovement::AirMove()
    {
        int i;
        Vector3 wishvel = new Vector3();
        float fmove, smove;
        Vector3 wishdir;
        float wishspeed;
        Vector3 forward, right, up;

        AngleVectors(mv->m_vecViewAngles, &forward, &right, &up);  // Determine movement angles 動作角度の決定

        // Copy movement amounts コピー移動量
        fmove = mv->m_flForwardMove;
        smove = mv->m_flSideMove;

        // Zero out z components of movement vectors 移動ベクトルのz成分をゼロにする 多分今のy軸じゃね
        forward[2] = 0;
        right[2] = 0;
        VectorNormalize(forward);  // Normalize remainder of vectors ベクトルの余りを正規化する
        VectorNormalize(right);    // 

        for (i = 0; i < 2; i++)       // Determine x and y parts of velocity 速度のx部分とy部分を決定する
            wishvel[i] = forward[i] * fmove + right[i] * smove;
        wishvel[2] = 0;             // Zero out z part of velocity 速度のz部分をゼロにする

        VectorCopy(wishvel, wishdir);   // Determine maginitude of speed of move 移動速度のマジニチュードを決定する
        wishspeed = VectorNormalize(wishdir);

        //
        // clamp to server defined max speed サーバーが定義した最高速度にクランプ
        //
        if (wishspeed != 0 && (wishspeed > mv->m_flMaxSpeed))
        {
            VectorScale(wishvel, mv->m_flMaxSpeed / wishspeed, wishvel);
            wishspeed = mv->m_flMaxSpeed;
        }

        AirAccelerate(wishdir, wishspeed, sv_airaccelerate.GetFloat());

        // Add in any base velocity to the current velocity. 現在の速度に任意の基本速度を加える
        VectorAdd(mv->m_vecVelocity, player->GetBaseVelocity(), mv->m_vecVelocity);

        TryPlayerMove();

        // Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
        // 今度はベース速度を引き戻す 基本速度は、コンベアのような動く物体に乗っている場合に設定される(それとも別のモンスター？)
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