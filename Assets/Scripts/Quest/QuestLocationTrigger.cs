using UnityEngine;

/// <summary>
/// Simple trigger component for GoToLocationStep.
/// Attach this to a trigger collider to fire OnPlayerEnterArea event.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class QuestLocationTrigger : MonoBehaviour
{
    [Header("Location Settings")]
    [SerializeField] private string areaID;
    [SerializeField] private bool triggerOnce = true;

    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnce)
            return;

        if (other.CompareTag(playerTag))
        {
            QuestEventSystem.TriggerPlayerEnterArea(areaID);
            hasTriggered = true;

            if (triggerOnce)
            {
                // Disable collider or component after triggering
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
            return;

        if (other.CompareTag(playerTag))
        {
            QuestEventSystem.TriggerPlayerEnterArea(areaID);
            hasTriggered = true;

            if (triggerOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    /// <summary>
    /// Reset trigger state (useful for reloading)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = true;
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = true;
    }
}
