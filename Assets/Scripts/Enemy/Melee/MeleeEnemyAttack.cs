using UnityEngine;

/// <summary>
/// Component xử lý tấn công cận chiến của Melee Enemy
/// Sử dụng Physics2D.OverlapCircle để phát hiện và gây damage
/// </summary>
public class MeleeEnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Bán kính hitbox tấn công")]
    [SerializeField] private float attackRange = 1.5f;
    
    [Tooltip("Sát thương mỗi đòn tấn công")]
    [SerializeField] private float attackDamage = 20f;
    
    [Tooltip("Layer của Player để tấn công")]
    [SerializeField] private LayerMask playerLayer;
    
    [Tooltip("Offset của hitbox từ vị trí enemy (theo hướng tấn công)")]
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;
    
    private const string PLAYER_TAG = "Player";
    
    /// <summary>
    /// Thực hiện tấn công và trả về true nếu trúng Player
    /// </summary>
    public bool PerformAttack(Vector2 attackDirection)
    {
        // Phát audio tấn công
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(AudioID.Enemy_Melee_Attack, transform.position);
        }
        
        // Tính vị trí hitbox (từ vị trí enemy + offset theo hướng tấn công)
        Vector2 hitboxPosition = (Vector2)transform.position + attackOffset + attackDirection.normalized * (attackRange * 0.5f);
        
        // Sử dụng OverlapCircle để phát hiện Player trong phạm vi tấn công
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitboxPosition, attackRange, playerLayer);
        
        bool hitPlayer = false;
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(PLAYER_TAG))
            {
                // Tìm component Health trên Player
                Health playerHealth = hit.GetComponent<Health>();
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(attackDamage);
                    hitPlayer = true;
                    
                    // Phát audio đánh trúng
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayAudio(AudioID.Enemy_Melee_Hit, hitboxPosition);
                    }
                    
                    Debug.Log($"[MeleeEnemyAttack] {gameObject.name} gây {attackDamage} sát thương cho Player!");
                }
            }
        }
        
        return hitPlayer;
    }
    
    /// <summary>
    /// Set attack range (có thể override từ MeleeEnemyData)
    /// </summary>
    public void SetAttackRange(float range)
    {
        attackRange = range;
    }
    
    /// <summary>
    /// Set attack damage (có thể override từ MeleeEnemyData)
    /// </summary>
    public void SetAttackDamage(float damage)
    {
        attackDamage = damage;
    }
    
    /// <summary>
    /// Set player layer (có thể override từ MeleeEnemyData)
    /// </summary>
    public void SetPlayerLayer(LayerMask layer)
    {
        playerLayer = layer;
    }
    
    #region Debug Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos)
        {
            return;
        }
        
        // Vẽ hitbox tấn công
        Gizmos.color = gizmoColor;
        Vector2 hitboxPosition = (Vector2)transform.position + attackOffset;
        
        // Vẽ circle hitbox
        DrawWireCircle(hitboxPosition, attackRange);
        
        // Vẽ hướng tấn công (nếu đang trong play mode và có direction)
        if (Application.isPlaying)
        {
            // Có thể lấy direction từ MeleeEnemyController nếu cần
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(hitboxPosition, Vector2.up * attackRange);
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
