using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controller quản lý ammo của Player
/// Quản lý nhiều loại đạn khác nhau và magazine/reserve cho mỗi loại
/// </summary>
public class AmmoController : MonoBehaviour
{
    [Header("Ammo Data References")]
    [Tooltip("Danh sách AmmoData cho các loại đạn khác nhau")]
    [SerializeField] private AmmoData[] ammoDataList;
    
    // Dictionary để truy cập nhanh AmmoData theo AmmoType
    private Dictionary<AmmoType, AmmoData> ammoDataDict;
    
    // Dictionary lưu trữ ammo state cho mỗi loại đạn
    // Key: AmmoType, Value: (currentMagazine, currentReserve)
    private Dictionary<AmmoType, AmmoState> ammoStates;
    
    // State của reload
    private bool isReloading = false;
    private AmmoType reloadingAmmoType;
    private Coroutine reloadCoroutine;
    
    // Events
    public event Action<AmmoType, int, int> OnAmmoChanged; // ammoType, magazine, reserve
    public event Action<AmmoType> OnReloadStarted; // ammoType
    public event Action<AmmoType> OnReloadCompleted; // ammoType
    public event Action<AmmoType> OnReloadCancelled; // ammoType
    public event Action<AmmoType> OnOutOfAmmo; // ammoType (khi cả magazine và reserve đều = 0)
    
    // Singleton
    public static AmmoController Instance { get; private set; }
    
    /// <summary>
    /// Struct lưu trữ state của ammo cho một loại đạn
    /// </summary>
    [System.Serializable]
    public struct AmmoState
    {
        public int currentMagazine;
        public int currentReserve;
        
        public AmmoState(int magazine, int reserve)
        {
            currentMagazine = magazine;
            currentReserve = reserve;
        }
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeAmmoData();
        InitializeAmmoStates();
    }
    
    /// <summary>
    /// Khởi tạo dictionary AmmoData
    /// </summary>
    private void InitializeAmmoData()
    {
        ammoDataDict = new Dictionary<AmmoType, AmmoData>();
        
        if (ammoDataList != null)
        {
            foreach (var ammoData in ammoDataList)
            {
                if (ammoData != null && !ammoDataDict.ContainsKey(ammoData.ammoType))
                {
                    ammoDataDict[ammoData.ammoType] = ammoData;
                }
            }
        }
    }
    
    /// <summary>
    /// Khởi tạo ammo states cho tất cả các loại đạn
    /// </summary>
    private void InitializeAmmoStates()
    {
        ammoStates = new Dictionary<AmmoType, AmmoState>();
        
        // Khởi tạo state cho tất cả các loại đạn có trong ammoDataDict
        foreach (var kvp in ammoDataDict)
        {
            AmmoData data = kvp.Value;
            ammoStates[kvp.Key] = new AmmoState(0, 0);
        }
    }
    
    /// <summary>
    /// Khởi tạo ammo cho một loại đạn từ WeaponAmmoData
    /// </summary>
    public void InitializeAmmoForWeapon(WeaponAmmoData weaponAmmoData)
    {
        if (weaponAmmoData == null) return;
        
        AmmoType ammoType = weaponAmmoData.ammoType;
        
        if (!ammoStates.ContainsKey(ammoType))
        {
            ammoStates[ammoType] = new AmmoState(0, 0);
        }
        
        var state = ammoStates[ammoType];
        state.currentMagazine = weaponAmmoData.initialMagazineAmmo;
        state.currentReserve = weaponAmmoData.initialReserveAmmo;
        ammoStates[ammoType] = state;
        
        OnAmmoChanged?.Invoke(ammoType, state.currentMagazine, state.currentReserve);
    }
    
    /// <summary>
    /// Kiểm tra xem có thể bắn không (có đạn trong băng và không đang reload)
    /// </summary>
    public bool CanShoot(AmmoType ammoType)
    {
        if (isReloading && reloadingAmmoType == ammoType)
            return false;
        
        if (!ammoStates.ContainsKey(ammoType))
            return false;
        
        return ammoStates[ammoType].currentMagazine > 0;
    }
    
    /// <summary>
    /// Sử dụng một viên đạn (gọi khi bắn)
    /// </summary>
    public bool ConsumeAmmo(AmmoType ammoType, int amount = 1)
    {
        if (!ammoStates.ContainsKey(ammoType))
            return false;
        
        var state = ammoStates[ammoType];
        
        if (state.currentMagazine < amount)
            return false;
        
        state.currentMagazine -= amount;
        ammoStates[ammoType] = state;
        
        OnAmmoChanged?.Invoke(ammoType, state.currentMagazine, state.currentReserve);
        
        // Kiểm tra nếu hết đạn hoàn toàn
        if (state.currentMagazine == 0 && state.currentReserve == 0)
        {
            OnOutOfAmmo?.Invoke(ammoType);
        }
        
        return true;
    }
    
