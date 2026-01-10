using UnityEngine;

/// <summary>
/// Example script showing how to use the Quest System.
/// This demonstrates basic usage patterns - you can delete this file once you understand the system.
/// </summary>
public class QuestSystemExample : MonoBehaviour
{
    [Header("Example Quest ID")]
    [SerializeField] private string exampleQuestID = "Quest_01";

    void Start()
    {
        // Example: Subscribe to quest events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnStepChanged += OnStepChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        }

        // Example: Load quest progress on game start
        // QuestSaveLoadHelper.LoadQuestProgress();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnStepChanged -= OnStepChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    // Example event handlers
    private void OnQuestStarted(Quest quest)
    {
        Debug.Log($"Quest Started: {quest.questName}");
        // Update UI, play sound, etc.
    }

    private void OnStepChanged(QuestStep step)
    {
        Debug.Log($"Step Changed: {step.stepDescription}");
        // Update quest UI, show objectives, etc.
    }

    private void OnQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest Completed: {quest.questName}");
        // Show completion UI, give rewards, etc.

        // Save progress
        QuestSaveLoadHelper.SaveQuestProgress();
    }

    // Example: Manually trigger quest start (usually done by NPC)
    [ContextMenu("Start Example Quest")]
    public void StartExampleQuest()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(exampleQuestID);
        }
    }

    // Example: Manually trigger cutscene end (usually done by cutscene system)
    [ContextMenu("Trigger Cutscene End")]
    public void TriggerCutsceneEnd()
    {
        QuestEventSystem.TriggerCutsceneEnded("Cutscene_01");
    }
}
