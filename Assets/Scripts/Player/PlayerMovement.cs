using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 movement;
    public float deadZone = 0.1f;

    private Animator ani;
    public Camera cam;

    // Health System
    private Health health;

    [Header("Attack Settings")]
    public float attackRange = 10f;          // Tầm bắn raycast
    public int attackDamage = 1;             // Sát thương gây ra (tuỳ bạn xử lý)
    public LayerMask attackLayerMask;        // Layer kẻ địch / vật thể có thể trúng đạn
    public float fireRate = 5f;              // Số viên / giây (5 nghĩa là tối đa 5 phát/giây)

    [Header("Bullet Line Settings")]

    public float lineFadeTime = 0.15f;       // Thời gian mờ dần
    public float lineSpeed = 30f;     // Khoảng cách tối đa để vẽ đường đạn

    [Header("Hit Effect Settings")]
    public GameObject hitParticlePrefab;   // Particle System prefab khi bắn trúng
    public GameObject tankHitParticlePrefab; // Particle System prefab riêng khi bắn Tank
    public float particleLifetime = 2f;      // Thời gian tồn tại của particle (nếu prefab không tự hủy)

    private float nextFireTime = 0f;         // Thời gian được phép bắn tiếp theo
    private LineRenderer lineRenderer;       // Vẽ đường đạn
    public Transform firePoint;
    public Vector3 firePointOffset;

    [Header("Audio Settings")]
    [Tooltip("Thời gian giữa 2 tiếng bước chân khi đang di chuyển")]
    public float footstepInterval = 0.35f;
    private float footstepTimer = 0f;
    private bool wasMoving = false; // Track trạng thái di chuyển trước đó

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        lineRenderer = GetComponent<LineRenderer>();
        health = GetComponent<Health>();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
        }
    }

    void Start()
    {
        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath += OnPlayerDeath;
            health.OnDamageTaken += OnPlayerDamageTaken;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from health events
        if (health != null)
        {
            health.OnDeath -= OnPlayerDeath;
            health.OnDamageTaken -= OnPlayerDamageTaken;
        }
    }

    void Update()
    {
        // Không cho phép di chuyển nếu player đã chết
        if (health != null && health.IsDead)
        {
            movement = Vector2.zero;
            rb.velocity = Vector2.zero;
            return;
        }

        // Kiểm tra nếu weapon hiện tại đang lock movement
        bool isLockingMovement = false;
        var equipmentSystem = GetEquipmentSystem();
        if (equipmentSystem != null)
        {
            var method = equipmentSystem.GetType().GetMethod("IsCurrentWeaponLockingMovement");
            if (method != null)
            {
                isLockingMovement = (bool)method.Invoke(equipmentSystem, null);
            }
        }

        // Fallback: Kiểm tra nếu đang ném grenade hoặc molotov (cho tương thích ngược)
        if (GrenadeController.IsThrowingGrenade || MolotovController.IsThrowingMolotov)
        {
            isLockingMovement = true;
        }

        if (isLockingMovement)
        {
            movement = Vector2.zero;
            //ani.SetFloat("Speed", 0f);
            return;
        }

        // Lấy input 8 hướng (WASD / Arrow)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        // Chuẩn hóa vector để đi chéo không nhanh hơn
        movement = movementInput.normalized;
        float speed = movement.magnitude;
        ani.SetFloat("Speed", speed);

        bool isMoving = speed > 0.01f;
        bool isHoldingAttack = Input.GetMouseButton(0);

        // ===== ATTACK STATES =====
        // Kiểm tra xem có nên lock animation attack không (khi đang reload hoặc hết đạn ở slot Gun)
        bool shouldLockAttackAnimation = ShouldLockAttackAnimation();
        
        bool isAttackRun = !shouldLockAttackAnimation && isMoving && isHoldingAttack;
        bool isAttackIdle = !shouldLockAttackAnimation && !isMoving && !isHoldingAttack;

        ani.SetBool("IsAttackRun", isAttackRun);
        ani.SetBool("IsAttackIdle", isAttackIdle);

        // Audio bước chân
        HandleFootstepAudio(isMoving);
        CalculateAnimation();

        // Sử dụng EquipmentSystem nếu có, nếu không thì dùng logic cũ
        var equipmentSystemForAttack = GetEquipmentSystem();
        if (equipmentSystemForAttack != null)
        {
            HandleWeaponAttack();
        }
        else
        {
            HandleShooting();
        }
    }

    // Helper method để tránh lỗi compile khi EquipmentSystem chưa được compile
    private MonoBehaviour GetEquipmentSystem()
    {
        Type equipmentSystemType = Type.GetType("EquipmentSystem");
        if (equipmentSystemType != null)
        {
            return FindObjectOfType(equipmentSystemType) as MonoBehaviour;
        }
        return null;
    }
    void FixedUpdate()
    {
        // Kiểm tra nếu weapon hiện tại đang lock movement
        bool isLockingMovement = false;
        var equipmentSystem = GetEquipmentSystem();
        if (equipmentSystem != null)
        {
            var method = equipmentSystem.GetType().GetMethod("IsCurrentWeaponLockingMovement");
            if (method != null)
            {
                isLockingMovement = (bool)method.Invoke(equipmentSystem, null);
            }
        }

        // Fallback: Kiểm tra nếu đang ném grenade hoặc molotov (cho tương thích ngược)
        if (GrenadeController.IsThrowingGrenade || MolotovController.IsThrowingMolotov)
        {
            isLockingMovement = true;
        }

        if (isLockingMovement)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = movement * moveSpeed;
    }

    void CalculateAnimation()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDirection = mouseWorld - transform.position;
        lookDirection.Normalize();

        if (lookDirection.magnitude < deadZone)
        {
            return;
        }

        ani.SetFloat("LookX", lookDirection.x);
        ani.SetFloat("LookY", lookDirection.y);
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);

        firePoint.localPosition = lookDirection * firePointOffset.magnitude;
    }

    // Track trạng thái chuột để xử lý Single mode
    private bool wasHoldingFireButton = false;
    
    /// <summary>
    /// Xử lý tấn công bằng weapon hiện tại từ EquipmentSystem
    /// </summary>
    void HandleWeaponAttack()
    {
        var equipmentSystem = GetEquipmentSystem();
        if (equipmentSystem == null)
        {
            return;
        }

        var currentWeaponProp = equipmentSystem.GetType().GetProperty("CurrentWeapon");
        if (currentWeaponProp == null || currentWeaponProp.GetValue(equipmentSystem) == null)
        {
            return;
        }
        
        var currentWeapon = currentWeaponProp.GetValue(equipmentSystem);
        bool isHoldingFireButton = Input.GetMouseButton(0);
        
        // Xử lý Single mode: reset khi thả chuột
        if (wasHoldingFireButton && !isHoldingFireButton)
        {
            // Gọi OnFireButtonReleased nếu weapon có method này
            var onFireButtonReleasedMethod = currentWeapon.GetType().GetMethod("OnFireButtonReleased");
            if (onFireButtonReleasedMethod != null)
            {
                onFireButtonReleasedMethod.Invoke(currentWeapon, null);
            }
        }
        
        wasHoldingFireButton = isHoldingFireButton;
        
        // Xử lý reload (phím R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            HandleReload();
            return;
        }
        
        // Giữ chuột trái để tấn công
        if (!isHoldingFireButton)
        {
            return;
        }

        // Tính hướng tấn công từ player đến chuột
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;
        direction.Normalize();

        // Lấy vị trí xuất phát
        Vector2 attackPosition = firePoint != null ? firePoint.position : transform.position;

        // Sử dụng weapon hiện tại
        var useMethod = equipmentSystem.GetType().GetMethod("UseCurrentWeapon");
        if (useMethod != null)
        {
            useMethod.Invoke(equipmentSystem, new object[] { direction, attackPosition });
        }
    }
    
    /// <summary>
    /// Kiểm tra xem có nên lock animation attack không
    /// Lock khi đang reload hoặc hết đạn và đang ở slot Gun
    /// </summary>
    bool ShouldLockAttackAnimation()
    {
        var equipmentSystem = GetEquipmentSystem();
        if (equipmentSystem == null) return false;
        
        var currentWeaponProp = equipmentSystem.GetType().GetProperty("CurrentWeapon");
        if (currentWeaponProp == null) return false;
        
        var currentWeapon = currentWeaponProp.GetValue(equipmentSystem);
        
        // Chỉ lock khi đang ở slot Gun
        if (currentWeapon is IShootableWeapon shootableWeapon)
        {
            AmmoType ammoType = shootableWeapon.AmmoType;
            
            if (AmmoController.Instance == null) return false;
            
            // Lock nếu đang reload
            if (AmmoController.Instance.IsReloading(ammoType))
            {
                return true;
            }
            
            // Lock nếu hết đạn (không có đạn trong băng và không có đạn dự trữ)
            int magazine = AmmoController.Instance.GetCurrentMagazine(ammoType);
            int reserve = AmmoController.Instance.GetCurrentReserve(ammoType);
            
            if (magazine == 0 && reserve == 0)
            {
                return true;
            }
            
            // Lock nếu hết đạn trong băng và không thể reload (không có đạn dự trữ)
            if (magazine == 0 && !AmmoController.Instance.CanReload(ammoType))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Xử lý reload cho weapon hiện tại
    /// </summary>
    void HandleReload()
    {
        var equipmentSystem = GetEquipmentSystem();
        if (equipmentSystem == null) return;
        
        var currentWeaponProp = equipmentSystem.GetType().GetProperty("CurrentWeapon");
        if (currentWeaponProp == null) return;
        
        var currentWeapon = currentWeaponProp.GetValue(equipmentSystem);
        
        // Kiểm tra xem weapon có phải IShootableWeapon không
        if (currentWeapon is IShootableWeapon shootableWeapon)
        {
            if (AmmoController.Instance != null)
            {
                AmmoController.Instance.StartReload(shootableWeapon.AmmoType);
            }
        }
    }

    /// <summary>
    /// Xử lý bắn đạn bằng Raycast theo hướng trỏ chuột + fire rate (logic cũ, dùng khi không có EquipmentSystem)
    /// </summary>
    void HandleShooting()
    {
        // Giữ chuột trái để bắn, nhưng bị giới hạn bởi fireRate
        if (Input.GetMouseButton(0) == false)
        {
            return;
        }

        // Giới hạn tốc độ bắn: chỉ bắn khi Time.time >= nextFireTime
        if (Time.time < nextFireTime)
        {
            return;
        }

        // Tính thời điểm được bắn tiếp theo
        float fireInterval = 1f / fireRate; // fireRate = số viên/giây
        nextFireTime = Time.time + fireInterval;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;
        direction.Normalize();

        // Raycast 2D từ vị trí nhân vật về phía chuột
        RaycastHit2D hit = Physics2D.Raycast(firePoint.transform.position, direction, attackRange, attackLayerMask);

        Vector3 startPos = firePoint.transform.position;
        Vector3 endPos = hit.collider != null ? (Vector3)hit.point : (Vector3)(Vector2)firePoint.transform.position + (Vector3)(direction * attackRange);

        if (hit.collider != null)
        {
            Debug.Log("Bắn trúng: " + hit.collider.name);

            // Gây sát thương cho đối tượng bị bắn trúng
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);

                // Spawn particle system tại vị trí bắn trúng và set làm child của Enemy
                SpawnHitParticle(hit.point, hit.normal, hit.collider.transform);
            }
        }

        // Vẽ đường đạn bằng LineRenderer (nếu có)
        if (lineRenderer != null)
        {
            // Mỗi lần bắn sẽ reset lại màu về startColor ban đầu
            StopAllCoroutines();
            StartCoroutine(DrawBulletLine(startPos, endPos));
        }
        else
        {
            // Fallback: chỉ vẽ debug ray nếu chưa gắn LineRenderer
            Debug.DrawRay(startPos, direction * attackRange, lineRenderer.material.color, lineFadeTime);
        }
    }

    /// <summary>
    /// Vẽ đường đạn bằng LineRenderer (public để GunWeapon có thể gọi)
    /// </summary>
    public IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;

        lineRenderer.enabled = true;
        Vector3 direction = end - start;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Thiết lập màu ban đầu (luôn reset alpha = 1 mỗi lần bắn)
        Color startColor = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, 1f);
        Color endColor = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, 1f);
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        float t = 0f;
        while (t < lineFadeTime)
        {
            // Cần hạn chế để start không vượt quá end
            float distance = Vector3.Distance(start, end);
            if (distance > 0.2f)
            {
                start += direction.normalized * Time.deltaTime * lineSpeed;
            }
            lineRenderer.SetPosition(0, start);
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / lineFadeTime);
            Color c = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, alpha);
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            yield return null;
        }

        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Spawn particle system tại vị trí bắn trúng
    /// </summary>
    /// <param name="hitPoint">Vị trí bắn trúng</param>
    /// <param name="hitNormal">Hướng pháp tuyến của bề mặt bị bắn trúng</param>
    /// <param name="parentTransform">Transform của Enemy để set làm parent của particle</param>
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

        // Spawn particle tại vị trí bắn trúng
        GameObject particleInstance = Instantiate(particlePrefab, hitPoint, Quaternion.identity);

        // Set particle làm child của Enemy nếu có parentTransform
        if (parentTransform != null)
        {
            particleInstance.transform.SetParent(parentTransform);
        }

        // Xoay particle theo hướng pháp tuyến (nếu cần)
        if (hitNormal != Vector2.zero)
        {
            float angle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
            particleInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Tự động hủy particle sau một khoảng thời gian (nếu prefab không tự hủy)
        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps != null && !ps.main.playOnAwake)
        {
            ps.Play();
        }

        // Hủy particle sau khi kết thúc (nếu prefab không tự hủy)
        if (particleLifetime > 0f)
        {
            Destroy(particleInstance, particleLifetime);
        }
    }

    #region Health System

    /// <summary>
    /// Xử lý khi player nhận sát thương
    /// </summary>
    private void OnPlayerDamageTaken(float damage)
    {
        // Có thể thêm hiệu ứng như màn hình đỏ, camera shake, v.v.
        Debug.Log($"Player nhận {damage} sát thương!");
    }

    /// <summary>
    /// Xử lý khi player chết
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("Player đã chết!");

        // Dừng di chuyển
        rb.velocity = Vector2.zero;
        movement = Vector2.zero;

        // Dừng footstep audio khi chết
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudioByID(AudioID.Player_Footstep);
        }

        // Trigger animation Die
        if (ani != null)
        {
            ani.SetTrigger("IsDie");
        }

        // Disable movement và attack
        enabled = false;

        // Có thể thêm logic khác như:
        // - Hiển thị Game Over UI
        // - Respawn sau một khoảng thời gian
        // - V.v.
    }

    /// <summary>
    /// Lấy Health component (public để các script khác có thể truy cập)
    /// </summary>
    public Health GetHealth()
    {
        return health;
    }

    #endregion

    #region Audio

    /// <summary>
    /// Xử lý phát tiếng bước chân của player
    /// </summary>
    /// <param name="isMoving">Player có đang di chuyển hay không</param>
    private void HandleFootstepAudio(bool isMoving)
    {
        if (AudioManager.Instance == null) return;
        if (health != null && health.IsDead) return;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.Instance.PlayAudio(AudioID.Player_Footstep, transform.position);
                footstepTimer = Mathf.Max(0.05f, footstepInterval);
            }
            wasMoving = true;
        }
        else
        {
            // Nếu player vừa dừng lại (từ moving -> stopped), dừng tất cả footstep audio đang phát
            if (wasMoving)
            {
                AudioManager.Instance.StopAllAudioByID(AudioID.Player_Footstep);
            }
            // Reset để khi bắt đầu di chuyển lại sẽ phát tiếng ngay (sau 1 interval)
            footstepTimer = 0f;
            wasMoving = false;
        }
    }

    #endregion

}

