using UnityEngine;

/// <summary>
/// Vũ khí C4 - ném/đặt C4 và kích nổ bằng C4Detonator
/// </summary>
[System.Serializable]
public class C4Weapon : IConsumableWeapon
{
    [Header("C4 Settings")]
    public GameObject c4Prefab;
    public Transform throwPoint;
    public float throwCooldown = 2f;
    public float throwAnimationDuration = 0.5f;
    public float spawnDelay = 0.2f;

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

    public string WeaponName => "C4";
    public Sprite WeaponIcon => weaponIcon;
    public WeaponType Type => WeaponType.C4;

    public C4Weapon(MonoBehaviour owner, Animator animator, Transform throwPoint, Camera cam)
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
    }

    public void Unequip()
    {
        isEquipped = false;
    }

    public void Use(Vector2 direction, Vector2 position)
    {
        if (!CanUse()) return;

        // Kiểm tra và giảm số lượng từ inventory
        if (InventorySystem.Instance == null || !InventorySystem.Instance.UseItem(WeaponType.C4, 1))
        {
            Debug.LogWarning("C4Weapon: Không đủ C4 để đặt!");
            return;
        }

        // Cache hướng ném
        cachedPosition = throwPoint != null ? (Vector2)throwPoint.position : position;
        cachedDirection = direction.normalized;

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("IsThrowNade");
        }

        // Lock movement và delay spawn
        owner.StartCoroutine(LockMovementDuringThrow());
        owner.StartCoroutine(DelayedSpawnC4());

        lastThrowTime = Time.time;
    }

    public bool CanUse()
    {
        if (!isEquipped) return false;
        if (Time.time < lastThrowTime + throwCooldown) return false;
        if (isThrowing) return false;
        if (C4Pool.Instance == null || !C4Pool.Instance.HasAvailableC4()) return false;

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
        return InventorySystem.Instance.GetItemAmount(WeaponType.C4);
    }

    public int GetMaxStack()
    {
        if (InventorySystem.Instance == null) return 0;
        return InventorySystem.Instance.GetMaxStack(WeaponType.C4);
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
        if (c4Prefab == null)
        {
            Debug.LogError("C4Weapon: C4 Prefab chưa được gán!");
            return;
        }

        if (C4Pool.Instance == null)
        {
            GameObject poolObject = new GameObject("C4Pool");
            C4Pool pool = poolObject.AddComponent<C4Pool>();
            pool.SetPrefab(c4Prefab);
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

    private System.Collections.IEnumerator DelayedSpawnC4()
    {
        yield return new WaitForSeconds(spawnDelay);
        PlaceC4WithCachedDirection();
    }

    private void PlaceC4WithCachedDirection()
    {
        GameObject c4Obj = C4Pool.Instance.GetC4();
        if (c4Obj == null) return;

        C4Placement placement = c4Obj.GetComponent<C4Placement>();
        if (placement == null)
        {
            Debug.LogError("C4Weapon: Không tìm thấy C4Placement trên prefab!");
            return;
        }

        placement.Place(cachedDirection, cachedPosition);
    }
}

