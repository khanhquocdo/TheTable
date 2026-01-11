using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu cấu hình cho Enemy
/// Dễ dàng tạo nhiều loại enemy khác nhau (shotgun, sniper, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của enemy")]
    public float moveSpeed = 3f;
    
    [Tooltip("Khoảng cách tối thiểu giữa enemy và player (nếu quá gần sẽ lùi lại)")]
    public float minDistance = 2f;
    
    [Tooltip("Khoảng cách tối đa để enemy tấn công (nếu quá xa sẽ tiến tới)")]
    public float maxAttackDistance = 8f;
    
    [Header("Detection Settings")]
    [Tooltip("Bán kính phát hiện player")]
    public float detectRadius = 10f;
    
    [Tooltip("Layer của Player để phát hiện")]
    public LayerMask playerLayer;
    
    [Tooltip("Layer của Obstacle để kiểm tra Line of Sight")]
    public LayerMask obstacleLayer;
    
    [Header("Attack Settings")]
    [Tooltip("Tốc độ bắn (số viên/giây)")]
    public float fireRate = 2f;
    
    [Tooltip("Sát thương mỗi viên đạn")]
    public float bulletDamage = 10f;
    
    [Tooltip("Tốc độ đạn")]
    public float bulletSpeed = 10f;
    
    [Tooltip("Thời gian đạn tồn tại (giây)")]
    public float bulletLifetime = 3f;
    
    [Tooltip("Góc lệch đạn tối đa (độ). Đạn sẽ bị lệch ngẫu nhiên trong phạm vi này")]
    [Range(0f, 45f)]
    public float bulletSpreadAngle = 5f;
    
    [Header("Idle/Patrol Settings")]
    [Tooltip("Có patrol không?")]
    public bool canPatrol = false;
    
    [Tooltip("Khoảng cách patrol")]
    public float patrolRadius = 5f;
    
    [Tooltip("Thời gian đứng yên tại một điểm (giây)")]
    public float idleTime = 2f;
}

