using UnityEngine;

/// <summary>
/// Controller xử lý input và ném grenade cho player
/// </summary>
public class GrenadeController : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private GameObject grenadePrefab;         // Prefab của grenade (dùng để khởi tạo pool)
    [SerializeField] private Transform throwPoint;             // Điểm xuất phát của grenade (nếu null thì dùng transform.position)
    [SerializeField] private float throwCooldown = 1f;          // Thời gian chờ giữa các lần ném

    [Header("Throw Settings")]
    //[SerializeField] private float throwSpeed = 15f;            // Tốc độ ném (có thể override từ Grenade.cs)
    [SerializeField] private float throwAnimationDuration = 0.5f; // Thời gian animation ném (lock movement trong khoảng này)
    [SerializeField] private float spawnDelay = 0.3f;            // Delay trước khi spawn grenade

    private Camera mainCamera;
    private float lastThrowTime = 0f;
    private bool isInitialized = false;
    private Animator ani;

    // Static flag để PlayerMovement check
    public static bool IsThrowingGrenade { get; private set; } = false;

    // Cache hướng ném để dùng sau delay
    private Vector2 cachedDirection;
    private Vector2 cachedPosition;

    void Start()
    {
        mainCamera = Camera.main;
        ani = GetComponent<Animator>();
        if (mainCamera == null)
        {
            Debug.LogError("GrenadeController: Không tìm thấy Camera! Hãy tag Camera là 'MainCamera'.");
            return;
        }

        // Khởi tạo pool nếu chưa có
        InitializePool();
        
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
    }

    /// <summary>
    /// Khởi tạo GrenadePool nếu chưa có
    /// </summary>
    private void InitializePool()
    {
        if (grenadePrefab == null)
        {
            Debug.LogError("GrenadeController: Grenade Prefab chưa được gán! Hãy gán prefab trong Inspector.");
            return;
        }

        if (GrenadePool.Instance == null)
        {
            // Tạo GameObject mới cho pool
            GameObject poolObject = new GameObject("GrenadePool");
            GrenadePool pool = poolObject.AddComponent<GrenadePool>();
            
            // Gán prefab cho pool
            pool.SetPrefab(grenadePrefab);
            
            // Khởi tạo pool sau khi gán prefab
            pool.InitializePool();
        }
        // Nếu pool đã tồn tại, giả định đã được setup đúng trong scene
    }

    /// <summary>
    /// Thử ném grenade (kiểm tra cooldown trước)
    /// </summary>
    /// <returns>True nếu ném thành công, False nếu không thể ném</returns>
    private bool TryThrowGrenade()
    {
        // Kiểm tra cooldown
        if (Time.time < lastThrowTime + throwCooldown)
        {
            return false;
        }

        // Kiểm tra đang ném thì không ném tiếp
        if (IsThrowingGrenade)
        {
            return false;
        }

        // Kiểm tra pool có sẵn không
        if (GrenadePool.Instance == null)
        {
            Debug.LogWarning("GrenadeController: GrenadePool chưa được khởi tạo!");
            return false;
        }

        if (!GrenadePool.Instance.HasAvailableGrenade())
        {
            Debug.LogWarning("GrenadeController: Không còn grenade trong pool!");
            return false;
        }

        lastThrowTime = Time.time;
        return true;
    }

    /// <summary>
    /// Cache hướng ném ngay khi nhấn chuột
    /// </summary>
    private void CacheThrowDirection()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        cachedPosition = throwPoint != null ? throwPoint.position : transform.position;
        cachedDirection = (mouseWorldPos - (Vector3)cachedPosition).normalized;
    }

    /// <summary>
    /// Delay spawn grenade sau một khoảng thời gian
    /// </summary>
    private System.Collections.IEnumerator DelayedSpawnGrenade()
    {
        yield return new WaitForSeconds(spawnDelay);
        ThrowGrenadeWithCachedDirection();
    }

    /// <summary>
    /// Ném grenade với hướng đã cache
    /// </summary>
    private void ThrowGrenadeWithCachedDirection()
    {
        GameObject grenadeObj = GrenadePool.Instance.GetGrenade();
        if (grenadeObj == null) return;

        Grenade grenade = grenadeObj.GetComponent<Grenade>();
        if (grenade == null) return;

        grenade.Throw(cachedDirection, cachedPosition);
    }

    /// <summary>
    /// Lock movement trong thời gian animation ném
    /// </summary>
    private System.Collections.IEnumerator LockMovementDuringThrow()
    {
        IsThrowingGrenade = true;
        
        // Dừng velocity của player ngay lập tức
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Chờ animation hoàn thành
        yield return new WaitForSeconds(throwAnimationDuration);

        IsThrowingGrenade = false;
    }

    /// <summary>
    /// Ném grenade theo hướng chuột
    /// </summary>
    private void ThrowGrenade()
    {
        // Lấy vị trí chuột trong world space (2D)
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Đảm bảo z = 0 cho 2D

        // Tính hướng ném từ vị trí player đến chuột
        Vector2 throwPosition = throwPoint != null ? throwPoint.position : transform.position;
        Vector2 throwDirection = (mouseWorldPos - (Vector3)throwPosition).normalized;

        // Lấy grenade từ pool
        GameObject grenadeObj = GrenadePool.Instance.GetGrenade();
        if (grenadeObj == null)
        {
            Debug.LogError("GrenadeController: Không thể lấy grenade từ pool!");
            return;
        }

        // Lấy component Grenade và ném
        Grenade grenade = grenadeObj.GetComponent<Grenade>();
        if (grenade == null)
        {
            Debug.LogError("GrenadeController: Grenade object không có component Grenade!");
            return;
        }

        grenade.Throw(throwDirection, throwPosition);
    }

    /// <summary>
    /// Ném grenade theo hướng chỉ định (có thể gọi từ code khác)
    /// </summary>
    public void ThrowGrenadeAtDirection(Vector2 direction)
    {
        if (GrenadePool.Instance == null || !GrenadePool.Instance.HasAvailableGrenade())
        {
            return;
        }

        Vector2 throwPosition = throwPoint != null ? throwPoint.position : transform.position;
        Vector2 normalizedDirection = direction.normalized;

        GameObject grenadeObj = GrenadePool.Instance.GetGrenade();
        Grenade grenade = grenadeObj?.GetComponent<Grenade>();
        
        if (grenade != null)
        {
            grenade.Throw(normalizedDirection, throwPosition);
        }
    }

    /// <summary>
    /// Ném grenade đến vị trí chỉ định
    /// </summary>
    public void ThrowGrenadeAtPosition(Vector2 targetPosition)
    {
        Vector2 throwPosition = throwPoint != null ? throwPoint.position : transform.position;
        Vector2 direction = (targetPosition - throwPosition).normalized;
        ThrowGrenadeAtDirection(direction);
    }
}

