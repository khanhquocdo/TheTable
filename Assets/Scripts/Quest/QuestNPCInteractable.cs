using UnityEngine;

/// <summary>
/// Component to handle NPC interaction via F key or proximity trigger.
/// Attach this to NPC GameObject along with QuestNPC component.
/// </summary>
[RequireComponent(typeof(QuestNPC))]
public class QuestNPCInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private InteractionType interactionType = InteractionType.PressF;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Proximity Settings")]
    [Tooltip("Distance to trigger interaction automatically")]
    [SerializeField] private float interactionRange = 2f;
    [Tooltip("Tag of the player GameObject")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Use trigger collider or distance check for proximity")]
    [SerializeField] private bool useTriggerCollider = true;

    [Header("Visual Feedback")]
    [Tooltip("Show interaction prompt UI")]
    [SerializeField] private bool showInteractionPrompt = true;
    [SerializeField] private GameObject interactionPromptUI;

    private QuestNPC questNPC;
    private bool isPlayerInRange = false;
    private GameObject playerObject;
    private bool hasInteracted = false;

    public enum InteractionType
    {
        PressF,         // Player must press F to interact
        Proximity,      // Auto-interact when player enters range
        Both            // Both F key and proximity work
    }

    void Start()
    {
        questNPC = GetComponent<QuestNPC>();
        if (questNPC == null)
        {
            Debug.LogError($"[QuestNPCInteractable] {gameObject.name} requires QuestNPC component!");
        }

        // Setup interaction prompt UI
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Handle F key interaction
        if (interactionType == InteractionType.PressF || interactionType == InteractionType.Both)
        {
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }

        // Handle proximity interaction with distance check (if not using trigger)
        if (!useTriggerCollider && (interactionType == InteractionType.Proximity || interactionType == InteractionType.Both))
        {
            CheckPlayerDistance();
        }
    }

    private void CheckPlayerDistance()
    {
        if (playerObject == null)
        {
            // Try to find player
            GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);
            if (foundPlayer == null) return;
            playerObject = foundPlayer;
        }

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;

        // Player entered range
        if (!wasInRange && isPlayerInRange)
        {
            OnPlayerEnterRange(playerObject);
        }
        // Player exited range
        else if (wasInRange && !isPlayerInRange)
        {
            OnPlayerExitRange();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerEnterRange(other.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerEnterRange(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerExitRange();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerExitRange();
        }
    }

    private void OnPlayerEnterRange(GameObject player)
    {
        isPlayerInRange = true;
        playerObject = player;

        // Show interaction prompt
        if (showInteractionPrompt && interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }

        // Auto-interact if proximity mode and hasn't interacted yet
        if (interactionType == InteractionType.Proximity && !hasInteracted)
        {
            Interact();
        }
    }

    private void OnPlayerExitRange()
    {
        isPlayerInRange = false;
        playerObject = null;

        // Hide interaction prompt
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    /// <summary>
    /// Manually trigger interaction
    /// </summary>
    public void Interact()
    {
        if (questNPC != null && !hasInteracted)
        {
            hasInteracted = true;
            questNPC.Interact();
            
            // Hide prompt after interaction
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Reset interaction state (useful for allowing multiple interactions)
    /// </summary>
    public void ResetInteraction()
    {
        hasInteracted = false;
    }

    /// <summary>
    /// Check if player is in interaction range
    /// </summary>
    public bool IsPlayerInRange()
    {
        return isPlayerInRange;
    }

    /// <summary>
    /// Set interaction type
    /// </summary>
    public void SetInteractionType(InteractionType type)
    {
        interactionType = type;
    }

    /// <summary>
    /// Set interaction key
    /// </summary>
    public void SetInteractKey(KeyCode key)
    {
        interactKey = key;
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range in editor
        if (interactionType == InteractionType.Proximity || interactionType == InteractionType.Both)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
