using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest state enumeration
/// </summary>
public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

/// <summary>
/// Represents a quest with multiple linear steps.
/// Each quest has a unique ID and tracks its current progress.
/// QuestSteps are MonoBehaviour components attached to GameObjects.
/// </summary>
[Serializable]
public class Quest
{
    [Header("Quest Info")]
    public string questID;
    public string questName;
    public string questDescription;

    [Header("Quest Steps")]
    [Tooltip("GameObjects that have QuestStep components. These will be activated in order.")]
    public List<GameObject> stepObjects = new List<GameObject>();

    [Header("Progress")]
    public int currentStepIndex = 0;
    public QuestState state = QuestState.NotStarted;

    // Runtime cache of QuestStep components
    private List<QuestStep> steps = new List<QuestStep>();
    private bool stepsInitialized = false;

    /// <summary>
    /// Initialize steps from stepObjects. Call this before using the quest.
    /// </summary>
    public void InitializeSteps()
    {
        if (stepsInitialized) return;

        steps.Clear();
        foreach (var stepObj in stepObjects)
        {
            if (stepObj != null)
            {
                QuestStep step = stepObj.GetComponent<QuestStep>();
                if (step != null)
                {
                    steps.Add(step);
                }
                else
                {
                    Debug.LogWarning($"[Quest] GameObject {stepObj.name} does not have a QuestStep component!");
                }
            }
        }
        stepsInitialized = true;
    }

    /// <summary>
    /// Returns the current active step, or null if quest is completed
    /// </summary>
    public QuestStep CurrentStep
    {
        get
        {
            if (!stepsInitialized) InitializeSteps();
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
                return steps[currentStepIndex];
            return null;
        }
    }

    /// <summary>
    /// Check if quest has been completed
    /// </summary>
    public bool IsCompleted => state == QuestState.Completed;

    /// <summary>
    /// Move to the next step. Returns true if there are more steps, false if quest is complete.
    /// </summary>
    public bool MoveToNextStep()
    {
        if (state == QuestState.Completed)
            return false;

        if (!stepsInitialized) InitializeSteps();

        // Complete current step
        if (CurrentStep != null && !CurrentStep.IsCompleted)
        {
            CurrentStep.Complete();
        }

        // Move to next step
        currentStepIndex++;

        // Check if quest is complete
        if (currentStepIndex >= steps.Count)
        {
            state = QuestState.Completed;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Initialize quest from save data
    /// </summary>
    public void LoadFromSaveData(string savedQuestID, int savedStepIndex)
    {
        if (questID != savedQuestID)
        {
            Debug.LogWarning($"[Quest] Cannot load quest {savedQuestID} into quest {questID}");
            return;
        }

        if (!stepsInitialized) InitializeSteps();
        currentStepIndex = Mathf.Clamp(savedStepIndex, 0, steps.Count - 1);
        
        if (currentStepIndex > 0)
        {
            state = QuestState.InProgress;
        }
        else
        {
            state = QuestState.NotStarted;
        }
    }
}
