using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneManagerObject;

    private void Start()
    {
        if (cutsceneManagerObject != null && !cutsceneManagerObject.activeInHierarchy)
        {
            cutsceneManagerObject.SetActive(true);
        }
    }
}
