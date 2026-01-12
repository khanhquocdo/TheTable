using UnityEngine;

/// <summary>
/// Component xử lý bắn đạn pháo của Tank
/// </summary>
public class TankWeapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint; // Điểm bắn đạn (thường là đầu nòng pháo)
    
    [Header("Settings")]
    [SerializeField] private float fireRate = 1f; // Số viên/giây
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileDamage = 30f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float minDamage = 5f;
    [SerializeField] private bool damageThroughObstacles = false;
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    private float nextFireTime = 0f;
    private TankTurret turret;
    
    void Awake()
    {
        turret = GetComponent<TankTurret>();
        
        // Nếu không có firePoint, tạo một điểm mặc định
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector2.right * 0.5f; // Phía trước turret
            firePoint = firePointObj.transform;
        }
    }
    
    /// <summary>
    /// Bắn đạn pháo về hướng hiện tại của turret
    /// </summary>
    public bool Fire()
    {
        if (Time.time < nextFireTime)
        {
            return false; // Chưa đến lúc bắn
        }
        
        if (TankProjectilePool.Instance == null)
        {
            Debug.LogError("TankWeapon: TankProjectilePool.Instance không tồn tại!");
            return false;
        }
        
        // Lấy projectile từ pool
        GameObject projectileObj = TankProjectilePool.Instance.GetProjectile();
        if (projectileObj == null)
        {
            return false;
        }
        
        // Đặt vị trí và hướng
        projectileObj.transform.position = firePoint.position;
        
        // Lấy hướng từ turret
        Vector2 shootDirection = turret != null ? turret.GetForwardDirection() : transform.right;
        
        // Khởi tạo projectile
        TankProjectile projectile = projectileObj.GetComponent<TankProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                shootDirection,
                projectileSpeed,
                projectileDamage,
                projectileLifetime,
                explosionRadius,
                minDamage,
                damageThroughObstacles,
                playerLayer,
                obstacleLayer
            );
        }
        
        // Cập nhật thời gian bắn tiếp theo
        nextFireTime = Time.time + (1f / fireRate);
        
        return true;
    }
    
    /// <summary>
    /// Bắn đạn pháo về một hướng cụ thể
    /// </summary>
    public bool FireAtDirection(Vector2 direction)
    {
        if (Time.time < nextFireTime)
        {
            return false;
        }
        
        if (TankProjectilePool.Instance == null)
        {
            Debug.LogError("TankWeapon: TankProjectilePool.Instance không tồn tại!");
            return false;
        }
        
        // Lấy projectile từ pool
        GameObject projectileObj = TankProjectilePool.Instance.GetProjectile();
        if (projectileObj == null)
        {
            return false;
        }
        
        // Đặt vị trí và hướng
        projectileObj.transform.position = firePoint.position;
        
        // Khởi tạo projectile
        TankProjectile projectile = projectileObj.GetComponent<TankProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                direction.normalized,
                projectileSpeed,
                projectileDamage,
                projectileLifetime,
                explosionRadius,
                minDamage,
                damageThroughObstacles,
                playerLayer,
                obstacleLayer
            );
        }
        
        // Cập nhật thời gian bắn tiếp theo
        nextFireTime = Time.time + (1f / fireRate);
        
        return true;
    }
    
    /// <summary>
    /// Kiểm tra có thể bắn không (đã hết cooldown)
    /// </summary>
    public bool CanFire()
    {
        return Time.time >= nextFireTime;
    }
    
    /// <summary>
    /// Set các thông số từ TankEnemyData
    /// </summary>
    public void SetWeaponSettings(float fireRateValue, float projectileSpeedValue, 
                                   float projectileDamageValue, float projectileLifetimeValue,
                                   float explosionRadiusValue, float minDamageValue,
                                   bool canDamageThroughObstacles, LayerMask playerLayerMask, LayerMask obstacleLayerMask)
    {
        fireRate = fireRateValue;
        projectileSpeed = projectileSpeedValue;
        projectileDamage = projectileDamageValue;
        projectileLifetime = projectileLifetimeValue;
        explosionRadius = explosionRadiusValue;
        minDamage = minDamageValue;
        damageThroughObstacles = canDamageThroughObstacles;
        playerLayer = playerLayerMask;
        obstacleLayer = obstacleLayerMask;
    }
}
