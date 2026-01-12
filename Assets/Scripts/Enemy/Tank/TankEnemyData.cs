using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu cấu hình cho Tank Enemy
/// Dễ dàng tạo nhiều loại tank khác nhau (light tank, heavy tank, etc.)
/// </summary>
[CreateAssetMenu(fileName = "New Tank Enemy Data", menuName = "Enemy/Tank Enemy Data")]
public class TankEnemyData : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển của tank (chậm)")]
    public float moveSpeed = 2f;
    
    [Tooltip("Tốc độ xoay thân xe (độ/giây)")]
    public float bodyRotationSpeed = 180f;
    
    [Header("Detection Settings")]
    [Tooltip("Bán kính phát hiện player")]
    public float detectRadius = 15f;
    
    [Tooltip("Layer của Player để phát hiện")]
    public LayerMask playerLayer;
    
    [Tooltip("Layer của Obstacle để kiểm tra Line of Sight")]
    public LayerMask obstacleLayer;
    
    [Header("Combat Settings")]
    [Tooltip("Khoảng cách tối ưu để bắn (tank sẽ di chuyển đến khoảng cách này)")]
    public float optimalAttackDistance = 8f;
    
    [Tooltip("Khoảng cách tối thiểu để bắn (nếu quá gần sẽ lùi lại)")]
    public float minAttackDistance = 5f;
    
    [Tooltip("Khoảng cách tối đa để bắn (nếu quá xa sẽ tiến tới)")]
    public float maxAttackDistance = 12f;
    
    [Header("Turret Settings")]
    [Tooltip("Tốc độ xoay tháp pháo (độ/giây)")]
    public float turretRotationSpeed = 360f;
    
    [Header("Weapon Settings")]
    [Tooltip("Tốc độ bắn (số viên/giây)")]
    public float fireRate = 1f;
    
    [Tooltip("Sát thương đạn pháo (damage tại tâm nổ)")]
    public float projectileDamage = 30f;
    
    [Tooltip("Tốc độ đạn pháo")]
    public float projectileSpeed = 8f;
    
    [Tooltip("Thời gian đạn tồn tại (giây)")]
    public float projectileLifetime = 5f;
    
    [Header("Projectile Explosion Settings")]
    [Tooltip("Bán kính nổ (splash damage)")]
    public float explosionRadius = 3f;
    
    [Tooltip("Sát thương tối thiểu (ở rìa explosion radius)")]
    public float minDamage = 5f;
    
    [Tooltip("Có thể gây damage qua obstacle không?")]
    public bool damageThroughObstacles = false;
    
    [Header("Idle Settings")]
    [Tooltip("Thời gian đứng yên tại một điểm (giây)")]
    public float idleTime = 2f;
}
