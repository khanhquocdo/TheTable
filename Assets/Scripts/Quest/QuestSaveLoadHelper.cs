using System.IO;
using UnityEngine;

/// <summary>
/// Simple helper class for saving and loading quest progress.
/// Uses PlayerPrefs for simplicity. Can be extended to use file system or cloud save.
/// </summary>
public static class QuestSaveLoadHelper
{
    private const string QUEST_SAVE_KEY = "QuestSaveData";

    /// <summary>
    /// Save current quest progress
    /// </summary>
    public static void SaveQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestSaveLoadHelper] QuestManager not found, cannot save");
            return;
        }

        QuestSaveData saveData = QuestManager.Instance.GetSaveData();

        if (saveData.HasActiveQuest())
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(QUEST_SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log($"[QuestSaveLoadHelper] Saved quest progress: {saveData.questID} at step {saveData.currentStepIndex}");
        }
        else
        {
            // Clear save if no active quest
            PlayerPrefs.DeleteKey(QUEST_SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[QuestSaveLoadHelper] No active quest to save");
        }
    }

    /// <summary>
    /// Load quest progress and restore quest state
    /// </summary>
    public static void LoadQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestSaveLoadHelper] QuestManager not found, cannot load");
            return;
        }

        if (!PlayerPrefs.HasKey(QUEST_SAVE_KEY))
        {
            Debug.Log("[QuestSaveLoadHelper] No saved quest data found");
            return;
        }

        string json = PlayerPrefs.GetString(QUEST_SAVE_KEY);
        QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(json);

        if (saveData.HasActiveQuest())
        {
            QuestManager.Instance.LoadQuestProgress(saveData.questID, saveData.currentStepIndex);
            Debug.Log($"[QuestSaveLoadHelper] Loaded quest progress: {saveData.questID} at step {saveData.currentStepIndex}");
        }
    }

    /// <summary>
    /// Clear saved quest progress
    /// </summary>
    public static void ClearQuestProgress()
    {
        PlayerPrefs.DeleteKey(QUEST_SAVE_KEY);
        PlayerPrefs.Save();
        Debug.Log("[QuestSaveLoadHelper] Cleared quest progress");
    }

    /// <summary>
    /// Check if saved quest data exists
    /// </summary>
    public static bool HasSavedQuestProgress()
    {
        return PlayerPrefs.HasKey(QUEST_SAVE_KEY);
    }
}
