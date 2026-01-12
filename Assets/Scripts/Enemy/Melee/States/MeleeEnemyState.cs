using UnityEngine;

/// <summary>
/// Base class cho tất cả các state của Melee Enemy
/// Sử dụng State Pattern để quản lý hành vi AI
/// </summary>
public abstract class MeleeEnemyState
{
    protected MeleeEnemyController controller;
    protected MeleeEnemyData data;
    
    public MeleeEnemyState(MeleeEnemyController controller, MeleeEnemyData data)
    {
        this.controller = controller;
        this.data = data;
    }
    
    /// <summary>
    /// Được gọi khi vào state này
    /// </summary>
    public virtual void OnEnter() { }
    
    /// <summary>
    /// Được gọi mỗi frame khi đang ở state này
    /// </summary>
    public abstract void Update();
    
    /// <summary>
    /// Được gọi mỗi FixedUpdate khi đang ở state này
    /// </summary>
    public virtual void FixedUpdate() { }
    
    /// <summary>
    /// Được gọi khi rời khỏi state này
    /// </summary>
    public virtual void OnExit() { }
}
