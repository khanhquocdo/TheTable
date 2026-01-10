using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script xử lý vùng lửa: damage over time cho enemy trong vùng
/// </summary>
public class FireArea : MonoBehaviour
{
    [Header("Fire Area Settings")]
    [SerializeField] private float damagePerTick = 5f;         // Sát thương mỗi tick
    [SerializeField] private float tickInterval = 0.5f;        // Thời gian giữa các tick (giây)
    [SerializeField] private LayerMask enemyLayer;              // Layer của enemy
    [SerializeField] private float fireRadius = 3f;            // Bán kính vùng lửa (có thể override khi activate)

    [Header("Visual & Audio")]
    [SerializeField] private GameObject fireVFXPrefab;          // Prefab hiệu ứng lửa (có thể null)
    [SerializeField] private AudioClip fireSFX;                 // Âm thanh lửa (có thể null)
    [SerializeField] private AudioClip burningSFX;              // Âm thanh cháy (có thể null, loop)

    private CircleCollider2D areaCollider;
    private bool isActive = false;
    private float activationTime;
    private float duration;
    private GameObject fireVFXInstance;
    private AudioSource audioSource;
    private HashSet<Health> enemiesInArea = new HashSet<Health>(); // Track enemies trong vùng

    // Events để hook vào các hệ thống khác
    public System.Action<Vector2> OnFireAreaActivated; // position
    public System.Action<Vector2> OnFireAreaExpired;   // position
    public System.Action<Health, float> OnEnemyDamaged; // enemy, damage

    void Awake()
    {
        // Tạo hoặc lấy CircleCollider2D để detect enemy
        areaCollider = GetComponent<CircleCollider2D>();
        if (areaCollider == null)
        {
            areaCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        areaCollider.isTrigger = true;
        areaCollider.radius = fireRadius;

        // Tạo AudioSource nếu cần
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (fireSFX != null || burningSFX != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    void OnEnable()
    {
        // Reset trạng thái khi được kích hoạt từ pool
        isActive = false;
        enemiesInArea.Clear();
    }

    void OnDisable()
    {
        // Dừng tất cả coroutines khi bị disable
        StopAllCoroutines();
        
        // Dọn dẹp VFX và SFX
        CleanupVFX();
        CleanupSFX();
    }

    /// <summary>
    /// Kích hoạt vùng lửa tại vị trí chỉ định
    /// </summary>
    /// <param name="position">Vị trí kích hoạt</param>
    /// <param name="radius">Bán kính vùng lửa</param>
    /// <param name="duration">Thời gian tồn tại (giây)</param>
    public void Activate(Vector2 position, float radius, float duration)
    {
        // Đảm bảo GameObject active trước khi start coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        transform.position = position;
        fireRadius = radius;
        this.duration = duration;
        activationTime = Time.time;
        isActive = true;

        // Cập nhật collider radius
        if (areaCollider != null)
        {
            areaCollider.radius = fireRadius;
        }

        // Spawn VFX
        SpawnFireVFX(position);

        // Phát SFX
        PlayFireSFX();

        // Bắt đầu damage coroutine
        StartCoroutine(DamageOverTimeCoroutine());

        // Tự động tắt sau duration
        StartCoroutine(ExpireAfterDuration());

        // Gọi event
        OnFireAreaActivated?.Invoke(position);
    }

    /// <summary>
    /// Coroutine gây damage theo thời gian cho enemy trong vùng
    /// </summary>
    private IEnumerator DamageOverTimeCoroutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(tickInterval);

            if (!isActive) break;

            // Sử dụng OverlapCircle để tìm tất cả enemy trong bán kính
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, fireRadius, enemyLayer);

            // Clear và cập nhật danh sách enemy
            enemiesInArea.Clear();
            foreach (Collider2D hitCollider in hitColliders)
            {
                Health health = hitCollider.GetComponent<Health>();
                if (health != null && !health.IsDead)
                {
                    enemiesInArea.Add(health);
                }
            }

            // Gây damage cho tất cả enemy trong vùng
            foreach (Health enemyHealth in enemiesInArea)
            {
                if (enemyHealth != null && !enemyHealth.IsDead)
                {
                    enemyHealth.TakeDamage(damagePerTick);
                    OnEnemyDamaged?.Invoke(enemyHealth, damagePerTick);
                }
            }
        }
    }

    /// <summary>
    /// Tự động tắt vùng lửa sau duration
    /// </summary>
    private IEnumerator ExpireAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        
        if (isActive)
        {
            Deactivate();
        }
    }

    /// <summary>
    /// Tắt vùng lửa và trả về pool
    /// </summary>
    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        Vector2 position = transform.position;

        // Dọn dẹp
        CleanupVFX();
        CleanupSFX();
        enemiesInArea.Clear();

        // Gọi event
        OnFireAreaExpired?.Invoke(position);

        // Trả về pool
        if (MolotovPool.Instance != null)
        {
            MolotovPool.Instance.ReturnFireAreaToPool(gameObject);
        }
        else
        {
            // Fallback: nếu không có pool, destroy object
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawn hiệu ứng lửa (VFX)
    /// </summary>
    private void SpawnFireVFX(Vector2 position)
    {
        if (fireVFXPrefab != null)
        {
            fireVFXInstance = Instantiate(fireVFXPrefab, position, Quaternion.identity, transform);
            // VFX prefab nên tự hủy sau khi phát xong hoặc được quản lý bởi FireArea
        }
    }

    /// <summary>
    /// Phát âm thanh lửa (SFX)
    /// </summary>
    private void PlayFireSFX()
    {
        if (audioSource == null) return;

        // Phát âm thanh lửa một lần
        if (fireSFX != null)
        {
            AudioSource.PlayClipAtPoint(fireSFX, transform.position);
        }

        // Phát âm thanh cháy loop
        if (burningSFX != null)
        {
            audioSource.clip = burningSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Dọn dẹp VFX
    /// </summary>
    private void CleanupVFX()
    {
        if (fireVFXInstance != null)
        {
            Destroy(fireVFXInstance);
            fireVFXInstance = null;
        }
    }

    /// <summary>
    /// Dọn dẹp SFX
    /// </summary>
    private void CleanupSFX()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // Debug: Vẽ bán kính vùng lửa trong Scene view khi đang active
    void OnDrawGizmosSelected()
    {
        if (isActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fireRadius);
        }
    }

    void OnDrawGizmos()
    {
        if (isActive)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Màu cam mờ
            Gizmos.DrawSphere(transform.position, fireRadius);
        }
    }
}

