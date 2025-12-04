using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages dialogue box display system.
/// Handles sprite selection, text display, positioning, and multiple dialogue instances.
/// </summary>
public class DialogueBoxManager : MonoBehaviour
{
    [Header("Dialogue Box Sprites")]
    [Tooltip("Array of dialogue box sprites from your sprite sheet. Drag sliced sprites here.")]
    [SerializeField] private Sprite[] dialogueBoxSprites;
    
    [Header("Default Settings")]
    [Tooltip("Default sprite index to use when showing dialogue")]
    [SerializeField] private int defaultSpriteIndex = 0;
    
    [Header("Text Settings")]
    [Tooltip("Font for dialogue text. Leave null to use default.")]
    [SerializeField] private Font textFont;
    
    [Tooltip("Font size for dialogue text")]
    [SerializeField] private int textFontSize = 24;
    
    [Tooltip("Color of dialogue text")]
    [SerializeField] private Color textColor = Color.black;
    
    [Tooltip("Padding around text (X, Y)")]
    [SerializeField] private Vector2 textPadding = new Vector2(20f, 20f);
    
    [Header("Dialogue Box Prefab")]
    [Tooltip("Prefab for dialogue box UI. If null, will create one automatically.")]
    [SerializeField] private GameObject dialogueBoxPrefab;
    
    [Header("Display Settings")]
    [Tooltip("Scale of the dialogue box")]
    [SerializeField] private Vector3 dialogueBoxScale = Vector3.one;
    
    [Tooltip("Auto-hide dialogue after this many seconds. Set to 0 to disable.")]
    [SerializeField] private float autoHideDuration = 0f;
    
    [Header("Animation Settings")]
    [Tooltip("Use fade animations when showing/hiding")]
    [SerializeField] private bool useAnimations = true;
    
    private Canvas canvas;
    private DialogueBoxUI currentDialogueBox;
    private Transform followTarget;
    private Vector3 followOffset;
    private Coroutine autoHideCoroutine;
    
    void Awake()
    {
        EnsureCanvas();
        ValidateSprites();
    }
    
    void Update()
    {
        // Update position if following a target
        if (currentDialogueBox != null && followTarget != null && currentDialogueBox.IsVisible())
        {
            UpdateFollowingPosition();
        }
    }
    
    /// <summary>
    /// Ensures a Canvas exists for UI rendering
    /// </summary>
    private void EnsureCanvas()
    {
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            // Try to find existing canvas in scene
            canvas = FindFirstObjectByType<Canvas>();
            
            if (canvas == null)
            {
                // Create new canvas
                GameObject canvasObj = new GameObject("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                // Make this object a child of canvas
                transform.SetParent(canvas.transform, false);
            }
            else
            {
                // Make this object a child of existing canvas
                transform.SetParent(canvas.transform, false);
            }
        }
    }
    
    /// <summary>
    /// Validates that sprites are assigned
    /// </summary>
    private void ValidateSprites()
    {
        if (dialogueBoxSprites == null || dialogueBoxSprites.Length == 0)
        {
            Debug.LogWarning("DialogueBoxManager: No dialogue box sprites assigned! Please assign sprites in the Inspector.");
        }
        
        if (defaultSpriteIndex < 0 || (dialogueBoxSprites != null && defaultSpriteIndex >= dialogueBoxSprites.Length))
        {
            Debug.LogWarning($"DialogueBoxManager: Default sprite index {defaultSpriteIndex} is out of range. Resetting to 0.");
            defaultSpriteIndex = 0;
        }
    }
    
    /// <summary>
    /// Gets or creates a dialogue box UI instance
    /// </summary>
    private DialogueBoxUI GetOrCreateDialogueBox()
    {
        if (currentDialogueBox == null)
        {
            GameObject dialogueObj;
            
            if (dialogueBoxPrefab != null)
            {
                dialogueObj = Instantiate(dialogueBoxPrefab, transform);
            }
            else
            {
                // Create dialogue box UI structure
                dialogueObj = new GameObject("DialogueBox", typeof(RectTransform), typeof(CanvasGroup));
                dialogueObj.transform.SetParent(transform, false);
                
                // Create Image for dialogue box sprite
                GameObject imageObj = new GameObject("DialogueBoxImage", typeof(RectTransform), typeof(Image));
                imageObj.transform.SetParent(dialogueObj.transform, false);
                
                RectTransform imageRect = imageObj.GetComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = Vector2.zero;
                imageRect.offsetMax = Vector2.zero;
                
                Image image = imageObj.GetComponent<Image>();
                image.type = Image.Type.Sliced;
                
                // Create Text for dialogue text
                GameObject textObj = new GameObject("DialogueText", typeof(RectTransform), typeof(Text));
                textObj.transform.SetParent(dialogueObj.transform, false);
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(textPadding.x, textPadding.y);
                textRect.offsetMax = new Vector2(-textPadding.x, -textPadding.y);
                
                Text text = textObj.GetComponent<Text>();
                text.text = "";
                text.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = textFontSize;
                text.color = textColor;
                text.alignment = TextAnchor.MiddleCenter;
                text.supportRichText = false;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                
                // Add DialogueBoxUI component
                DialogueBoxUI dialogueUI = dialogueObj.AddComponent<DialogueBoxUI>();
                
                // Set references using reflection (since fields are private)
                var imageField = typeof(DialogueBoxUI).GetField("dialogueBoxImage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var textField = typeof(DialogueBoxUI).GetField("dialogueText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (imageField != null) imageField.SetValue(dialogueUI, image);
                if (textField != null) textField.SetValue(dialogueUI, text);
            }
            
            currentDialogueBox = dialogueObj.GetComponent<DialogueBoxUI>();
            
            if (currentDialogueBox == null)
            {
                currentDialogueBox = dialogueObj.AddComponent<DialogueBoxUI>();
            }
            
            // Configure dialogue box
            currentDialogueBox.SetFont(textFont);
            currentDialogueBox.SetFontSize(textFontSize);
            currentDialogueBox.SetTextColor(textColor);
            currentDialogueBox.SetPadding(textPadding);
            currentDialogueBox.SetScale(dialogueBoxScale);
        }
        
        return currentDialogueBox;
    }
    
    /// <summary>
    /// Shows dialogue with default sprite
    /// </summary>
    public void ShowDialogue(string text)
    {
        ShowDialogue(text, defaultSpriteIndex);
    }
    
    /// <summary>
    /// Shows dialogue with specified sprite index
    /// </summary>
    public void ShowDialogue(string text, int spriteIndex)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("DialogueBoxManager: Cannot show empty dialogue text.");
            return;
        }
        
