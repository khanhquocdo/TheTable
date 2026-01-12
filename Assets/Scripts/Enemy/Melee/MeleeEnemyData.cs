using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu cấu hình cho Melee Enemy
/// Dễ dàng tạo nhiều loại melee enemy khác nhau (fast, tank, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Melee Enemy Data", menuName = "Enemy/Melee Enemy Data")]
public class MeleeEnemyData : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của enemy")]
    public float moveSpeed = 3f;
    
    [Header("Detection Settings")]
    [Tooltip("Bán kính phát hiện player")]
    public float detectRadius = 10f;
    
    [Tooltip("Layer của Player để phát hiện")]
    public LayerMask playerLayer;
    
    [Tooltip("Layer của Obstacle để kiểm tra Line of Sight")]
    public LayerMask obstacleLayer;
    
    [Header("Attack Settings")]
    [Tooltip("Khoảng cách tấn công (bán kính hitbox)")]
    public float attackRange = 1.5f;
    
    [Tooltip("Sát thương mỗi đòn tấn công")]
    public float attackDamage = 20f;
    
    [Tooltip("Thời gian wind-up trước khi gây damage (giây)")]
    public float windUpTime = 0.3f;
    
    [Tooltip("Thời gian cooldown giữa các đòn tấn công (giây)")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("Thời gian animation tấn công (giây)")]
    public float attackDuration = 0.5f;
    
    [Header("Idle/Patrol Settings")]
    [Tooltip("Có patrol không?")]
    public bool canPatrol = false;
    
    [Tooltip("Khoảng cách patrol")]
    public float patrolRadius = 5f;
    
    [Tooltip("Thời gian đứng yên tại một điểm (giây)")]
    public float idleTime = 2f;
}
