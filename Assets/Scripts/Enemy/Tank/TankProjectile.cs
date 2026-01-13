using UnityEngine;

/// <summary>
/// Đạn pháo của Tank: bay thẳng 2D, nổ khi va chạm, gây splash damage.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TankProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float minDamage = 5f;
    [SerializeField] private bool damageThroughObstacles = false;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Visual Effects")]
    [SerializeField] private GameObject explosionEffectPrefab; // Optional: Prefab cho hiệu ứng nổ

    private Rigidbody2D rb;
    private Vector2 direction;
    private float spawnTime;
    private bool isInitialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// Khởi tạo đạn pháo với các thông số.
    /// </summary>
    public void Initialize(
        Vector2 shootDirection,
        float projectileSpeed,
        float projectileDamage,
        float projectileLifetime,
        float explosionRadiusValue,
        float minDamageValue,
        bool canDamageThroughObstacles,
        LayerMask playerLayerMask,
        LayerMask obstacleLayerMask)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        lifetime = projectileLifetime;
        explosionRadius = explosionRadiusValue;
        minDamage = minDamageValue;
        damageThroughObstacles = canDamageThroughObstacles;
        playerLayer = playerLayerMask;
        obstacleLayer = obstacleLayerMask;

        spawnTime = Time.time;
        isInitialized = true;

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

        rb.velocity = direction * speed;

        if (Time.time - spawnTime >= lifetime)
        {
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isInitialized) return;

        if (IsInLayerMask(collision.gameObject.layer, playerLayer))
        {
            Explode();
            return;
        }

        if (IsInLayerMask(collision.gameObject.layer, obstacleLayer))
        {
            Explode();
            return;
        }
    }

    /// <summary>
    /// Nổ đạn và gây splash damage.
    /// </summary>
    private void Explode()
    {
        Vector2 explosionPosition = transform.position;

        // Phát audio nổ
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(AudioID.Tank_Explosion, explosionPosition);
        }

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, explosionPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (!damageThroughObstacles)
            {
                Vector2 hitPosition = hit.transform.position;
                Vector2 dirToHit = (hitPosition - explosionPosition).normalized;
                float distToHit = Vector2.Distance(explosionPosition, hitPosition);

                RaycastHit2D obstacleCheck = Physics2D.Raycast(explosionPosition, dirToHit, distToHit, obstacleLayer);
                if (obstacleCheck.collider != null)
                {
                    continue; // obstacle chặn
                }
            }

            float distance = Vector2.Distance(explosionPosition, hit.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius);
            damageMultiplier = Mathf.Clamp01(damageMultiplier);
            float finalDamage = Mathf.Lerp(minDamage, damage, damageMultiplier);

            Health targetHealth = hit.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(finalDamage);
            }
        }

        ReturnToPool();
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void ReturnToPool()
    {
        if (TankProjectilePool.Instance != null)
        {
            TankProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        isInitialized = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
