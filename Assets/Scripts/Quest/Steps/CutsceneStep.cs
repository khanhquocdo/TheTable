using UnityEngine;

/// <summary>
/// Quest step that completes when a cutscene finishes.
/// Listens to OnCutsceneEnded event.
/// </summary>
public class CutsceneStep : QuestStep
{
    [Header("Cutscene Settings")]
    [SerializeField] private string targetCutsceneID;

    protected override void OnActivate()
    {
        // Subscribe to cutscene ended event
        QuestEventSystem.OnCutsceneEnded += HandleCutsceneEnded;
    }

    protected override void OnComplete()
    {
        // Unsubscribe from events
        QuestEventSystem.OnCutsceneEnded -= HandleCutsceneEnded;
    }

    private void HandleCutsceneEnded(string cutsceneId)
    {
        if (cutsceneId == targetCutsceneID)
        {
            FinishStep();
        }
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe when destroyed
        QuestEventSystem.OnCutsceneEnded -= HandleCutsceneEnded;
    }
}
