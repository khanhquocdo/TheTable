using UnityEngine;

/// <summary>
/// State khi Enemy tấn công Player
/// Quay mặt về Player, bắn đạn, và giữ khoảng cách
/// </summary>
public class EnemyAttackState : EnemyState
{
    private float nextFireTime = 0f;
    
    public EnemyAttackState(EnemyController controller, EnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        nextFireTime = 0f; // Có thể bắn ngay khi vào state
    }
    
    public override void Update()
    {
        // Kiểm tra mất Player
        if (!controller.CanDetectPlayer())
        {
            controller.ChangeState(EnemyController.EnemyStateType.Idle);
            return;
        }
        
        if (!controller.HasPlayerTarget())
        {
            return;
        }
        
        Vector2 playerPosition = controller.GetPlayerPosition();
        float distanceToPlayer = controller.GetDistanceToPlayer();
        
        // Quay mặt về Player
        controller.LookAt(playerPosition);
        
        // Kiểm tra Line of Sight
        if (!controller.HasLineOfSightToPlayer())
        {
            // Mất tầm nhìn, đuổi theo
            controller.ChangeState(EnemyController.EnemyStateType.Chase);
            return;
        }
        
        // Giữ khoảng cách
        HandleDistanceControl(distanceToPlayer, playerPosition);
        
        // Bắn đạn
        HandleShooting();
    }
    
    public override void FixedUpdate()
    {
        // Movement được xử lý trong Update() thông qua HandleDistanceControl
    }
    
    private void HandleDistanceControl(float distanceToPlayer, Vector2 playerPosition)
    {
        Vector2 enemyPosition = controller.transform.position;
        Vector2 directionToPlayer = (playerPosition - enemyPosition).normalized;
        
        if (distanceToPlayer < data.minDistance)
        {
            // Quá gần, lùi lại
            controller.Move(-directionToPlayer);
        }
        else if (distanceToPlayer > data.maxAttackDistance)
        {
            // Quá xa, tiến tới
            controller.Move(directionToPlayer);
        }
        else
        {
            // Ở khoảng cách tốt, đứng yên và bắn
            controller.Move(Vector2.zero);
        }
    }
    
    private void HandleShooting()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }
        
        // Tính thời điểm bắn tiếp theo
        float fireInterval = 1f / data.fireRate;
        nextFireTime = Time.time + fireInterval;
        
        // Bắn đạn
        Vector2 playerPosition = controller.GetPlayerPosition();
        Vector2 fireDirection = (playerPosition - (Vector2)controller.transform.position).normalized;
        controller.Shoot(fireDirection);
    }
}

