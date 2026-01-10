using System;

/// <summary>
/// Simple save data structure for quest progress.
/// Contains only the essential information needed to restore quest state.
/// </summary>
[Serializable]
public class QuestSaveData
{
    public string questID = "";
    public int currentStepIndex = 0;

    /// <summary>
    /// Check if this save data represents an active quest
    /// </summary>
    public bool HasActiveQuest()
    {
        return !string.IsNullOrEmpty(questID) && currentStepIndex >= 0;
    }
}
