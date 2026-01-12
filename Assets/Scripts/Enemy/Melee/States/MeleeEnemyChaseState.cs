using UnityEngine;

/// <summary>
/// State khi Melee Enemy đuổi theo Player
/// Chuyển sang Attack khi ở trong tầm tấn công
/// Chuyển về Idle nếu mất Player
/// </summary>
public class MeleeEnemyChaseState : MeleeEnemyState
{
    public MeleeEnemyChaseState(MeleeEnemyController controller, MeleeEnemyData data) : base(controller, data)
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
            controller.ChangeState(MeleeEnemyController.MeleeEnemyStateType.Idle);
            return;
        }
        
        // Kiểm tra vào tầm tấn công
        float distanceToPlayer = controller.GetDistanceToPlayer();
        if (distanceToPlayer <= data.attackRange && controller.HasLineOfSightToPlayer())
        {
            controller.ChangeState(MeleeEnemyController.MeleeEnemyStateType.Attack);
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
        
        // Quay mặt về phía Player (để animation đúng hướng)
        controller.LookAt(playerPosition);
    }
}
