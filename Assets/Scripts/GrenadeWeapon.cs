using UnityEngine;

/// <summary>
/// Vũ khí Grenade - ném grenade theo hướng chuột
/// </summary>
[System.Serializable]
public class GrenadeWeapon : IWeapon
{
    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public float throwCooldown = 1f;
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
    
    public string WeaponName => "Grenade";
    public Sprite WeaponIcon => weaponIcon;
    public WeaponType Type => WeaponType.Grenade;
    
    public GrenadeWeapon(MonoBehaviour owner, Animator animator, Transform throwPoint, Camera cam)
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
        Debug.Log("Grenade equipped");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        Debug.Log("Grenade unequipped");
    }
    
    public void Use(Vector2 direction, Vector2 position)
    {
        if (!CanUse()) return;
        
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
        owner.StartCoroutine(DelayedSpawnGrenade());
        
        lastThrowTime = Time.time;
    }
    
    public bool CanUse()
    {
        if (!isEquipped) return false;
        if (Time.time < lastThrowTime + throwCooldown) return false;
        if (isThrowing) return false;
        if (GrenadePool.Instance == null || !GrenadePool.Instance.HasAvailableGrenade()) return false;
        return true;
    }
    
    public bool IsLockingMovement()
    {
        return isThrowing;
    }
    
    private void InitializePool()
    {
        if (grenadePrefab == null)
        {
            Debug.LogError("GrenadeWeapon: Grenade Prefab chưa được gán!");
            return;
        }
        
        if (GrenadePool.Instance == null)
        {
            GameObject poolObject = new GameObject("GrenadePool");
            GrenadePool pool = poolObject.AddComponent<GrenadePool>();
            pool.SetPrefab(grenadePrefab);
            pool.InitializePool();
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
    
    private System.Collections.IEnumerator DelayedSpawnGrenade()
    {
        yield return new WaitForSeconds(spawnDelay);
        ThrowGrenadeWithCachedDirection();
    }
    
    private void ThrowGrenadeWithCachedDirection()
    {
        GameObject grenadeObj = GrenadePool.Instance.GetGrenade();
        if (grenadeObj == null) return;
        
        Grenade grenade = grenadeObj.GetComponent<Grenade>();
        if (grenade == null) return;
        
        grenade.Throw(cachedDirection, cachedPosition);
    }
}

