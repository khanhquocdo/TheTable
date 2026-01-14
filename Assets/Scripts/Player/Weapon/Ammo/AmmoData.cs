using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin về một loại đạn cụ thể
/// </summary>
[CreateAssetMenu(fileName = "New Ammo Data", menuName = "Weapon/Ammo Data")]
public class AmmoData : ScriptableObject
{
    [Header("Ammo Type")]
    [Tooltip("Loại đạn này")]
    public AmmoType ammoType = AmmoType.Pistol;
    
    [Header("Magazine Settings")]
    [Tooltip("Số đạn tối đa trong một băng đạn")]
    public int maxMagazineSize = 30;
    
    [Tooltip("Số đạn dự trữ tối đa có thể mang")]
    public int maxReserveAmmo = 120;
    
    [Header("Reload Settings")]
    [Tooltip("Thời gian reload (giây)")]
    public float reloadTime = 2f;
    
    [Tooltip("Có cho phép reload khi băng đạn chưa hết không?")]
    public bool allowPartialReload = true;
    
    [Header("Display")]
    [Tooltip("Tên hiển thị của loại đạn")]
    public string displayName = "Pistol Ammo";
    
    [Tooltip("Icon của loại đạn (để hiển thị UI)")]
    public Sprite ammoIcon;
    
    /// <summary>
    /// Kiểm tra xem có thể reload không
    /// </summary>
    public bool CanReload(int currentMagazine, int currentReserve)
    {
        // Không thể reload nếu không có đạn dự trữ
        if (currentReserve <= 0)
            return false;
        
        // Nếu băng đạn đã đầy thì không cần reload
        if (currentMagazine >= maxMagazineSize)
            return false;
        
        // Nếu không cho phép partial reload và băng đạn chưa hết thì không reload
        if (!allowPartialReload && currentMagazine > 0)
            return false;
        
        return true;
    }
}
