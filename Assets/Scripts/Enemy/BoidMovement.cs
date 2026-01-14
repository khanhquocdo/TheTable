using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Boid Movement System cho Enemy
/// Xử lý di chuyển theo kiểu bầy đàn với các lực: Separation, Cohesion, Alignment, Target Attraction
/// Chỉ tính toán hướng di chuyển (Vector2), không quyết định AI state
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BoidMovement : MonoBehaviour
{
    [Header("Boid Settings")]
    [Tooltip("Bán kính phát hiện neighbor (enemy khác trong bầy)")]
    [SerializeField] private float neighborRadius = 3f;
    
    [Tooltip("Layer của Enemy để phát hiện neighbor")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Tooltip("Layer của Obstacle để tránh (có thể mở rộng sau)")]
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Boid Forces Weights")]
    [Tooltip("Trọng số lực Separation (tránh đứng chồng lên nhau)")]
    [Range(0f, 10f)]
    [SerializeField] private float separationWeight = 2f;
    
    [Tooltip("Trọng số lực Cohesion (giữ khoảng cách vừa phải với đồng đội)")]
    [Range(0f, 10f)]
    [SerializeField] private float cohesionWeight = 1f;
    
    [Tooltip("Trọng số lực Alignment (di chuyển cùng hướng)")]
    [Range(0f, 10f)]
    [SerializeField] private float alignmentWeight = 1f;
    
    [Tooltip("Trọng số lực Target Attraction (hướng về Player)")]
    [Range(0f, 10f)]
    [SerializeField] private float targetWeight = 3f;
    
    [Header("State Control")]
    [Tooltip("Boid có hoạt động trong Chase state không?")]
    [SerializeField] private bool enableInChase = true;
    
    [Tooltip("Boid có hoạt động trong Attack state không? (thường là false hoặc giảm lực)")]
    [SerializeField] private bool enableInAttack = false;
    
    [Tooltip("Hệ số giảm lực khi ở Attack state (0 = tắt hoàn toàn, 1 = giữ nguyên)")]
    [Range(0f, 1f)]
    [SerializeField] private float attackStateForceMultiplier = 0.3f;
    
    [Header("Debug")]
    [Tooltip("Hiển thị gizmos để debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    [Tooltip("Màu gizmo cho neighbor radius")]
    [SerializeField] private Color neighborRadiusColor = new Color(0f, 1f, 0f, 0.3f);
    
    // Components
    private Rigidbody2D rb;
    
    // Cache
    private Collider2D[] neighborColliders = new Collider2D[20]; // Cache để tối ưu performance
    
    // Current state (được set từ bên ngoài)
    private bool isChaseState = false;
    private bool isAttackState = false;
    
    // Target (thường là Player)
    private Transform target;
    private const string PLAYER_TAG = "Player";
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Tìm Player target
        FindTarget();
    }
    
    void Start()
    {
        // Đảm bảo có Rigidbody2D
        if (rb == null)
        {
            Debug.LogError($"BoidMovement: {gameObject.name} cần có Rigidbody2D component!");
        }
    }
    
    void Update()
    {
        // Tìm target nếu mất
        if (target == null)
        {
            FindTarget();
        }
    }
    
    /// <summary>
    /// Tính toán và trả về hướng di chuyển dựa trên các lực Boid
    /// </summary>
    /// <returns>Vector2 direction đã được normalize</returns>
    public Vector2 CalculateBoidDirection()
    {
        // Kiểm tra có được bật không dựa trên state
        if (!ShouldApplyBoid())
        {
            return Vector2.zero;
        }
        
        Vector2 separationForce = CalculateSeparation();
        Vector2 cohesionForce = CalculateCohesion();
        Vector2 alignmentForce = CalculateAlignment();
        Vector2 targetForce = CalculateTargetAttraction();
        
        // Tính tổng hợp các lực
        Vector2 totalForce = Vector2.zero;
        totalForce += separationForce * separationWeight;
        totalForce += cohesionForce * cohesionWeight;
        totalForce += alignmentForce * alignmentWeight;
        totalForce += targetForce * targetWeight;
        
        // Áp dụng hệ số giảm lực nếu ở Attack state
        if (isAttackState && enableInAttack)
        {
            totalForce *= attackStateForceMultiplier;
        }
        
        // Normalize và trả về
        return totalForce.normalized;
    }
    
    /// <summary>
    /// Tính lực Separation - tránh đứng chồng lên enemy khác
    /// Lực này đẩy enemy ra xa các neighbor quá gần
    /// </summary>
    private Vector2 CalculateSeparation()
    {
        Vector2 separationForce = Vector2.zero;
        int neighborCount = GetNeighbors();
        
        if (neighborCount == 0)
        {
            return separationForce;
        }
        
        Vector2 currentPos = transform.position;
        
        for (int i = 0; i < neighborCount; i++)
        {
            if (neighborColliders[i] == null || neighborColliders[i].gameObject == gameObject)
            {
                continue;
            }
            
            Vector2 neighborPos = neighborColliders[i].transform.position;
            Vector2 directionAway = currentPos - neighborPos;
            float distance = directionAway.magnitude;
            
            // Nếu quá gần, tạo lực đẩy (càng gần càng mạnh)
            if (distance > 0f && distance < neighborRadius)
            {
                // Normalize và chia cho distance để lực mạnh hơn khi gần
                separationForce += directionAway.normalized / distance;
            }
        }
        
        return separationForce.normalized;
    }
    
    /// <summary>
    /// Tính lực Cohesion - giữ khoảng cách vừa phải với đồng đội
    /// Lực này kéo enemy về phía trung tâm của các neighbor
    /// </summary>
    private Vector2 CalculateCohesion()
    {
        Vector2 cohesionForce = Vector2.zero;
        int neighborCount = GetNeighbors();
        
        if (neighborCount == 0)
        {
            return cohesionForce;
        }
        
        Vector2 centerOfMass = Vector2.zero;
        int validNeighbors = 0;
        
        for (int i = 0; i < neighborCount; i++)
        {
            if (neighborColliders[i] == null || neighborColliders[i].gameObject == gameObject)
            {
                continue;
            }
            
            centerOfMass += (Vector2)neighborColliders[i].transform.position;
            validNeighbors++;
        }
        
        if (validNeighbors > 0)
        {
            centerOfMass /= validNeighbors;
            cohesionForce = (centerOfMass - (Vector2)transform.position).normalized;
        }
        
        return cohesionForce;
    }
    
    /// <summary>
    /// Tính lực Alignment - di chuyển cùng hướng với neighbor
    /// Lực này làm cho enemy di chuyển theo hướng trung bình của các neighbor
    /// </summary>
    private Vector2 CalculateAlignment()
    {
        Vector2 alignmentForce = Vector2.zero;
        int neighborCount = GetNeighbors();
        
        if (neighborCount == 0)
        {
            return alignmentForce;
        }
        
        Vector2 averageVelocity = Vector2.zero;
        int validNeighbors = 0;
        
        for (int i = 0; i < neighborCount; i++)
        {
            if (neighborColliders[i] == null || neighborColliders[i].gameObject == gameObject)
            {
                continue;
            }
            
            Rigidbody2D neighborRb = neighborColliders[i].GetComponent<Rigidbody2D>();
            if (neighborRb != null)
            {
                averageVelocity += neighborRb.velocity.normalized;
                validNeighbors++;
            }
        }
        
        if (validNeighbors > 0)
        {
            averageVelocity /= validNeighbors;
            alignmentForce = averageVelocity.normalized;
        }
        
        return alignmentForce;
    }
    
    /// <summary>
    /// Tính lực Target Attraction - hướng về Player
    /// Lực này kéo enemy về phía target (thường là Player)
    /// </summary>
    private Vector2 CalculateTargetAttraction()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            return Vector2.zero;
        }
        
        Vector2 directionToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
        return directionToTarget;
    }
    
    /// <summary>
    /// Lấy danh sách neighbor trong phạm vi neighborRadius
    /// </summary>
    private int GetNeighbors()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            neighborRadius,
            neighborColliders,
            enemyLayer
        );
        
        return count;
    }
    
    /// <summary>
    /// Kiểm tra có nên áp dụng Boid không dựa trên state hiện tại
    /// </summary>
    private bool ShouldApplyBoid()
    {
        if (isChaseState && enableInChase)
        {
            return true;
        }
        
        if (isAttackState && enableInAttack)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Set state hiện tại (được gọi từ Enemy Controller)
    /// </summary>
    public void SetState(bool chase, bool attack)
    {
        isChaseState = chase;
        isAttackState = attack;
    }
    
    /// <summary>
    /// Set target (thường là Player)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Tìm target (Player)
    /// </summary>
    private void FindTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
        {
            return;
        }
        
        // Vẽ neighbor radius
        Gizmos.color = neighborRadiusColor;
        DrawWireCircle(transform.position, neighborRadius);
        
        // Vẽ các lực Boid (chỉ khi đang chạy)
        if (Application.isPlaying)
        {
            Vector2 separation = CalculateSeparation();
            Vector2 cohesion = CalculateCohesion();
            Vector2 alignment = CalculateAlignment();
            Vector2 target = CalculateTargetAttraction();
            
            Vector2 pos = transform.position;
            
            // Separation - màu đỏ (đẩy ra)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, separation * separationWeight * 0.5f);
            
            // Cohesion - màu xanh dương (kéo về trung tâm)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pos, cohesion * cohesionWeight * 0.5f);
            
            // Alignment - màu vàng (cùng hướng)
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pos, alignment * alignmentWeight * 0.5f);
            
            // Target - màu xanh lá (hướng về target)
            Gizmos.color = Color.green;
            Gizmos.DrawRay(pos, target * targetWeight * 0.5f);
            
            // Tổng hợp lực - màu trắng
            Vector2 totalDirection = CalculateBoidDirection();
            Gizmos.color = Color.white;
            Gizmos.DrawRay(pos, totalDirection * 1f);
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
