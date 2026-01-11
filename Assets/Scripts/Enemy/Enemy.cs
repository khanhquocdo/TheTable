using UnityEngine;

/// <summary>
/// Component chính cho Enemy
/// Sử dụng EnemyController để xử lý AI
/// </summary>
[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    // Enemy class này có thể được mở rộng thêm logic riêng nếu cần
    // Phần lớn logic AI được xử lý bởi EnemyController
    
    private EnemyController controller;
    private Health health;
    
    void Awake()
    {
        controller = GetComponent<EnemyController>();
        health = GetComponent<Health>();
    }
}
