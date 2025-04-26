using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// 角色控制器，处理移动、跳跃和碰撞逻辑
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField] private PlayerStats _stats;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _collider2D;
    private PlayerInput _playerInput;

    [Header("Inputs")]
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _dashAction;

    [Header("Movement State")]
    private Vector2 _currentInput;
    private Vector2 _velocity;
    private float _time;
    private bool _cachedQueryStartInColliders;

    [Header("Jump State")]
    private bool _jumpRequested;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    [Header("Ground State")]
    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    [Header("Wall State")]

    private WallTouchState _wallTouchState;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<CapsuleCollider2D>();
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _interactAction = _playerInput.actions["Interact"];
        _dashAction = _playerInput.actions["Dash"];

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void OnEnable()
    {
        // 订阅输入事件
        _jumpAction.performed += OnJumpPerformed;
        _jumpAction.canceled += OnJumpCanceled;
        _interactAction.performed += OnInteractPerformed;
        _dashAction.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        // 取消订阅输入事件
        _jumpAction.performed -= OnJumpPerformed;
        _jumpAction.canceled -= OnJumpCanceled;
        _interactAction.performed -= OnInteractPerformed;
        _dashAction.performed -= OnDashPerformed;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    #endregion

    #region Inputs

    /// <summary>
    /// 收集当前帧的输入
    /// </summary>
    private void GatherInput()
    {
        _currentInput = _moveAction.ReadValue<Vector2>();

        // 处理输入死区
        if (Mathf.Abs(_currentInput.x) < 0.1f) _currentInput.x = 0;
        if (Mathf.Abs(_currentInput.y) < 0.3f) _currentInput.y = 0;
    }

    /// <summary>
    /// 跳跃按钮按下回调
    /// </summary>
    /// <param name="context"></param>
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        _jumpRequested = true;
        _timeJumpWasPressed = _time;
    }

    /// <summary>
    /// 跳跃按钮释放回调
    /// </summary>
    /// <param name="context"></param>
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        // 如果在空中且向上运动，提前结束跳跃
        if (!_grounded && _velocity.y > 0)
            _endedJumpEarly = true;
    }

    /// <summary>
    /// 交互按钮触发回调
    /// </summary>
    /// <param name="context"></param>
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
        {
            // 处理交互逻辑
            Debug.Log("Interact button pressed!");
        }
        else if (context.interaction is HoldInteraction)
        {
            // 处理长按交互逻辑
            Debug.Log("Interact button held!");
        }
    }

    /// <summary>
    /// 冲刺按钮触发回调
    /// </summary>
    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        // 处理冲刺逻辑
        Debug.Log("Dash button pressed!");
    }

    #endregion

    #region Collisions

    /// <summary>
    /// 检测角色与环境的碰撞
    /// </summary>
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // 检测地面和天花板
        bool groundHit = Physics2D.CapsuleCast(
            _collider2D.bounds.center,
            _collider2D.size,
            _collider2D.direction,
            0,
            Vector2.down,
            _stats.GrounderDistance,
            ~_stats.PlayerLayer);

        bool ceilingHit = Physics2D.CapsuleCast(
            _collider2D.bounds.center,
            _collider2D.size,
            _collider2D.direction,
            0,
            Vector2.up,
            _stats.GrounderDistance,
            ~_stats.PlayerLayer);

        // 检测左右墙体
        bool leftWallHit = Physics2D.CapsuleCast(
            _collider2D.bounds.center,
            _collider2D.size,
            _collider2D.direction,
            0,
            Vector2.left,
            _stats.WallDetectionDistance,
            ~_stats.PlayerLayer);

        bool rightWallHit = Physics2D.CapsuleCast(
            _collider2D.bounds.center,
            _collider2D.size,
            _collider2D.direction,
            0,
            Vector2.right,
            _stats.WallDetectionDistance,
            ~_stats.PlayerLayer);

        // 碰到天花板，停止向上移动
        if (ceilingHit) _velocity.y = Mathf.Min(0, _velocity.y);

        // 碰到左墙，停止向左移动
        if (leftWallHit) _velocity.x = Mathf.Max(0, _velocity.x);

        // 碰到右墙，停止向右移动
        if (rightWallHit) _velocity.x = Mathf.Min(0, _velocity.x);

        // 落地
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_velocity.y));
        }
        // 离开地面
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        // 与墙体接触状态更新
        WallTouchState newWallTouchState = WallTouchState.None;
        if (leftWallHit && rightWallHit)
        {
            // 同时碰到左右墙时，根据移动方向或面向方向决定优先级
            newWallTouchState = _currentInput.x < 0 || _spriteRenderer.flipX ? WallTouchState.Left : WallTouchState.Right;
        }
        else if (leftWallHit)
        {
            newWallTouchState = WallTouchState.Left;
        }
        else if (rightWallHit)
        {
            newWallTouchState = WallTouchState.Right;
        }

        if (_wallTouchState != newWallTouchState)
        {
            _wallTouchState = newWallTouchState;
            WallTouchChanged?.Invoke(_wallTouchState);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Jumping

    /// <summary>
    /// 是否有可用的缓冲跳跃
    /// </summary>
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;

    /// <summary>
    /// 是否可以使用土狼时间跳跃
    /// </summary>
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    /// <summary>
    /// 处理跳跃逻辑
    /// </summary>
    private void HandleJump()
    {
        // 检查是否提前结束跳跃
        if (!_endedJumpEarly && !_grounded && !_jumpAction.IsPressed() && _rigidbody2D.linearVelocity.y > 0)
            _endedJumpEarly = true;

        // 如果没有跳跃请求或缓冲跳跃，直接返回
        if (!_jumpRequested && !HasBufferedJump) return;

        // 如果在地面上或可以使用土狼时间，执行跳跃
        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpRequested = false;
    }

    /// <summary>
    /// 执行跳跃动作
    /// </summary>
    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _velocity.y = _stats.JumpPower;
        Jumped?.Invoke();
    }

    #endregion

    #region Horizontal

    /// <summary>
    /// 处理水平方向移动
    /// </summary>
    private void HandleDirection()
    {
        if (_currentInput.x == 0)
        {
            // 停止输入时，根据是否在地面使用不同的减速度
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            // 根据输入方向加速
            _velocity.x = Mathf.MoveTowards(
                _velocity.x,
                _currentInput.x * _stats.MaxSpeed,
                _stats.Acceleration * Time.fixedDeltaTime);

            // 根据移动方向翻转精灵
            if (_currentInput.x != 0)
                _spriteRenderer.flipX = _currentInput.x < 0;
        }
    }

    #endregion

    #region Gravity

    /// <summary>
    /// 处理垂直方向的重力
    /// </summary>
    private void HandleGravity()
    {
        if (_grounded && _velocity.y <= 0f)
        {
            // 在地面上施加一个向下的力，帮助角色在斜坡上移动
            _velocity.y = _stats.GroundingForce;
        }
        else
        {
            // 空中重力
            var inAirGravity = _stats.FallAcceleration;

            // 如果提前结束跳跃，施加更大的重力
            if (_endedJumpEarly && _velocity.y > 0)
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;

            _velocity.y = Mathf.MoveTowards(_velocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// 将计算好的速度应用到刚体
    /// </summary>
    private void ApplyMovement() => _rigidbody2D.linearVelocity = _velocity;

    #endregion

    #region Events

    /// <summary>
    /// 当角色接触地面状态改变时触发
    /// </summary>
    /// <param name="isGrounded">是否接触地面</param>
    /// <param name="velocityY">当前垂直速度</param>
    public event Action<bool, float> GroundedChanged;

    /// <summary>
    /// 当角色执行跳跃时触发
    /// </summary>
    public event Action Jumped;

    /// <summary>
    /// 当角色接触墙体状态改变时触发
    /// </summary>
    /// <param name="state">墙体接触状态</param>
    public event Action<WallTouchState> WallTouchChanged;

    #endregion

    /// <summary>
    /// 墙体接触状态枚举
    /// </summary>
    public enum WallTouchState
    {
        None,
        Left,
        Right
    }
}
