using UnityEngine;
using System;

/// <summary>
/// Hệ thống quản lý 4 slot trang bị và đổi slot
/// </summary>
public class EquipmentSystem : MonoBehaviour
{
    [Header("Equipment Slots")]
    [SerializeField] private IWeapon[] slots = new IWeapon[4]; // Slot 0,1,2,3
    
    [Header("Current Active Slot")]
    [SerializeField] private int activeSlotIndex = 0;
    
    [Header("Input Settings")]
    [SerializeField] private bool enableScrollWheel = true;
    [SerializeField] private float autoSwitchDelay = 0.2f;
    
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
    
    private void Start()
    {
        // Lắng nghe thay đổi số lượng item để auto đổi slot khi consumable hết
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAmountChanged += HandleItemAmountChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAmountChanged -= HandleItemAmountChanged;
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
        // Phím số 1, 2, 3, 4
        for (int i = 0; i < slots.Length; i++)
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
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogWarning($"EquipmentSystem: Slot index {slotIndex} không hợp lệ!");
            return;
        }
        
        // Không cho chuyển sang slot trống hoặc consumable đã hết
        if (!IsSlotUsable(slotIndex))
        {
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
    }
    
    /// <summary>
    /// Chuyển sang slot tiếp theo (1 -> 2 -> 3 -> 4 -> 1)
    /// </summary>
    public void SwitchToNextSlot()
    {
        int nextSlot = FindNextUsableSlot(activeSlotIndex + 1, 1);
        if (nextSlot != -1)
        {
            SwitchToSlot(nextSlot);
        }
    }
    
    /// <summary>
    /// Chuyển sang slot trước đó (4 -> 3 -> 2 -> 1 -> 4)
    /// </summary>
    public void SwitchToPreviousSlot()
    {
        int prevSlot = FindNextUsableSlot(activeSlotIndex - 1, -1);
        if (prevSlot != -1)
        {
            SwitchToSlot(prevSlot);
        }
    }
    
    /// <summary>
    /// Gán weapon vào slot
    /// </summary>
    public void SetWeaponInSlot(int slotIndex, IWeapon weapon)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
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
        if (slotIndex < 0 || slotIndex >= slots.Length)
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

    /// <summary>
    /// Được gọi khi số lượng một loại item thay đổi trong InventorySystem
    /// Dùng để auto nhảy slot khi consumable trong slot hiện tại đã hết
    /// </summary>
    private void HandleItemAmountChanged(WeaponType itemType, int newAmount)
    {
        // Chỉ quan tâm khi số lượng về 0
        if (newAmount > 0) return;

        if (CurrentWeapon is IConsumableWeapon consumableWeapon)
        {
            // Đảm bảo đây đúng là loại weapon đang active và đã hết ammo
            if (CurrentWeapon.Type == itemType && !consumableWeapon.HasAmmo())
            {
                // Thêm delay trước khi auto chuyển slot
                StartCoroutine(AutoSwitchFromDepletedConsumableWithDelay());
            }
        }
    }

    /// <summary>
    /// Auto chuyển slot khi slot hiện tại là consumable đã hết:
    /// - Ưu tiên slot khác còn consumable (HasAmmo() == true)
    /// - Nếu không có slot nào còn item thì tự động về slot có Gun
    /// </summary>
    private void AutoSwitchFromDepletedConsumable()
    {
        // 1. Tìm slot khác còn consumable có ammo
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == activeSlotIndex) continue;

            IWeapon weapon = slots[i];
            if (weapon is IConsumableWeapon consumable && consumable.HasAmmo())
            {
                SwitchToSlot(i);
                return;
            }
        }

        // 2. Nếu không có consumable nào còn item, tìm slot có Gun
        for (int i = 0; i < slots.Length; i++)
        {
            IWeapon weapon = slots[i];
            if (weapon != null && weapon.Type == WeaponType.Gun)
            {
                SwitchToSlot(i);
                return;
            }
        }

        // 3. Nếu không tìm thấy Gun trong slots thì không làm gì thêm
    }

    /// <summary>
    /// Coroutine thêm delay trước khi gọi AutoSwitchFromDepletedConsumable
    /// </summary>
    private System.Collections.IEnumerator AutoSwitchFromDepletedConsumableWithDelay()
    {
        if (autoSwitchDelay > 0f)
        {
            yield return new WaitForSeconds(autoSwitchDelay);
        }

        // Sau delay, kiểm tra lại xem weapon hiện tại vẫn là consumable và đã hết ammo
        if (CurrentWeapon is IConsumableWeapon consumableWeapon && !consumableWeapon.HasAmmo())
        {
            AutoSwitchFromDepletedConsumable();
        }
    }

    /// <summary>
    /// Kiểm tra slot có usable hay không (null => lock, consumable hết ammo => lock)
    /// </summary>
    private bool IsSlotUsable(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;

        IWeapon weapon = slots[slotIndex];
        if (weapon == null) return false;

        if (weapon is IConsumableWeapon consumable)
        {
            return consumable.HasAmmo();
        }

        return true; // Non-consumable (Gun) luôn usable
    }

    /// <summary>
    /// Tìm slot usable kế tiếp theo hướng (direction = 1: forward, -1: backward)
    /// Nếu không tìm thấy trả về -1
    /// </summary>
    private int FindNextUsableSlot(int startIndex, int direction)
    {
        int len = slots.Length;
        if (len == 0) return -1;

        // Chuẩn hóa startIndex
        int index = (startIndex % len + len) % len;

        for (int step = 0; step < len; step++)
        {
            int checkIndex = (index + step * direction + len) % len;
            if (checkIndex == activeSlotIndex) continue; // tránh trả về chính slot hiện tại

            if (IsSlotUsable(checkIndex))
            {
                return checkIndex;
            }
        }

        return -1;
    }
}

