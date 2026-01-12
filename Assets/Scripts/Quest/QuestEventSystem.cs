using System;
using UnityEngine;

/// <summary>
/// Static event system for quest step completion.
/// Steps subscribe to these events to detect when objectives are completed.
/// </summary>
public static class QuestEventSystem
{
    // NPC Dialog Events
    public static event Action<string> OnNPCDialogFinished; // npcId

    // Location Events
    public static event Action<string> OnPlayerEnterArea; // areaId

    // Combat Events
    public static event Action<string> OnEnemyKilled; // enemyId

    // Cutscene Events
    public static event Action<string> OnCutsceneEnded; // cutsceneId

    /// <summary>
    /// Call this when an NPC dialog finishes
    /// </summary>
    public static void TriggerNPCDialogFinished(string npcId)
    {
        Debug.Log($"[QuestEventSystem] NPC Dialog Finished: {npcId}");
        OnNPCDialogFinished?.Invoke(npcId);
    }

    /// <summary>
    /// Call this when player enters a trigger area
    /// </summary>
    public static void TriggerPlayerEnterArea(string areaId)
    {
        Debug.Log($"[QuestEventSystem] Player Entered Area: {areaId}");
        OnPlayerEnterArea?.Invoke(areaId);
    }

    /// <summary>
    /// Call this when an enemy is killed
    /// </summary>
    public static void TriggerEnemyKilled(string enemyId)
    {
        Debug.Log($"[QuestEventSystem] Enemy Killed: {enemyId}");
        OnEnemyKilled?.Invoke(enemyId);
    }

    /// <summary>
    /// Call this when a cutscene ends
    /// </summary>
    public static void TriggerCutsceneEnded(string cutsceneId)
    {
        Debug.Log($"[QuestEventSystem] Cutscene Ended: {cutsceneId}");
        OnCutsceneEnded?.Invoke(cutsceneId);
    }
}
