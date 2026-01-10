using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    public Transform playerTransform;
    public GameObject questDirectorPrefab;
    public List<Quest> quests = new List<Quest>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddQuest(Quest quest)
    {
        quests.Add(quest);
        quest.questDirector = Instantiate(questDirectorPrefab, new Vector2(-999f, -999f), Quaternion.identity);
    }

    public void RemoveQuest(Quest quest)
    {
        quests.Remove(quest);
    }

    public void RemoveQuest(string questName)
    {
        quests.RemoveAll(quest => quest.questName == questName);
    }

    void Update()
    {
        if (quests.Count == 0) return;
        Vector2 direction = playerTransform.position - quests[0].questTarget;
        direction.Normalize();
        quests[0].questDirector.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        quests[0].questDirector.transform.position = playerTransform.position - new Vector3(direction.x, direction.y, 0);
    }
}
