using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// UI Health Bar hiển thị máu của Player
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill; // Image fill của health bar
    [SerializeField] private Image healthBarBackground; // Background của health bar (optional)
    [SerializeField] private TextMeshProUGUI healthText; // Text hiển thị số máu (optional)
    
    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; // Reference trực tiếp đến Player (optional)
    [SerializeField] private bool autoFindPlayer = true; // Tự động tìm Player nếu chưa gán
    
    [Header("Settings")]
    [SerializeField] private bool showHealthText = true; // Có hiển thị text không
    [SerializeField] private float smoothFillSpeed = 5f; // Tốc độ fill mượt mà
    
    [Header("Color Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // Ngưỡng máu thấp (30%)
    
    [Header("Visual Effects")]
    [SerializeField] private bool enableDamageFlash = true; // Bật flash khi nhận damage
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f); // Màu flash khi nhận damage
    [SerializeField] private float damageFlashDuration = 0.2f; // Thời gian flash
    
    [SerializeField] private bool enableHealFlash = true; // Bật flash khi hồi máu
    [SerializeField] private Color healFlashColor = new Color(0.3f, 1f, 0.3f, 1f); // Màu flash khi hồi máu
    [SerializeField] private float healFlashDuration = 0.2f; // Thời gian flash
    
    [SerializeField] private bool enableLowHealthPulse = true; // Bật pulse khi máu thấp
    [SerializeField] private float pulseSpeed = 2f; // Tốc độ pulse
    [SerializeField] private float pulseMinScale = 0.95f; // Scale nhỏ nhất khi pulse
    [SerializeField] private float pulseMaxScale = 1.05f; // Scale lớn nhất khi pulse
    
    private Health playerHealth;
    private float targetFillAmount = 1f;
    private Color targetColor;
    private Color originalFillColor;
    private bool isFlashing = false;
    private Coroutine flashCoroutine;
    private RectTransform healthBarRectTransform;
    
    void Awake()
    {
        // Lưu màu gốc của health bar
        if (healthBarFill != null)
        {
            originalFillColor = healthBarFill.color;
            healthBarRectTransform = healthBarFill.GetComponent<RectTransform>();
        }
    }
    
    void Start()
    {
        InitializePlayerHealth();
        
        // Validate UI references
        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHealthBar: Chưa gán healthBarFill!");
        }
    }
    
    /// <summary>
    /// Khởi tạo và kết nối với Player Health
    /// </summary>
    private void InitializePlayerHealth()
    {
        // Tìm Player
        GameObject player = playerObject;
        
        if (player == null && autoFindPlayer)
        {
            // Thử tìm bằng tag trước
            player = GameObject.FindGameObjectWithTag("Player");
            
            // Nếu không tìm thấy, thử tìm bằng component PlayerMovement
            if (player == null)
            {
                PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
                if (playerMovement != null)
                {
                    player = playerMovement.gameObject;
                }
            }
        }
        
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
                
                Debug.Log("PlayerHealthBar: Đã kết nối với Player Health thành công!");
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
    }
    
    void Update()
    {
        // Smooth fill animation
        if (healthBarFill != null && !isFlashing)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * smoothFillSpeed);
            
            // Smooth color transition
            healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, Time.deltaTime * smoothFillSpeed);
        }
        
        // Low health pulse effect
        if (enableLowHealthPulse && healthBarRectTransform != null && playerHealth != null)
        {
            float healthPercentage = playerHealth.HealthPercentage;
            if (healthPercentage <= lowHealthThreshold && healthPercentage > 0f)
            {
                float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
                healthBarRectTransform.localScale = Vector3.one * pulse;
            }
            else
            {
                healthBarRectTransform.localScale = Vector3.one;
            }
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
        
        // Stop any running coroutines
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
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
    /// Xử lý khi player nhận sát thương - Flash effect
    /// </summary>
    private void OnDamageTaken(float damage)
    {
        if (enableDamageFlash && healthBarFill != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffect(damageFlashColor, damageFlashDuration));
        }
    }
    
    /// <summary>
    /// Xử lý khi player hồi máu - Flash effect
    /// </summary>
    private void OnHealed()
    {
        if (enableHealFlash && healthBarFill != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffect(healFlashColor, healFlashDuration));
        }
    }
    
    /// <summary>
    /// Hiệu ứng flash cho health bar
    /// </summary>
    private IEnumerator FlashEffect(Color flashColor, float duration)
    {
        isFlashing = true;
        float elapsed = 0f;
        Color startColor = healthBarFill.color;
        
        // Flash to target color
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            healthBarFill.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
        }
        
        // Flash back to original color
        elapsed = 0f;
        startColor = flashColor;
        Color endColor = targetColor;
        
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            healthBarFill.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        healthBarFill.color = targetColor;
        isFlashing = false;
        flashCoroutine = null;
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
    
    /// <summary>
    /// Thiết lập Player reference thủ công (dùng khi Player spawn sau)
    /// </summary>
    public void SetPlayer(GameObject player)
    {
        // Unsubscribe từ player cũ nếu có
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnDamageTaken -= OnDamageTaken;
            playerHealth.OnHealed -= OnHealed;
        }
        
        playerObject = player;
        InitializePlayerHealth();
    }
    
    /// <summary>
    /// Lấy Health component của Player (public để các script khác có thể truy cập)
    /// </summary>
    public Health GetPlayerHealth()
    {
        return playerHealth;
    }
}


