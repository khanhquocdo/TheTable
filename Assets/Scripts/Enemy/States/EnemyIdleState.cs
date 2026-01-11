using UnityEngine;

/// <summary>
/// State khi Enemy đứng yên hoặc patrol
/// Chuyển sang Chase khi phát hiện Player
/// </summary>
public class EnemyIdleState : EnemyState
{
    private Vector2 patrolStartPosition;
    private Vector2 patrolTargetPosition;
    private float idleTimer = 0f;
    private bool isPatrolling = false;
    
    public EnemyIdleState(EnemyController controller, EnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        patrolStartPosition = controller.transform.position;
        idleTimer = 0f;
        isPatrolling = false;
        
        if (data.canPatrol)
        {
            SetNewPatrolTarget();
        }
    }
    
    public override void Update()
    {
        // Kiểm tra phát hiện Player
        if (controller.CanDetectPlayer())
        {
            controller.ChangeState(EnemyController.EnemyStateType.Chase);
            return;
        }
        
        // Xử lý patrol nếu có
        if (data.canPatrol)
        {
            HandlePatrol();
        }
    }
    
    public override void FixedUpdate()
    {
        if (data.canPatrol && isPatrolling)
        {
            // Di chuyển đến điểm patrol
            Vector2 direction = (patrolTargetPosition - (Vector2)controller.transform.position).normalized;
            controller.Move(direction);
        }
        else
        {
            controller.Move(Vector2.zero);
        }
    }
    
    private void HandlePatrol()
    {
        Vector2 currentPos = controller.transform.position;
        float distanceToTarget = Vector2.Distance(currentPos, patrolTargetPosition);
        
        if (distanceToTarget < 0.5f)
        {
            // Đã đến điểm patrol, đợi một chút
            isPatrolling = false;
            controller.Move(Vector2.zero);
            
            idleTimer += Time.deltaTime;
            if (idleTimer >= data.idleTime)
            {
                SetNewPatrolTarget();
                idleTimer = 0f;
            }
        }
        else
        {
            isPatrolling = true;
        }
    }
    
    private void SetNewPatrolTarget()
    {
        // Tạo điểm patrol ngẫu nhiên trong bán kính
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        patrolTargetPosition = patrolStartPosition + randomDirection * Random.Range(0f, data.patrolRadius);
    }
}

