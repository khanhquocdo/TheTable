using UnityEngine;
using System.Collections;

public class MockStartQuest : MonoBehaviour
{
    public string questID;
    void Start()
    {
        StartCoroutine(StartQuest());
    }

    IEnumerator StartQuest()
    {
        yield return new WaitForSeconds(1);
        QuestManager.Instance.StartQuest(questID);
    }
}