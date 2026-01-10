using UnityEngine;

/// <summary>
/// Helper component to connect enemy Health system to QuestEventSystem.
/// Attach this to enemies that should trigger quest events when killed.
/// </summary>
[RequireComponent(typeof(Health))]
public class QuestEnemyHelper : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private string enemyID;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"[QuestEnemyHelper] {gameObject.name} does not have a Health component!");
        }
    }

    private void OnEnemyDeath()
    {
        if (!string.IsNullOrEmpty(enemyID))
        {
            QuestEventSystem.TriggerEnemyKilled(enemyID);
        }
        else
        {
            // Use gameObject name as fallback ID
            QuestEventSystem.TriggerEnemyKilled(gameObject.name);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= OnEnemyDeath;
        }
    }
}
