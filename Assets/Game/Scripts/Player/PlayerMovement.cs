using UnityEngine;
using Photon.Pun;

/// <summary>
/// Playerを動かすコンポーネント
/// </summary>
// コンポーネント分けられるか
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun
{
    Rigidbody _rb;
    PlayerManager _playerManager;

    [Header("移動")]
    // 接地中
    [SerializeField, Tooltip("接地中の加速度")] float _groundAcceleration;
    [SerializeField, Tooltip("摩擦")] float _friction;
    [SerializeField] float _maxSpeed;
    [Space(5)]
    // 空中
    [SerializeField, Tooltip("空中での加速度")] float _airAcceleration;
    [SerializeField, Tooltip("空中摩擦")] float _airFriction;
    [SerializeField] float _airMaxSpeed;
    [Space(5)]
    // その他
    [SerializeField] float _adsMoveSpeedRate = 0.5f;
    [SerializeField, Tooltip("地面と判定する最大の傾斜角度")] float _maxSlopeAngle;
    [SerializeField, Tooltip("地面のレイヤー")] LayerMask _whatIsGround;
    /// <summary>rbに直接代入するplayerのvelocity</summary>
    Vector3 _playerVelocity;
    bool _isGround;
    /// <summary>接地中の面のnomalVector</summary>
    Vector3 _groundNormalVector = Vector3.up;
    bool _cancellingGrounded;

    [Header("ジャンプ")]
    [SerializeField] float _jumpCooldown;
    [SerializeField] float _jumpForce;
    bool _readyToJump = true;
    [Space(10)]
    // 壁ジャン
    [SerializeField, Tooltip("壁ジャンの強さ")] float _wallJumpPower;
    [SerializeField, Tooltip("壁ジャン出来る壁として扱える最大の角度")] float _maxWallAngle; // 現時点上下の角度を考慮出来てない
    bool _onWall;
    /// <summary>最も正面にある壁のnomalVector</summary>
    Vector3 _wallNormalVector;
    bool _cancellingOnWall;

    [Header("しゃがみ")]
    [SerializeField, Tooltip("キャラコン用colliderのscale変更のため")] GameObject _moveBodyObject;
    [SerializeField, Tooltip("しゃがみ時の視点移動のため")] GameObject _headObjct;
    [SerializeField, Tooltip("しゃがみ中の最大速度割合")] float _crouchMaxSpeedRate;
    [SerializeField, Tooltip("しゃがみでのスケール")] Vector3 _crouchScale;
    float _crouchTransitionTime = 0.2f;
    Vector3 _playerScale;
    int _crouchDir = 1;

    [Header("スライディング")]
    [SerializeField, Tooltip("スライディングの初速")] float _slidingSpeed;
    [SerializeField, Tooltip("スライディング時の滑りやすさ(摩擦)")] float _slidingFriction;
    [SerializeField, Tooltip("スライディイングがかかるスピードの最小値")] float _minCnaSlidingSpeed;
    [SerializeField, Tooltip("スライディングのクールダウン")] float _slidingCooldown;
    bool _readyToSliding = true;
    float _slidingTimer;

    // 現在の状態を管理　enumでやる必要がでてくるかも
    bool _jumping, _crouching, _isSliding;

    // AnimationManagerのためのプロパティ
    public Vector3 PlayerVelocity { get => _playerVelocity; }
    public bool IsGround { get => _isGround; }
    public bool IsJumping { get => _jumping; }
    public bool IsCrouching { get => _crouching; }
    public bool IsSliding { get => _isSliding; }

    void Awake()
    {
        if (!photonView.IsMine) this.enabled = false;
        _rb = GetComponent<Rigidbody>();
        _playerManager = GetComponent<PlayerManager>();
    }

    void Start()
    {
        _playerScale = transform.localScale;
    }

    private void ThisUpdate()
    {
        if (_playerManager.PlayerState != PlayerState.Nomal)
        {
            return;
        }

        _jumping = false;
        if (Input.mouseScrollDelta.y < 0 || PlayerInput.Instance.OnJumpButton) { Jump(); WallJump(); } // マウスホイールをボタンみたいに使いたいんだけどな
        CrouchTransition();
        SlidingTimerCounter();
    }

    private void FixedUpdate()
    {
        Movement(); // 摩擦関係がFixedでやらないと不安
    }

    void Movement()
    {
        if (_isGround && !PlayerInput.Instance.OnJumpButton && !_jumping) GroundMove();
        else AirMove();

        _playerVelocity.y = _rb.velocity.y;
        _rb.velocity = _playerVelocity;
        _playerVelocity.y = 0;

        //Debug.Log(_playerVelocity.magnitude);
    }

    /// <summary>地上での動き</summary>
    public void GroundMove()
    {
        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;
        wishdir = transform.TransformDirection(wishdir);
        Vector2 wishdir2 = new Vector2(wishdir.x, wishdir.z);

        if (_isSliding)
        {
            CalcVector(wishdir2, _slidingFriction, _groundAcceleration, _maxSpeed * _crouchMaxSpeedRate);
            Debug.Log("isSliding");
            if (_playerVelocity.magnitude <= 5) _isSliding = false;
        }
        else if (_crouching)
        {
            Sliding(_playerVelocity.magnitude);

            if (!_isSliding)
            {
                CalcVector(wishdir2, _friction, _groundAcceleration, _maxSpeed * _crouchMaxSpeedRate);
            }
        }
        else if (PlayerInput.Instance.IsADS) // ads中は遅くなる
        {
            CalcVector(wishdir2, _friction, _groundAcceleration, _maxSpeed * _adsMoveSpeedRate);
        }
        else
        {
            CalcVector(wishdir2, _friction, _groundAcceleration, _maxSpeed);
        }
    }

    /// <summary>空中での動き</summary>
    public void AirMove()
    {
        Vector3 wishdir = PlayerInput.Instance.InputMoveVector;
        wishdir = transform.TransformDirection(wishdir);
        CalcVector(new Vector2(wishdir.x, wishdir.z), _airFriction, _airAcceleration, _airMaxSpeed);
    }

    /// <summary>ベクトルを計算する</summary>
    void CalcVector(Vector2 inputVector, float friction, float accel, float maxSpeed)
    {
        // 水平ベクトル
        Vector2 currentVector2d = new Vector2(_playerVelocity.x, _playerVelocity.z);
        // 摩擦量の計算
        float frictionMag = Mathf.Clamp(currentVector2d.magnitude, 0.0f, friction * Time.fixedDeltaTime);
        // 摩擦を加味したplayer velocity
        currentVector2d += currentVector2d.normalized * (-frictionMag);
        // 現在の射影ベクトルMag
        float currentSpeed = Vector2.Dot(currentVector2d, inputVector);
        // add量を計算
        float addSpeed = Mathf.Clamp(maxSpeed - currentSpeed, 0.0f, accel * Time.fixedDeltaTime);
        // 現在のvelocityと合わせる
        Vector2 calcVelocity = currentVector2d + inputVector * addSpeed;

        // velocityに反映
        _playerVelocity = new Vector3(calcVelocity.x, 0, calcVelocity.y);
    }

    void Jump()
    {
        if (_isGround && _readyToJump)
        {
            _readyToJump = false;
            _jumping = true;
            _isSliding = false;

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

    void ResetJump()
    {
        _readyToJump = true;
    }

    void WallJump()
    {
        if (!_onWall || _isGround) return; // 壁に接していない || 接地中
        Debug.Log("wall jump");

        _jumping = true;
        // 角度の計算
        Vector3 jumpVec = Vector3.Reflect(_wallNormalVector, transform.forward); // 反射角
        Quaternion wallQ = // 壁から見たforwardからジャンプベクトルへの角度
            Quaternion.AngleAxis((Mathf.Atan2(jumpVec.x, jumpVec.z) - Mathf.Atan2(_wallNormalVector.x, _wallNormalVector.z))
            * Mathf.Rad2Deg, Vector3.up);
        Vector3 wallLookJumpVec = wallQ * Vector3.forward; // 壁から見たジャンプベクトル
        wallLookJumpVec = new Vector3(wallLookJumpVec.x, 1, 1) * _wallJumpPower; // * 速度
        wallLookJumpVec = Quaternion.AngleAxis(Mathf.Atan2(_wallNormalVector.x, _wallNormalVector.z) * Mathf.Rad2Deg, Vector3.up) 
            * wallLookJumpVec; // world vectorに直す

        // vectorの代入
        _playerVelocity = wallLookJumpVec; // 計算用変数に代入
        _rb.velocity = wallLookJumpVec; // velocityに直接代入 y軸は直接入れないと面倒
        // cool timeは必要だろうか
    }

    /// <summary>しゃがみ状態を切り替える</summary>
    void SwitchCrouch()
    {
        if (PlayerInput.Instance.IsCrouching) // しゃがみ開始処理
        {
            _crouchDir = -1;
            _crouching = true;
            return;
        } // return切替してしゃがみ解除の処理

        _crouchDir = 1;
        _crouching = false;
        _isSliding = false;
    }

    /// <summary>updateでしゃがみの遷移をする</summary>
    void CrouchTransition()
    {
        if ((_crouchDir == 1 && _moveBodyObject.transform.localScale.y < 0.9f) || 
            (_crouchDir == -1 && _moveBodyObject.transform.localScale.y > _crouchScale.y))
        {
            _moveBodyObject.transform.localScale += new Vector3(0, (1 - _crouchScale.y) * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
            _moveBodyObject.transform.position += new Vector3(0, 0.5f * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
            _headObjct.transform.position += new Vector3(0, 0.8f * _crouchDir * Time.deltaTime / _crouchTransitionTime, 0);
        }
        else // 遷移が終了したら代入
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
        Invoke(nameof(FinishSliding), 1f); // 1s後スライディング強制解除
        _slidingTimer = _slidingCooldown;
    }

    void FinishSliding()
    {
        _isSliding = false;
    }

    /// <summary>sliding timerの計算</summary>
    void SlidingTimerCounter()
    {
        if (_slidingTimer > 0 && _readyToJump && !_crouching)
        {
            _slidingTimer -= Time.deltaTime;

            if ( _slidingTimer <= 0)
            {
                _readyToSliding = true;
            }
        }
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
        float minAngle = 180;

        for (int i = 0; i < other.contactCount; i++) // すべての接触点においてIsFloorをかける
        {
            Vector3 normal = other.contacts[i].normal; // 接している面の法線ベクトル

            if (IsFloor(normal)) // 地面として扱えるか
            {
                _isGround = true;
                _cancellingGrounded = false;
                _groundNormalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
            else
            {
                float angle = Vector3.Angle(transform.forward, normal);

                if (angle > 180 - _maxWallAngle && minAngle > angle) // 壁ジャン可能か && 最も正面か
                {
                    minAngle = angle;
                    _wallNormalVector = normal;
                    _onWall = true;
                    _cancellingOnWall = false;
                    CancelInvoke(nameof(StopOnWall));
                }
            }
        }

        // CollisionExitで法線をチェックできないため、地面／壁のキャンセルを呼び出す
        float delay = 3f; // それぞれfalseにするフレーム数
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
        PlayerInput.Instance.SetInputAction(InputType.Crouch, SwitchCrouch);
    }

    private void OnDisable()
    {
        InGameManager.Instance.UpdateAction -= ThisUpdate;
        PlayerInput.Instance.DelInputAction(InputType.Crouch, SwitchCrouch);
    }
}