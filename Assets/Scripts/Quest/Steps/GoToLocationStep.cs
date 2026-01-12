using UnityEngine;

/// <summary>
/// Quest step that completes when player enters a specific trigger area.
/// Listens to OnPlayerEnterArea event.
/// </summary>
public class GoToLocationStep : QuestStep
{
    [Header("Location Settings")]
    [SerializeField] private string targetAreaID;

    [Header("Guidance")]
    [SerializeField] private bool showGuidance = true;
    [SerializeField] private Transform locationMarker;

    protected override void OnActivate()
    {
        // Subscribe to player enter area event
        QuestEventSystem.OnPlayerEnterArea += HandlePlayerEnterArea;

        // Show guidance marker if enabled
        if (showGuidance && QuestGuidanceSystem.Instance != null)
        {
            if (locationMarker != null)
            {
                QuestGuidanceSystem.Instance.ShowLocationMarker(locationMarker.position);
            }
            else if (transform != null)
            {
                QuestGuidanceSystem.Instance.ShowLocationMarker(transform.position);
            }
        }
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnPlayerEnterArea -= HandlePlayerEnterArea;

        // Clear guidance
        if (QuestGuidanceSystem.Instance != null)
        {
            QuestGuidanceSystem.Instance.ClearLocationMarker();
        }
    }

    private void HandlePlayerEnterArea(string areaId)
    {
        if (areaId == targetAreaID)
        {
            FinishStep();
        }
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe when destroyed
        QuestEventSystem.OnPlayerEnterArea -= HandlePlayerEnterArea;
    }
}
