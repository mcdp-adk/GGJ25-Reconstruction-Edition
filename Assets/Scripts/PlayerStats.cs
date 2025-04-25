using UnityEngine;

/// <summary>
/// 角色统计数据，包含各种运动参数
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    #region Movement

    [Header("Movement")]
    [Tooltip("最大水平移动速度")]
    public float MaxSpeed = 14f;

    [Tooltip("水平加速度")]
    public float Acceleration = 120f;

    [Tooltip("地面上的减速度")]
    public float GroundDeceleration = 60f;

    [Tooltip("空中的减速度")]
    public float AirDeceleration = 30f;

    #endregion

    #region Jump

    [Header("Jump")]
    [Tooltip("跳跃初始速度")]
    public float JumpPower = 36f;

    [Tooltip("最大下落速度")]
    public float MaxFallSpeed = 40f;

    [Tooltip("下落加速度（重力）")]
    public float FallAcceleration = 110f;

    [Tooltip("提前松开跳跃键时的重力修正")]
    public float JumpEndEarlyGravityModifier = 3f;

    [Tooltip("土狼时间（离开平台后仍能跳跃的时间）")]
    public float CoyoteTime = 0.15f;

    [Tooltip("跳跃缓冲（提前按跳跃键的有效时间）")]
    public float JumpBuffer = 0.2f;

    #endregion

    #region Collision

    [Header("Collision")]
    [Tooltip("地面检测距离")]
    public float GrounderDistance = 0.05f;

    [Tooltip("地面吸附力，帮助角色在斜坡上移动")]
    public float GroundingForce = -1.5f;

    [Tooltip("检测墙体的距离")]
    public float WallDetectionDistance = 0.1f;

    [Tooltip("角色所在的层")]
    public LayerMask PlayerLayer;

    #endregion
}
