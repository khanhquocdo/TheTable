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

    void Start()
    {
        // Hide target marker initially if option is enabled
        if (showTargetOnlyWhenActive && locationMarker != null)
        {
            locationMarker.gameObject.SetActive(false);
        }
    }

    protected override void OnActivate()
    {
        // Subscribe to player enter area event
        QuestEventSystem.OnPlayerEnterArea += HandlePlayerEnterArea;

        // Show target marker first if option is enabled
        if (showTargetOnlyWhenActive && locationMarker != null)
        {
            locationMarker.gameObject.SetActive(true);
        }

        // Then show guidance marker if enabled
        if (showGuidance && showTargetOnlyWhenActive && QuestGuidanceSystem.Instance != null)
        {
            if (locationMarker != null)
            {
                QuestGuidanceSystem.Instance.ShowLocationMarker(locationMarker.gameObject);
            }
            else if (transform != null)
            {
                QuestGuidanceSystem.Instance.ShowLocationMarker(gameObject);
            }
        }
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnPlayerEnterArea -= HandlePlayerEnterArea;

        // Hide target marker if option is enabled
        if (showTargetOnlyWhenActive && locationMarker != null)
        {
            locationMarker.gameObject.SetActive(false);
        }

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
