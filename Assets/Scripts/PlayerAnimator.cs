using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region Fields

    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    private PlayerController _player;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        HandleSpriteFlipping();
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
