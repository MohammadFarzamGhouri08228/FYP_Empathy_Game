using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles individual dialogue box UI elements.
/// Manages sprite display, text rendering, and basic animations.
/// </summary>
public class DialogueBoxUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image dialogueBoxImage;
    [SerializeField] private Text dialogueText;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useFadeAnimation = true;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    void Awake()
    {
        // Get or add required components
        if (dialogueBoxImage == null)
            dialogueBoxImage = GetComponentInChildren<Image>();
        
        if (dialogueText == null)
            dialogueText = GetComponentInChildren<Text>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        rectTransform = GetComponent<RectTransform>();
        
        // Initially hide
        if (useFadeAnimation)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Sets the sprite for the dialogue box
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (dialogueBoxImage != null && sprite != null)
        {
            dialogueBoxImage.sprite = sprite;
            dialogueBoxImage.preserveAspect = true;
        }
    }
    
    /// <summary>
    /// Sets the text content of the dialogue box
    /// </summary>
    public void SetText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text ?? "";
        }
    }
    
    /// <summary>
    /// Sets the font for the text
    /// </summary>
    public void SetFont(Font font)
    {
        if (dialogueText != null && font != null)
        {
            dialogueText.font = font;
        }
    }
    
    /// <summary>
    /// Sets the font size
    /// </summary>
    public void SetFontSize(int fontSize)
    {
        if (dialogueText != null)
        {
            dialogueText.fontSize = fontSize;
        }
    }
    
    /// <summary>
    /// Sets the text color
    /// </summary>
    public void SetTextColor(Color color)
    {
        if (dialogueText != null)
        {
            dialogueText.color = color;
        }
    }
    
    /// <summary>
    /// Sets the padding around the text
    /// </summary>
    public void SetPadding(Vector2 padding)
    {
        if (rectTransform != null && dialogueText != null)
        {
            RectTransform textRect = dialogueText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.offsetMin = new Vector2(padding.x, padding.y);
                textRect.offsetMax = new Vector2(-padding.x, -padding.y);
            }
        }
    }
    
    /// <summary>
    /// Shows the dialogue box with optional fade animation
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        
        if (useFadeAnimation)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Hides the dialogue box with optional fade animation
    /// </summary>
    public void Hide()
    {
        if (useFadeAnimation)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Immediately shows without animation
    /// </summary>
    public void ShowImmediate()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Immediately hides without animation
    /// </summary>
    public void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Sets the world position (for world space canvas)
    /// </summary>
    public void SetWorldPosition(Vector3 worldPosition)
    {
        if (rectTransform != null)
        {
            rectTransform.position = worldPosition;
        }
    }
    
    /// <summary>
    /// Sets the screen position (for screen space canvas)
    /// </summary>
    public void SetScreenPosition(Vector2 screenPosition)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = screenPosition;
        }
    }
    
    /// <summary>
    /// Sets the scale of the dialogue box
    /// </summary>
    public void SetScale(Vector3 scale)
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = scale;
        }
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Checks if the dialogue box is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return gameObject.activeSelf && canvasGroup.alpha > 0f;
    }
}

