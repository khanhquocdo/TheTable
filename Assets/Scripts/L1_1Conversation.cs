using UnityEngine;

public class L1_1Conversation : MonoBehaviour
{


    // Event khi conversation hoàn thành
    public System.Action OnConversationCompleted;

    void Start()
    {
        string chat = "Hahahahaha";
        UIChatManager.Instance.SendChat(chat, "test", ChatPosition.Right);
        chat = "? ";
        UIChatManager.Instance.SendChat(chat, "test", ChatPosition.Left);
        chat = "ToNy?";
        UIChatManager.Instance.SendChat(chat, "test2", ChatPosition.Right);
        chat = "You suck";
        UIChatManager.Instance.SendChat(chat, "test", ChatPosition.Left);
        chat = "You suck too";
        UIChatManager.Instance.SendChat(chat, "test2", ChatPosition.Right);
        chat = "OK but you sucker";
        UIChatManager.Instance.SendChat(chat, "test", ChatPosition.Right);
        chat = "You are so stupid";
        UIChatManager.Instance.SendChat(chat, "test2", ChatPosition.Right);
        chat = "You are so stupid too";
    }
}