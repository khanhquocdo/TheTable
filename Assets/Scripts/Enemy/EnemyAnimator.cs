using UnityEngine;

/// <summary>
/// Quản lý animation cho Enemy sử dụng Animator + Blend Tree
/// Hỗ trợ 4 animation chính: Idle, AttackIdle, Run, AttackRun với 8 hướng
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    // Animator Parameters
    private static readonly int PARAM_SPEED = Animator.StringToHash("Speed");
    private static readonly int PARAM_DIRECTION_X = Animator.StringToHash("DirectionX");
    private static readonly int PARAM_DIRECTION_Y = Animator.StringToHash("DirectionY");
    private static readonly int PARAM_IS_ATTACKING = Animator.StringToHash("IsAttacking");
    private static readonly int PARAM_IS_MOVING = Animator.StringToHash("IsMoving");
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyController enemyController;
    
    [Header("Settings")]
    [Tooltip("Ngưỡng vận tốc để coi là đang di chuyển")]
    [SerializeField] private float movementThreshold = 0.1f;
    
    [Tooltip("Dead zone cho direction để tránh animation nhấp nháy")]
    [SerializeField] private float directionDeadZone = 0.01f;
    
    // State tracking
    private Vector2 lastValidDirection = Vector2.up; // Hướng mặc định là lên trên
    private Vector2 currentMoveDirection;
    private Vector2 currentAimDirection;
    private bool isCurrentlyMoving;
    private bool isCurrentlyAttacking;
    
    // Direction mapping cho 8 hướng (normalized)
    private static readonly Vector2[] EightDirections = new Vector2[]
    {
        Vector2.up,                              // 0° (0, 1)
        new Vector2(0.707f, 0.707f),            // 45° (up-right)
        Vector2.right,                           // 90° (1, 0)
        new Vector2(0.707f, -0.707f),           // 135° (down-right)
        Vector2.down,                            // 180° (0, -1)
        new Vector2(-0.707f, -0.707f),          // 225° (down-left)
        Vector2.left,                            // 270° (-1, 0)
        new Vector2(-0.707f, 0.707f)            // 315° (up-left)
    };
    
    void Awake()
    {
        // Auto-assign nếu chưa gán
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        if (enemyController == null)
        {
            enemyController = GetComponent<EnemyController>();
        }
        
        // Validate
        if (animator == null)
        {
            Debug.LogError($"[EnemyAnimator] {gameObject.name} cần có Animator component!");
        }
        
        if (rb == null)
        {
            Debug.LogError($"[EnemyAnimator] {gameObject.name} cần có Rigidbody2D component!");
        }
    }
    
    void Start()
    {
        // Khởi tạo với hướng mặc định
        UpdateAnimatorDirection(lastValidDirection);
    }
    
    void Update()
    {
        UpdateAnimationState();
    }
    
    /// <summary>
    /// Cập nhật trạng thái animation dựa trên movement và attack state
    /// </summary>
    private void UpdateAnimationState()
    {
        // Lấy thông tin từ EnemyController
        UpdateMoveDirection();
        UpdateAimDirection();
        UpdateMovementState();
        UpdateAttackState();
        
        // Tính toán hướng cuối cùng dựa trên priority
        Vector2 finalDirection = CalculateFinalDirection();
        
        // Cập nhật Animator Parameters
        UpdateAnimatorParameters(finalDirection);
    }
    
    /// <summary>
    /// Lấy hướng di chuyển từ Rigidbody2D velocity
    /// </summary>
    private void UpdateMoveDirection()
    {
        if (rb != null)
        {
            currentMoveDirection = rb.velocity.normalized;
        }
        else
        {
            currentMoveDirection = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Lấy hướng nhắm từ EnemyController (firePoint direction)
    /// </summary>
    private void UpdateAimDirection()
    {
        if (enemyController != null && enemyController.HasPlayerTarget())
        {
            Vector2 playerPos = enemyController.GetPlayerPosition();
            Vector2 enemyPos = transform.position;
            currentAimDirection = (playerPos - enemyPos).normalized;
        }
        else
        {
            currentAimDirection = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Kiểm tra có đang di chuyển không
    /// </summary>
    private void UpdateMovementState()
    {
        if (rb != null)
        {
            float speed = rb.velocity.magnitude;
            isCurrentlyMoving = speed > movementThreshold;
        }
        else
        {
            isCurrentlyMoving = false;
        }
    }
    
    /// <summary>
    /// Kiểm tra có đang tấn công không (dựa trên EnemyController.IsAttacking)
    /// </summary>
    private void UpdateAttackState()
    {
        if (enemyController != null)
        {
            isCurrentlyAttacking = enemyController.IsAttacking;
        }
        else
        {
            isCurrentlyAttacking = false;
        }
    }
    
    /// <summary>
    /// Tính toán hướng cuối cùng dựa trên priority:
    /// - Ưu tiên Aim Direction khi bắn
    /// - Dùng Move Direction khi không bắn
    /// - Giữ hướng cuối cùng khi đứng yên
    /// </summary>
    private Vector2 CalculateFinalDirection()
    {
        Vector2 selectedDirection;
        
        // Priority 1: Nếu đang bắn, dùng Aim Direction
        if (isCurrentlyAttacking && currentAimDirection.magnitude > directionDeadZone)
        {
            selectedDirection = currentAimDirection;
        }
        // Priority 2: Nếu đang di chuyển, dùng Move Direction
        else if (isCurrentlyMoving && currentMoveDirection.magnitude > directionDeadZone)
        {
            selectedDirection = currentMoveDirection;
        }
        // Priority 3: Nếu đứng yên, giữ hướng cuối cùng
        else
        {
            selectedDirection = lastValidDirection;
        }
        
        // Normalize và snap về 8 hướng gần nhất
        Vector2 snappedDirection = SnapToEightDirections(selectedDirection);
        
        // Cập nhật lastValidDirection nếu direction hợp lệ
        if (snappedDirection.magnitude > directionDeadZone)
        {
            lastValidDirection = snappedDirection;
        }
        
        return lastValidDirection;
    }
    
    /// <summary>
    /// Snap direction về hướng gần nhất trong 8 hướng
    /// </summary>
    private Vector2 SnapToEightDirections(Vector2 direction)
    {
        if (direction.magnitude < directionDeadZone)
        {
            return lastValidDirection;
        }
        
        float maxDot = -1f;
        int bestDirectionIndex = 0;
        
        // Tìm hướng có dot product lớn nhất (gần nhất với direction)
        for (int i = 0; i < EightDirections.Length; i++)
        {
            float dot = Vector2.Dot(direction.normalized, EightDirections[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestDirectionIndex = i;
            }
        }
        
        return EightDirections[bestDirectionIndex];
    }
    
    /// <summary>
    /// Cập nhật tất cả Animator Parameters
    /// </summary>
    private void UpdateAnimatorParameters(Vector2 direction)
    {
        if (animator == null)
        {
            return;
        }
        
        // Cập nhật Speed (magnitude của velocity)
        float speed = rb != null ? rb.velocity.magnitude : 0f;
        animator.SetFloat(PARAM_SPEED, speed);
        
        // Cập nhật Direction (dùng cho Blend Tree)
        animator.SetFloat(PARAM_DIRECTION_X, direction.x);
        animator.SetFloat(PARAM_DIRECTION_Y, direction.y);
        
        // Cập nhật boolean flags
        animator.SetBool(PARAM_IS_MOVING, isCurrentlyMoving);
        animator.SetBool(PARAM_IS_ATTACKING, isCurrentlyAttacking);
    }
    
    /// <summary>
    /// Helper method để cập nhật direction trong Animator
    /// </summary>
    private void UpdateAnimatorDirection(Vector2 direction)
    {
        if (animator != null)
        {
            animator.SetFloat(PARAM_DIRECTION_X, direction.x);
            animator.SetFloat(PARAM_DIRECTION_Y, direction.y);
        }
    }
    
    #region Public Methods (Optional - để control từ bên ngoài nếu cần)
    
    /// <summary>
    /// Set trạng thái tấn công (có thể gọi từ EnemyController hoặc EnemyAttackState)
    /// </summary>
    public void SetIsAttacking(bool attacking)
    {
        isCurrentlyAttacking = attacking;
    }
    
    /// <summary>
    /// Set hướng nhắm trực tiếp (nếu cần override logic tự động)
    /// </summary>
    public void SetAimDirection(Vector2 direction)
    {
        currentAimDirection = direction.normalized;
    }
    
    /// <summary>
    /// Reset về trạng thái ban đầu
    /// </summary>
    public void ResetToDefault()
    {
        lastValidDirection = Vector2.up;
        isCurrentlyMoving = false;
        isCurrentlyAttacking = false;
        UpdateAnimatorDirection(lastValidDirection);
    }
    
    /// <summary>
    /// Disable Animator (gọi khi enemy chết để tối ưu performance)
    /// </summary>
    public void DisableAnimator()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }
    
    /// <summary>
    /// Enable Animator (nếu cần enable lại)
    /// </summary>
    public void EnableAnimator()
    {
        if (animator != null)
        {
            animator.enabled = true;
        }
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmosSelected()
    {
        // Vẽ hướng hiện tại
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, lastValidDirection * 1.5f);
            
            // Vẽ aim direction
            if (isCurrentlyAttacking && currentAimDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, currentAimDirection * 1.2f);
            }
            
            // Vẽ move direction
            if (isCurrentlyMoving && currentMoveDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, currentMoveDirection * 1f);
            }
        }
    }
    
    #endregion
}
