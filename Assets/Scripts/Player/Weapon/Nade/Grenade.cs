using System.Collections;
using UnityEngine;

/// <summary>
/// Script xử lý logic của từng grenade: bay, nổ và gây damage
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private float fuseTime = 2f;              // Thời gian nổ (giây)
    [SerializeField] private float throwSpeed = 15f;           // Tốc độ ném
    [SerializeField] private float explosionRadius = 5f;       // Bán kính nổ
    [SerializeField] private float maxDamage = 50f;            // Sát thương tối đa tại tâm
    [SerializeField] private float minDamage = 10f;            // Sát thương tối thiểu ở rìa
    [SerializeField] private LayerMask enemyLayer;              // Layer của kẻ địch

    [Header("Visual & Audio")]
    [SerializeField] private GameObject explosionVFXPrefab;    // Prefab hiệu ứng nổ (có thể null)
    [SerializeField] private AudioClip explosionSFX;            // Âm thanh nổ (có thể null)
    [SerializeField] private SpriteRenderer spriteRenderer;     // Sprite của grenade (để ẩn khi nổ)

    private Rigidbody2D rb;
    private bool hasExploded = false;
    private float spawnTime;
    private Camera mainCamera;

    // Events để hook vào các hệ thống khác
    public System.Action<Vector2, float> OnExploded; // position, damage

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Tắt gravity cho Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        // Tự động tìm SpriteRenderer nếu chưa gán
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void OnEnable()
    {
        // Reset trạng thái khi được kích hoạt từ pool
        hasExploded = false;
        spawnTime = Time.time;

        // Hiện lại sprite nếu đã bị ẩn
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Bắt đầu đếm ngược để nổ
        StartCoroutine(FuseCountdown());
    }

    void OnDisable()
    {
        // Dừng tất cả coroutines khi bị disable
        StopAllCoroutines();
    }

    /// <summary>
    /// Khởi tạo và ném grenade theo hướng chỉ định
    /// </summary>
    /// <param name="throwDirection">Hướng ném (đã normalized)</param>
    /// <param name="startPosition">Vị trí bắt đầu</param>
    public void Throw(Vector2 throwDirection, Vector2 startPosition)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        // Đặt velocity để grenade bay
        if (rb != null)
        {
            rb.velocity = throwDirection * throwSpeed;
        }
    }

    /// <summary>
    /// Đếm ngược thời gian nổ
    /// </summary>
    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        
        if (!hasExploded)
        {
            Explode();
        }
    }

    /// <summary>
    /// Xử lý khi grenade nổ
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return; // Tránh nổ nhiều lần

        hasExploded = true;
        Vector2 explosionPosition = transform.position;

        // Ẩn sprite của grenade
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Dừng di chuyển
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Phát hiện và gây damage cho các enemy trong bán kính
        DealDamage(explosionPosition);

        // Spawn hiệu ứng nổ
        SpawnExplosionVFX(explosionPosition);

        // Phát âm thanh nổ
        PlayExplosionSFX(explosionPosition);

        // Gọi event
        OnExploded?.Invoke(explosionPosition, maxDamage);

        // Trả về pool sau một khoảng thời gian ngắn (để VFX/SFX có thời gian phát)
        StartCoroutine(ReturnToPoolAfterDelay(0.5f));
    }

    /// <summary>
    /// Gây damage cho các enemy trong bán kính nổ
    /// </summary>
    private void DealDamage(Vector2 explosionPosition)
    {
        // Sử dụng OverlapCircle để tìm tất cả collider trong bán kính
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius, enemyLayer);

        foreach (Collider2D hitCollider in hitColliders)
        {
            // Tính khoảng cách từ tâm nổ đến enemy
            float distance = Vector2.Distance(explosionPosition, hitCollider.transform.position);
            
            // Tính damage dựa trên khoảng cách (giảm dần từ tâm ra ngoài)
            float damageMultiplier = 1f - (distance / explosionRadius);
            damageMultiplier = Mathf.Clamp01(damageMultiplier); // Đảm bảo trong khoảng [0, 1]
            
            float finalDamage = Mathf.Lerp(minDamage, maxDamage, damageMultiplier);

            // Gây damage cho enemy
            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(finalDamage);
                Debug.Log($"{hitCollider.name} nhận {finalDamage:F1} damage từ grenade (khoảng cách: {distance:F2})");
            }
        }

        // Debug: Vẽ bán kính nổ trong Scene view (sẽ hiển thị trong OnDrawGizmos)
    }

    /// <summary>
    /// Spawn hiệu ứng nổ (VFX)
    /// </summary>
    private void SpawnExplosionVFX(Vector2 position)
    {
        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, position, Quaternion.identity);
            // VFX prefab nên tự hủy sau khi phát xong
            // Nếu không, có thể thêm: Destroy(vfx, 2f);
        }
    }

    /// <summary>
    /// Phát âm thanh nổ (SFX)
    /// </summary>
    private void PlayExplosionSFX(Vector2 position)
    {
        if (explosionSFX != null && mainCamera != null)
        {
            // Sử dụng AudioSource.PlayClipAtPoint để phát âm thanh tại vị trí
            AudioSource.PlayClipAtPoint(explosionSFX, position);
        }
    }

    /// <summary>
    /// Trả grenade về pool sau một khoảng thời gian
    /// </summary>
    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GrenadePool.Instance != null)
        {
            GrenadePool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            // Fallback: nếu không có pool, destroy object
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi nổ ngay lập tức (có thể dùng cho va chạm hoặc trigger)
    /// </summary>
    public void ExplodeImmediately()
    {
        if (!hasExploded)
        {
            StopAllCoroutines();
            Explode();
        }
    }

    // Debug: Vẽ bán kính nổ trong Scene view khi đang active
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

