using UnityEngine;

/// <summary>
/// Simple guidance system for quest steps.
/// Provides methods to highlight NPCs, show location markers, etc.
/// Separate from quest logic - quest steps can request guidance when activated.
/// </summary>
public class QuestGuidanceSystem : MonoBehaviour
{
    public static QuestGuidanceSystem Instance { get; private set; }

    [Header("Guidance Prefabs")]
    [SerializeField] private GameObject npcHighlightPrefab;
    [SerializeField] private GameObject locationMarkerPrefab;

    [Header("Guidance Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float markerScale = 1f;

    private GameObject currentNPCHighlight;
    private GameObject currentLocationMarker;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Highlight an NPC (e.g., with a glow effect or arrow above head)
    /// </summary>
    public void HighlightNPC(GameObject npcObject)
    {
        ClearAllGuidance();

        if (npcObject == null)
        {
            Debug.LogWarning("[QuestGuidanceSystem] Cannot highlight null NPC");
            return;
        }

        // If we have a prefab, instantiate it. Otherwise, use simple visual indication.
        if (npcHighlightPrefab != null)
        {
            currentNPCHighlight = Instantiate(npcHighlightPrefab, npcObject.transform);
            currentNPCHighlight.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Simple fallback: add a colored sprite renderer or particle effect
            Debug.Log($"[QuestGuidanceSystem] Highlighting NPC: {npcObject.name} (No prefab set)");
            // You can add a simple visual indicator here if needed
        }
    }

    /// <summary>
    /// Show a marker at a specific location (e.g., circle on ground)
    /// </summary>
    public void ShowLocationMarker(Vector3 position)
    {
        ClearLocationMarker();

        if (locationMarkerPrefab != null)
        {
            currentLocationMarker = Instantiate(locationMarkerPrefab, position, Quaternion.identity);
            currentLocationMarker.transform.localScale = Vector3.one * markerScale;
        }
        else
        {
            Debug.Log($"[QuestGuidanceSystem] Showing location marker at {position} (No prefab set)");
            // You can add a simple visual indicator here if needed
        }
    }

    /// <summary>
    /// Show a marker at a GameObject's position
    /// </summary>
    public void ShowLocationMarker(GameObject targetObject)
    {
        if (targetObject != null)
        {
            ShowLocationMarker(targetObject.transform.position);
        }
    }

    /// <summary>
    /// Clear NPC highlight
    /// </summary>
    public void ClearNPCHighlight()
    {
        if (currentNPCHighlight != null)
        {
            Destroy(currentNPCHighlight);
            currentNPCHighlight = null;
        }
    }

    /// <summary>
    /// Clear location marker
    /// </summary>
    public void ClearLocationMarker()
    {
        if (currentLocationMarker != null)
        {
            Destroy(currentLocationMarker);
            currentLocationMarker = null;
        }
    }

    /// <summary>
    /// Clear all guidance visuals
    /// </summary>
    public void ClearAllGuidance()
    {
        ClearNPCHighlight();
        ClearLocationMarker();
    }
}
