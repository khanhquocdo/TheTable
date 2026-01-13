using UnityEngine;

/// <summary>
/// Component xử lý di chuyển của Tank Body
/// Di chuyển chậm và xoay thân xe theo hướng di chuyển
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TankMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 180f; // Độ/giây
    [SerializeField] private float rotationOffset = -90f; // Độ lệch góc để điều chỉnh hướng asset (mặc định -90 độ)
    
    private Vector2 movementDirection;
    private bool isMoving = false;
    
    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        // Cấu hình Rigidbody2D cho tank
        rb.gravityScale = 0f;
        rb.drag = 5f; // Tăng drag để tank dừng nhanh hơn
        rb.angularDrag = 10f;
    }
    
    /// <summary>
    /// Di chuyển tank theo hướng
    /// </summary>
    public void Move(Vector2 direction)
    {
        movementDirection = direction.normalized;
        isMoving = movementDirection.magnitude > 0.1f;
    }
    
    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void Stop()
    {
        movementDirection = Vector2.zero;
        isMoving = false;
    }
    
    void FixedUpdate()
    {
        if (isMoving && movementDirection.magnitude > 0.1f)
        {
            // Di chuyển
            rb.velocity = movementDirection * moveSpeed;
            
            // Xoay thân xe theo hướng di chuyển
            // Atan2 trả về góc từ trục X dương (hướng phải), cộng thêm offset để điều chỉnh hướng asset
            float targetAngle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg + rotationOffset;
            float currentAngle = transform.eulerAngles.z;
            
            // Tính góc quay ngắn nhất
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
            
            // Xoay từ từ
            float rotationStep = rotationSpeed * Time.fixedDeltaTime;
            float newAngle = currentAngle;
            
            if (Mathf.Abs(angleDifference) > rotationStep)
            {
                newAngle += Mathf.Sign(angleDifference) * rotationStep;
            }
            else
            {
                newAngle = targetAngle;
            }
            
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        else
        {
            // Dừng di chuyển
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }
    }
    
    /// <summary>
    /// Set tốc độ di chuyển
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// Set tốc độ xoay
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    /// <summary>
    /// Set độ lệch góc để điều chỉnh hướng asset
    /// </summary>
    public void SetRotationOffset(float offset)
    {
        rotationOffset = offset;
    }
    
    /// <summary>
    /// Kiểm tra đang di chuyển không
    /// </summary>
    public bool IsMoving => isMoving;
}