    /// <summary>
    /// Bắt đầu reload cho một loại đạn
    /// </summary>
    public bool StartReload(AmmoType ammoType)
    {
        if (isReloading)
        {
            // Nếu đang reload loại đạn khác, hủy reload cũ
            if (reloadingAmmoType != ammoType)
            {
                CancelReload();
            }
            else
            {
                // Đang reload cùng loại đạn, không làm gì
                return false;
            }
        }
        
        if (!ammoDataDict.ContainsKey(ammoType))
            return false;
        
        AmmoData ammoData = ammoDataDict[ammoType];
        
        if (!ammoStates.ContainsKey(ammoType))
            return false;
        
        var state = ammoStates[ammoType];
        
        // Kiểm tra có thể reload không
        if (!ammoData.CanReload(state.currentMagazine, state.currentReserve))
            return false;
        
        // Bắt đầu reload
        isReloading = true;
        reloadingAmmoType = ammoType;
        OnReloadStarted?.Invoke(ammoType);
        
        reloadCoroutine = StartCoroutine(ReloadCoroutine(ammoType, ammoData));
        
        return true;
    }
    
    /// <summary>
    /// Hủy reload hiện tại
    /// </summary>
    public void CancelReload()
    {
        if (!isReloading) return;
        
        AmmoType cancelledType = reloadingAmmoType;
        
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        isReloading = false;
        reloadingAmmoType = AmmoType.None;
        
        OnReloadCancelled?.Invoke(cancelledType);
    }
    
    /// <summary>
    /// Coroutine xử lý reload
    /// </summary>
    private IEnumerator ReloadCoroutine(AmmoType ammoType, AmmoData ammoData)
    {
        yield return new WaitForSeconds(ammoData.reloadTime);
        
        // Kiểm tra lại sau khi chờ (có thể đã bị hủy)
        if (!isReloading || reloadingAmmoType != ammoType)
            yield break;
        
        // Thực hiện reload
        var state = ammoStates[ammoType];
        int ammoNeeded = ammoData.maxMagazineSize - state.currentMagazine;
        int ammoToReload = Mathf.Min(ammoNeeded, state.currentReserve);
        
        state.currentMagazine += ammoToReload;
        state.currentReserve -= ammoToReload;
        ammoStates[ammoType] = state;
        
        // Reset reload state
        isReloading = false;
        reloadingAmmoType = AmmoType.None;
        reloadCoroutine = null;
        
        OnReloadCompleted?.Invoke(ammoType);
        OnAmmoChanged?.Invoke(ammoType, state.currentMagazine, state.currentReserve);
    }
    
    /// <summary>
    /// Thêm đạn vào reserve (khi nhặt pickup)
    /// </summary>
    public int AddAmmo(AmmoType ammoType, int amount)
    {
        if (!ammoStates.ContainsKey(ammoType))
        {
            // Nếu chưa có state cho loại đạn này, tạo mới
            ammoStates[ammoType] = new AmmoState(0, 0);
        }
        
        if (!ammoDataDict.ContainsKey(ammoType))
            return 0;
        
        AmmoData ammoData = ammoDataDict[ammoType];
        var state = ammoStates[ammoType];
        
        int oldReserve = state.currentReserve;
        state.currentReserve = Mathf.Min(state.currentReserve + amount, ammoData.maxReserveAmmo);
        int actualAdded = state.currentReserve - oldReserve;
        
        ammoStates[ammoType] = state;
        
        OnAmmoChanged?.Invoke(ammoType, state.currentMagazine, state.currentReserve);
        
        return actualAdded;
    }
    
    /// <summary>
    /// Lấy số đạn hiện tại trong băng
    /// </summary>
    public int GetCurrentMagazine(AmmoType ammoType)
    {
        if (!ammoStates.ContainsKey(ammoType))
            return 0;
        
        return ammoStates[ammoType].currentMagazine;
    }
    
    /// <summary>
    /// Lấy số đạn dự trữ
    /// </summary>
    public int GetCurrentReserve(AmmoType ammoType)
    {
        if (!ammoStates.ContainsKey(ammoType))
            return 0;
        
        return ammoStates[ammoType].currentReserve;
    }
    
    /// <summary>
    /// Lấy số đạn tối đa trong băng
    /// </summary>
    public int GetMaxMagazineSize(AmmoType ammoType)
    {
        if (!ammoDataDict.ContainsKey(ammoType))
            return 0;
        
        return ammoDataDict[ammoType].maxMagazineSize;
    }
    
    /// <summary>
    /// Kiểm tra xem có đang reload không
    /// </summary>
    public bool IsReloading(AmmoType ammoType)
    {
        return isReloading && reloadingAmmoType == ammoType;
    }
    
    /// <summary>
    /// Kiểm tra xem có thể reload không
    /// </summary>
    public bool CanReload(AmmoType ammoType)
    {
        if (isReloading && reloadingAmmoType == ammoType)
            return false;
        
        if (!ammoDataDict.ContainsKey(ammoType))
            return false;
        
        if (!ammoStates.ContainsKey(ammoType))
            return false;
        
        AmmoData ammoData = ammoDataDict[ammoType];
        var state = ammoStates[ammoType];
        
        return ammoData.CanReload(state.currentMagazine, state.currentReserve);
    }
    
    /// <summary>
    /// Lấy AmmoData cho một loại đạn
    /// </summary>
    public AmmoData GetAmmoData(AmmoType ammoType)
    {
        if (ammoDataDict.ContainsKey(ammoType))
            return ammoDataDict[ammoType];
        
        return null;
    }
}
