using UnityEngine;
using TMPro;

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

    [Header("Directional Indicator")]
    [Tooltip("Indicator object (with Canvas/Text) attached to the player")]
    [SerializeField] private GameObject directionalIndicator;
    [Tooltip("Distance text on the directional indicator")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [Tooltip("Player transform used to compute direction/position")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Automatically set indicator as child of player on start")]
    [SerializeField] private float hideDistanceThreshold = 1.5f;
    [Tooltip("Offset from player to place the indicator")]
    [SerializeField] private float indicatorOffset = 0.5f;

    private GameObject currentNPCHighlight;
    private GameObject currentLocationMarker;
    private Transform targetTransform;
    private Vector3? targetPosition;

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

        if (directionalIndicator != null && distanceText == null)
        {
            // Try to find TMP text on the indicator or its children
            distanceText = directionalIndicator.GetComponentInChildren<TextMeshProUGUI>(true);
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

        // Set directional target to NPC
        targetTransform = npcObject.transform;
        targetPosition = null;
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
            targetTransform = currentLocationMarker.transform;
            targetPosition = null;
        }
        else
        {
            Debug.Log($"[QuestGuidanceSystem] Showing location marker at {position} (No prefab set)");
            // You can add a simple visual indicator here if needed
            targetTransform = null;
            targetPosition = position;
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
            targetTransform = targetObject.transform;
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
        targetTransform = null;
        targetPosition = null;
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
        targetTransform = null;
        targetPosition = null;
    }

    /// <summary>
    /// Clear all guidance visuals
    /// </summary>
    public void ClearAllGuidance()
    {
        ClearNPCHighlight();
        ClearLocationMarker();
        targetTransform = null;
        targetPosition = null;
    }

    void Update()
    {
        UpdateDirectionalIndicator();
    }

    /// <summary>
    /// Update directional indicator to point from player to current target.
    /// Uses existing highlight/marker as target source.
    /// </summary>
    private void UpdateDirectionalIndicator()
    {
        if (directionalIndicator == null || playerTransform == null)
        {
            return;
        }

        // Determine current target position
        Vector3? targetPos = null;
        if (targetTransform != null)
        {
            targetPos = targetTransform.position;
        }
        else if (targetPosition.HasValue)
        {
            targetPos = targetPosition.Value;
        }

        if (!targetPos.HasValue)
        {
            if (directionalIndicator.activeSelf)
                directionalIndicator.SetActive(false);
            return;
        }

        Vector3 playerPos = playerTransform.position;
        Vector3 dir = targetPos.Value - playerPos;
        float distance = dir.magnitude;

        // Hide indicator if within threshold
        if (distance < hideDistanceThreshold)
        {
            if (directionalIndicator.activeSelf)
                directionalIndicator.SetActive(false);
            return;
        }

        dir.Normalize();

        Vector3 indicatorPos = playerPos + dir * indicatorOffset;
        directionalIndicator.transform.position = indicatorPos;

        // Rotate to face target
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        directionalIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (distanceText != null)
        {
            distanceText.text = $"{distance:0.0} m";
        }

        if (!directionalIndicator.activeSelf)
            directionalIndicator.SetActive(true);
    }
}
