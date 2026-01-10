using UnityEngine;
using System;

/// <summary>
/// Hệ thống quản lý 3 slot trang bị và đổi slot
/// </summary>
public class EquipmentSystem : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private IWeapon[] slots = new IWeapon[3]; // Slot 0, 1, 2
    
    [Header("Current Active Slot")]
    [SerializeField] private int activeSlotIndex = 0;
    
    [Header("Input Settings")]
    [SerializeField] private bool enableScrollWheel = true;
    
    // Events
    public event Action<int, IWeapon> OnSlotChanged; // slotIndex, weapon
    public event Action<IWeapon> OnWeaponEquipped;
    public event Action<IWeapon> OnWeaponUnequipped;
    
    // Singleton pattern (optional)
    public static EquipmentSystem Instance { get; private set; }
    
    // Properties
    public IWeapon CurrentWeapon => slots[activeSlotIndex];
    public int ActiveSlotIndex => activeSlotIndex;
    
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
    }
    
    void Update()
    {
        HandleSlotSwitching();
    }
    
    /// <summary>
    /// Xử lý đổi slot bằng phím số và scroll wheel
    /// </summary>
    private void HandleSlotSwitching()
    {
        // Phím số 1, 2, 3
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SwitchToSlot(i);
                return;
            }
        }
        
        // Scroll wheel
        if (enableScrollWheel)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f) // Scroll up
            {
                SwitchToNextSlot();
            }
            else if (scroll < 0f) // Scroll down
            {
                SwitchToPreviousSlot();
            }
        }
    }
    
    /// <summary>
    /// Chuyển sang slot cụ thể
    /// </summary>
    public void SwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3)
        {
            Debug.LogWarning($"EquipmentSystem: Slot index {slotIndex} không hợp lệ!");
            return;
        }
        
        if (slotIndex == activeSlotIndex)
        {
            return; // Đã ở slot này rồi
        }
        
        // Unequip weapon cũ
        if (slots[activeSlotIndex] != null)
        {
            slots[activeSlotIndex].Unequip();
            OnWeaponUnequipped?.Invoke(slots[activeSlotIndex]);
        }
        
        // Đổi slot
        activeSlotIndex = slotIndex;
        
        // Equip weapon mới
        if (slots[activeSlotIndex] != null)
        {
            slots[activeSlotIndex].Equip();
            OnWeaponEquipped?.Invoke(slots[activeSlotIndex]);
        }
        
        // Gọi event
        OnSlotChanged?.Invoke(activeSlotIndex, slots[activeSlotIndex]);
        
        Debug.Log($"Switched to slot {activeSlotIndex + 1}");
    }
    
    /// <summary>
    /// Chuyển sang slot tiếp theo (1 -> 2 -> 3 -> 1)
    /// </summary>
    public void SwitchToNextSlot()
    {
        int nextSlot = (activeSlotIndex + 1) % 3;
        SwitchToSlot(nextSlot);
    }
    
    /// <summary>
    /// Chuyển sang slot trước đó (3 -> 2 -> 1 -> 3)
    /// </summary>
    public void SwitchToPreviousSlot()
    {
        int prevSlot = (activeSlotIndex - 1 + 3) % 3;
        SwitchToSlot(prevSlot);
    }
    
    /// <summary>
    /// Gán weapon vào slot
    /// </summary>
    public void SetWeaponInSlot(int slotIndex, IWeapon weapon)
    {
        if (slotIndex < 0 || slotIndex >= 3)
        {
            Debug.LogWarning($"EquipmentSystem: Slot index {slotIndex} không hợp lệ!");
            return;
        }
        
        // Nếu slot đang active và có weapon cũ, unequip nó
        if (slotIndex == activeSlotIndex && slots[slotIndex] != null)
        {
            slots[slotIndex].Unequip();
            OnWeaponUnequipped?.Invoke(slots[slotIndex]);
        }
        
        slots[slotIndex] = weapon;
        
        // Nếu slot đang active, equip weapon mới
        if (slotIndex == activeSlotIndex && weapon != null)
        {
            weapon.Equip();
            OnWeaponEquipped?.Invoke(weapon);
        }
        
        OnSlotChanged?.Invoke(slotIndex, weapon);
    }
    
    /// <summary>
    /// Lấy weapon từ slot
    /// </summary>
    public IWeapon GetWeaponInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3)
        {
            return null;
        }
        return slots[slotIndex];
    }
    
    /// <summary>
    /// Sử dụng weapon hiện tại
    /// </summary>
    public void UseCurrentWeapon(Vector2 direction, Vector2 position)
    {
        if (CurrentWeapon != null && CurrentWeapon.CanUse())
        {
            CurrentWeapon.Use(direction, position);
        }
    }
    
    /// <summary>
    /// Kiểm tra xem weapon hiện tại có đang lock movement không
    /// </summary>
    public bool IsCurrentWeaponLockingMovement()
    {
        if (CurrentWeapon == null) return false;
        return CurrentWeapon.IsLockingMovement();
    }
}

