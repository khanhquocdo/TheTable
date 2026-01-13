using UnityEngine;

/// <summary>
/// Quest step that completes when player finishes talking to a specific NPC.
/// Listens to OnNPCDialogFinished event.
/// </summary>
public class TalkToNPCStep : QuestStep
{
    [Header("Talk To NPC Settings")]
    [SerializeField] private string targetNPCID;

    [Header("Guidance")]
    [SerializeField] private bool showGuidance = true;
    [SerializeField] private GameObject npcObject;

    void Start()
    {
        // Hide NPC object initially if option is enabled
        if (showTargetOnlyWhenActive && npcObject != null)
        {
            npcObject.SetActive(false);
        }
    }

    protected override void OnActivate()
    {
        // Subscribe to NPC dialog finished event
        QuestEventSystem.OnNPCDialogFinished += HandleNPCDialogFinished;

        // Show NPC object first if option is enabled
        if (showTargetOnlyWhenActive && npcObject != null)
        {
            npcObject.SetActive(true);
        }

        // Then show guidance if enabled
        if (showGuidance && showTargetOnlyWhenActive && npcObject != null && QuestGuidanceSystem.Instance != null)
        {
            QuestGuidanceSystem.Instance.HighlightNPC(npcObject);
        }
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnNPCDialogFinished -= HandleNPCDialogFinished;

        // Hide NPC object if option is enabled
        if (showTargetOnlyWhenActive && npcObject != null)
        {
            npcObject.SetActive(false);
        }

        // Clear guidance
        if (QuestGuidanceSystem.Instance != null)
        {
            QuestGuidanceSystem.Instance.ClearNPCHighlight();
        }
    }

    private void HandleNPCDialogFinished(string npcId)
    {
        if (npcId == targetNPCID)
        {
            FinishStep();
        }
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe when destroyed
        QuestEventSystem.OnNPCDialogFinished -= HandleNPCDialogFinished;
    }
}
