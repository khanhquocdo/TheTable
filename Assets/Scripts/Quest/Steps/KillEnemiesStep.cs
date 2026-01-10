using UnityEngine;

/// <summary>
/// Quest step that completes after killing a specific number of enemies.
/// Listens to OnEnemyKilled event and tracks kill count.
/// </summary>
public class KillEnemiesStep : QuestStep
{
    [Header("Kill Enemies Settings")]
    [SerializeField] private int requiredKills = 1;
    [SerializeField] private string targetEnemyID = ""; // Empty string means any enemy

    private int currentKillCount = 0;

    protected override void OnActivate()
    {
        currentKillCount = 0;
        // Subscribe to enemy killed event
        QuestEventSystem.OnEnemyKilled += HandleEnemyKilled;
        Debug.Log($"[KillEnemiesStep] Need to kill {requiredKills} enemies (ID: {targetEnemyID})");
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(string enemyId)
    {
        // If targetEnemyID is empty, count all enemies. Otherwise, only count matching IDs.
        if (string.IsNullOrEmpty(targetEnemyID) || enemyId == targetEnemyID)
        {
            currentKillCount++;
            Debug.Log($"[KillEnemiesStep] Kill count: {currentKillCount}/{requiredKills}");

            if (currentKillCount >= requiredKills)
            {
                FinishStep();
            }
        }
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe when destroyed
        QuestEventSystem.OnEnemyKilled -= HandleEnemyKilled;
    }
}
