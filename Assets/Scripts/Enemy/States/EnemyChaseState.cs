using UnityEngine;

/// <summary>
/// State khi Enemy đuổi theo Player
/// Chuyển sang Attack khi ở trong tầm bắn
/// Chuyển về Idle nếu mất Player
/// </summary>
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController controller, EnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        // Có thể thêm animation hoặc sound effect khi bắt đầu đuổi
    }
    
    public override void Update()
    {
        // Kiểm tra mất Player
        if (!controller.CanDetectPlayer())
        {
            controller.ChangeState(EnemyController.EnemyStateType.Idle);
            return;
        }
        
        // Kiểm tra vào tầm bắn
        float distanceToPlayer = controller.GetDistanceToPlayer();
        if (distanceToPlayer <= data.maxAttackDistance && controller.HasLineOfSightToPlayer())
        {
            controller.ChangeState(EnemyController.EnemyStateType.Attack);
            return;
        }
    }
    
    public override void FixedUpdate()
    {
        if (!controller.HasPlayerTarget())
        {
            return;
        }
        
        Vector2 playerPosition = controller.GetPlayerPosition();
        Vector2 enemyPosition = controller.transform.position;
        Vector2 directionToPlayer = (playerPosition - enemyPosition).normalized;
        
        // Di chuyển về phía Player
        controller.Move(directionToPlayer);
        
        // Quay mặt về phía Player
        controller.LookAt(playerPosition);
    }
}

