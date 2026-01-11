using UnityEngine;

/// <summary>
/// Base class cho tất cả các state của Enemy
/// Sử dụng State Pattern để quản lý hành vi AI
/// </summary>
public abstract class EnemyState
{
    protected EnemyController controller;
    protected EnemyData data;
    
    public EnemyState(EnemyController controller, EnemyData data)
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

