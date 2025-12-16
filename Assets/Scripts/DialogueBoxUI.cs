using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles individual dialogue box UI elements.
/// Manages sprite display, text rendering, and basic animations.
/// </summary>
public class DialogueBoxUI : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Assign a sprite directly here. If assigned, it will automatically be used for the dialogue box image.")]
    [SerializeField] private Sprite dialogueSprite;
    
    [Tooltip("Image component for the dialogue box background. Will be auto-created if sprite is assigned.")]
    [SerializeField] private Image dialogueBoxImage;
    
    [Tooltip("Text component for dialogue messages.")]
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
        EnsureComponents();
        
        // If a sprite is assigned directly, create Image component and use it
        if (dialogueSprite != null && dialogueBoxImage == null)
        {
            CreateImageComponentFromSprite();
        }
        
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
        
        // Don't deactivate in Awake() - let Show()/Hide() handle activation state
        // This prevents issues when components are being created dynamically
    }
    
    void Start()
    {
        // Ensure components are found after all initialization
        // This handles cases where components are created after Awake
        
        // If sprite is assigned, make sure Image component exists and uses it
        if (dialogueSprite != null)
        {
            if (dialogueBoxImage == null)
            {
                CreateImageComponentFromSprite();
            }
            else if (dialogueBoxImage.sprite != dialogueSprite)
            {
                dialogueBoxImage.sprite = dialogueSprite;
                dialogueBoxImage.enabled = true;
            }
        }
        
        EnsureComponents();
    }
    
    /// <summary>
    /// Creates an Image component from the assigned sprite
    /// </summary>
    private void CreateImageComponentFromSprite()
    {
        if (dialogueSprite == null) return;
        
        // Check if Image component already exists
        dialogueBoxImage = GetComponentInChildren<Image>(true);
        
        if (dialogueBoxImage == null)
        {
            // Create Image GameObject as child
            GameObject imageObj = new GameObject("DialogueBoxImage", typeof(RectTransform), typeof(Image));
            imageObj.transform.SetParent(transform, false);
            imageObj.SetActive(true);
            
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
            
            dialogueBoxImage = imageObj.GetComponent<Image>();
            dialogueBoxImage.sprite = dialogueSprite;
            dialogueBoxImage.type = Image.Type.Sliced;
            dialogueBoxImage.preserveAspect = true;
            dialogueBoxImage.enabled = true;
            
            Debug.Log($"DialogueBoxUI: Created Image component from assigned sprite '{dialogueSprite.name}'");
        }
        else
        {
            // Image exists, just assign the sprite
            dialogueBoxImage.sprite = dialogueSprite;
            dialogueBoxImage.enabled = true;
            dialogueBoxImage.gameObject.SetActive(true);
            Debug.Log($"DialogueBoxUI: Assigned sprite '{dialogueSprite.name}' to existing Image component");
        }
    }
    
    /// <summary>
    /// Ensures all required components are found or created
    /// </summary>
    private void EnsureComponents()
    {
        // If sprite is assigned but Image is null, create it
        if (dialogueSprite != null && dialogueBoxImage == null)
        {
            CreateImageComponentFromSprite();
        }
        
        // Find Image component (search even if inactive)
        if (dialogueBoxImage == null)
        {
            dialogueBoxImage = GetComponentInChildren<Image>(true); // true = include inactive
            if (dialogueBoxImage == null)
            {
                // Try to find by name
                Transform imageTransform = transform.Find("DialogueBoxImage");
                if (imageTransform != null)
                    dialogueBoxImage = imageTransform.GetComponent<Image>();
            }
        }
        
        // Find Text component (search even if inactive)
        if (dialogueText == null)
        {
            dialogueText = GetComponentInChildren<Text>(true); // true = include inactive
            if (dialogueText == null)
            {
                // Try to find by name
                Transform textTransform = transform.Find("DialogueText");
                if (textTransform != null)
                    dialogueText = textTransform.GetComponent<Text>();
            }
        }
    }
    
    /// <summary>
    /// Sets the sprite for the dialogue box
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        // Try to find Image component if it's null
        if (dialogueBoxImage == null)
        {
            EnsureComponents();
        }
        
        // If still null and we have a sprite assigned directly, create Image component
        if (dialogueBoxImage == null && dialogueSprite != null)
        {
            CreateImageComponentFromSprite();
        }
        
        // If still null, try to create a basic Image component
        if (dialogueBoxImage == null)
        {
            // Try to find any Image component in children
            Image foundImage = GetComponentInChildren<Image>(true);
            if (foundImage != null)
            {
                dialogueBoxImage = foundImage;
            }
            else
            {
                // Create Image component as fallback
                GameObject imageObj = new GameObject("DialogueBoxImage", typeof(RectTransform), typeof(Image));
                imageObj.transform.SetParent(transform, false);
                imageObj.SetActive(true);
                
                RectTransform imageRect = imageObj.GetComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
                
                dialogueBoxImage = imageObj.GetComponent<Image>();
                dialogueBoxImage.type = Image.Type.Sliced;
                dialogueBoxImage.preserveAspect = true;
                
                Debug.Log($"DialogueBoxUI: Created fallback Image component for GameObject: {gameObject.name}");
            }
        }
        
        if (dialogueBoxImage != null && sprite != null)
        {
            dialogueBoxImage.sprite = sprite;
            dialogueBoxImage.preserveAspect = true;
            dialogueBoxImage.enabled = true;
            dialogueBoxImage.gameObject.SetActive(true);
            
            // Ensure the Image component is visible
            Canvas.ForceUpdateCanvases();
            
            Debug.Log($"DialogueBoxUI: Set sprite '{sprite.name}'. Image enabled: {dialogueBoxImage.enabled}, Active: {dialogueBoxImage.gameObject.activeSelf}");
        }
        else if (dialogueBoxImage == null)
        {
            Debug.LogError($"DialogueBoxUI: SetSprite called but dialogueBoxImage is null! GameObject: {gameObject.name}, Children: {transform.childCount}");
        }
        else if (sprite == null)
        {
            Debug.LogWarning($"DialogueBoxUI: SetSprite called with null sprite!");
        }
    }
    
    /// <summary>
    /// Sets the text content of the dialogue box
    /// </summary>
    public void SetText(string text)
    {
        // Try to find text component if it's null
        if (dialogueText == null)
        {
            EnsureComponents();
        }
        
        if (dialogueText != null)
        {
            dialogueText.text = text ?? "";
            // Make sure text is enabled and visible
            dialogueText.enabled = true;
            dialogueText.gameObject.SetActive(true);
            
            // Ensure text is on top (higher sorting order)
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                dialogueText.transform.SetAsLastSibling();
            }
            
            // Force text to update
            dialogueText.SetAllDirty();
            Canvas.ForceUpdateCanvases();
            
            Debug.Log($"DialogueBoxUI: Set text to '{text}'. Font size: {dialogueText.fontSize}, Color: {dialogueText.color}, Enabled: {dialogueText.enabled}");
        }
        else
        {
            Debug.LogError($"DialogueBoxUI: SetText called but dialogueText is null! GameObject: {gameObject.name}, Children: {transform.childCount}");
            // Try to create text component as fallback
            CreateTextComponentIfMissing();
            if (dialogueText != null)
            {
                dialogueText.text = text ?? "";
                dialogueText.enabled = true;
                dialogueText.gameObject.SetActive(true);
                dialogueText.SetAllDirty();
                Canvas.ForceUpdateCanvases();
                Debug.Log($"DialogueBoxUI: Created fallback text component and set text to '{text}'");
            }
        }
    }
    
    /// <summary>
    /// Creates a text component if it's missing (fallback)
    /// </summary>
    private void CreateTextComponentIfMissing()
    {
        if (dialogueText != null) return;
        
        GameObject textObj = new GameObject("DialogueText", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(transform, false);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        dialogueText = textObj.GetComponent<Text>();
        dialogueText.text = "";
        dialogueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dialogueText.fontSize = 2400; // Large size for world space
        dialogueText.color = Color.black;
        dialogueText.alignment = TextAnchor.MiddleCenter;
        dialogueText.supportRichText = false;
        dialogueText.horizontalOverflow = HorizontalWrapMode.Wrap;
        dialogueText.verticalOverflow = VerticalWrapMode.Truncate;
        
        Debug.Log("DialogueBoxUI: Created fallback Text component");
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
        
        // Ensure Image and Text components are active and enabled
        if (dialogueBoxImage != null)
        {
            dialogueBoxImage.gameObject.SetActive(true);
            dialogueBoxImage.enabled = true;
        }
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.enabled = true;
        }
        
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
        
        // Ensure Image and Text components are active and enabled
        if (dialogueBoxImage != null)
        {
            dialogueBoxImage.gameObject.SetActive(true);
            dialogueBoxImage.enabled = true;
        }
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            dialogueText.enabled = true;
        }
        
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

