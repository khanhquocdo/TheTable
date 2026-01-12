using UnityEngine;

/// <summary>
/// State khi Tank di chuyển đến vị trí tối ưu để bắn
/// Di chuyển đến khoảng cách optimalAttackDistance
/// </summary>
public class TankMoveToPositionState : TankEnemyState
{
    public TankMoveToPositionState(TankEnemyController controller, TankEnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        // Bắt đầu aim turret về Player
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
        
        // Kiểm tra đã vào tầm bắn chưa
        if (distanceToPlayer >= data.minAttackDistance && distanceToPlayer <= data.maxAttackDistance)
        {
            // Đã vào tầm bắn, chuyển sang Attack
            controller.ChangeState(TankEnemyController.TankStateType.Attack);
            return;
        }
        
        // Tính toán hướng di chuyển
        Vector2 moveDirection = CalculateMoveDirection(playerPosition, distanceToPlayer);
        controller.Move(moveDirection);
    }
    
    public override void FixedUpdate()
    {
        // Movement được xử lý bởi TankMovement component
    }
    
    /// <summary>
    /// Tính toán hướng di chuyển dựa trên khoảng cách đến Player
    /// </summary>
    private Vector2 CalculateMoveDirection(Vector2 playerPosition, float distanceToPlayer)
    {
        Vector2 tankPosition = controller.transform.position;
        
        // Nếu quá gần, lùi lại
        if (distanceToPlayer < data.minAttackDistance)
        {
            Vector2 directionAway = (tankPosition - playerPosition).normalized;
            return directionAway;
        }
        
        // Nếu quá xa, tiến tới
        if (distanceToPlayer > data.maxAttackDistance)
        {
            Vector2 directionToPlayer = (playerPosition - tankPosition).normalized;
            return directionToPlayer;
        }
        
        // Nếu trong khoảng tối ưu, di chuyển đến optimalAttackDistance
        Vector2 directionToOptimal = (playerPosition - tankPosition).normalized;
        Vector2 optimalPosition = playerPosition - directionToOptimal * data.optimalAttackDistance;
        Vector2 directionToOptimalPos = (optimalPosition - tankPosition).normalized;
        
        return directionToOptimalPos;
    }
}
