using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Animation Parameters")]
    private static readonly int VelocityYKey = Animator.StringToHash("VelosityY");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int IsGroundedKey = Animator.StringToHash("IsGrounded");
    private static readonly int OnWallKey = Animator.StringToHash("OnWall");

    [Header("Other Parameters")]
    private PlayerController.WallTouchState _wallTouchState = PlayerController.WallTouchState.None;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.WallStateChanged += OnWallStateChanged;
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.WallStateChanged -= OnWallStateChanged;
    }

    private void Update()
    {
        _animator.SetFloat(VelocityYKey, _player.VelocityY);

        HandleSpriteFlipping();
    }

    #endregion

    #region Event Handlers

    private void OnJumped()
    {
        // 播放跳跃动画
        _animator.SetTrigger(JumpKey);
    }

    private void OnWallStateChanged(PlayerController.WallTouchState wallTouchState)
    {
        // 更新墙壁状态
        _wallTouchState = wallTouchState;

        // 更新动画参数
        _animator.SetBool(OnWallKey, _wallTouchState != PlayerController.WallTouchState.None);
    }

    private void OnGroundedChanged(bool isGrounded)
    {
        // 更新动画参数
        _animator.SetBool(IsGroundedKey, isGrounded);
    }

    #endregion

    #region Animations

    /// <summary>
    /// 根据移动方向和墙体接触状态翻转精灵
    /// </summary>
    private void HandleSpriteFlipping()
    {
        // 如果角色正在接触墙体，根据墙体方向翻转
        if (_wallTouchState != PlayerController.WallTouchState.None)
        {
            // 左墙时面向右边（不翻转），右墙时面向左边（翻转）
            _spriteRenderer.flipX = _wallTouchState == PlayerController.WallTouchState.Right;
        }
        // 否则使用常规的输入方向控制翻转
        else if (_player.FrameInput.x != 0)
        {
            _spriteRenderer.flipX = _player.FrameInput.x < 0;
        }
    }

    #endregion
}
