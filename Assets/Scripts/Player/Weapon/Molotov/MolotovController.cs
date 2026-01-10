using UnityEngine;

/// <summary>
/// Controller xử lý input và ném Molotov cho player
/// </summary>
public class MolotovController : MonoBehaviour
{
    [Header("Molotov Settings")]
    [SerializeField] private GameObject molotovPrefab;         // Prefab của Molotov (dùng để khởi tạo pool)
    [SerializeField] private GameObject fireAreaPrefab;        // Prefab của FireArea (dùng để khởi tạo pool)
    [SerializeField] private Transform throwPoint;             // Điểm xuất phát của Molotov (nếu null thì dùng transform.position)
    [SerializeField] private float throwCooldown = 2f;          // Thời gian chờ giữa các lần ném

    [Header("Throw Settings")]
    //[SerializeField] private float throwSpeed = 15f;           // Tốc độ ném (có thể override từ Molotov.cs)
    [SerializeField] private float throwAnimationDuration = 0.5f; // Thời gian animation ném (lock movement trong khoảng này)
    [SerializeField] private float spawnDelay = 0.3f;           // Delay trước khi spawn Molotov

    private Camera mainCamera;
    private float lastThrowTime = 0f;
    private bool isInitialized = false;
    private Animator ani;

    // Static flag để PlayerMovement check
    public static bool IsThrowingMolotov { get; private set; } = false;

    // Cache hướng ném để dùng sau delay
    private Vector2 cachedDirection;
    private Vector2 cachedPosition;

    void Start()
    {
        mainCamera = Camera.main;
        ani = GetComponent<Animator>();
        if (mainCamera == null)
        {
            Debug.LogError("MolotovController: Không tìm thấy Camera! Hãy tag Camera là 'MainCamera'.");
            return;
        }

        // Khởi tạo pool nếu chưa có
        InitializePool();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Kiểm tra input phím F
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (TryThrowMolotov())
            {
                // Cache hướng ném ngay khi nhấn phím
                CacheThrowDirection();

                // Trigger animation nếu có
                if (ani != null)
                {
                    ani.SetTrigger("IsThrowNade");
                }

                // Lock movement trong thời gian animation
                StartCoroutine(LockMovementDuringThrow());
                // Delay spawn Molotov
                StartCoroutine(DelayedSpawnMolotov());
            }
        }
    }

    /// <summary>
    /// Khởi tạo MolotovPool nếu chưa có
    /// </summary>
    private void InitializePool()
    {
        if (molotovPrefab == null)
        {
            Debug.LogError("MolotovController: Molotov Prefab chưa được gán! Hãy gán prefab trong Inspector.");
            return;
        }

        if (MolotovPool.Instance == null)
        {
            // Tạo GameObject mới cho pool
            GameObject poolObject = new GameObject("MolotovPool");
            MolotovPool pool = poolObject.AddComponent<MolotovPool>();

            // Gán prefab cho pool
            pool.SetMolotovPrefab(molotovPrefab);
            if (fireAreaPrefab != null)
            {
                pool.SetFireAreaPrefab(fireAreaPrefab);
            }

            // Khởi tạo pool sau khi gán prefab
            pool.InitializeMolotovPool();
            if (fireAreaPrefab != null)
            {
                pool.InitializeFireAreaPool();
            }
        }
        else
        {
            // Nếu pool đã tồn tại, đảm bảo nó có prefab
            Debug.Log("MolotovController: MolotovPool đã tồn tại. Đảm bảo prefab đã được gán trong Inspector.");
        }
    }

    /// <summary>
    /// Thử ném Molotov (kiểm tra cooldown trước)
    /// </summary>
    /// <returns>True nếu ném thành công, False nếu không thể ném</returns>
    private bool TryThrowMolotov()
    {
        // Kiểm tra cooldown
        if (Time.time < lastThrowTime + throwCooldown)
        {
            return false;
        }

        // Kiểm tra đang ném thì không ném tiếp
        if (IsThrowingMolotov)
        {
            return false;
        }

        // Kiểm tra pool có sẵn không
        if (MolotovPool.Instance == null)
        {
            Debug.LogWarning("MolotovController: MolotovPool chưa được khởi tạo!");
            return false;
        }

        if (!MolotovPool.Instance.HasAvailableMolotov())
        {
            Debug.LogWarning("MolotovController: Không còn Molotov trong pool!");
            return false;
        }

        lastThrowTime = Time.time;
        return true;
    }

    /// <summary>
    /// Cache hướng ném ngay khi nhấn phím
    /// </summary>
    private void CacheThrowDirection()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        cachedPosition = throwPoint != null ? throwPoint.position : transform.position;
        cachedDirection = (mouseWorldPos - (Vector3)cachedPosition).normalized;
    }

    /// <summary>
    /// Delay spawn Molotov sau một khoảng thời gian
    /// </summary>
    private System.Collections.IEnumerator DelayedSpawnMolotov()
    {
        yield return new WaitForSeconds(spawnDelay);
        ThrowMolotovWithCachedDirection();
    }

    /// <summary>
    /// Ném Molotov với hướng đã cache
    /// </summary>
    private void ThrowMolotovWithCachedDirection()
    {
        GameObject molotovObj = MolotovPool.Instance.GetMolotov();
        if (molotovObj == null) return;

        Molotov molotov = molotovObj.GetComponent<Molotov>();
        if (molotov == null) return;

        molotov.Throw(cachedDirection, cachedPosition);
    }

    /// <summary>
    /// Lock movement trong thời gian animation ném
    /// </summary>
    private System.Collections.IEnumerator LockMovementDuringThrow()
    {
        IsThrowingMolotov = true;

        // Dừng velocity của player ngay lập tức
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // Chờ animation hoàn thành
        yield return new WaitForSeconds(throwAnimationDuration);

        IsThrowingMolotov = false;
    }

    /// <summary>
    /// Ném Molotov theo hướng chỉ định (có thể gọi từ code khác)
    /// </summary>
    public void ThrowMolotovAtDirection(Vector2 direction)
    {
        if (MolotovPool.Instance == null || !MolotovPool.Instance.HasAvailableMolotov())
        {
            return;
        }

        Vector2 throwPosition = throwPoint != null ? throwPoint.position : transform.position;
        Vector2 normalizedDirection = direction.normalized;

        GameObject molotovObj = MolotovPool.Instance.GetMolotov();
        Molotov molotov = molotovObj?.GetComponent<Molotov>();

        if (molotov != null)
        {
            molotov.Throw(normalizedDirection, throwPosition);
        }
    }

    /// <summary>
    /// Ném Molotov đến vị trí chỉ định
    /// </summary>
    public void ThrowMolotovAtPosition(Vector2 targetPosition)
    {
        Vector2 throwPosition = throwPoint != null ? throwPoint.position : transform.position;
        Vector2 direction = (targetPosition - throwPosition).normalized;
        ThrowMolotovAtDirection(direction);
    }
}

