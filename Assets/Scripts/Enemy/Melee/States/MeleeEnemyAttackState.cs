using UnityEngine;
using System.Collections;

/// <summary>
/// State khi Melee Enemy tấn công Player
/// Dừng lại, quay mặt về Player, chạy animation Attack, và gây damage
/// </summary>
public class MeleeEnemyAttackState : MeleeEnemyState
{
    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    
    public MeleeEnemyAttackState(MeleeEnemyController controller, MeleeEnemyData data) : base(controller, data)
    {
    }
    
    public override void OnEnter()
    {
        nextAttackTime = 0f; // Có thể tấn công ngay khi vào state
        isAttacking = false;
    }
    
    public override void OnExit()
    {
        // Dừng coroutine tấn công nếu đang chạy
        if (attackCoroutine != null && controller != null)
        {
            controller.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        
        isAttacking = false;
    }
    
    public override void Update()
    {
        // Kiểm tra mất Player
        if (!controller.CanDetectPlayer())
        {
            controller.ChangeState(MeleeEnemyController.MeleeEnemyStateType.Idle);
            return;
        }
        
        if (!controller.HasPlayerTarget())
        {
            return;
        }
        
        Vector2 playerPosition = controller.GetPlayerPosition();
        float distanceToPlayer = controller.GetDistanceToPlayer();
        
        // Quay mặt về Player (để animation đúng hướng)
        controller.LookAt(playerPosition);
        
        // Kiểm tra Line of Sight
        if (!controller.HasLineOfSightToPlayer())
        {
            // Mất tầm nhìn, đuổi theo
            controller.ChangeState(MeleeEnemyController.MeleeEnemyStateType.Chase);
            return;
        }
        
        // Nếu quá xa, đuổi theo
        if (distanceToPlayer > data.attackRange * 1.2f) // Thêm buffer để tránh nhấp nháy
        {
            controller.ChangeState(MeleeEnemyController.MeleeEnemyStateType.Chase);
            return;
        }
        
        // Dừng lại khi tấn công
        controller.Move(Vector2.zero);
        
        // Xử lý tấn công
        HandleAttack();
    }
    
    public override void FixedUpdate()
    {
        // Movement được xử lý trong Update() - dừng lại khi tấn công
    }
    
    private void HandleAttack()
    {
        // Kiểm tra cooldown
        if (Time.time < nextAttackTime)
        {
            return;
        }
        
        // Kiểm tra không đang tấn công
        if (isAttacking)
        {
            return;
        }
        
        // Bắt đầu tấn công
        if (controller != null)
        {
            attackCoroutine = controller.StartCoroutine(PerformAttackSequence());
        }
    }
    
    /// <summary>
    /// Thực hiện chuỗi tấn công: Wind-up -> Attack -> Cooldown
    /// </summary>
    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;
        
        // Set trạng thái tấn công cho animation
        controller.SetIsAttacking(true);
        
        // Wind-up time
        yield return new WaitForSeconds(data.windUpTime);
        
        // Thực hiện tấn công (gây damage)
        Vector2 playerPosition = controller.GetPlayerPosition();
        Vector2 attackDirection = (playerPosition - (Vector2)controller.transform.position).normalized;
        
        if (controller.MeleeAttack != null)
        {
            controller.MeleeAttack.PerformAttack(attackDirection);
        }
        
        // Đợi animation tấn công hoàn thành
        yield return new WaitForSeconds(data.attackDuration - data.windUpTime);
        
        // Kết thúc tấn công
        isAttacking = false;
        controller.SetIsAttacking(false);
        
        // Tính thời điểm tấn công tiếp theo
        nextAttackTime = Time.time + data.attackCooldown;
        
        attackCoroutine = null;
    }
}
