using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene3Story : MonoBehaviour
{
    public static Scene3Story Instance { get; private set; }
    public List<GameObject> toDestroyList;
    public string npcName = "Bình Minh";
    public string you = "Tôi";
    public string yourName = "Hoàng";
    [SerializeField] private string nextSceneName = "Ending";
    public GameObject ps;
    public Transform psTransform;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        StartCoroutine(ShowIntroText());
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(Quest quest)
    {
        if (quest.questID == "Q3")
        {
            UIChatManager.Instance.SendChat("Xong rồi sao???", you, ChatPosition.Right);
            Instantiate(ps, transform.position, Quaternion.identity);
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowIntroText()
    {
        yield return new WaitForSeconds(3f);
        UIChatManager.Instance.SendChat("Đúng như anh nói anh Minh, bọn chúng thực sự ở đây", you, ChatPosition.Right);
        UIChatManager.Instance.SendChat(
            $"Thôi nào {yourName}! Hãy quay lại và gọi cảnh sát",
            npcName, ChatPosition.Left);
        UIChatManager.Instance.SendChat(
            $"(nói dối) Được, đợi em chụp một ít bằng chứng",
            you, ChatPosition.Right,
            null,
            () =>
            {
                QuestManager.Instance.StartQuest("Q3");
            });
    }
}
