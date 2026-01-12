using UnityEngine;

/// <summary>
/// Simple NPC script that starts/finishes quests and fires dialog events.
/// NPC only triggers quests - no quest logic inside NPC.
/// </summary>
public class QuestNPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcID;
    [SerializeField] private string npcName = "NPC";

    [Header("Quest Triggers")]
    [Tooltip("Quest to start when dialog begins")]
    [SerializeField] private string questToStartID = "";

    [Header("Dialog")]
    [Tooltip("Dialog messages to display")]
    [SerializeField] private string[] dialogMessages = new string[0];

    [Header("References")]
    [SerializeField] private bool useUIChatManager = true;

    /// <summary>
    /// Called when player interacts with NPC (e.g., on trigger or button press)
    /// </summary>
    public void Interact()
    {
        // Start quest if needed
        if (!string.IsNullOrEmpty(questToStartID))
        {
            QuestManager.Instance?.StartQuest(questToStartID);
        }

        // Show dialog
        ShowDialog();
    }

    private bool dialogEventFired = false;

    /// <summary>
    /// Show dialog and fire completion event when done
    /// </summary>
    private void ShowDialog()
    {
        dialogEventFired = false;

        if (useUIChatManager && UIChatManager.Instance != null && dialogMessages.Length > 0)
        {
            // Subscribe to chat completion event
            UIChatManager.Instance.OnAllChatsCompleted += OnAllChatsCompleted;

            // Use existing UIChatManager to send all messages
            foreach (string message in dialogMessages)
            {
                UIChatManager.Instance.SendChat(
                    message,
                    npcName,
                    ChatPosition.Middle,
                    transform
                );
            }
        }
        else
        {
            // Simple fallback: fire event immediately
            Debug.Log($"[QuestNPC] Dialog with {npcName} (ID: {npcID})");
            QuestEventSystem.TriggerNPCDialogFinished(npcID);
            dialogEventFired = true;
        }
    }

    private void OnAllChatsCompleted()
    {
        if (!dialogEventFired && UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnAllChatsCompleted -= OnAllChatsCompleted;
            QuestEventSystem.TriggerNPCDialogFinished(npcID);
            dialogEventFired = true;
        }
    }

    private void OnDestroy()
    {
        // Ensure we unsubscribe if destroyed during dialog
        if (UIChatManager.Instance != null)
        {
            UIChatManager.Instance.OnAllChatsCompleted -= OnAllChatsCompleted;
        }
    }

    /// <summary>
    /// Get NPC ID
    /// </summary>
    public string GetNPCID()
    {
        return npcID;
    }

    /// <summary>
    /// Set quest to start on interaction
    /// </summary>
    public void SetQuestToStart(string questID)
    {
        questToStartID = questID;
    }

    /// <summary>
    /// Set dialog messages
    /// </summary>
    public void SetDialogMessages(string[] messages)
    {
        dialogMessages = messages;
    }
}
