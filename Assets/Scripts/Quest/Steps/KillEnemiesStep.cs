using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quest step that completes after killing a specific number of enemies.
/// Listens to OnEnemyKilled event and tracks kill count.
/// </summary>
public class KillEnemiesStep : QuestStep
{
    [Header("Kill Enemies Settings")]
    [SerializeField] private int requiredKills = 1;
    [SerializeField] private string targetEnemyID = ""; // Empty string means any enemy

    [Header("Enemy Targets")]
    [Tooltip("List of enemy GameObjects to kill. If empty, any enemy with matching ID will count.")]
    [SerializeField] private List<GameObject> targetEnemies = new List<GameObject>();

    [Header("Guidance")]
    [SerializeField] private bool showGuidance = true;
    [Tooltip("Show marker at first enemy position for guidance")]
    [SerializeField] private bool showMarkerAtEnemy = true;

    private int currentKillCount = 0;

    void Start()
    {
        // Hide enemies initially if option is enabled
        if (showTargetOnlyWhenActive && targetEnemies != null)
        {
            foreach (GameObject enemy in targetEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }
        }
    }

    protected override void OnActivate()
    {
        currentKillCount = 0;
        // Subscribe to enemy killed event
        QuestEventSystem.OnEnemyKilled += HandleEnemyKilled;

        // Show enemies first if option is enabled
        if (showTargetOnlyWhenActive && targetEnemies != null)
        {
            foreach (GameObject enemy in targetEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(true);
                }
            }
        }

        // Show guidance if enabled
        if (showGuidance && showTargetOnlyWhenActive && QuestGuidanceSystem.Instance != null)
        {
            if (showMarkerAtEnemy && targetEnemies != null && targetEnemies.Count > 0)
            {
                // Find first active enemy to show marker
                GameObject firstEnemy = null;
                foreach (GameObject enemy in targetEnemies)
                {
                    if (enemy != null && enemy.activeSelf)
                    {
                        firstEnemy = enemy;
                        break;
                    }
                }

                if (firstEnemy != null)
                {
                    QuestGuidanceSystem.Instance.ShowLocationMarker(firstEnemy);
                }
            }
        }
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnEnemyKilled -= HandleEnemyKilled;

        // Hide enemies if option is enabled
        if (showTargetOnlyWhenActive && targetEnemies != null)
        {
            foreach (GameObject enemy in targetEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }
        }

        // Clear guidance
        if (QuestGuidanceSystem.Instance != null)
        {
            QuestGuidanceSystem.Instance.ClearLocationMarker();
        }
    }

    private void HandleEnemyKilled(string enemyId)
    {
        // If targetEnemyID is empty, count all enemies. Otherwise, only count matching IDs.
        if (string.IsNullOrEmpty(targetEnemyID) || enemyId == targetEnemyID)
        {
            currentKillCount++;

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
