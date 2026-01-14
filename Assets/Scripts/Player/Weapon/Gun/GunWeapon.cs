using UnityEngine;
using System.Collections;

/// <summary>
/// Vũ khí súng - bắn đạn bằng raycast với hệ thống ammo/magazine
/// </summary>
[System.Serializable]
public class GunWeapon : IWeapon, IShootableWeapon
{
    [Header("Gun Settings")]
    public float attackRange = 10f;
    public int attackDamage = 1;
    public LayerMask attackLayerMask;
    public float fireRate = 5f; // Số viên / giây
    
    [Header("Ammo Settings")]
    [Tooltip("Dữ liệu ammo của vũ khí này")]
    public WeaponAmmoData weaponAmmoData;
    
    [Header("Visual Settings")]
    public Sprite weaponIcon;
    public float lineFadeTime = 0.15f;
    public GameObject hitParticlePrefab;
    public GameObject tankHitParticlePrefab; // Particle system riêng khi bắn Tank
    public float particleLifetime = 2f;
    
    // References
    private PlayerMovement playerMovement;
    private LineRenderer lineRenderer;
    private Transform firePoint;
    private Camera mainCamera;
    private MonoBehaviour coroutineRunner; // Để chạy coroutine
    
    // State
    private float nextFireTime = 0f;
    private bool isEquipped = false;
    private bool isFiring = false;
    private Coroutine burstCoroutine;
    
    // IShootableWeapon properties
    public AmmoType AmmoType => weaponAmmoData != null ? weaponAmmoData.ammoType : AmmoType.None;
    public WeaponAmmoData WeaponAmmoData => weaponAmmoData;
    public FireMode FireMode => weaponAmmoData != null ? weaponAmmoData.fireMode : FireMode.Auto;
    
    public string WeaponName => "Gun";
    public Sprite WeaponIcon => weaponIcon;
    public WeaponType Type => WeaponType.Gun;
    
    public GunWeapon(PlayerMovement playerMovement, LineRenderer lineRenderer, Transform firePoint, Camera cam)
    {
        this.playerMovement = playerMovement;
        this.lineRenderer = lineRenderer;
        this.firePoint = firePoint;
        this.mainCamera = cam;
        this.coroutineRunner = playerMovement; // Dùng PlayerMovement để chạy coroutine
    }
    
    public void Equip()
    {
        isEquipped = true;
        
        // Khởi tạo ammo cho vũ khí này nếu chưa có
        if (weaponAmmoData != null && AmmoController.Instance != null)
        {
            AmmoController.Instance.InitializeAmmoForWeapon(weaponAmmoData);
        }
    }
    
    public void Unequip()
    {
        isEquipped = false;
        
        // Hủy reload nếu đang reload loại đạn này
        if (AmmoController.Instance != null && AmmoController.Instance.IsReloading(AmmoType))
        {
            AmmoController.Instance.CancelReload();
        }
        
        // Dừng burst nếu đang bắn burst
        if (burstCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }
        
        isFiring = false;
    }
    
    public void Use(Vector2 direction, Vector2 position)
    {
        if (!CanUse()) return;
        
        // Xử lý theo fire mode
        switch (FireMode)
        {
            case FireMode.Single:
                // Single shot: chỉ bắn một viên mỗi lần nhấn
                if (!isFiring)
                {
                    FireSingleShot(direction);
                    isFiring = true;
                }
                break;
                
            case FireMode.Auto:
                // Auto: bắn liên tục khi giữ chuột
                FireSingleShot(direction);
                break;
                
            case FireMode.Burst:
                // Burst: bắn theo chùm
                if (!isFiring && burstCoroutine == null)
                {
                    burstCoroutine = coroutineRunner.StartCoroutine(FireBurst(direction));
                }
                break;
        }
    }
    
    /// <summary>
    /// Bắn một viên đạn
    /// </summary>
    private void FireSingleShot(Vector2 direction)
    {
        // Kiểm tra ammo trước khi bắn
        if (AmmoController.Instance == null || !AmmoController.Instance.CanShoot(AmmoType))
        {
            // Không có đạn, tự động reload nếu có thể
            if (AmmoController.Instance != null && AmmoController.Instance.CanReload(AmmoType))
            {
                AmmoController.Instance.StartReload(AmmoType);
            }
            return;
        }
        
        // Consume ammo
        if (!AmmoController.Instance.ConsumeAmmo(AmmoType, 1))
        {
            return;
        }
        
        // Tính thời điểm được bắn tiếp theo
        float fireInterval = 1f / fireRate;
        nextFireTime = Time.time + fireInterval;
        
        // Phát audio bắn súng
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(AudioID.Gun_Fire);
        }
        
