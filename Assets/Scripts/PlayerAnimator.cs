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

    }

    private void OnGroundedChanged(bool isGrounded)
    {
        // 更新动画参数
        _animator.SetBool(IsGroundedKey, isGrounded);
    }

    #endregion

    #region Animations

    /// <summary>
    /// 根据移动方向翻转精灵
    /// </summary>
    private void HandleSpriteFlipping()
    {
        // 使用PlayerController中的输入信息控制精灵翻转
        if (_player.FrameInput.x != 0)
        {
            _spriteRenderer.flipX = _player.FrameInput.x < 0;
        }
    }

    #endregion
}
