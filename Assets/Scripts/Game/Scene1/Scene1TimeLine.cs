using System.Collections;
using UnityEngine;

public class Scene1TimeLine : MonoBehaviour
{
    public Camera mainCamera;
    public Camera introCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera.gameObject.SetActive(false);
        introCamera.gameObject.SetActive(true);
        StartCoroutine(ShowIntroText());
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
                "Mafia kiểm soát đường phố. Cắm rễ sâu trong những ngóc ngách sâu nhất",
                UINoticeManager.NoticeType.JustTalk
            );
            UINoticeManager.Instance.ShowNotice(
                "Bạn quay trở lại cố thành - không với tư cách anh hùng \n Nhưng tâm huyết đánh tan bọn Mafia đang nuốt chửng quê hương mình",
                UINoticeManager.NoticeType.JustTalk,
                () =>
                {
                    mainCamera.gameObject.SetActive(true);
                    introCamera.gameObject.SetActive(false);
                }
            );
        }
    }
}
