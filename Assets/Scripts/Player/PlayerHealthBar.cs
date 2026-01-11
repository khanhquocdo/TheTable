using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI Health Bar hiển thị máu của Player
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill; // Image fill của health bar
    [SerializeField] private Image healthBarBackground; // Background của health bar (optional)
    [SerializeField] private TextMeshProUGUI healthText; // Text hiển thị số máu (optional)
    
    [Header("Settings")]
    [SerializeField] private bool showHealthText = true; // Có hiển thị text không
    [SerializeField] private float smoothFillSpeed = 5f; // Tốc độ fill mượt mà
    
    [Header("Color Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // Ngưỡng máu thấp (30%)
    
    private Health playerHealth;
    private float targetFillAmount = 1f;
    private Color targetColor;
    
    void Start()
    {
        // Tìm Player trong scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Subscribe to health events
                playerHealth.OnHealthChanged += UpdateHealthBar;
                playerHealth.OnDamageTaken += OnDamageTaken;
                playerHealth.OnHealed += OnHealed;
                
                // Khởi tạo health bar
                UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
            else
            {
                Debug.LogError("PlayerHealthBar: Player không có Health component!");
            }
        }
        else
        {
            Debug.LogError("PlayerHealthBar: Không tìm thấy Player trong scene!");
        }
        
        // Validate UI references
        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHealthBar: Chưa gán healthBarFill!");
        }
    }
    
    void Update()
    {
        // Smooth fill animation
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);
            
            // Smooth color transition
            healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, Time.deltaTime * smoothFillSpeed);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from health events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnDamageTaken -= OnDamageTaken;
            playerHealth.OnHealed -= OnHealed;
        }
    }
    
    /// <summary>
    /// Cập nhật health bar khi máu thay đổi
    /// </summary>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;
        
        float healthPercentage = currentHealth / maxHealth;
        targetFillAmount = healthPercentage;
        
        // Tính màu dựa trên phần trăm máu
        if (healthPercentage <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else
        {
            // Lerp từ màu đỏ đến màu xanh
            float t = (healthPercentage - lowHealthThreshold) / (1f - lowHealthThreshold);
            targetColor = Color.Lerp(lowHealthColor, fullHealthColor, t);
        }
        
        // Cập nhật text nếu có
        if (showHealthText && healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)}/{Mathf.Ceil(maxHealth)}";
        }
    }
    
    /// <summary>
    /// Xử lý khi player nhận sát thương (có thể thêm hiệu ứng flash)
    /// </summary>
    private void OnDamageTaken(float damage)
    {
        // Có thể thêm hiệu ứng flash đỏ khi nhận sát thương
        // Ví dụ: Flash màu đỏ trong 0.1 giây
    }
    
    /// <summary>
    /// Xử lý khi player hồi máu (có thể thêm hiệu ứng)
    /// </summary>
    private void OnHealed()
    {
        // Có thể thêm hiệu ứng flash xanh khi hồi máu
    }
    
    /// <summary>
    /// Thiết lập màu health bar thủ công
    /// </summary>
    public void SetHealthBarColor(Color color)
    {
        targetColor = color;
        if (healthBarFill != null)
        {
            healthBarFill.color = color;
        }
    }
}


