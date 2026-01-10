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
        
        // Khởi tạo UI ban đầu
        UpdateAllSlots();
    }
    
    void OnDestroy()
    {
        if (equipmentSystem != null)
        {
            equipmentSystem.OnSlotChanged -= OnSlotChanged;
        }
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
}

