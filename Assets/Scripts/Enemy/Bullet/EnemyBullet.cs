using UnityEngine;

/// <summary>
/// Đạn của Enemy
/// Di chuyển thẳng và gây damage khi trúng Player
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private float spawnTime;
    private bool isInitialized = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Đạn không bị trọng lực
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Phát hiện va chạm tốt hơn
    }
    
    /// <summary>
    /// Khởi tạo đạn với các thông số
    /// </summary>
    public void Initialize(Vector2 shootDirection, float bulletSpeed, float bulletDamage, float bulletLifetime, LayerMask playerLayerMask, LayerMask obstacleLayerMask)
    {
        direction = shootDirection.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        lifetime = bulletLifetime;
        playerLayer = playerLayerMask;
        obstacleLayer = obstacleLayerMask;
        
        spawnTime = Time.time;
        isInitialized = true;
        
        // Quay đạn về hướng bắn
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    void FixedUpdate()
    {
        if (!isInitialized)
        {
            return;
        }
        
        // Di chuyển đạn
        rb.velocity = direction * speed;
        
        // Kiểm tra lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isInitialized)
        {
            return;
        }
        
        // Kiểm tra trúng Player
        if (IsInLayerMask(collision.gameObject.layer, playerLayer))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            
            ReturnToPool();
            return;
        }
        
        // Kiểm tra trúng Obstacle
        if (IsInLayerMask(collision.gameObject.layer, obstacleLayer))
        {
            ReturnToPool();
            return;
        }
    }
    
    /// <summary>
    /// Kiểm tra layer có trong LayerMask không
    /// </summary>
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
    
    /// <summary>
    /// Trả đạn về pool
    /// </summary>
    private void ReturnToPool()
    {
        if (EnemyBulletPool.Instance != null)
        {
            EnemyBulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            // Nếu không có pool, destroy
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        spawnTime = Time.time;
    }
    
    void OnDisable()
    {
        // Reset khi disable
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        isInitialized = false;
    }
}



