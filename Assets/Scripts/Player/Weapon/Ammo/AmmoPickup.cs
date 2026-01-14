using UnityEngine;

/// <summary>
/// Item nhặt đạn trong world
/// Khi Player va chạm sẽ cộng đạn vào Reserve Ammo
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AmmoPickup : MonoBehaviour
{
    [Header("Ammo Settings")]
    [Tooltip("Loại đạn này")]
    public AmmoType ammoType = AmmoType.Pistol;
    
    [Tooltip("Số lượng đạn sẽ được thêm vào reserve")]
    public int ammoAmount = 30;
    
    [Header("Pickup Settings")]
    [Tooltip("Có tự động nhặt khi va chạm không?")]
    public bool autoPickup = true;
    
    [Tooltip("Có hủy object sau khi nhặt không?")]
    public bool destroyOnPickup = true;
    
    [Tooltip("Thời gian delay trước khi có thể nhặt (để tránh nhặt ngay khi spawn)")]
    public float pickupDelay = 0.5f;
    
    [Header("Visual Settings")]
    [Tooltip("Sprite hiển thị của pickup (nếu không set sẽ dùng sprite của GameObject)")]
    public Sprite pickupSprite;
    
    [Tooltip("Particle effect khi nhặt")]
    public GameObject pickupEffectPrefab;
    
    [Tooltip("Audio clip khi nhặt")]
    public AudioClip pickupSound;
    
    [Header("Animation")]
    [Tooltip("Có tự động xoay không?")]
    public bool rotateOnIdle = true;
    
    [Tooltip("Tốc độ xoay (độ/giây)")]
    public float rotationSpeed = 90f;
    
    [Tooltip("Có tự động float lên xuống không?")]
    public bool floatAnimation = true;
    
    [Tooltip("Biên độ float (đơn vị Unity)")]
    public float floatAmplitude = 0.2f;
    
    [Tooltip("Tốc độ float (chu kỳ/giây)")]
    public float floatSpeed = 2f;
    
    private bool canPickup = false;
    private Vector3 startPosition;
    private float floatTimer = 0f;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set sprite nếu có
        if (pickupSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = pickupSprite;
        }
        
        // Delay trước khi có thể nhặt
        if (pickupDelay > 0f)
        {
            Invoke(nameof(EnablePickup), pickupDelay);
        }
        else
        {
            canPickup = true;
        }
        
        // Đảm bảo collider là trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (!canPickup) return;
        
        // Animation xoay
        if (rotateOnIdle)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        
        // Animation float
        if (floatAnimation)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            float offset = Mathf.Sin(floatTimer) * floatAmplitude;
            transform.position = startPosition + Vector3.up * offset;
        }
    }
    
    void EnablePickup()
    {
        canPickup = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup || !autoPickup) return;
        
        // Kiểm tra xem có phải Player không
        if (other.CompareTag("Player"))
        {
            PickupAmmo();
        }
    }
    
    /// <summary>
    /// Nhặt đạn (có thể gọi từ bên ngoài nếu không dùng auto pickup)
    /// </summary>
    public void PickupAmmo()
    {
        if (!canPickup) return;
        
        // Thêm đạn vào AmmoController
        if (AmmoController.Instance != null)
        {
            int actualAdded = AmmoController.Instance.AddAmmo(ammoType, ammoAmount);
            
            if (actualAdded > 0)
            {
                // Phát audio (tạo AudioSource tạm thời vì AudioManager dùng AudioID)
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn effect
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                
                // Hủy object
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Nếu không hủy, disable để không nhặt lại
                    canPickup = false;
                    gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("AmmoPickup: Không tìm thấy AmmoController.Instance!");
        }
    }
    
    /// <summary>
    /// Set loại đạn và số lượng (dùng khi spawn từ code)
    /// </summary>
    public void Setup(AmmoType type, int amount)
    {
        ammoType = type;
        ammoAmount = amount;
    }
    
    /// <summary>
    /// Spawn một AmmoPickup tại vị trí cụ thể
    /// </summary>
    public static GameObject SpawnAmmoPickup(Vector3 position, AmmoType ammoType, int amount, GameObject prefab = null)
    {
        GameObject pickupObj;
        
        if (prefab != null)
        {
            pickupObj = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            // Tạo GameObject mới nếu không có prefab
            pickupObj = new GameObject("AmmoPickup");
            pickupObj.transform.position = position;
            
            // Thêm SpriteRenderer
            SpriteRenderer sr = pickupObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            
            // Thêm Collider2D
            CircleCollider2D col = pickupObj.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;
            
            // Thêm AmmoPickup component
            AmmoPickup pickup = pickupObj.AddComponent<AmmoPickup>();
            pickup.ammoType = ammoType;
            pickup.ammoAmount = amount;
        }
        
        AmmoPickup pickupComponent = pickupObj.GetComponent<AmmoPickup>();
        if (pickupComponent != null)
        {
            pickupComponent.Setup(ammoType, amount);
        }
        
        return pickupObj;
    }
}
