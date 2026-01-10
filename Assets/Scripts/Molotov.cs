using System.Collections;
using UnityEngine;

/// <summary>
/// Script xử lý logic của từng Molotov: bay, vỡ và tạo vùng lửa
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Molotov : MonoBehaviour
{
    [Header("Molotov Settings")]
    [SerializeField] private float throwSpeed = 15f;           // Tốc độ ném
    [SerializeField] private float maxFlightTime = 3f;         // Thời gian bay tối đa trước khi tự vỡ
    [SerializeField] private LayerMask wallLayer;              // Layer của tường (để detect va chạm)
    [SerializeField] private LayerMask enemyLayer;              // Layer của enemy (để detect va chạm)

    [Header("Fire Area Settings")]
    [SerializeField] private GameObject fireAreaPrefab;          // Prefab của vùng lửa
    [SerializeField] private float fireRadius = 3f;            // Bán kính vùng lửa
    [SerializeField] private float fireDuration = 5f;           // Thời gian tồn tại vùng lửa (giây)

    [Header("Visual & Audio")]
    [SerializeField] private GameObject breakVFXPrefab;         // Prefab hiệu ứng vỡ chai (có thể null)
    [SerializeField] private AudioClip breakSFX;                // Âm thanh vỡ chai (có thể null)
    [SerializeField] private SpriteRenderer spriteRenderer;     // Sprite của molotov (để ẩn khi vỡ)

    private Rigidbody2D rb;
    private bool hasBroken = false;
    private float spawnTime;
    private Camera mainCamera;

    // Events để hook vào các hệ thống khác
    public System.Action<Vector2> OnBroken; // position

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
        hasBroken = false;
        spawnTime = Time.time;

        // Hiện lại sprite nếu đã bị ẩn
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Bắt đầu đếm ngược để tự vỡ sau thời gian
        StartCoroutine(FlightTimer());
    }

    void OnDisable()
    {
        // Dừng tất cả coroutines khi bị disable
        StopAllCoroutines();
    }

    /// <summary>
    /// Khởi tạo và ném Molotov theo hướng chỉ định
    /// </summary>
    /// <param name="throwDirection">Hướng ném (đã normalized)</param>
    /// <param name="startPosition">Vị trí bắt đầu</param>
    public void Throw(Vector2 throwDirection, Vector2 startPosition)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        // Đặt velocity để Molotov bay
        if (rb != null)
        {
            rb.velocity = throwDirection * throwSpeed;
        }
    }

    /// <summary>
    /// Đếm ngược thời gian bay, tự vỡ sau maxFlightTime
    /// </summary>
    private IEnumerator FlightTimer()
    {
        yield return new WaitForSeconds(maxFlightTime);
        
        if (!hasBroken)
        {
            Break();
        }
    }

    /// <summary>
    /// Xử lý va chạm với tường hoặc enemy
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBroken) return;

        // Kiểm tra nếu va chạm với tường hoặc enemy
        int otherLayer = 1 << other.gameObject.layer;
        if ((wallLayer.value & otherLayer) != 0 || (enemyLayer.value & otherLayer) != 0)
        {
            Break();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasBroken) return;

        // Kiểm tra nếu va chạm với tường hoặc enemy
        int otherLayer = 1 << collision.gameObject.layer;
        if ((wallLayer.value & otherLayer) != 0 || (enemyLayer.value & otherLayer) != 0)
        {
            Break();
        }
    }

    /// <summary>
    /// Xử lý khi Molotov vỡ
    /// </summary>
    private void Break()
    {
        if (hasBroken) return; // Tránh vỡ nhiều lần

        hasBroken = true;
        Vector2 breakPosition = transform.position;

        // Ẩn sprite của Molotov
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Dừng di chuyển
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Tạo vùng lửa
        CreateFireArea(breakPosition);

        // Spawn hiệu ứng vỡ chai
        SpawnBreakVFX(breakPosition);

        // Phát âm thanh vỡ chai
        PlayBreakSFX(breakPosition);

        // Gọi event
        OnBroken?.Invoke(breakPosition);

        // Trả về pool sau một khoảng thời gian ngắn (để VFX/SFX có thời gian phát)
        StartCoroutine(ReturnToPoolAfterDelay(0.5f));
    }

    /// <summary>
    /// Tạo vùng lửa tại vị trí vỡ
    /// </summary>
    private void CreateFireArea(Vector2 position)
    {
        GameObject fireAreaObj = null;

        // Ưu tiên lấy từ pool nếu có
        if (MolotovPool.Instance != null && MolotovPool.Instance.HasAvailableFireArea())
        {
            fireAreaObj = MolotovPool.Instance.GetFireArea();
        }
        // Nếu không có pool hoặc pool rỗng, tạo mới từ prefab
        else if (fireAreaPrefab != null)
        {
            fireAreaObj = Instantiate(fireAreaPrefab);
        }

        if (fireAreaObj == null)
        {
            Debug.LogError("Molotov: Không thể tạo FireArea! Hãy đảm bảo FireArea Prefab đã được gán hoặc MolotovPool đã được setup.");
            return;
        }

        FireArea fireArea = fireAreaObj.GetComponent<FireArea>();
        if (fireArea == null)
        {
            Debug.LogError("Molotov: FireArea object không có component FireArea!");
            return;
        }

        // Kích hoạt vùng lửa
        fireArea.Activate(position, fireRadius, fireDuration);
    }

    /// <summary>
    /// Spawn hiệu ứng vỡ chai (VFX)
    /// </summary>
    private void SpawnBreakVFX(Vector2 position)
    {
        if (breakVFXPrefab != null)
        {
            GameObject vfx = Instantiate(breakVFXPrefab, position, Quaternion.identity);
            // VFX prefab nên tự hủy sau khi phát xong
            // Nếu không, có thể thêm: Destroy(vfx, 2f);
        }
    }

    /// <summary>
    /// Phát âm thanh vỡ chai (SFX)
    /// </summary>
    private void PlayBreakSFX(Vector2 position)
    {
        if (breakSFX != null && mainCamera != null)
        {
            // Sử dụng AudioSource.PlayClipAtPoint để phát âm thanh tại vị trí
            AudioSource.PlayClipAtPoint(breakSFX, position);
        }
    }

    /// <summary>
    /// Trả Molotov về pool sau một khoảng thời gian
    /// </summary>
    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (MolotovPool.Instance != null)
        {
            MolotovPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            // Fallback: nếu không có pool, destroy object
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi vỡ ngay lập tức (có thể dùng cho trigger hoặc code khác)
    /// </summary>
    public void BreakImmediately()
    {
        if (!hasBroken)
        {
            StopAllCoroutines();
            Break();
        }
    }

    // Debug: Vẽ bán kính vùng lửa trong Scene view khi đang active
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRadius);
    }
}

