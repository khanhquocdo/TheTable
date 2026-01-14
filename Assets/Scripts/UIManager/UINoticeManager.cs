using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Notice system for quest notifications (quest started, quest completed).
/// Similar to UIChatManager but simpler, focused on quest notifications only.
/// </summary>
public class UINoticeManager : MonoBehaviour
{
    public static UINoticeManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject noticePanel;
    public TextMeshProUGUI noticeText;
    public Image backgroundImage; // Optional background for color change

    [Header("Settings")]
    public float displayDuration = 3f;
    public float panelAnimationDuration = 0.3f;
    public float typingSpeed = 0.05f;

    [Header("Speed Presets")]
    [Tooltip("Normal: 3s, 0.3s, 0.05s | Fast: 2s, 0.2s, 0.01s")]
    public NoticeSpeed defaultSpeed = NoticeSpeed.Normal;

    public enum NoticeSpeed
    {
        Normal,
        Fast
    }

    private struct SpeedSettings
    {
        public float displayDuration;
        public float panelAnimationDuration;
        public float typingSpeed;
    }

    private SpeedSettings GetSpeedSettings(NoticeSpeed speed)
    {
        switch (speed)
        {
            case NoticeSpeed.Normal:
                return new SpeedSettings
                {
                    displayDuration = 3f,
                    panelAnimationDuration = 0.3f,
                    typingSpeed = 0.05f
                };
            case NoticeSpeed.Fast:
                return new SpeedSettings
                {
                    displayDuration = 2f,
                    panelAnimationDuration = 0.2f,
                    typingSpeed = 0.01f
                };
            default:
                return new SpeedSettings
                {
                    displayDuration = 3f,
                    panelAnimationDuration = 0.3f,
                    typingSpeed = 0.05f
                };
        }
    }

    [Header("Panel Position Settings")]
    public float panelStartY = 300f;
    public float panelEndY = 0f;

    [Header("Quest Colors")]
    [Tooltip("Color for quest started notification (Yellow)")]
    public Color questStartedColor = new Color(1f, 0.84f, 0f); // Yellow
    [Tooltip("Color for quest completed notification (Green)")]
    public Color questCompletedColor = new Color(0f, 0.8f, 0.2f); // Green
    [Tooltip("Color for step started notification (Yellow)")]
    public Color stepStartedColor = new Color(1f, 0.84f, 0f); // Yellow
    [Tooltip("Color for step completed notification (Green)")]
    public Color stepCompletedColor = new Color(0f, 0.8f, 0.2f); // Green

    private Queue<NoticeData> noticeQueue = new Queue<NoticeData>();
    private bool isDisplaying = false;
    private Coroutine currentTypingCoroutine;

    private struct NoticeData
    {
        public string message;
        public NoticeType type;
        public NoticeSpeed speed;
        public System.Action onComplete;
    }

    public enum NoticeType
    {
        QuestStarted,
        QuestCompleted,
        StepStarted,
        StepCompleted,
        JustTalk
    }

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

    void Start()
    {
        if (noticePanel != null)
            noticePanel.SetActive(false);
    }

    /// <summary>
    /// Show a quest notice
    /// </summary>
    public void ShowNotice(string message, NoticeType type, NoticeSpeed speed = NoticeSpeed.Normal, System.Action onComplete = null)
    {
        NoticeData newNotice = new NoticeData
        {
            message = message,
            type = type,
            speed = speed,
            onComplete = onComplete
        };

        noticeQueue.Enqueue(newNotice);

        if (!isDisplaying)
        {
            StartCoroutine(ProcessNoticeQueue());
        }
    }

    /// <summary>
    /// Show quest started notice
    /// </summary>
    public void ShowQuestStarted(string questName)
    {
        ShowNotice($"Quest Started: {questName}", NoticeType.QuestStarted);
    }

    /// <summary>
    /// Show quest completed notice
    /// </summary>
    public void ShowQuestCompleted(string questName)
    {
        ShowNotice($"Quest Completed: {questName}", NoticeType.QuestCompleted);
    }

