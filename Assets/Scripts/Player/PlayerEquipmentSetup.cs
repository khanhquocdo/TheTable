using UnityEngine;

/// <summary>
/// Script khởi tạo và setup equipment system cho player
/// Gắn script này vào GameObject Player
/// </summary>
public class PlayerEquipmentSetup : MonoBehaviour
{
    [Header("Equipment System")]
    [SerializeField] private EquipmentSystem equipmentSystem;
    
    [Header("Weapon References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private LineRenderer lineRenderer;
    
    [Header("Gun Weapon Settings")]
    [SerializeField] private float gunAttackRange = 10f;
    [SerializeField] private int gunAttackDamage = 1;
    [SerializeField] private LayerMask gunAttackLayerMask;
    [SerializeField] private float gunFireRate = 5f;
    [SerializeField] private Sprite gunIcon;
    [SerializeField] private float gunLineFadeTime = 0.15f;
    [SerializeField] private GameObject gunHitParticlePrefab;
    [SerializeField] private float gunParticleLifetime = 2f;
    
    [Header("Grenade Weapon Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private float grenadeThrowCooldown = 1f;
    [SerializeField] private float grenadeThrowAnimationDuration = 0.5f;
    [SerializeField] private float grenadeSpawnDelay = 0.3f;
    [SerializeField] private Sprite grenadeIcon;
    
    [Header("Molotov Weapon Settings")]
    [SerializeField] private GameObject molotovPrefab;
    [SerializeField] private GameObject fireAreaPrefab;
    [SerializeField] private float molotovThrowCooldown = 2f;
    [SerializeField] private float molotovThrowAnimationDuration = 0.5f;
    [SerializeField] private float molotovSpawnDelay = 0.3f;
    [SerializeField] private Sprite molotovIcon;
    
    [Header("Initial Slot Setup")]
    [SerializeField] private WeaponType slot1Weapon = WeaponType.Gun;
    [SerializeField] private WeaponType slot2Weapon = WeaponType.Grenade;
    [SerializeField] private WeaponType slot3Weapon = WeaponType.Molotov;
    
    void Start()
    {
        // Tự động tìm references nếu chưa gán
        if (equipmentSystem == null)
        {
            equipmentSystem = GetComponent<EquipmentSystem>();
            if (equipmentSystem == null)
            {
                equipmentSystem = gameObject.AddComponent<EquipmentSystem>();
            }
        }
        
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (firePoint == null && playerMovement != null)
        {
            firePoint = playerMovement.firePoint;
        }
        
        if (throwPoint == null)
        {
            throwPoint = transform;
        }
        
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        
        // Khởi tạo weapons và gán vào slots
        SetupWeapons();
    }
    
    /// <summary>
    /// Khởi tạo và gán weapons vào slots
    /// </summary>
    private void SetupWeapons()
    {
        // Tạo Gun Weapon
        GunWeapon gunWeapon = new GunWeapon(playerMovement, lineRenderer, firePoint, mainCamera)
        {
            attackRange = gunAttackRange,
            attackDamage = gunAttackDamage,
            attackLayerMask = gunAttackLayerMask,
            fireRate = gunFireRate,
            weaponIcon = gunIcon,
            lineFadeTime = gunLineFadeTime,
            hitParticlePrefab = gunHitParticlePrefab,
            particleLifetime = gunParticleLifetime
        };
        
        // Tạo Grenade Weapon
        GrenadeWeapon grenadeWeapon = new GrenadeWeapon(this, animator, throwPoint, mainCamera)
        {
            grenadePrefab = grenadePrefab,
            throwPoint = throwPoint,
            throwCooldown = grenadeThrowCooldown,
            throwAnimationDuration = grenadeThrowAnimationDuration,
            spawnDelay = grenadeSpawnDelay,
            weaponIcon = grenadeIcon
        };
        
        // Tạo Molotov Weapon
        MolotovWeapon molotovWeapon = new MolotovWeapon(this, animator, throwPoint, mainCamera)
        {
            molotovPrefab = molotovPrefab,
            fireAreaPrefab = fireAreaPrefab,
            throwPoint = throwPoint,
            throwCooldown = molotovThrowCooldown,
            throwAnimationDuration = molotovThrowAnimationDuration,
            spawnDelay = molotovSpawnDelay,
            weaponIcon = molotovIcon
        };
        
        // Gán weapons vào slots theo cấu hình
        SetWeaponToSlot(0, slot1Weapon, gunWeapon, grenadeWeapon, molotovWeapon);
        SetWeaponToSlot(1, slot2Weapon, gunWeapon, grenadeWeapon, molotovWeapon);
        SetWeaponToSlot(2, slot3Weapon, gunWeapon, grenadeWeapon, molotovWeapon);
        
        Debug.Log("Equipment System đã được khởi tạo!");
    }
    
    /// <summary>
    /// Gán weapon vào slot dựa trên WeaponType
    /// </summary>
    private void SetWeaponToSlot(int slotIndex, WeaponType weaponType, GunWeapon gun, GrenadeWeapon grenade, MolotovWeapon molotov)
    {
        IWeapon weapon = null;
        
        switch (weaponType)
        {
            case WeaponType.Gun:
                weapon = gun;
                break;
            case WeaponType.Grenade:
                weapon = grenade;
                break;
            case WeaponType.Molotov:
                weapon = molotov;
                break;
            default:
                weapon = null;
                break;
        }
        
        equipmentSystem.SetWeaponInSlot(slotIndex, weapon);
    }
}

