using UnityEngine;
using System.Collections;

/// <summary>
/// Controller chính cho Enemy AI
/// Quản lý State Machine, Detection, Movement, và Shooting
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyStateType
    {
        Idle,
        Chase,
        Attack
    }
    
    [Header("References")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    [Header("Bullet Line Settings")]
    [SerializeField] private float lineFadeTime = 0.15f;       // Thời gian mờ dần
    
    // Components
    private Rigidbody2D rb;
    private Health health;
    private LineRenderer lineRenderer;       // Vẽ đường đạn
    private BoidMovement boidMovement;      // Boid Movement System (optional)
    
    // State Machine
    private EnemyState currentState;
    private EnemyIdleState idleState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    public Vector2 firePointOffset;
    
    // Player Detection
    private Transform playerTarget;
    private const string PLAYER_TAG = "Player";
    
    // Movement
    private Vector2 movementDirection;
    
    // Attack state tracking (for animation)
    private bool isAttacking = false;
    
    /// <summary>
    /// Property để EnemyAnimator kiểm tra trạng thái tấn công
    /// </summary>
    public bool IsAttacking => isAttacking;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        lineRenderer = GetComponent<LineRenderer>();
        boidMovement = GetComponent<BoidMovement>(); // BoidMovement là optional
        
        // Validate
        if (enemyData == null)
        {
            Debug.LogError($"EnemyController: {gameObject.name} chưa có EnemyData được gán!");
        }
        
        if (firePoint == null)
        {
            // Tạo firePoint nếu chưa có
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            // Sử dụng firePointOffset nếu có, nếu không thì dùng giá trị mặc định
            firePointObj.transform.localPosition = firePointOffset != Vector2.zero ? firePointOffset : Vector2.up * 0.5f;
            firePoint = firePointObj.transform;
        }
        else
        {
            // Đảm bảo firePoint có vị trí ban đầu dựa trên offset
            if (firePointOffset != Vector2.zero)
            {
                firePoint.localPosition = firePointOffset;
            }
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Khởi tạo LineRenderer nếu có
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
        }
    }
    
    void Start()
    {
        // Khởi tạo các state
        idleState = new EnemyIdleState(this, enemyData);
        chaseState = new EnemyChaseState(this, enemyData);
        attackState = new EnemyAttackState(this, enemyData);
        
        // Bắt đầu với Idle state
        ChangeState(EnemyStateType.Idle);
        
        // Tìm Player nếu chưa có
        if (playerTarget == null)
        {
            FindPlayer();
        }
        
        // Set target cho BoidMovement nếu có
        if (boidMovement != null && HasPlayerTarget())
        {
            boidMovement.SetTarget(playerTarget);
        }
        
        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }
    }
    
    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
        
        // Tìm Player nếu mất target
        if (playerTarget == null)
        {
            FindPlayer();
            // Cập nhật target cho BoidMovement
            if (boidMovement != null && HasPlayerTarget())
            {
                boidMovement.SetTarget(playerTarget);
            }
        }
        
        // Luôn xoay firePoint về player nếu có player target và trong phạm vi phát hiện
        // Điều này đảm bảo firePoint luôn theo dõi player khi player di chuyển xung quanh enemy
        if (HasPlayerTarget())
        {
            float distanceToPlayer = GetDistanceToPlayer();
            if (distanceToPlayer <= enemyData.detectRadius)
            {
                Vector2 playerPosition = GetPlayerPosition();
                LookAt(playerPosition);
            }
        }
    }
    
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
        
        // Áp dụng movement
        rb.velocity = movementDirection * enemyData.moveSpeed;
    }
    
    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= OnEnemyDeath;
        }
    }
    
    #region State Management
    
    /// <summary>
    /// Chuyển đổi state
    /// </summary>
    public void ChangeState(EnemyStateType newStateType)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        
        // Cập nhật attack state cho animation
        isAttacking = (newStateType == EnemyStateType.Attack);
        
        // Cập nhật BoidMovement state nếu có
        if (boidMovement != null)
        {
            bool isChase = (newStateType == EnemyStateType.Chase);
            bool isAttack = (newStateType == EnemyStateType.Attack);
            boidMovement.SetState(isChase, isAttack);
            
            // Set target cho BoidMovement
            if (HasPlayerTarget())
            {
                boidMovement.SetTarget(playerTarget);
            }
        }
        
        switch (newStateType)
        {
            case EnemyStateType.Idle:
                currentState = idleState;
                break;
            case EnemyStateType.Chase:
                currentState = chaseState;
                break;
            case EnemyStateType.Attack:
                currentState = attackState;
                break;
        }
        
        if (currentState != null)
        {
            currentState.OnEnter();
        }
    }
    
    #endregion
    
    #region Player Detection
    
    /// <summary>
    /// Tìm Player trong scene
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
    }
    
    /// <summary>
    /// Kiểm tra có thể phát hiện Player không (khoảng cách + Line of Sight)
    /// </summary>
    public bool CanDetectPlayer()
    {
        if (!HasPlayerTarget())
        {
            return false;
        }
        
        float distance = GetDistanceToPlayer();
        if (distance > enemyData.detectRadius)
        {
            return false;
        }
        
        return HasLineOfSightToPlayer();
    }
    
    /// <summary>
    /// Kiểm tra có Line of Sight đến Player không
    /// </summary>
    public bool HasLineOfSightToPlayer()
    {
        if (!HasPlayerTarget())
        {
            return false;
        }
        
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = GetPlayerPosition();
        Vector2 direction = (playerPos - enemyPos).normalized;
        float distance = Vector2.Distance(enemyPos, playerPos);
        
        // Raycast để kiểm tra obstacle
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, direction, distance, enemyData.obstacleLayer);
        
        // Nếu không có obstacle, có Line of Sight
        return hit.collider == null;
    }
    
    /// <summary>
    /// Kiểm tra có Player target không
    /// </summary>
    public bool HasPlayerTarget()
    {
        return playerTarget != null && playerTarget.gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Lấy vị trí Player
    /// </summary>
    public Vector2 GetPlayerPosition()
    {
        if (HasPlayerTarget())
        {
            return playerTarget.position;
        }
        return Vector2.zero;
    }
    
    /// <summary>
    /// Lấy khoảng cách đến Player
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (!HasPlayerTarget())
        {
            return float.MaxValue;
        }
        
        return Vector2.Distance(transform.position, GetPlayerPosition());
    }
    
    #endregion
    
    #region Movement
    
    /// <summary>
    /// Di chuyển enemy theo hướng (được gọi từ states)
    /// Nếu có BoidMovement và đang ở Chase/Attack state, sẽ sử dụng Boid direction
    /// </summary>
    public void Move(Vector2 direction)
    {
        // Nếu có BoidMovement và đang ở state phù hợp, sử dụng Boid direction
        if (boidMovement != null)
        {
            Vector2 boidDirection = boidMovement.CalculateBoidDirection();
            if (boidDirection != Vector2.zero)
            {
                movementDirection = boidDirection;
                return;
            }
        }
        
        // Nếu không có BoidMovement hoặc Boid không hoạt động, dùng direction gốc
        movementDirection = direction.normalized;
    }
    
    /// <summary>
    /// Lấy hướng di chuyển từ BoidMovement nếu có và đang hoạt động
    /// </summary>
    public Vector2 GetBoidDirection()
    {
        if (boidMovement != null)
        {
            return boidMovement.CalculateBoidDirection();
        }
        return Vector2.zero;
    }
    
    /// <summary>
    /// Quay mặt về một vị trí
    /// </summary>
    public void LookAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Quay sprite về hướng target
        // if (spriteRenderer != null)
        // {
        //     // Nếu direction.x < 0, flip sprite
        //     spriteRenderer.flipX = direction.x < 0;
        // }
        
        // Quay firePoint về hướng target và điều chỉnh vị trí theo offset
        if (firePoint != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, angle);
            
            // Điều chỉnh vị trí firePoint dựa trên hướng nhìn và offset
            if (firePointOffset != Vector2.zero)
            {
                firePoint.localPosition = direction * firePointOffset.magnitude;
            }
        }
    }
    
    #endregion
    
    #region Shooting
    
    /// <summary>
    /// Áp dụng độ lệch đạn ngẫu nhiên vào hướng bắn
    /// </summary>
    private Vector2 ApplyBulletSpread(Vector2 direction)
    {
        // Nếu không có độ lệch, trả về hướng gốc
        if (enemyData.bulletSpreadAngle <= 0f)
        {
            return direction;
        }
        
        // Tính góc hiện tại của hướng bắn
        float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Random góc lệch trong phạm vi [-spreadAngle/2, spreadAngle/2]
        float randomSpread = Random.Range(-enemyData.bulletSpreadAngle / 2f, enemyData.bulletSpreadAngle / 2f);
        
        // Áp dụng góc lệch
        float finalAngle = currentAngle + randomSpread;
        float angleInRadians = finalAngle * Mathf.Deg2Rad;
        
        // Tính toán hướng mới
        return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
    
    /// <summary>
    /// Bắn đạn theo hướng với độ lệch ngẫu nhiên
    /// </summary>
    public void Shoot(Vector2 direction)
    {
        if (EnemyBulletPool.Instance == null)
        {
            Debug.LogWarning("EnemyBulletPool chưa được khởi tạo!");
            return;
        }
        
        // Phát audio bắn
        Vector2 spawnPosition = firePoint != null ? firePoint.position : (Vector2)transform.position + firePointOffset;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(AudioID.Enemy_Shooter_Fire, spawnPosition);
        }
        
        // Áp dụng độ lệch đạn ngẫu nhiên
        Vector2 finalDirection = ApplyBulletSpread(direction);
        
        EnemyBulletPool.Instance.SpawnBullet(
            spawnPosition, 
            finalDirection, 
            enemyData.bulletSpeed, 
            enemyData.bulletDamage, 
            enemyData.bulletLifetime,
            enemyData.playerLayer,
            enemyData.obstacleLayer
        );
        
        // Vẽ đường đạn
        if (lineRenderer != null)
        {
            Vector3 startPos = spawnPosition;
            Vector3 endPos;
            
            // Vẽ theo hướng bắn thực tế (đã áp dụng độ lệch) với độ dài tối đa
            float maxLineLength = enemyData.bulletSpeed * enemyData.bulletLifetime;
            endPos = startPos + (Vector3)(finalDirection.normalized * maxLineLength);
            
            StopAllCoroutines();
            StartCoroutine(DrawBulletLine(startPos, endPos));
        }
    }
    
    /// <summary>
    /// Vẽ đường đạn bằng LineRenderer
    /// </summary>
    private IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;
        
        lineRenderer.enabled = true;
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
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / lineFadeTime);
            Color c = new Color(lineRenderer.material.color.r, lineRenderer.material.color.g, lineRenderer.material.color.b, alpha);
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            yield return null;
        }

        lineRenderer.enabled = false;
    }
    
    #endregion
    
    #region Events
    
    private void OnEnemyDeath()
    {
        // Dừng tất cả hành vi khi chết
        rb.velocity = Vector2.zero;
        movementDirection = Vector2.zero;
        
        // Trigger animation Die trước khi disable
        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.enabled)
        {
            anim.SetTrigger("IsDie");
        }
        
        enabled = false;
        
        // Disable animation sau khi đã trigger (để tối ưu performance)
        // Note: EnemyAnimator component sẽ được disable nếu có
        Component animatorComponent = GetComponent("EnemyAnimator");
        if (animatorComponent != null)
        {
            // Disable EnemyAnimator component để ngừng cập nhật animation
            if (animatorComponent is MonoBehaviour mb)
            {
                mb.enabled = false;
            }
        }
        
        // Lưu ý: Không disable Animator ngay lập tức để animation Die có thể phát
        // Animator sẽ được disable sau khi animation Die kết thúc (nếu cần)
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (enemyData == null || !showDebugGizmos)
        {
            return;
        }
        
        // Vẽ detect radius
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, enemyData.detectRadius);
        
        // Vẽ min/max distance
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, enemyData.minDistance);
        
        Gizmos.color = Color.green;
        DrawWireCircle(transform.position, enemyData.maxAttackDistance);
        
        // Vẽ line to player
        if (HasPlayerTarget())
        {
            Vector2 playerPos = GetPlayerPosition();
            bool hasLOS = HasLineOfSightToPlayer();
            Gizmos.color = hasLOS ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerPos);
        }
    }
    
    /// <summary>
    /// Vẽ wire circle bằng cách vẽ nhiều line segments (tương thích với mọi phiên bản Unity)
    /// </summary>
    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.right * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    #endregion
}

