using UnityEngine;

public class TriggerQuest2 : MonoBehaviour
{
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
            hasTriggered = true;

            QuestManager.Instance.StartQuest("Q2");
        }
    }
}