using UnityEngine;

/// <summary>
/// State khi Tank đứng yên (Idle)
/// Chờ phát hiện Player hoặc patrol (nếu có)
/// </summary>
public class TankIdleState : TankEnemyState
{
    private float idleStartTime;
    
    public TankIdleState(TankEnemyController controller, TankEnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        idleStartTime = Time.time;
        controller.StopMovement();
        controller.ClearTurretTarget();
    }
    
    public override void Update()
    {
        // Kiểm tra phát hiện Player
        if (controller.CanDetectPlayer())
        {
            // Chuyển sang MoveToPosition để tiến tới khoảng cách bắn
            controller.ChangeState(TankEnemyController.TankStateType.MoveToPosition);
            return;
        }
        
        // TODO: Có thể thêm patrol logic ở đây nếu cần
    }
    
    public override void FixedUpdate()
    {
        // Không di chuyển trong Idle
    }
}
