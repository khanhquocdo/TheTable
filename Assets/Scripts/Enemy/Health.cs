using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0f;

    // Events
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action<float> OnDamageTaken; // damage amount
    public event Action OnDeath;
    public event Action OnHealed; // when health is restored

    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => currentHealth <= 0f;

    void Start()
    {
        // Khởi tạo máu đầy khi bắt đầu
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Nhận sát thương
    /// </summary>
    /// <param name="damage">Lượng sát thương nhận vào</param>
    public void TakeDamage(float damage)
    {
        if (IsDead) return; // Đã chết rồi thì không nhận sát thương nữa

        if (damage < 0) damage = 0; // Đảm bảo sát thương không âm

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Phát audio hurt (trước khi chết)
        if (currentHealth > 0f && AudioManager.Instance != null)
        {
            AudioID hurtAudioID = GetEnemyHurtAudioID();
            if (hurtAudioID != AudioID.None)
            {
                AudioManager.Instance.PlayAudio(hurtAudioID, transform.position);
            }
        }

        // Gọi events
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} nhận {damage} sát thương. Máu còn lại: {currentHealth}/{maxHealth}");

        // Kiểm tra chết
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Xác định loại enemy và trả về AudioID hurt tương ứng
    /// </summary>
    private AudioID GetEnemyHurtAudioID()
    {
        if (GetComponent<MeleeEnemyController>() != null)
        {
            return AudioID.Enemy_Melee_Hurt;
        }
        else if (GetComponent<TankEnemyController>() != null)
        {
            return AudioID.None; // Tank không có hurt sound riêng
        }
        else if (GetComponent<EnemyController>() != null)
        {
            return AudioID.Enemy_Shooter_Hurt;
        }
        return AudioID.None;
    }
    
    /// <summary>
    /// Xác định loại enemy và trả về AudioID death tương ứng
    /// </summary>
    private AudioID GetEnemyDeathAudioID()
    {
        if (GetComponent<MeleeEnemyController>() != null)
        {
            return AudioID.Enemy_Melee_Death;
        }
        else if (GetComponent<TankEnemyController>() != null)
        {
            return AudioID.Tank_Death;
        }
        else if (GetComponent<EnemyController>() != null)
        {
            return AudioID.Enemy_Shooter_Death;
        }
        return AudioID.None;
    }

    /// <summary>
    /// Hồi máu
    /// </summary>
    /// <param name="healAmount">Lượng máu hồi</param>
    public void Heal(float healAmount)
    {
        if (IsDead) return; // Đã chết thì không hồi máu được

        if (healAmount < 0) healAmount = 0;

        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth > oldHealth)
        {
            OnHealed?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"{gameObject.name} hồi {currentHealth - oldHealth} máu. Máu hiện tại: {currentHealth}/{maxHealth}");
        }
    }

    /// <summary>
    /// Đặt máu tối đa (dùng khi level up hoặc upgrade)
    /// </summary>
    public void SetMaxHealth(float newMaxHealth, bool fillToMax = false)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        if (fillToMax)
        {
            currentHealth = maxHealth;
        }
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Xử lý khi chết
    /// </summary>
    private void Die()
    {
        if (IsDead == false) return; // Chỉ chết một lần

        Debug.Log($"{gameObject.name} đã chết!");

        // Phát audio chết
        if (AudioManager.Instance != null)
        {
            AudioID deathAudioID = GetEnemyDeathAudioID();
            if (deathAudioID != AudioID.None)
            {
                AudioManager.Instance.PlayAudio(deathAudioID, transform.position);
            }
        }

        // Gọi event chết
        OnDeath?.Invoke();

        // Xử lý sau khi chết
        if (deathDelay > 0f)
        {
            StartCoroutine(DelayedDeath());
        }
        else if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Hồi máu đầy
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }

    /// <summary>
    /// Kiểm tra xem có đủ máu không
    /// </summary>
    public bool HasEnoughHealth(float requiredHealth)
    {
        return currentHealth >= requiredHealth;
    }
}
