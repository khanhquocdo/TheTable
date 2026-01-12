using UnityEngine;

/// <summary>
/// Quản lý animation cho Melee Enemy sử dụng Animator + Blend Tree
/// Hỗ trợ animation 8 hướng: Idle, Run, Attack
/// Sử dụng MoveX, MoveY, IsMoving, IsAttacking, LastMoveX, LastMoveY
/// </summary>
[RequireComponent(typeof(Animator))]
public class MeleeEnemyAnimator : MonoBehaviour
{
    // Animator Parameters
    private static readonly int PARAM_MOVE_X = Animator.StringToHash("MoveX");
    private static readonly int PARAM_MOVE_Y = Animator.StringToHash("MoveY");
    private static readonly int PARAM_IS_MOVING = Animator.StringToHash("IsMoving");
    private static readonly int PARAM_IS_ATTACKING = Animator.StringToHash("IsAttacking");
    private static readonly int PARAM_LAST_MOVE_X = Animator.StringToHash("LastMoveX");
    private static readonly int PARAM_LAST_MOVE_Y = Animator.StringToHash("LastMoveY");
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MeleeEnemyController meleeEnemyController;
    
    [Header("Settings")]
    [Tooltip("Ngưỡng vận tốc để coi là đang di chuyển")]
    [SerializeField] private float movementThreshold = 0.1f;
    
    [Tooltip("Dead zone cho direction để tránh animation nhấp nháy")]
    [SerializeField] private float directionDeadZone = 0.01f;
    
    // State tracking
    private Vector2 lastValidDirection = Vector2.up; // Hướng mặc định là lên trên
    private Vector2 currentMoveDirection;
    private bool isCurrentlyMoving;
    private bool isCurrentlyAttacking;
    
    // Direction mapping cho 8 hướng (normalized)
    private static readonly Vector2[] EightDirections = new Vector2[]
    {
        Vector2.up,                              // 0° (0, 1) - Up
        new Vector2(0.707f, 0.707f),            // 45° (up-right)
        Vector2.right,                           // 90° (1, 0) - Right
        new Vector2(0.707f, -0.707f),           // 135° (down-right)
        Vector2.down,                            // 180° (0, -1) - Down
        new Vector2(-0.707f, -0.707f),          // 225° (down-left)
        Vector2.left,                            // 270° (-1, 0) - Left
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
        
        if (meleeEnemyController == null)
        {
            meleeEnemyController = GetComponent<MeleeEnemyController>();
        }
        
        // Validate
        if (animator == null)
        {
            Debug.LogError($"[MeleeEnemyAnimator] {gameObject.name} cần có Animator component!");
        }
        
        if (rb == null)
        {
            Debug.LogError($"[MeleeEnemyAnimator] {gameObject.name} cần có Rigidbody2D component!");
        }
    }
    
    void Start()
    {
        // Khởi tạo với hướng mặc định
        UpdateAnimatorDirection(lastValidDirection);
        UpdateLastMoveDirection(lastValidDirection);
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
        // Lấy thông tin từ MeleeEnemyController
        UpdateMoveDirection();
        UpdateMovementState();
        UpdateAttackState();
        
        // Tính toán hướng cuối cùng
        Vector2 finalDirection = CalculateFinalDirection();
        
        // Cập nhật Animator Parameters
        UpdateAnimatorParameters(finalDirection);
    }
    
    /// <summary>
    /// Lấy hướng di chuyển từ Rigidbody2D velocity hoặc từ controller
    /// </summary>
    private void UpdateMoveDirection()
    {
        if (meleeEnemyController != null)
        {
            // Lấy direction từ controller (nếu có method GetMovementDirection)
            currentMoveDirection = meleeEnemyController.GetMovementDirection();
        }
        else if (rb != null)
        {
            currentMoveDirection = rb.velocity.normalized;
        }
        else
        {
            currentMoveDirection = Vector2.zero;
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
    /// Kiểm tra có đang tấn công không (dựa trên MeleeEnemyController.IsAttacking)
    /// </summary>
    private void UpdateAttackState()
    {
        if (meleeEnemyController != null)
        {
            isCurrentlyAttacking = meleeEnemyController.IsAttacking;
        }
        else
        {
            isCurrentlyAttacking = false;
        }
    }
    
    /// <summary>
    /// Tính toán hướng cuối cùng dựa trên priority:
    /// - Nếu đang di chuyển, dùng Move Direction
    /// - Nếu đang tấn công, giữ hướng cuối cùng (không đổi hướng khi attack)
    /// - Nếu đứng yên, giữ hướng cuối cùng
    /// </summary>
    private Vector2 CalculateFinalDirection()
    {
        Vector2 selectedDirection;
        
        // Nếu đang tấn công, giữ hướng cuối cùng (không đổi hướng)
        if (isCurrentlyAttacking)
        {
            selectedDirection = lastValidDirection;
        }
        // Nếu đang di chuyển, dùng Move Direction
        else if (isCurrentlyMoving && currentMoveDirection.magnitude > directionDeadZone)
        {
            selectedDirection = currentMoveDirection;
        }
        // Nếu đứng yên, giữ hướng cuối cùng
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
        
        // Cập nhật MoveX, MoveY (cho animation Run)
        if (isCurrentlyMoving && !isCurrentlyAttacking)
        {
            animator.SetFloat(PARAM_MOVE_X, direction.x);
            animator.SetFloat(PARAM_MOVE_Y, direction.y);
        }
        else
        {
            // Khi không di chuyển hoặc đang tấn công, set về 0
            animator.SetFloat(PARAM_MOVE_X, 0f);
            animator.SetFloat(PARAM_MOVE_Y, 0f);
        }
        
        // Cập nhật LastMoveX, LastMoveY (cho animation Idle và Attack)
        animator.SetFloat(PARAM_LAST_MOVE_X, direction.x);
        animator.SetFloat(PARAM_LAST_MOVE_Y, direction.y);
        
        // Cập nhật boolean flags
        animator.SetBool(PARAM_IS_MOVING, isCurrentlyMoving && !isCurrentlyAttacking);
        animator.SetBool(PARAM_IS_ATTACKING, isCurrentlyAttacking);
    }
    
    /// <summary>
    /// Helper method để cập nhật direction trong Animator
    /// </summary>
    private void UpdateAnimatorDirection(Vector2 direction)
    {
        if (animator != null)
        {
            animator.SetFloat(PARAM_MOVE_X, direction.x);
            animator.SetFloat(PARAM_MOVE_Y, direction.y);
        }
    }
    
    /// <summary>
    /// Helper method để cập nhật LastMove direction trong Animator
    /// </summary>
    private void UpdateLastMoveDirection(Vector2 direction)
    {
        if (animator != null)
        {
            animator.SetFloat(PARAM_LAST_MOVE_X, direction.x);
            animator.SetFloat(PARAM_LAST_MOVE_Y, direction.y);
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Set trạng thái tấn công (có thể gọi từ MeleeEnemyController hoặc MeleeEnemyAttackState)
    /// </summary>
    public void SetIsAttacking(bool attacking)
    {
        isCurrentlyAttacking = attacking;
    }
    
    /// <summary>
    /// Set hướng di chuyển trực tiếp (nếu cần override logic tự động)
    /// </summary>
    public void SetMoveDirection(Vector2 direction)
    {
        currentMoveDirection = direction.normalized;
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
        UpdateLastMoveDirection(lastValidDirection);
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
