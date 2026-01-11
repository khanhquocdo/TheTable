using UnityEngine;

/// <summary>
/// Vũ khí Molotov - ném molotov theo hướng chuột
/// </summary>
[System.Serializable]
public class MolotovWeapon : IConsumableWeapon
{
    [Header("Molotov Settings")]
    public GameObject molotovPrefab;
    public GameObject fireAreaPrefab;
    public Transform throwPoint;
    public float throwCooldown = 2f;
    public float throwAnimationDuration = 0.5f;
    public float spawnDelay = 0.3f;
    
    [Header("Visual Settings")]
    public Sprite weaponIcon;
    
    // References
    private MonoBehaviour owner;
    private Animator animator;
    private Camera mainCamera;
    
    // State
    private float lastThrowTime = 0f;
    private bool isEquipped = false;
    private bool isThrowing = false;
    private Vector2 cachedDirection;
    private Vector2 cachedPosition;
    
    public string WeaponName => "Molotov";
    public Sprite WeaponIcon => weaponIcon;
    public WeaponType Type => WeaponType.Molotov;
    
    public MolotovWeapon(MonoBehaviour owner, Animator animator, Transform throwPoint, Camera cam)
    {
        this.owner = owner;
        this.animator = animator;
        this.throwPoint = throwPoint;
        this.mainCamera = cam;
        
        InitializePool();
    }
    
    public void Equip()
    {
        isEquipped = true;
        Debug.Log("Molotov equipped");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        Debug.Log("Molotov unequipped");
    }
    
    public void Use(Vector2 direction, Vector2 position)
    {
        if (!CanUse()) return;
        
        // Kiểm tra và giảm số lượng từ inventory
        if (InventorySystem.Instance == null || !InventorySystem.Instance.UseItem(WeaponType.Molotov, 1))
        {
            Debug.LogWarning("MolotovWeapon: Không đủ molotov để ném!");
            return;
        }
        
        // Cache hướng ném
        cachedPosition = throwPoint != null ? throwPoint.position : position;
        cachedDirection = direction.normalized;
        
        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("IsThrowNade");
        }
        
        // Lock movement và delay spawn
        owner.StartCoroutine(LockMovementDuringThrow());
        owner.StartCoroutine(DelayedSpawnMolotov());
        
        lastThrowTime = Time.time;
    }
    
    public bool CanUse()
    {
        if (!isEquipped) return false;
        if (Time.time < lastThrowTime + throwCooldown) return false;
        if (isThrowing) return false;
        if (MolotovPool.Instance == null || !MolotovPool.Instance.HasAvailableMolotov()) return false;
        
        // Kiểm tra số lượng trong inventory
        if (InventorySystem.Instance == null || !HasAmmo())
        {
            return false;
        }
        
        return true;
    }
    
    // IConsumableWeapon implementation
    public int GetCurrentAmount()
    {
        if (InventorySystem.Instance == null) return 0;
        return InventorySystem.Instance.GetItemAmount(WeaponType.Molotov);
    }
    
    public int GetMaxStack()
    {
        if (InventorySystem.Instance == null) return 0;
        return InventorySystem.Instance.GetMaxStack(WeaponType.Molotov);
    }
    
    public bool HasAmmo()
    {
        return GetCurrentAmount() > 0;
    }
    
    public bool IsLockingMovement()
    {
        return isThrowing;
    }
    
    private void InitializePool()
    {
        if (molotovPrefab == null)
        {
            Debug.LogError("MolotovWeapon: Molotov Prefab chưa được gán!");
            return;
        }
        
        if (MolotovPool.Instance == null)
        {
            GameObject poolObject = new GameObject("MolotovPool");
            MolotovPool pool = poolObject.AddComponent<MolotovPool>();
            pool.SetMolotovPrefab(molotovPrefab);
            if (fireAreaPrefab != null)
            {
                pool.SetFireAreaPrefab(fireAreaPrefab);
            }
            pool.InitializeMolotovPool();
            if (fireAreaPrefab != null)
            {
                pool.InitializeFireAreaPool();
            }
        }
    }
    
    private System.Collections.IEnumerator LockMovementDuringThrow()
    {
        isThrowing = true;
        
        Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        yield return new WaitForSeconds(throwAnimationDuration);
        
        isThrowing = false;
    }
    
    private System.Collections.IEnumerator DelayedSpawnMolotov()
    {
        yield return new WaitForSeconds(spawnDelay);
        ThrowMolotovWithCachedDirection();
    }
    
    private void ThrowMolotovWithCachedDirection()
    {
        GameObject molotovObj = MolotovPool.Instance.GetMolotov();
        if (molotovObj == null) return;
        
        Molotov molotov = molotovObj.GetComponent<Molotov>();
        if (molotov == null) return;
        
        molotov.Throw(cachedDirection, cachedPosition);
    }
}

