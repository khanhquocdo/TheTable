using UnityEngine;

/// <summary>
/// Script cho Molotov Pickup - Player nhặt để thêm vào inventory
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MolotovPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int amount = 1; // Số lượng molotov khi nhặt
    [SerializeField] private float pickupRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private AudioClip pickupSFX;
    
    [Header("Animation Settings")]
    [SerializeField] private bool enableFloatAnimation = true;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmount = 0.2f;
    
    private Vector3 startPosition;
    private Collider2D pickupCollider;
    private bool isPickedUp = false;
    
    void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        
        // Đảm bảo collider là trigger
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
        
        // Tự động tìm SpriteRenderer nếu chưa gán
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (enableFloatAnimation && !isPickedUp)
        {
            // Animation float lên xuống
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;
        
        // Kiểm tra có phải player không
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Pickup(other.gameObject);
        }
    }
    
    /// <summary>
    /// Xử lý khi player nhặt item
    /// </summary>
    private void Pickup(GameObject player)
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("MolotovPickup: Không tìm thấy InventorySystem!");
            return;
        }
        
        // Thêm vào inventory
        int addedAmount = InventorySystem.Instance.AddItem(WeaponType.Molotov, amount);
        
        if (addedAmount > 0)
        {
            isPickedUp = true;
            
            // Spawn effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play sound
            if (pickupSFX != null)
            {
                AudioSource.PlayClipAtPoint(pickupSFX, transform.position);
            }
            
            // Ẩn sprite trước khi destroy
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            
            // Disable collider
            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }
            
            // Destroy sau một frame để đảm bảo các effect được spawn
            Destroy(gameObject, 0.1f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Vẽ pickup radius trong Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
