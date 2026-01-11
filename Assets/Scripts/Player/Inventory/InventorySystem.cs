using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Hệ thống quản lý inventory - lưu số lượng các items như Grenade, Molotov
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class ItemData
    {
        public WeaponType itemType;
        public int amount;
        public int maxStack;
        
        public ItemData(WeaponType type, int maxStack)
        {
            this.itemType = type;
            this.amount = 0;
            this.maxStack = maxStack;
        }
    }
    
    [Header("Item Settings")]
    [SerializeField] private int grenadeMaxStack = 10;
    [SerializeField] private int molotovMaxStack = 10;
    
    // Dictionary để lưu số lượng items
    private Dictionary<WeaponType, ItemData> items = new Dictionary<WeaponType, ItemData>();
    
    // Events
    public event Action<WeaponType, int> OnItemAmountChanged; // itemType, newAmount
    public event Action<WeaponType> OnItemAdded; // itemType
    public event Action<WeaponType> OnItemUsed; // itemType
    
    // Singleton pattern
    public static InventorySystem Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeItems();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// Khởi tạo items trong inventory
    /// </summary>
    private void InitializeItems()
    {
        items[WeaponType.Grenade] = new ItemData(WeaponType.Grenade, grenadeMaxStack);
        items[WeaponType.Molotov] = new ItemData(WeaponType.Molotov, molotovMaxStack);
        
        // Có thể thêm các items khác sau này
        // items[WeaponType.SmokeBomb] = new ItemData(WeaponType.SmokeBomb, smokeBombMaxStack);
        // items[WeaponType.FlashBomb] = new ItemData(WeaponType.FlashBomb, flashBombMaxStack);
    }
    
    /// <summary>
    /// Thêm item vào inventory
    /// </summary>
    /// <param name="itemType">Loại item</param>
    /// <param name="amount">Số lượng thêm vào</param>
    /// <returns>Số lượng thực sự được thêm vào</returns>
    public int AddItem(WeaponType itemType, int amount = 1)
    {
        if (!items.ContainsKey(itemType))
        {
            Debug.LogWarning($"InventorySystem: Không tìm thấy item type {itemType}!");
            return 0;
        }
        
        ItemData itemData = items[itemType];
        int oldAmount = itemData.amount;
        int addedAmount = Mathf.Min(amount, itemData.maxStack - itemData.amount);
        
        itemData.amount += addedAmount;
        
        OnItemAmountChanged?.Invoke(itemType, itemData.amount);
        
        if (addedAmount > 0)
        {
            OnItemAdded?.Invoke(itemType);
            Debug.Log($"Added {addedAmount} {itemType}. Total: {itemData.amount}/{itemData.maxStack}");
        }
        else if (itemData.amount >= itemData.maxStack)
        {
            Debug.Log($"{itemType} đã đạt max stack ({itemData.maxStack})!");
        }
        
        return addedAmount;
    }
    
    /// <summary>
    /// Sử dụng item (giảm số lượng)
    /// </summary>
    /// <param name="itemType">Loại item</param>
    /// <param name="amount">Số lượng sử dụng</param>
    /// <returns>True nếu sử dụng thành công</returns>
    public bool UseItem(WeaponType itemType, int amount = 1)
    {
        if (!items.ContainsKey(itemType))
        {
            Debug.LogWarning($"InventorySystem: Không tìm thấy item type {itemType}!");
            return false;
        }
        
        ItemData itemData = items[itemType];
        
        if (itemData.amount < amount)
        {
            Debug.LogWarning($"InventorySystem: Không đủ {itemType}! Cần {amount}, có {itemData.amount}");
            return false;
        }
        
        itemData.amount -= amount;
        OnItemAmountChanged?.Invoke(itemType, itemData.amount);
        OnItemUsed?.Invoke(itemType);
        
        Debug.Log($"Used {amount} {itemType}. Remaining: {itemData.amount}/{itemData.maxStack}");
        return true;
    }
    
    /// <summary>
    /// Lấy số lượng item hiện tại
    /// </summary>
    public int GetItemAmount(WeaponType itemType)
    {
        if (!items.ContainsKey(itemType))
        {
            return 0;
        }
        return items[itemType].amount;
    }
    
    /// <summary>
    /// Kiểm tra có đủ số lượng item không
    /// </summary>
    public bool HasItem(WeaponType itemType, int amount = 1)
    {
        return GetItemAmount(itemType) >= amount;
    }
    
    /// <summary>
    /// Lấy max stack của item
    /// </summary>
    public int GetMaxStack(WeaponType itemType)
    {
        if (!items.ContainsKey(itemType))
        {
            return 0;
        }
        return items[itemType].maxStack;
    }
    
    /// <summary>
    /// Kiểm tra inventory có đầy không
    /// </summary>
    public bool IsFull(WeaponType itemType)
    {
        if (!items.ContainsKey(itemType))
        {
            return false;
        }
        ItemData itemData = items[itemType];
        return itemData.amount >= itemData.maxStack;
    }
    
    /// <summary>
    /// Set số lượng item (dùng cho testing hoặc save/load)
    /// </summary>
    public void SetItemAmount(WeaponType itemType, int amount)
    {
        if (!items.ContainsKey(itemType))
        {
            Debug.LogWarning($"InventorySystem: Không tìm thấy item type {itemType}!");
            return;
        }
        
        ItemData itemData = items[itemType];
        itemData.amount = Mathf.Clamp(amount, 0, itemData.maxStack);
        OnItemAmountChanged?.Invoke(itemType, itemData.amount);
    }
}