        if (dialogueBoxSprites == null || dialogueBoxSprites.Length == 0)
        {
            Debug.LogError("DialogueBoxManager: No dialogue box sprites assigned!");
            return;
        }
        
        // Clamp sprite index
        spriteIndex = Mathf.Clamp(spriteIndex, 0, dialogueBoxSprites.Length - 1);
        
        DialogueBoxUI dialogueBox = GetOrCreateDialogueBox();
        
        // Set sprite and text
        dialogueBox.SetSprite(dialogueBoxSprites[spriteIndex]);
        dialogueBox.SetText(text);
        
        // Show dialogue box
        if (useAnimations)
        {
            dialogueBox.Show();
        }
        else
        {
            dialogueBox.ShowImmediate();
        }
        
        // Stop any existing auto-hide coroutine
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        
        // Start auto-hide if enabled
        if (autoHideDuration > 0f)
        {
            autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
        }
        
        // Clear follow target
        followTarget = null;
    }
    
    /// <summary>
    /// Shows dialogue at a specific world position
    /// </summary>
    public void ShowDialogueAtPosition(string text, Vector3 worldPosition, int spriteIndex = -1)
    {
        if (spriteIndex < 0) spriteIndex = defaultSpriteIndex;
        
        ShowDialogue(text, spriteIndex);
        
        if (currentDialogueBox != null)
        {
            currentDialogueBox.SetWorldPosition(worldPosition);
        }
        
        followTarget = null;
    }
    
    /// <summary>
    /// Shows dialogue following a GameObject transform
    /// </summary>
    public void ShowDialogueFollowing(string text, Transform target, Vector3 offset, int spriteIndex = -1)
    {
        if (target == null)
        {
            Debug.LogWarning("DialogueBoxManager: Cannot follow null target.");
            ShowDialogue(text, spriteIndex >= 0 ? spriteIndex : defaultSpriteIndex);
            return;
        }
        
        if (spriteIndex < 0) spriteIndex = defaultSpriteIndex;
        
        ShowDialogue(text, spriteIndex);
        
        followTarget = target;
        followOffset = offset;
        
        UpdateFollowingPosition();
    }
    
    /// <summary>
    /// Updates the position when following a target
    /// </summary>
    private void UpdateFollowingPosition()
    {
        if (currentDialogueBox != null && followTarget != null)
        {
            Vector3 worldPos = followTarget.position + followOffset;
            currentDialogueBox.SetWorldPosition(worldPos);
        }
    }
    
    /// <summary>
    /// Hides the current dialogue
    /// </summary>
    public void HideDialogue()
    {
        if (currentDialogueBox != null)
        {
            if (useAnimations)
            {
                currentDialogueBox.Hide();
            }
            else
            {
                currentDialogueBox.HideImmediate();
            }
        }
        
        followTarget = null;
        
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }
    
    /// <summary>
    /// Changes the sprite without changing text
    /// </summary>
    public void SetSprite(int spriteIndex)
    {
        if (currentDialogueBox != null && dialogueBoxSprites != null && spriteIndex >= 0 && spriteIndex < dialogueBoxSprites.Length)
        {
            currentDialogueBox.SetSprite(dialogueBoxSprites[spriteIndex]);
        }
    }
    
    /// <summary>
    /// Changes the text without changing sprite
    /// </summary>
    public void SetText(string text)
    {
        if (currentDialogueBox != null)
        {
            currentDialogueBox.SetText(text);
        }
    }
    
    /// <summary>
    /// Auto-hide coroutine
    /// </summary>
    private IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(autoHideDuration);
        HideDialogue();
    }
    
    /// <summary>
    /// Checks if dialogue is currently visible
    /// </summary>
    public bool IsDialogueVisible()
    {
        return currentDialogueBox != null && currentDialogueBox.IsVisible();
    }
    
    void OnDestroy()
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
    }
}

