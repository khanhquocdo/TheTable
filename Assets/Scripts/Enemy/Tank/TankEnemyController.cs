using UnityEngine;

/// <summary>
/// Controller chính cho Tank Enemy AI
/// Quản lý State Machine, Detection, Movement, Turret, và Weapon
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class TankEnemyController : MonoBehaviour
{
    public enum TankStateType
    {
        Idle,
        MoveToPosition,
        Attack
    }
    
    [Header("References")]
    [SerializeField] private TankEnemyData tankData;
    [SerializeField] private Transform tankBody; // Transform của thân xe (nếu tách riêng)
    [SerializeField] private Transform tankTurret; // Transform của tháp pháo
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Components
    private Rigidbody2D rb;
    private Health health;
    private TankMovement tankMovement;
    private TankTurret tankTurretComponent;
    private TankWeapon tankWeapon;
    
    // State Machine
    private TankEnemyState currentState;
    private TankIdleState idleState;
    private TankMoveToPositionState moveToPositionState;
    private TankAttackState attackState;
    
    // Player Detection
    private Transform playerTarget;
    private const string PLAYER_TAG = "Player";
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        
        // Tự động tìm hoặc tạo các component
        tankMovement = GetComponent<TankMovement>();
        if (tankMovement == null)
        {
            tankMovement = gameObject.AddComponent<TankMovement>();
        }
        
        // Tìm turret transform
        if (tankTurret == null)
        {
            // Tìm child có tên "Turret" hoặc tạo mới
            Transform turretTransform = transform.Find("Turret");
            if (turretTransform == null)
            {
                GameObject turretObj = new GameObject("Turret");
                turretObj.transform.SetParent(transform);
                turretObj.transform.localPosition = Vector3.zero;
                turretTransform = turretObj.transform;
            }
            tankTurret = turretTransform;
        }
        
        // Tạo hoặc tìm TankTurret component
        tankTurretComponent = tankTurret.GetComponent<TankTurret>();
        if (tankTurretComponent == null)
        {
            tankTurretComponent = tankTurret.gameObject.AddComponent<TankTurret>();
        }
        
        // Tạo hoặc tìm TankWeapon component
        tankWeapon = tankTurret.GetComponent<TankWeapon>();
        if (tankWeapon == null)
        {
            tankWeapon = tankTurret.gameObject.AddComponent<TankWeapon>();
        }
        
        // Tìm tank body transform
        if (tankBody == null)
        {
            tankBody = transform;
        }
    }
    
    void Start()
    {
        // Khởi tạo từ data
        if (tankData != null)
        {
            InitializeFromData();
        }
        else
        {
            Debug.LogError("TankEnemyController: TankEnemyData chưa được gán!");
        }
        
        // Tìm Player
        FindPlayer();
        
        // Khởi tạo State Machine
        InitializeStates();
        
        // Bắt đầu với Idle state
        ChangeState(TankStateType.Idle);
        
        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath += OnDeath;
        }
    }
    
    void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }
    
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedUpdate();
        }
    }
    
    /// <summary>
    /// Khởi tạo các thông số từ TankEnemyData
    /// </summary>
    private void InitializeFromData()
    {
        // Movement
        if (tankMovement != null)
        {
            tankMovement.SetMoveSpeed(tankData.moveSpeed);
            tankMovement.SetRotationSpeed(tankData.bodyRotationSpeed);
        }
        
        // Turret
        if (tankTurretComponent != null)
        {
            tankTurretComponent.SetRotationSpeed(tankData.turretRotationSpeed);
        }
        
        // Weapon
        if (tankWeapon != null)
        {
            tankWeapon.SetWeaponSettings(
                tankData.fireRate,
                tankData.projectileSpeed,
                tankData.projectileDamage,
                tankData.projectileLifetime,
                tankData.explosionRadius,
                tankData.minDamage,
                tankData.damageThroughObstacles,
                tankData.playerLayer,
                tankData.obstacleLayer
            );
        }
    }
    
    /// <summary>
    /// Khởi tạo các states
    /// </summary>
    private void InitializeStates()
    {
        idleState = new TankIdleState(this, tankData);
        moveToPositionState = new TankMoveToPositionState(this, tankData);
        attackState = new TankAttackState(this, tankData);
    }
    
    #region State Machine
    
    /// <summary>
    /// Chuyển state
    /// </summary>
    public void ChangeState(TankStateType newStateType)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        
        switch (newStateType)
        {
            case TankStateType.Idle:
                currentState = idleState;
                break;
            case TankStateType.MoveToPosition:
                currentState = moveToPositionState;
                break;
            case TankStateType.Attack:
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
        if (distance > tankData.detectRadius)
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
        
        Vector2 tankPos = transform.position;
        Vector2 playerPos = GetPlayerPosition();
        Vector2 direction = (playerPos - tankPos).normalized;
        float distance = Vector2.Distance(tankPos, playerPos);
        
        // Raycast để kiểm tra obstacle
        RaycastHit2D hit = Physics2D.Raycast(tankPos, direction, distance, tankData.obstacleLayer);
        
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
    /// Lấy Transform của Player
    /// </summary>
    public Transform GetPlayerTransform()
    {
        return playerTarget;
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
    /// Di chuyển tank theo hướng
    /// </summary>
    public void Move(Vector2 direction)
    {
        if (tankMovement != null)
        {
            tankMovement.Move(direction);
        }
    }
    
    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void StopMovement()
    {
        if (tankMovement != null)
        {
            tankMovement.Stop();
        }
    }
    
    #endregion
    
    #region Turret
    
    /// <summary>
    /// Set target cho turret để aim
    /// </summary>
    public void SetTurretTarget(Transform target)
    {
        if (tankTurretComponent != null)
        {
            tankTurretComponent.SetTarget(target);
        }
    }
    
    /// <summary>
    /// Clear target của turret
    /// </summary>
    public void ClearTurretTarget()
    {
        if (tankTurretComponent != null)
        {
            tankTurretComponent.ClearTarget();
        }
    }
    
    /// <summary>
    /// Kiểm tra turret đã aim trúng Player chưa
    /// </summary>
    public bool IsTurretAimedAtPlayer(float tolerance = 5f)
    {
        if (!HasPlayerTarget() || tankTurretComponent == null)
        {
            return false;
        }
        
        return tankTurretComponent.IsAimedAtTarget(GetPlayerPosition(), tolerance);
    }
    
    #endregion
    
    #region Weapon
    
    /// <summary>
    /// Bắn đạn pháo
    /// </summary>
    public bool Fire()
    {
        if (tankWeapon != null)
        {
            return tankWeapon.Fire();
        }
        return false;
    }
    
    /// <summary>
    /// Kiểm tra có thể bắn không
    /// </summary>
    public bool CanFire()
    {
        if (tankWeapon != null)
        {
            return tankWeapon.CanFire();
        }
        return false;
    }
    
    #endregion
    
    #region Death
    
    /// <summary>
    /// Xử lý khi tank chết
    /// </summary>
    private void OnDeath()
    {
        // Dừng mọi hoạt động
        StopMovement();
        ClearTurretTarget();
        
        // Disable controller
        enabled = false;
    }
    
    #endregion
    
    #region Debug Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || tankData == null)
        {
            return;
        }
        
        // Vẽ bán kính phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, tankData.detectRadius);
        
        // Vẽ khoảng cách tấn công
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tankData.maxAttackDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, tankData.minAttackDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tankData.optimalAttackDistance);
        
        // Vẽ đường đến Player
        if (HasPlayerTarget())
        {
            Vector2 playerPos = GetPlayerPosition();
            bool hasLOS = HasLineOfSightToPlayer();
            Gizmos.color = hasLOS ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerPos);
        }
    }
    
    #endregion
}
