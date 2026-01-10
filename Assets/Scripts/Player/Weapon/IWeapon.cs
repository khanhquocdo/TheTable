using UnityEngine;

/// <summary>
/// Interface cho tất cả các loại vũ khí (Gun, Grenade, Molotov, ...)
/// </summary>
public interface IWeapon
{
    /// <summary>
    /// Tên vũ khí (để hiển thị UI)
    /// </summary>
    string WeaponName { get; }
    
    /// <summary>
    /// Icon vũ khí (để hiển thị UI)
    /// </summary>
    Sprite WeaponIcon { get; }
    
    /// <summary>
    /// Loại vũ khí (để phân biệt)
    /// </summary>
    WeaponType Type { get; }
    
    /// <summary>
    /// Equip vũ khí (được gọi khi chuyển sang slot này)
    /// </summary>
    void Equip();
    
    /// <summary>
    /// Unequip vũ khí (được gọi khi chuyển sang slot khác)
    /// </summary>
    void Unequip();
    
    /// <summary>
    /// Sử dụng vũ khí (bắn/ném)
    /// </summary>
    /// <param name="direction">Hướng tấn công (từ player đến chuột)</param>
    /// <param name="position">Vị trí xuất phát</param>
    void Use(Vector2 direction, Vector2 position);
    
    /// <summary>
    /// Kiểm tra xem có thể sử dụng vũ khí không (cooldown, ammo, ...)
    /// </summary>
    bool CanUse();
    
    /// <summary>
    /// Kiểm tra xem vũ khí có đang trong trạng thái lock movement không
    /// </summary>
    bool IsLockingMovement();
}

/// <summary>
/// Enum định nghĩa các loại vũ khí
/// </summary>
public enum WeaponType
{
    Gun,
    Grenade,
    Molotov,
    SmokeBomb,
    FlashBomb
}

