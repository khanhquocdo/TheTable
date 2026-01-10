using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager that handles quest lifecycle, progress tracking, and events.
/// Use this to start quests and track active quest progress.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Database")]
    [SerializeField] private List<Quest> questDatabase = new List<Quest>();

    [Header("Current Quest")]
    [SerializeField] private Quest activeQuest;

    // Events
    public event Action<Quest> OnQuestStarted;
    public event Action<QuestStep> OnStepChanged;
    public event Action<Quest> OnQuestCompleted;
    public event Action<QuestStep> OnStepCompleted;

    // Internal quest lookup dictionary
    private Dictionary<string, Quest> questDictionary = new Dictionary<string, Quest>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeQuestDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize the quest database dictionary for quick lookups
    /// </summary>
    private void InitializeQuestDatabase()
    {
        questDictionary.Clear();
        foreach (var quest in questDatabase)
        {
            if (!string.IsNullOrEmpty(quest.questID))
            {
                questDictionary[quest.questID] = quest;
            }
        }
        Debug.Log($"[QuestManager] Initialized {questDictionary.Count} quests");
    }

    /// <summary>
    /// Start a quest by its ID. Returns true if quest was started successfully.
    /// </summary>
    public bool StartQuest(string questID)
    {
        if (activeQuest != null && activeQuest.state == QuestState.InProgress)
        {
            Debug.LogWarning($"[QuestManager] Cannot start quest {questID}. Quest {activeQuest.questID} is already active.");
            return false;
        }

        if (!questDictionary.TryGetValue(questID, out Quest quest))
        {
            Debug.LogError($"[QuestManager] Quest {questID} not found in database!");
            return false;
        }

        // Initialize quest steps
        quest.InitializeSteps();

        // Reset quest state
        quest.currentStepIndex = 0;
        quest.state = QuestState.InProgress;
        activeQuest = quest;

        // Activate first step
        if (quest.CurrentStep != null)
        {
            quest.CurrentStep.Activate();
            OnStepChanged?.Invoke(quest.CurrentStep);
        }
        else
        {
            Debug.LogWarning($"[QuestManager] Quest {questID} has no steps!");
            return false;
        }

        Debug.Log($"[QuestManager] Started quest: {questID}");
        OnQuestStarted?.Invoke(quest);
        return true;
    }

    /// <summary>
    /// Called by QuestStep when it completes. Moves to next step automatically.
    /// </summary>
    public void HandleStepCompleted(QuestStep completedStep)
    {
        if (activeQuest == null || activeQuest.CurrentStep != completedStep)
        {
            Debug.LogWarning($"[QuestManager] Received step completion for non-active step");
            return;
        }

        OnStepCompleted?.Invoke(completedStep);

        // Move to next step
        bool hasMoreSteps = activeQuest.MoveToNextStep();

        if (hasMoreSteps)
        {
            // Activate next step
            if (activeQuest.CurrentStep != null)
            {
                activeQuest.CurrentStep.Activate();
                OnStepChanged?.Invoke(activeQuest.CurrentStep);
            }
        }
        else
        {
            // Quest completed
            Debug.Log($"[QuestManager] Quest {activeQuest.questID} completed!");
            OnQuestCompleted?.Invoke(activeQuest);
            activeQuest = null;
        }
    }

    /// <summary>
    /// Get the currently active quest
    /// </summary>
    public Quest GetActiveQuest()
    {
        return activeQuest;
    }

    /// <summary>
    /// Check if a quest is currently active
    /// </summary>
    public bool HasActiveQuest()
    {
        return activeQuest != null && activeQuest.state == QuestState.InProgress;
    }

    /// <summary>
    /// Get a quest by ID (for checking state, etc.)
    /// </summary>
    public Quest GetQuest(string questID)
    {
        questDictionary.TryGetValue(questID, out Quest quest);
        return quest;
    }

    /// <summary>
    /// Add a quest to the database at runtime
    /// </summary>
    public void RegisterQuest(Quest quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.questID))
        {
            Debug.LogError("[QuestManager] Cannot register invalid quest");
            return;
        }

        if (!questDictionary.ContainsKey(quest.questID))
        {
            questDatabase.Add(quest);
            questDictionary[quest.questID] = quest;
            Debug.Log($"[QuestManager] Registered quest: {quest.questID}");
        }
        else
        {
            Debug.LogWarning($"[QuestManager] Quest {quest.questID} already exists in database");
        }
    }

    /// <summary>
    /// Load quest progress from save data
    /// </summary>
    public void LoadQuestProgress(string questID, int stepIndex)
    {
        if (!questDictionary.TryGetValue(questID, out Quest quest))
        {
            Debug.LogError($"[QuestManager] Cannot load quest {questID} - not found in database");
            return;
        }

        quest.InitializeSteps();
        quest.LoadFromSaveData(questID, stepIndex);
        
        if (quest.state == QuestState.InProgress)
        {
            activeQuest = quest;
            if (quest.CurrentStep != null)
            {
                quest.CurrentStep.Activate();
                OnStepChanged?.Invoke(quest.CurrentStep);
            }
            Debug.Log($"[QuestManager] Loaded quest progress: {questID} at step {stepIndex}");
        }
    }

    /// <summary>
    /// Get save data for current quest progress
    /// </summary>
    public QuestSaveData GetSaveData()
    {
        if (activeQuest == null)
        {
            return new QuestSaveData { questID = "", currentStepIndex = 0 };
        }

        return new QuestSaveData
        {
            questID = activeQuest.questID,
            currentStepIndex = activeQuest.currentStepIndex
        };
    }
}
