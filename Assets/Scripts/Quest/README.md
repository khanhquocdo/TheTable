# Quest System - Quick Start Guide

## Overview
A simple, practical quest system for Unity 2D story-driven games. Quest-driven, event-based, and easy to expand.

## Core Components

### 1. QuestManager (Singleton)
- Manages quest lifecycle
- Tracks active quest
- Exposes events: `OnQuestStarted`, `OnStepChanged`, `OnQuestCompleted`

### 2. Quest
- Contains QuestID and list of QuestSteps
- Tracks current step index and state
- State: NotStarted / InProgress / Completed

### 3. QuestStep (Abstract Base)
- Subclasses: `TalkToNPCStep`, `GoToLocationStep`, `KillEnemiesStep`, `CutsceneStep`
- Subscribes to events in `Activate()`, unsubscribes in `Complete()`
- No Update() polling - purely event-driven

## Setup Instructions

### Step 1: Create QuestManager GameObject
1. Create empty GameObject named "QuestManager"
2. Add `QuestManager` component
3. The GameObject will persist across scenes automatically

### Step 2: Create QuestGuidanceSystem GameObject
1. Create empty GameObject named "QuestGuidanceSystem"
2. Add `QuestGuidanceSystem` component
3. (Optional) Assign prefabs for NPC highlights and location markers

### Step 3: Create a Quest
1. In QuestManager inspector, add a new Quest to the Quest Database list
2. Set QuestID (e.g., "Quest_01")
3. Set Quest Name and Description

### Step 4: Create Quest Steps
For each step in your quest:

1. **TalkToNPCStep:**
   - Create empty GameObject
   - Add `TalkToNPCStep` component
   - Set StepID and Description
   - Set TargetNPCID (must match NPC's npcID)
   - (Optional) Assign NPC GameObject for guidance highlighting
   - Add this GameObject to Quest's Step Objects list

2. **GoToLocationStep:**
   - Create empty GameObject at target location
   - Add `GoToLocationStep` component
   - Set StepID, Description, and TargetAreaID
   - Create trigger collider (2D or 3D)
   - Add `QuestLocationTrigger` component to trigger
   - Set AreaID on trigger (must match TargetAreaID)
   - Add step GameObject to Quest's Step Objects list

3. **KillEnemiesStep:**
   - Create empty GameObject
   - Add `KillEnemiesStep` component
   - Set StepID, Description, RequiredKills, and TargetEnemyID (empty = any enemy)
   - Add this GameObject to Quest's Step Objects list
   - On enemies: Add `QuestEnemyHelper` component with matching EnemyID

4. **CutsceneStep:**
   - Create empty GameObject
   - Add `CutsceneStep` component
   - Set StepID, Description, and TargetCutsceneID
   - Add this GameObject to Quest's Step Objects list
   - When cutscene ends, call: `QuestEventSystem.TriggerCutsceneEnded("CutsceneID")`

### Step 5: Setup NPCs
1. Add `QuestNPC` component to NPC GameObject
2. Set NPCID (must match TalkToNPCStep's TargetNPCID)
3. Set QuestToStartID (quest that starts when talking to this NPC)
4. Set dialog messages
5. Call `Interact()` when player interacts with NPC

### Step 6: Setup Location Triggers
1. Add Collider2D (or Collider) with "Is Trigger" checked
2. Add `QuestLocationTrigger` component
3. Set AreaID (must match GoToLocationStep's TargetAreaID)
4. Set Player Tag (default: "Player")

### Step 7: Setup Enemy Integration
1. On enemy GameObject with `Health` component, add `QuestEnemyHelper`
2. Set EnemyID (must match KillEnemiesStep's TargetEnemyID)
3. Enemy death will automatically trigger quest events

## Usage Examples

### Starting a Quest
```csharp
QuestManager.Instance.StartQuest("Quest_01");
```

### Subscribing to Events
```csharp
QuestManager.Instance.OnQuestStarted += (quest) => {
    Debug.Log($"Quest started: {quest.questName}");
};

QuestManager.Instance.OnStepChanged += (step) => {
    Debug.Log($"New step: {step.stepDescription}");
};

QuestManager.Instance.OnQuestCompleted += (quest) => {
    Debug.Log($"Quest completed: {quest.questName}");
};
```

### Triggering Events Manually
```csharp
// NPC dialog finished
QuestEventSystem.TriggerNPCDialogFinished("NPC_01");

// Player entered area
QuestEventSystem.TriggerPlayerEnterArea("Area_01");

// Enemy killed
QuestEventSystem.TriggerEnemyKilled("Enemy_01");

// Cutscene ended
QuestEventSystem.TriggerCutsceneEnded("Cutscene_01");
```

### Save/Load
```csharp
// Save progress
QuestSaveLoadHelper.SaveQuestProgress();

// Load progress
QuestSaveLoadHelper.LoadQuestProgress();

// Clear progress
QuestSaveLoadHelper.ClearQuestProgress();
```

## Creating New Step Types

1. Create new script inheriting from `QuestStep`
2. Override `OnActivate()` to subscribe to events
3. Override `OnComplete()` to unsubscribe from events
4. Call `FinishStep()` when objective is complete

Example:
```csharp
public class CollectItemStep : QuestStep
{
    [SerializeField] private string targetItemID;
    
    protected override void OnActivate()
    {
        QuestEventSystem.OnItemCollected += HandleItemCollected;
    }
    
    protected override void OnComplete()
    {
        QuestEventSystem.OnItemCollected -= HandleItemCollected;
    }
    
    private void HandleItemCollected(string itemID)
    {
        if (itemID == targetItemID)
            FinishStep();
    }
}
```

## Notes
- Quest steps are activated sequentially
- Only one quest can be active at a time
- All events are static and global
- Guidance system is optional but recommended
- Save/Load uses PlayerPrefs (can be extended)
