using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Welcome Text")]
    [Tooltip("Text hiển thị chữ chào mừng")]
    public TextMeshProUGUI welcomeText;
    
    [Header("Press Anywhere Text")]
    [Tooltip("Text hiển thị 'Bấm bất kỳ đâu để bắt đầu'")]
    public TextMeshProUGUI pressAnywhereText;
    
    [Header("Menu Panel")]
    [Tooltip("Panel chứa menu chính")]
    public GameObject menuPanel;
    
    [Header("Background Image")]
    [Tooltip("Hình ảnh background")]
    public Image backgroundImage;
    
    [Header("Animation Settings")]
    [Tooltip("Thời gian fade in chữ chào mừng")]
    public float welcomeFadeDuration = 1f;
    
    [Tooltip("Thời gian hiển thị chữ chào mừng trước khi fade out")]
    public float welcomeDisplayDuration = 2f;
    
    [Tooltip("Thời gian fade out chữ chào mừng")]
    public float welcomeFadeOutDuration = 0.5f;
    
    [Tooltip("Thời gian fade in text 'Bấm bất kỳ đâu'")]
    public float pressAnywhereFadeDuration = 0.8f;
    
    [Tooltip("Thời gian fade in menu panel")]
    public float menuFadeDuration = 0.5f;
    
    [Tooltip("Ease type cho welcome text")]
    public Ease welcomeEase = Ease.OutQuad;
    
    [Tooltip("Ease type cho press anywhere text")]
    public Ease pressAnywhereEase = Ease.InOutQuad;
    
    [Tooltip("Ease type cho menu panel")]
    public Ease menuEase = Ease.OutBack;
    
    [Header("Background Animation Settings")]
    [Tooltip("Bật/tắt animation cho background")]
    public bool enableBackgroundAnimation = true;
    
    [Tooltip("Scale animation cho background (ví dụ: 1.1 = phóng to 10%)")]
    public float backgroundScaleAmount = 1.05f;
    
    [Tooltip("Thời gian một chu kỳ animation background")]
    public float backgroundAnimationDuration = 3f;
    
    [Tooltip("Ease type cho background animation")]
    public Ease backgroundEase = Ease.InOutSine;
    
    private bool isWaitingForInput = false;
    private bool hasStarted = false;
    private Sequence welcomeSequence;
    private Tween backgroundTween;
    private CanvasGroup welcomeCanvasGroup;
    private CanvasGroup pressAnywhereCanvasGroup;
    private CanvasGroup menuCanvasGroup;
    
    void Start()
    {
        InitializeComponents();
        SetupInitialState();
        StartMainMenuSequence();
    }
    
    private void InitializeComponents()
    {
        // Tạo CanvasGroup cho welcome text nếu chưa có
        if (welcomeText != null)
        {
            welcomeCanvasGroup = welcomeText.GetComponent<CanvasGroup>();
            if (welcomeCanvasGroup == null)
            {
                welcomeCanvasGroup = welcomeText.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Tạo CanvasGroup cho press anywhere text nếu chưa có
        if (pressAnywhereText != null)
        {
            pressAnywhereCanvasGroup = pressAnywhereText.GetComponent<CanvasGroup>();
            if (pressAnywhereCanvasGroup == null)
            {
                pressAnywhereCanvasGroup = pressAnywhereText.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Tạo CanvasGroup cho menu panel nếu chưa có
        if (menuPanel != null)
        {
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
            }
        }
    }
    
    private void SetupInitialState()
    {
        // Ẩn welcome text ban đầu
        if (welcomeCanvasGroup != null)
        {
            welcomeCanvasGroup.alpha = 0f;
            welcomeCanvasGroup.blocksRaycasts = false;
        }
        else if (welcomeText != null)
        {
            Color color = welcomeText.color;
            color.a = 0f;
            welcomeText.color = color;
        }
        
        // Ẩn press anywhere text ban đầu
        if (pressAnywhereCanvasGroup != null)
        {
            pressAnywhereCanvasGroup.alpha = 0f;
            pressAnywhereCanvasGroup.blocksRaycasts = false;
        }
        else if (pressAnywhereText != null)
        {
            Color color = pressAnywhereText.color;
            color.a = 0f;
            pressAnywhereText.color = color;
        }
        
        // Ẩn menu panel ban đầu
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
                menuCanvasGroup.blocksRaycasts = false;
            }
        }
        
        // Setup background animation
        if (backgroundImage != null && enableBackgroundAnimation)
        {
            StartBackgroundAnimation();
        }
    }
    
    private void StartMainMenuSequence()
    {
        if (hasStarted) return;
        hasStarted = true;
        
        // Tạo sequence cho welcome text
        if (welcomeText != null)
        {
            welcomeSequence = DOTween.Sequence();
            
            // Fade in welcome text
            if (welcomeCanvasGroup != null)
            {
                welcomeSequence.Append(welcomeCanvasGroup.DOFade(1f, welcomeFadeDuration)
                    .SetEase(welcomeEase));
            }
            else
            {
                welcomeSequence.Append(welcomeText.DOFade(1f, welcomeFadeDuration)
                    .SetEase(welcomeEase));
            }
            
            // Giữ welcome text hiển thị
            welcomeSequence.AppendInterval(welcomeDisplayDuration);
            
            // Fade out welcome text
            if (welcomeCanvasGroup != null)
            {
                welcomeSequence.Append(welcomeCanvasGroup.DOFade(0f, welcomeFadeOutDuration)
                    .SetEase(welcomeEase));
            }
            else
            {
                welcomeSequence.Append(welcomeText.DOFade(0f, welcomeFadeOutDuration)
                    .SetEase(welcomeEase));
            }
            
            // Sau khi welcome text fade out, hiển thị press anywhere text
            welcomeSequence.AppendCallback(() => {
                ShowPressAnywhereText();
            });
        }
        else
        {
            // Nếu không có welcome text, hiển thị press anywhere text ngay
            ShowPressAnywhereText();
        }
    }
    
    private void ShowPressAnywhereText()
    {
        if (pressAnywhereText == null)
        {
            // Nếu không có press anywhere text, hiển thị menu ngay
            ShowMenu();
            return;
        }
        
        isWaitingForInput = true;
        
        // Fade in press anywhere text
        if (pressAnywhereCanvasGroup != null)
        {
            pressAnywhereCanvasGroup.blocksRaycasts = true;
            pressAnywhereCanvasGroup.DOFade(1f, pressAnywhereFadeDuration)
                .SetEase(pressAnywhereEase)
                .OnComplete(() => {
                    // Tạo hiệu ứng nhấp nháy nhẹ
                    StartPressAnywhereBlink();
                });
        }
        else
        {
            pressAnywhereText.DOFade(1f, pressAnywhereFadeDuration)
                .SetEase(pressAnywhereEase)
                .OnComplete(() => {
                    StartPressAnywhereBlink();
                });
        }
    }
    
    private void StartPressAnywhereBlink()
    {
        if (pressAnywhereText == null) return;
        
        // Tạo hiệu ứng nhấp nháy nhẹ
        if (pressAnywhereCanvasGroup != null)
        {
            pressAnywhereCanvasGroup.DOFade(0.5f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            pressAnywhereText.DOFade(0.5f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void ShowMenu()
    {
        isWaitingForInput = false;
        
        // Dừng hiệu ứng nhấp nháy của press anywhere text
        if (pressAnywhereText != null)
        {
            if (pressAnywhereCanvasGroup != null)
            {
                pressAnywhereCanvasGroup.DOKill();
            }
            else
            {
                pressAnywhereText.DOKill();
            }
        }
        
        // Fade out press anywhere text
        if (pressAnywhereCanvasGroup != null)
        {
            pressAnywhereCanvasGroup.DOFade(0f, 0.3f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    pressAnywhereCanvasGroup.blocksRaycasts = false;
                    ActivateMenu();
                });
        }
        else if (pressAnywhereText != null)
        {
            pressAnywhereText.DOFade(0f, 0.3f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    ActivateMenu();
                });
        }
        else
        {
            ActivateMenu();
        }
    }
    
    private void ActivateMenu()
    {
        if (menuPanel == null) return;
        
        menuPanel.SetActive(true);
        
        // Scale animation cho menu
        menuPanel.transform.localScale = Vector3.zero;
        menuPanel.transform.DOScale(Vector3.one, menuFadeDuration)
            .SetEase(menuEase);
        
        // Fade in menu
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.blocksRaycasts = true;
            menuCanvasGroup.DOFade(1f, menuFadeDuration)
                .SetEase(menuEase);
        }
    }
    
    private void StartBackgroundAnimation()
    {
        if (backgroundImage == null || !enableBackgroundAnimation) return;
        
        Vector3 originalScale = backgroundImage.transform.localScale;
        Vector3 targetScale = originalScale * backgroundScaleAmount;
        
        // Tạo animation loop cho background
        backgroundTween = backgroundImage.transform.DOScale(targetScale, backgroundAnimationDuration)
            .SetEase(backgroundEase)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    void Update()
    {
        // Kiểm tra input khi đang chờ người dùng bấm
        if (isWaitingForInput && Input.anyKeyDown)
        {
            ShowMenu();
        }
    }
    
    void OnDestroy()
    {
        // Dọn dẹp các tween khi destroy
        if (welcomeSequence != null)
        {
            welcomeSequence.Kill();
        }
        
        if (backgroundTween != null)
        {
            backgroundTween.Kill();
        }
        
        // Kill tất cả các tween liên quan
        if (welcomeCanvasGroup != null)
        {
            welcomeCanvasGroup.DOKill();
        }
        
        if (pressAnywhereCanvasGroup != null)
        {
            pressAnywhereCanvasGroup.DOKill();
        }
        
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.DOKill();
        }
        
        if (welcomeText != null)
        {
            welcomeText.DOKill();
        }
        
        if (pressAnywhereText != null)
        {
            pressAnywhereText.DOKill();
        }
        
        if (menuPanel != null)
        {
            menuPanel.transform.DOKill();
        }
    }
}
