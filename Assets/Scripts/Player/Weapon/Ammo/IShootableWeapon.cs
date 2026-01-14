using UnityEngine;

/// <summary>
/// Interface cho các vũ khí có thể bắn và sử dụng ammo system
/// </summary>
public interface IShootableWeapon : IWeapon
{
    /// <summary>
    /// Loại đạn mà vũ khí này sử dụng
    /// </summary>
    AmmoType AmmoType { get; }
    
    /// <summary>
    /// WeaponAmmoData của vũ khí này
    /// </summary>
    WeaponAmmoData WeaponAmmoData { get; }
    
    /// <summary>
    /// Chế độ bắn hiện tại
    /// </summary>
    FireMode FireMode { get; }
}