        // Raycast 2D từ vị trí firePoint về phía direction
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, attackRange, attackLayerMask);
        
        Vector3 startPos = firePoint.position;
        Vector3 endPos = hit.collider != null ? (Vector3)hit.point : (Vector3)(firePoint.position + (Vector3)(direction * attackRange));
        
        if (hit.collider != null)
        {
            // Gây sát thương
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
                SpawnHitParticle(hit.point, hit.normal, hit.collider.transform);
                
                // Phát audio khi bắn trúng
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAudio(AudioID.Gun_Hit, hit.point);
                }
            }
        }
        
        // Vẽ đường đạn (sử dụng method từ PlayerMovement)
        if (lineRenderer != null && playerMovement != null)
        {
            // Dừng tất cả coroutines trước để tránh overlap
            playerMovement.StopAllCoroutines();
            playerMovement.StartCoroutine(playerMovement.DrawBulletLine(startPos, endPos));
        }
    }
    
    /// <summary>
    /// Bắn burst (chùm đạn)
    /// </summary>
    private IEnumerator FireBurst(Vector2 direction)
    {
        isFiring = true;
        
        if (weaponAmmoData == null)
        {
            isFiring = false;
            burstCoroutine = null;
            yield break;
        }
        
        int burstCount = weaponAmmoData.burstCount;
        float burstInterval = weaponAmmoData.burstInterval;
        
        for (int i = 0; i < burstCount; i++)
        {
            // Kiểm tra ammo trước mỗi viên
            if (AmmoController.Instance == null || !AmmoController.Instance.CanShoot(AmmoType))
            {
                break;
            }
            
            FireSingleShot(direction);
            
            // Chờ interval trước khi bắn viên tiếp theo (trừ viên cuối)
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstInterval);
            }
        }
        
        isFiring = false;
        burstCoroutine = null;
    }
    
    public bool CanUse()
    {
        if (!isEquipped) return false;
        
        // Không thể bắn nếu đang reload
        if (AmmoController.Instance != null && AmmoController.Instance.IsReloading(AmmoType))
            return false;
        
        // Kiểm tra fire rate
        if (Time.time < nextFireTime)
            return false;
        
        // Kiểm tra ammo
        if (AmmoController.Instance != null && !AmmoController.Instance.CanShoot(AmmoType))
            return false;
        
        // Với Single mode, chỉ bắn khi mới nhấn (không giữ)
        if (FireMode == FireMode.Single && isFiring)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Được gọi khi ngừng giữ chuột (để reset Single mode)
    /// </summary>
    public void OnFireButtonReleased()
    {
        if (FireMode == FireMode.Single)
        {
            isFiring = false;
        }
    }
    
    public bool IsLockingMovement()
    {
        return false; // Gun không lock movement
    }
    
    private void SpawnHitParticle(Vector2 hitPoint, Vector2 hitNormal, Transform parentTransform = null)
    {
        // Kiểm tra nếu đối tượng bị bắn là Tank
        GameObject particlePrefab = hitParticlePrefab;
        if (parentTransform != null)
        {
            TankEnemyController tankController = parentTransform.GetComponent<TankEnemyController>();
            if (tankController != null && tankHitParticlePrefab != null)
            {
                particlePrefab = tankHitParticlePrefab;
            }
        }
        
        if (particlePrefab == null) return;
        
        GameObject particleInstance = Object.Instantiate(particlePrefab, hitPoint, Quaternion.identity);
        
        // Set particle làm child của Enemy nếu có parentTransform
        if (parentTransform != null)
        {
            particleInstance.transform.SetParent(parentTransform);
        }
        
        if (hitNormal != Vector2.zero)
        {
            float angle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
            particleInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null && !ps.main.playOnAwake)
        {
            ps.Play();
        }
        
        if (particleLifetime > 0f)
        {
            Object.Destroy(particleInstance, particleLifetime);
        }
    }
}

