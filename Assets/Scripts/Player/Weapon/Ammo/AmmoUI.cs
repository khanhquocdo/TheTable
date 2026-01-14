using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI hiển thị thông tin ammo của weapon hiện tại
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text hiển thị số đạn trong băng")]
    public TextMeshProUGUI magazineAmmoText;
    
    [Tooltip("Text hiển thị số đạn dự trữ")]
    public TextMeshProUGUI reserveAmmoText;
    
    [Tooltip("Text hiển thị trạng thái reload (optional)")]
    public TextMeshProUGUI reloadStatusText;
    
    [Tooltip("Image hiển thị icon loại đạn (optional)")]
    public Image ammoIconImage;
    
    [Tooltip("Slider hiển thị tiến trình reload (optional)")]
    public Slider reloadProgressSlider;
    
    [Header("Display Settings")]
    [Tooltip("Format hiển thị: {0} = magazine, {1} = reserve")]
    public string ammoFormat = "{0} / {1}";
    
    [Tooltip("Text hiển thị khi đang reload")]
    public string reloadingText = "RELOADING...";
    
    [Tooltip("Text hiển thị khi hết đạn")]
    public string outOfAmmoText = "OUT OF AMMO";
    
    [Tooltip("Màu khi đang reload")]
    public Color reloadingColor = Color.yellow;
    
    [Tooltip("Màu khi hết đạn")]
    public Color outOfAmmoColor = Color.red;
    
    [Tooltip("Màu bình thường")]
    public Color normalColor = Color.white;
    
    private AmmoType currentAmmoType = AmmoType.None;
    private bool isSubscribed = false;
    private float reloadStartTime = 0f;
    private float reloadDuration = 0f;
    
    void Start()
    {
        SubscribeToEvents();
    }
    
    void OnEnable()
    {
        SubscribeToEvents();
        UpdateUI();
    }
    
    void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    void Update()
    {
        // Cập nhật reload progress slider nếu có
        if (reloadProgressSlider != null && AmmoController.Instance != null && currentAmmoType != AmmoType.None)
        {
            if (AmmoController.Instance.IsReloading(currentAmmoType))
            {
                if (reloadDuration > 0f)
                {
                    float elapsed = Time.time - reloadStartTime;
                    float progress = Mathf.Clamp01(elapsed / reloadDuration);
                    reloadProgressSlider.value = progress;
                    reloadProgressSlider.gameObject.SetActive(true);
                }
            }
            else
            {
                reloadProgressSlider.gameObject.SetActive(false);
            }
        }
    }
    
    void SubscribeToEvents()
    {
        if (isSubscribed) return;
        
        if (EquipmentSystem.Instance != null)
        {
            EquipmentSystem.Instance.OnSlotChanged += OnWeaponChanged;
            EquipmentSystem.Instance.OnWeaponEquipped += OnWeaponEquipped;
            isSubscribed = true;
        }
        
        if (AmmoController.Instance != null)
        {
            AmmoController.Instance.OnAmmoChanged += OnAmmoChanged;
            AmmoController.Instance.OnReloadStarted += OnReloadStarted;
            AmmoController.Instance.OnReloadCompleted += OnReloadCompleted;
            AmmoController.Instance.OnReloadCancelled += OnReloadCancelled;
        }
    }
    
    void UnsubscribeFromEvents()
    {
        if (!isSubscribed) return;
        
        if (EquipmentSystem.Instance != null)
        {
            EquipmentSystem.Instance.OnSlotChanged -= OnWeaponChanged;
            EquipmentSystem.Instance.OnWeaponEquipped -= OnWeaponEquipped;
        }
        
        if (AmmoController.Instance != null)
        {
            AmmoController.Instance.OnAmmoChanged -= OnAmmoChanged;
            AmmoController.Instance.OnReloadStarted -= OnReloadStarted;
            AmmoController.Instance.OnReloadCompleted -= OnReloadCompleted;
            AmmoController.Instance.OnReloadCancelled -= OnReloadCancelled;
        }
        
        isSubscribed = false;
    }
    
    void OnWeaponChanged(int slotIndex, IWeapon weapon)
    {
        UpdateCurrentAmmoType();
        UpdateUI();
    }
    
    void OnWeaponEquipped(IWeapon weapon)
    {
        UpdateCurrentAmmoType();
        UpdateUI();
    }
    
    void OnAmmoChanged(AmmoType ammoType, int magazine, int reserve)
    {
        if (ammoType == currentAmmoType)
        {
            UpdateAmmoDisplay(magazine, reserve);
        }
    }
    
    void OnReloadStarted(AmmoType ammoType)
    {
        if (ammoType == currentAmmoType)
        {
            AmmoData ammoData = AmmoController.Instance.GetAmmoData(ammoType);
            if (ammoData != null)
            {
                reloadDuration = ammoData.reloadTime;
                reloadStartTime = Time.time;
            }
            
            UpdateReloadStatus(true);
        }
    }
    
    void OnReloadCompleted(AmmoType ammoType)
    {
        if (ammoType == currentAmmoType)
        {
            UpdateReloadStatus(false);
            UpdateUI();
        }
    }
    
    void OnReloadCancelled(AmmoType ammoType)
    {
        if (ammoType == currentAmmoType)
        {
            UpdateReloadStatus(false);
        }
    }
    
    void UpdateCurrentAmmoType()
    {
        currentAmmoType = AmmoType.None;
        
        if (EquipmentSystem.Instance != null && EquipmentSystem.Instance.CurrentWeapon != null)
        {
            if (EquipmentSystem.Instance.CurrentWeapon is IShootableWeapon shootableWeapon)
            {
                currentAmmoType = shootableWeapon.AmmoType;
            }
        }
    }
    
    void UpdateUI()
    {
        if (currentAmmoType == AmmoType.None)
        {
            // Không có weapon hoặc weapon không dùng ammo
            if (magazineAmmoText != null)
                magazineAmmoText.text = "";
            
            if (reserveAmmoText != null)
                reserveAmmoText.text = "";
            
            if (reloadStatusText != null)
                reloadStatusText.text = "";
            
            if (ammoIconImage != null)
                ammoIconImage.gameObject.SetActive(false);
            
            return;
        }
        
        if (AmmoController.Instance == null) return;
        
        int magazine = AmmoController.Instance.GetCurrentMagazine(currentAmmoType);
        int reserve = AmmoController.Instance.GetCurrentReserve(currentAmmoType);
        int maxMagazine = AmmoController.Instance.GetMaxMagazineSize(currentAmmoType);
        
        UpdateAmmoDisplay(magazine, reserve);
        
        // Update icon
        if (ammoIconImage != null)
        {
            AmmoData ammoData = AmmoController.Instance.GetAmmoData(currentAmmoType);
            if (ammoData != null && ammoData.ammoIcon != null)
            {
                ammoIconImage.sprite = ammoData.ammoIcon;
                ammoIconImage.gameObject.SetActive(true);
            }
            else
            {
                ammoIconImage.gameObject.SetActive(false);
            }
        }
        
        // Update reload status
        bool isReloading = AmmoController.Instance.IsReloading(currentAmmoType);
        UpdateReloadStatus(isReloading);
    }
    
    void UpdateAmmoDisplay(int magazine, int reserve)
    {
        // Update magazine text
        if (magazineAmmoText != null)
        {
            magazineAmmoText.text = magazine.ToString();
            
            // Đổi màu nếu hết đạn
            if (magazine == 0 && reserve == 0)
            {
                magazineAmmoText.color = outOfAmmoColor;
            }
            else if (magazine == 0)
            {
                magazineAmmoText.color = reloadingColor;
            }
            else
            {
                magazineAmmoText.color = normalColor;
            }
        }
        
        // Update reserve text
        if (reserveAmmoText != null)
        {
            reserveAmmoText.text = reserve.ToString();
            reserveAmmoText.color = normalColor;
        }
    }
    
    void UpdateReloadStatus(bool isReloading)
    {
        if (reloadStatusText != null)
        {
            if (isReloading)
            {
                reloadStatusText.text = reloadingText;
                reloadStatusText.color = reloadingColor;
                reloadStatusText.gameObject.SetActive(true);
            }
            else
            {
                // Kiểm tra xem có hết đạn không
                if (currentAmmoType != AmmoType.None && AmmoController.Instance != null)
                {
                    int magazine = AmmoController.Instance.GetCurrentMagazine(currentAmmoType);
                    int reserve = AmmoController.Instance.GetCurrentReserve(currentAmmoType);
                    
                    if (magazine == 0 && reserve == 0)
                    {
                        reloadStatusText.text = outOfAmmoText;
                        reloadStatusText.color = outOfAmmoColor;
                        reloadStatusText.gameObject.SetActive(true);
                    }
                    else
                    {
                        reloadStatusText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    reloadStatusText.gameObject.SetActive(false);
                }
            }
        }
    }
}
