using UnityEngine;

/// <summary>
/// Controller chính cho Melee Enemy AI
/// Quản lý State Machine, Detection, Movement, và Attack
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class MeleeEnemyController : MonoBehaviour
{
    public enum MeleeEnemyStateType
    {
        Idle,
        Chase,
        Attack
    }
    
    [Header("References")]
    [SerializeField] private MeleeEnemyData meleeEnemyData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Components
    private Rigidbody2D rb;
    private Health health;
    private MeleeEnemyAnimator meleeAnimator;
    private MeleeEnemyAttack meleeAttack;
    private BoidMovement boidMovement;      // Boid Movement System (optional)
    
    // State Machine
    private MeleeEnemyState currentState;
    private MeleeEnemyIdleState idleState;
    private MeleeEnemyChaseState chaseState;
    private MeleeEnemyAttackState attackState;
    
    // Player Detection
    private Transform playerTarget;
    private const string PLAYER_TAG = "Player";
    
    // Movement
    private Vector2 movementDirection;
    
    // Attack state tracking (for animation)
    private bool isAttacking = false;
    
    /// <summary>
    /// Property để MeleeEnemyAnimator kiểm tra trạng thái tấn công
    /// </summary>
    public bool IsAttacking => isAttacking;
    
    /// <summary>
    /// Property để truy cập MeleeEnemyAttack component
    /// </summary>
    public MeleeEnemyAttack MeleeAttack => meleeAttack;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        meleeAnimator = GetComponent<MeleeEnemyAnimator>();
        meleeAttack = GetComponent<MeleeEnemyAttack>();
        boidMovement = GetComponent<BoidMovement>(); // BoidMovement là optional
        
        // Validate
        if (meleeEnemyData == null)
        {
            Debug.LogError($"MeleeEnemyController: {gameObject.name} chưa có MeleeEnemyData được gán!");
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Khởi tạo MeleeEnemyAttack nếu chưa có
        if (meleeAttack == null)
        {
            meleeAttack = gameObject.AddComponent<MeleeEnemyAttack>();
        }
        
        // Cấu hình MeleeEnemyAttack từ data
        if (meleeAttack != null && meleeEnemyData != null)
        {
            meleeAttack.SetAttackRange(meleeEnemyData.attackRange);
            meleeAttack.SetAttackDamage(meleeEnemyData.attackDamage);
            meleeAttack.SetPlayerLayer(meleeEnemyData.playerLayer);
        }
    }
    
    void Start()
    {
        // Khởi tạo các state
        idleState = new MeleeEnemyIdleState(this, meleeEnemyData);
        chaseState = new MeleeEnemyChaseState(this, meleeEnemyData);
        attackState = new MeleeEnemyAttackState(this, meleeEnemyData);
        
        // Bắt đầu với Idle state
        ChangeState(MeleeEnemyStateType.Idle);
        
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
    }
    
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
        
        // Áp dụng movement
        rb.velocity = movementDirection * meleeEnemyData.moveSpeed;
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
    public void ChangeState(MeleeEnemyStateType newStateType)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        
        // Cập nhật attack state cho animation
        isAttacking = (newStateType == MeleeEnemyStateType.Attack);
        
        // Cập nhật BoidMovement state nếu có
        if (boidMovement != null)
        {
            bool isChase = (newStateType == MeleeEnemyStateType.Chase);
            bool isAttack = (newStateType == MeleeEnemyStateType.Attack);
            boidMovement.SetState(isChase, isAttack);
            
            // Set target cho BoidMovement
            if (HasPlayerTarget())
            {
                boidMovement.SetTarget(playerTarget);
            }
        }
        
        switch (newStateType)
        {
            case MeleeEnemyStateType.Idle:
                currentState = idleState;
                break;
            case MeleeEnemyStateType.Chase:
                currentState = chaseState;
                break;
            case MeleeEnemyStateType.Attack:
                currentState = attackState;
                break;
        }
        
        if (currentState != null)
        {
            currentState.OnEnter();
        }
    }
    
    /// <summary>
    /// Set trạng thái tấn công (được gọi từ MeleeEnemyAttackState)
    /// </summary>
    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
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
        if (distance > meleeEnemyData.detectRadius)
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
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, direction, distance, meleeEnemyData.obstacleLayer);
        
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
    /// Lấy hướng di chuyển hiện tại (cho MeleeEnemyAnimator)
    /// </summary>
    public Vector2 GetMovementDirection()
    {
        return movementDirection;
    }
    
    /// <summary>
    /// Quay mặt về một vị trí (để animation đúng hướng)
    /// </summary>
    public void LookAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Không flip sprite, animation sẽ tự xử lý hướng
        // Chỉ cần đảm bảo direction được truyền đúng cho animator
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
        if (anim != null)
        {
            anim.SetTrigger("IsDie");
        }
        
        enabled = false;
    
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (meleeEnemyData == null || !showDebugGizmos)
        {
            return;
        }
        
        // Vẽ detect radius
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, meleeEnemyData.detectRadius);
        
        // Vẽ attack range
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, meleeEnemyData.attackRange);
        
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
    /// Vẽ wire circle bằng cách vẽ nhiều line segments
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
