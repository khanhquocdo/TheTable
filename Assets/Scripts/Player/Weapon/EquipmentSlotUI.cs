using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI hiển thị 3 slot trang bị với icon và highlight
/// </summary>
public class EquipmentSlotUI : MonoBehaviour
{
    [Header("Slot UI References")]
    [SerializeField] private Image[] slotIcons = new Image[3]; // Icon của từng slot
    [SerializeField] private Image[] slotBorders = new Image[3]; // Viền highlight của từng slot
    [SerializeField] private TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[3]; // Số slot (1, 2, 3)
    [SerializeField] private TextMeshProUGUI[] slotAmountTexts = new TextMeshProUGUI[3]; // Hiển thị số lượng item (Grenade, Molotov)
    
    [Header("Highlight Settings")]
    [SerializeField] private Color normalBorderColor = Color.white;
    [SerializeField] private Color activeBorderColor = Color.yellow;
    [SerializeField] private float borderThickness = 2f;
    
    private EquipmentSystem equipmentSystem;
    
    void Start()
    {
        equipmentSystem = EquipmentSystem.Instance;
        
        if (equipmentSystem == null)
        {
            Debug.LogError("EquipmentSlotUI: Không tìm thấy EquipmentSystem!");
            return;
        }
        
        // Subscribe events
        equipmentSystem.OnSlotChanged += OnSlotChanged;
        
        // Subscribe to inventory events để cập nhật số lượng
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAmountChanged += OnItemAmountChanged;
        }
        
        // Khởi tạo UI ban đầu
        UpdateAllSlots();
    }
    
    void OnDestroy()
    {
        if (equipmentSystem != null)
        {
            equipmentSystem.OnSlotChanged -= OnSlotChanged;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnItemAmountChanged -= OnItemAmountChanged;
        }
    }
    
    /// <summary>
    /// Cập nhật UI khi số lượng item thay đổi
    /// </summary>
    private void OnItemAmountChanged(WeaponType itemType, int newAmount)
    {
        // Cập nhật tất cả slots để kiểm tra xem slot nào đang chứa item này
        UpdateAllSlots();
    }
    
    /// <summary>
    /// Cập nhật UI khi slot thay đổi
    /// </summary>
    private void OnSlotChanged(int slotIndex, IWeapon weapon)
    {
        UpdateAllSlots();
    }
    
    /// <summary>
    /// Cập nhật tất cả slots UI
    /// </summary>
    private void UpdateAllSlots()
    {
        if (equipmentSystem == null) return;
        
        for (int i = 0; i < 3; i++)
        {
            UpdateSlotUI(i);
        }
    }
    
    /// <summary>
    /// Cập nhật UI của một slot cụ thể
    /// </summary>
    private void UpdateSlotUI(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 3) return;
        
        IWeapon weapon = equipmentSystem.GetWeaponInSlot(slotIndex);
        bool isActive = equipmentSystem.ActiveSlotIndex == slotIndex;
        
        // Cập nhật icon
        if (slotIcons[slotIndex] != null)
        {
            if (weapon != null && weapon.WeaponIcon != null)
            {
                slotIcons[slotIndex].sprite = weapon.WeaponIcon;
                slotIcons[slotIndex].color = Color.white;
            }
            else
            {
                slotIcons[slotIndex].sprite = null;
                slotIcons[slotIndex].color = new Color(1f, 1f, 1f, 0.3f); // Mờ đi nếu không có weapon
            }
        }
        
        // Cập nhật viền highlight
        if (slotBorders[slotIndex] != null)
        {
            slotBorders[slotIndex].color = isActive ? activeBorderColor : normalBorderColor;
            
            // Tăng độ dày viền nếu active
            RectTransform borderRect = slotBorders[slotIndex].rectTransform;
            if (isActive)
            {
                borderRect.sizeDelta = new Vector2(borderThickness * 2, borderThickness * 2);
            }
            else
            {
                borderRect.sizeDelta = Vector2.zero;
            }
        }
        
        // Cập nhật số slot
        if (slotNumbers[slotIndex] != null)
        {
            slotNumbers[slotIndex].text = (slotIndex + 1).ToString();
            slotNumbers[slotIndex].color = isActive ? activeBorderColor : normalBorderColor;
        }
        
        // Cập nhật số lượng item (cho consumable weapons)
        UpdateSlotAmount(slotIndex, weapon, isActive);
    }
    
    /// <summary>
    /// Cập nhật hiển thị số lượng item cho slot
    /// </summary>
    private void UpdateSlotAmount(int slotIndex, IWeapon weapon, bool isActive)
    {
        if (slotAmountTexts[slotIndex] == null) return;
        
        // Kiểm tra nếu weapon là consumable (Grenade, Molotov)
        if (weapon is IConsumableWeapon consumableWeapon)
        {
            int amount = consumableWeapon.GetCurrentAmount();
            int maxStack = consumableWeapon.GetMaxStack();
            
            // Hiển thị số lượng
            slotAmountTexts[slotIndex].text = amount > 0 ? amount.ToString() : "0";
            
            // Đổi màu dựa trên số lượng
            if (amount == 0)
            {
                slotAmountTexts[slotIndex].color = Color.red; // Màu đỏ khi hết
            }
            else if (amount <= maxStack * 0.3f)
            {
                slotAmountTexts[slotIndex].color = Color.yellow; // Màu vàng khi sắp hết
            }
            else
            {
                slotAmountTexts[slotIndex].color = Color.white; // Màu trắng khi đủ
            }
            
            slotAmountTexts[slotIndex].gameObject.SetActive(true);
            
            // Làm mờ icon nếu số lượng = 0
            if (slotIcons[slotIndex] != null)
            {
                slotIcons[slotIndex].color = amount > 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
        else
        {
            // Ẩn số lượng cho non-consumable weapons (Gun)
            slotAmountTexts[slotIndex].gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Gán reference cho slot UI (có thể gọi từ Inspector hoặc code)
    /// </summary>
    public void SetSlotIcon(int slotIndex, Image iconImage)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            slotIcons[slotIndex] = iconImage;
        }
    }
    
    public void SetSlotBorder(int slotIndex, Image borderImage)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            slotBorders[slotIndex] = borderImage;
        }
    }
    
    public void SetSlotNumber(int slotIndex, TextMeshProUGUI numberText)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            slotNumbers[slotIndex] = numberText;
        }
    }
    
    public void SetSlotAmountText(int slotIndex, TextMeshProUGUI amountText)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            slotAmountTexts[slotIndex] = amountText;
        }
    }
}

