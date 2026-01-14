using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin ammo của một vũ khí cụ thể
/// Mỗi vũ khí sẽ có một WeaponAmmoData riêng
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Ammo Data", menuName = "Weapon/Weapon Ammo Data")]
public class WeaponAmmoData : ScriptableObject
{
    [Header("Weapon Info")]
    [Tooltip("Loại vũ khí này sử dụng loại đạn gì")]
    public AmmoType ammoType = AmmoType.Pistol;
    
    [Header("Initial Ammo")]
    [Tooltip("Số đạn ban đầu trong băng khi spawn")]
    public int initialMagazineAmmo = 30;
    
    [Tooltip("Số đạn dự trữ ban đầu khi spawn")]
    public int initialReserveAmmo = 90;
    
    [Header("Fire Mode")]
    [Tooltip("Chế độ bắn: Single, Auto, Burst")]
    public FireMode fireMode = FireMode.Auto;
    
    [Tooltip("Số viên bắn trong một burst (chỉ dùng khi fireMode = Burst)")]
    public int burstCount = 3;
    
    [Tooltip("Thời gian giữa các viên trong một burst (giây)")]
    public float burstInterval = 0.1f;
}

/// <summary>
/// Enum định nghĩa các chế độ bắn
/// </summary>
public enum FireMode
{
    Single,  // Bắn từng viên một (phải nhấn lại mỗi lần)
    Auto,    // Bắn liên tục khi giữ chuột
    Burst    // Bắn theo chùm (3 viên mỗi lần nhấn)
}
