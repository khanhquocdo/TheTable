using UnityEngine;

/// <summary>
/// Component xử lý xoay tháp pháo của Tank
/// Xoay độc lập với thân xe, luôn aim về Player
/// </summary>
public class TankTurret : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 360f; // Độ/giây
    
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
        
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
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
    /// </summary>
    public Vector2 GetForwardDirection()
    {
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
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
