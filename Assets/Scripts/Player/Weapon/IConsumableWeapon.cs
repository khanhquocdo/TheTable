using UnityEngine;

/// <summary>
/// Interface cho các weapons có số lượng (consumable) như Grenade, Molotov
/// </summary>
public interface IConsumableWeapon : IWeapon
{
    /// <summary>
    /// Lấy số lượng hiện tại từ InventorySystem
    /// </summary>
    int GetCurrentAmount();
    
    /// <summary>
    /// Lấy số lượng tối đa
    /// </summary>
    int GetMaxStack();
    
    /// <summary>
    /// Kiểm tra có thể sử dụng không (có số lượng > 0)
    /// </summary>
    bool HasAmmo();
}
