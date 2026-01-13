using UnityEngine;

/// <summary>
/// Component xử lý xoay tháp pháo của Tank
/// Xoay độc lập với thân xe, luôn aim về Player
/// </summary>
public class TankTurret : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 360f; // Độ/giây
    [SerializeField] private float rotationOffset = 90f; // Độ lệch góc để điều chỉnh hướng asset (mặc định -90 độ)
    
    private Transform target;
    private bool isAiming = false;
    
    /// <summary>
    /// Set target để aim (thường là Player)
    /// </summary>
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
        isAiming = target != null;
    }
    
    /// <summary>
    /// Clear target (ngừng aim)
    /// </summary>
    public void ClearTarget()
    {
        target = null;
        isAiming = false;
    }
    
    void Update()
    {
        if (isAiming && target != null)
        {
            AimAtTarget(target.position);
        }
    }
    
    /// <summary>
    /// Aim về một vị trí
    /// </summary>
    public void AimAtTarget(Vector2 targetPosition)
    {
        Vector2 turretPosition = transform.position;
        Vector2 direction = (targetPosition - turretPosition).normalized;
        
        // Atan2 trả về góc từ trục X dương (hướng phải), cộng thêm offset để điều chỉnh hướng asset
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        float currentAngle = transform.eulerAngles.z;
        
        // Tính góc quay ngắn nhất
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
        
        // Xoay từ từ
        float rotationStep = rotationSpeed * Time.deltaTime;
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
    
    /// <summary>
    /// Lấy hướng hiện tại của turret (normalized)
    /// Trừ đi offset để lấy hướng thực tế mà asset đang nhìn
    /// </summary>
    public Vector2 GetForwardDirection()
    {
        // Trừ đi offset vì transform.eulerAngles.z đã bao gồm offset
        float angle = (transform.eulerAngles.z - rotationOffset) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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
    /// Kiểm tra đã aim trúng target chưa (trong phạm vi tolerance)
    /// </summary>
    public bool IsAimedAtTarget(Vector2 targetPosition, float tolerance = 5f)
    {
        Vector2 turretPosition = transform.position;
        Vector2 directionToTarget = (targetPosition - turretPosition).normalized;
        Vector2 turretForward = GetForwardDirection();
        
        float angle = Vector2.Angle(turretForward, directionToTarget);
        return angle <= tolerance;
    }
}
