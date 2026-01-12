using UnityEngine;

/// <summary>
/// State khi Tank tấn công Player
/// Dừng di chuyển, aim turret về Player, và bắn đạn pháo
/// </summary>
public class TankAttackState : TankEnemyState
{
    public TankAttackState(TankEnemyController controller, TankEnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        // Dừng di chuyển
        controller.StopMovement();
        
        // Aim turret về Player
        if (controller.HasPlayerTarget())
        {
            controller.SetTurretTarget(controller.GetPlayerTransform());
        }
    }
    
    public override void Update()
    {
        // Kiểm tra mất Player
        if (!controller.CanDetectPlayer())
        {
            controller.ChangeState(TankEnemyController.TankStateType.Idle);
            return;
        }
        
        if (!controller.HasPlayerTarget())
        {
            return;
        }
        
        Vector2 playerPosition = controller.GetPlayerPosition();
        float distanceToPlayer = controller.GetDistanceToPlayer();
        
        // Aim turret về Player
        controller.SetTurretTarget(controller.GetPlayerTransform());
        
        // Kiểm tra khoảng cách
        if (distanceToPlayer < data.minAttackDistance)
        {
            // Quá gần, lùi lại
            controller.ChangeState(TankEnemyController.TankStateType.MoveToPosition);
            return;
        }
        
        if (distanceToPlayer > data.maxAttackDistance)
        {
            // Quá xa, tiến tới
            controller.ChangeState(TankEnemyController.TankStateType.MoveToPosition);
            return;
        }
        
        // Kiểm tra Line of Sight
        if (!controller.HasLineOfSightToPlayer())
        {
            // Mất tầm nhìn, di chuyển để tìm lại
            controller.ChangeState(TankEnemyController.TankStateType.MoveToPosition);
            return;
        }
        
        // Bắn đạn (nếu đã aim trúng và có thể bắn)
        if (controller.IsTurretAimedAtPlayer(5f) && controller.CanFire())
        {
            controller.Fire();
        }
    }
    
    public override void FixedUpdate()
    {
        // Không di chuyển trong Attack state
    }
}
