using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene1Story : MonoBehaviour
{
    public static Scene1Story Instance { get; private set; }
    public List<GameObject> toDestroyList;
    public string[] chatOutro = { "", "", "" };
    public string characterName = "ADMIN";
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public Camera mainCamera;
    public Camera introCamera;
    // Start is called before the first frame update
    void Start()
    {
        SetUp();
        StartCoroutine(ShowIntroText());
    }

    void SetUp()
    {
        mainCamera.gameObject.SetActive(false);
        introCamera.gameObject.SetActive(true);
    }

    IEnumerator ShowIntroText()
    {
        yield return new WaitForSeconds(1);
        if (UINoticeManager.Instance != null)
        {
            UINoticeManager.Instance.ShowNotice(
                "Hắc Thành – nơi ánh sáng không bao giờ chạm tới đáy của bóng tối.",
                UINoticeManager.NoticeType.JustTalk
            );
            UINoticeManager.Instance.ShowNotice(
                "Mafia kiểm soát đường phố. Cắm rễ trong những ngóc ngách sâu nhất",
                UINoticeManager.NoticeType.JustTalk
            );
            UINoticeManager.Instance.ShowNotice(
                "Bạn quay trở lại cố thành - không với tư cách anh hùng \n Nhưng tâm huyết đánh tan bọn Mafia đang nuốt chửng quê hương mình",
                UINoticeManager.NoticeType.JustTalk,
                UINoticeManager.NoticeSpeed.Normal,
                () =>
                {
                    mainCamera.gameObject.SetActive(true);
                    introCamera.gameObject.SetActive(false);
                    QuestManager.Instance.StartQuest("Q1");
                    toDestroyList.ForEach(item => Destroy(item));
                }
            );
        }
    }

    public void RunOutro()
    {
        foreach (string chat in chatOutro)
        {
            UIChatManager.Instance.SendChat(chat, characterName);
        }
    }
}
