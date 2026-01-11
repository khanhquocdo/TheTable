using UnityEngine;

/// <summary>
/// Vũ khí súng - bắn đạn bằng raycast
/// </summary>
[System.Serializable]
public class GunWeapon : IWeapon
{
    [Header("Gun Settings")]
    public float attackRange = 10f;
    public int attackDamage = 1;
    public LayerMask attackLayerMask;
    public float fireRate = 5f; // Số viên / giây
    
    [Header("Visual Settings")]
    public Sprite weaponIcon;
    public float lineFadeTime = 0.15f;
    public GameObject hitParticlePrefab;
    public float particleLifetime = 2f;
    
    // References
    private PlayerMovement playerMovement;
    private LineRenderer lineRenderer;
    private Transform firePoint;
    private Camera mainCamera;
    
    // State
    private float nextFireTime = 0f;
    private bool isEquipped = false;
    
    public string WeaponName => "Gun";
    public Sprite WeaponIcon => weaponIcon;
    public WeaponType Type => WeaponType.Gun;
    
    public GunWeapon(PlayerMovement playerMovement, LineRenderer lineRenderer, Transform firePoint, Camera cam)
    {
        this.playerMovement = playerMovement;
        this.lineRenderer = lineRenderer;
        this.firePoint = firePoint;
        this.mainCamera = cam;
    }
    
    public void Equip()
    {
        isEquipped = true;
        Debug.Log("Gun equipped");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        Debug.Log("Gun unequipped");
    }
    
    public void Use(Vector2 direction, Vector2 position)
    {
        if (!CanUse()) return;
        
        // Tính thời điểm được bắn tiếp theo
        float fireInterval = 1f / fireRate;
        nextFireTime = Time.time + fireInterval;
        
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
    
    public bool CanUse()
    {
        return isEquipped && Time.time >= nextFireTime;
    }
    
    public bool IsLockingMovement()
    {
        return false; // Gun không lock movement
    }
    
    private void SpawnHitParticle(Vector2 hitPoint, Vector2 hitNormal, Transform parentTransform = null)
    {
        if (hitParticlePrefab == null) return;
        
        GameObject particleInstance = Object.Instantiate(hitParticlePrefab, hitPoint, Quaternion.identity);
        
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