    /// <summary>
    /// Show step started notice (uses Fast speed)
    /// </summary>
    public void ShowStepStarted(string stepDescription)
    {
        ShowNotice(stepDescription, NoticeType.StepStarted, NoticeSpeed.Fast);
    }

    /// <summary>
    /// Show step completed notice (uses Fast speed)
    /// </summary>
    public void ShowStepCompleted(string stepDescription)
    {
        ShowNotice($"Completed: {stepDescription}", NoticeType.StepCompleted, NoticeSpeed.Fast);
    }

    private IEnumerator ProcessNoticeQueue()
    {
        isDisplaying = true;

        while (noticeQueue.Count > 0)
        {
            NoticeData noticeData = noticeQueue.Dequeue();
            yield return StartCoroutine(DisplayNotice(noticeData));
        }

        isDisplaying = false;
    }

    private IEnumerator DisplayNotice(NoticeData noticeData)
    {
        // Get speed settings for this notice
        SpeedSettings speedSettings = GetSpeedSettings(noticeData.speed);

        // Show panel with animation
        if (!noticePanel.activeInHierarchy)
        {
            noticePanel.SetActive(true);
            noticePanel.transform.localPosition = new Vector3(0, panelStartY, 0);
            noticePanel.transform.localScale = Vector3.zero;
            noticePanel.transform.DOLocalMoveY(panelEndY, speedSettings.panelAnimationDuration).SetEase(Ease.OutBack);
            noticePanel.transform.DOScale(Vector3.one, speedSettings.panelAnimationDuration).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(speedSettings.panelAnimationDuration);
        }

        // Set color based on notice type
        Color targetColor = Color.white;
        switch (noticeData.type)
        {
            case NoticeType.QuestStarted:
                targetColor = questStartedColor; // Yellow
                break;
            case NoticeType.QuestCompleted:
                targetColor = questCompletedColor; // Green
                break;
            case NoticeType.StepStarted:
                targetColor = stepStartedColor; // Yellow
                break;
            case NoticeType.StepCompleted:
                targetColor = stepCompletedColor; // Green
                break;
            case NoticeType.JustTalk:
                targetColor = Color.white;
                break;
        }

        // Apply color to text
        if (noticeText != null)
        {
            noticeText.color = targetColor;
        }

        // Apply color to background if available
        if (backgroundImage != null)
        {
            Color bgColor = targetColor;
            bgColor.a = 0.8f; // Slightly transparent background
            backgroundImage.color = bgColor;
        }

        // Typing animation
        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);

        currentTypingCoroutine = StartCoroutine(TypeText(noticeData.message, targetColor, speedSettings.typingSpeed));
        yield return currentTypingCoroutine;

        // Wait for display duration
        yield return new WaitForSeconds(speedSettings.displayDuration);

        // Hide panel if no more notices
        if (noticeQueue.Count == 0)
        {
            noticePanel.transform.DOLocalMoveY(panelStartY, speedSettings.panelAnimationDuration).SetEase(Ease.InBack);
            noticePanel.transform.DOScale(Vector3.zero, speedSettings.panelAnimationDuration).SetEase(Ease.InBack)
                .OnComplete(() => noticePanel.SetActive(false));
            yield return new WaitForSeconds(speedSettings.panelAnimationDuration);
        }

        noticeText.text = "";

        // Call completion callback
        noticeData.onComplete?.Invoke();
    }

    private IEnumerator TypeText(string text, Color textColor, float speed)
    {
        noticeText.text = "";
        noticeText.color = textColor;

        for (int i = 0; i <= text.Length; i++)
        {
            noticeText.text = text.Substring(0, i);
            yield return new WaitForSeconds(speed);
        }
    }

    /// <summary>
    /// Skip current notice and show next one
    /// </summary>
    public void SkipCurrentNotice()
    {
        if (currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
            currentTypingCoroutine = null;
        }
    }
}
