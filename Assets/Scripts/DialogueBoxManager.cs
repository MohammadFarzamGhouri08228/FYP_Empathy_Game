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
                
                // Ensure GameObject is active so components can be found
                dialogueObj.SetActive(true);
                
                // Convert Transform to RectTransform if needed (for UI)
                RectTransform rectTransform = dialogueObj.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    // Remove regular Transform and add RectTransform
                    Transform oldTransform = dialogueObj.transform;
                    Vector3 position = oldTransform.position;
                    Quaternion rotation = oldTransform.rotation;
                    Vector3 scale = oldTransform.localScale;
                    
                    // Can't directly replace Transform, so we'll work with what we have
                    // Just ensure we have a RectTransform component
                    rectTransform = dialogueObj.AddComponent<RectTransform>();
                    rectTransform.position = position;
                    rectTransform.rotation = rotation;
                    rectTransform.localScale = scale;
                }
                
                // Check if prefab has Sprite Renderer (2D sprite) - we need to convert it to UI Image
                SpriteRenderer spriteRenderer = dialogueObj.GetComponent<SpriteRenderer>();
                Sprite spriteToUse = null;
                
                if (spriteRenderer != null)
                {
                    // Extract sprite from Sprite Renderer
                    spriteToUse = spriteRenderer.sprite;
                    // Disable Sprite Renderer (we'll use UI Image instead)
                    spriteRenderer.enabled = false;
                    Debug.Log($"DialogueBoxManager: Found Sprite Renderer on prefab, extracted sprite: {(spriteToUse != null ? spriteToUse.name : "null")}");
                }
                
                // If prefab is used, ensure it has the proper structure
                // Check if it already has DialogueBoxUI component
                DialogueBoxUI existingUI = dialogueObj.GetComponent<DialogueBoxUI>();
                
                // Add CanvasGroup if missing
                if (dialogueObj.GetComponent<CanvasGroup>() == null)
                {
                    dialogueObj.AddComponent<CanvasGroup>();
                }
                
                // Check if it has Image component (for sprite)
                Image existingImage = dialogueObj.GetComponentInChildren<Image>(true);
                if (existingImage == null)
                {
                    // Create Image for dialogue box sprite
                    GameObject imageObj = new GameObject("DialogueBoxImage", typeof(RectTransform), typeof(Image));
                    imageObj.transform.SetParent(dialogueObj.transform, false);
                    imageObj.SetActive(true);
                    
                    RectTransform imageRect = imageObj.GetComponent<RectTransform>();
                    imageRect.anchorMin = Vector2.zero;
                    imageRect.anchorMax = Vector2.one;
                    imageRect.offsetMin = Vector2.zero;
                    imageRect.offsetMax = Vector2.zero;
                    
                    Image image = imageObj.GetComponent<Image>();
                    image.type = Image.Type.Sliced;
                    
                    // If we extracted a sprite from Sprite Renderer, use it
                    if (spriteToUse != null)
                    {
                        image.sprite = spriteToUse;
                        Debug.Log($"DialogueBoxManager: Assigned sprite from Sprite Renderer to Image component");
                    }
                    
                    existingImage = image;
                    
                    Debug.Log($"DialogueBoxManager: Created Image component for prefab");
                }
                else if (spriteToUse != null && existingImage.sprite == null)
                {
                    // Image exists but has no sprite, assign the extracted sprite
                    existingImage.sprite = spriteToUse;
                    Debug.Log($"DialogueBoxManager: Assigned sprite from Sprite Renderer to existing Image component");
                }
                
                // Check if it has Text component
                Text existingText = dialogueObj.GetComponentInChildren<Text>(true);
                if (existingText == null)
                {
                    // Create Text for dialogue text
                    GameObject textObj = new GameObject("DialogueText", typeof(RectTransform), typeof(Text));
                    textObj.transform.SetParent(dialogueObj.transform, false);
                    textObj.SetActive(true);
                    
                    RectTransform textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(textPadding.x, textPadding.y);
                    textRect.offsetMax = new Vector2(-textPadding.x, -textPadding.y);
                    
                    Text text = textObj.GetComponent<Text>();
                    text.text = "";
                    text.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    
                    // Check if we're in a World Space Canvas (scale < 1.0)
                    Canvas parentCanvas = GetComponentInParent<Canvas>();
                    float fontMultiplier = 1f;
                    if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
                    {
                        float canvasScale = parentCanvas.transform.localScale.x;
                        if (canvasScale < 1f && canvasScale > 0f)
                        {
                            fontMultiplier = 1f / canvasScale;
                        }
                    }
                    
                    text.fontSize = Mathf.RoundToInt(textFontSize * fontMultiplier);
                    text.color = textColor;
                    text.alignment = TextAnchor.MiddleCenter;
                    text.supportRichText = false;
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Truncate;
                    text.enabled = true;
                    
                    existingText = text;
                    
                    Debug.Log($"DialogueBoxManager: Created text component for prefab with font size {text.fontSize}");
                }
                
                // Add DialogueBoxUI component if missing
                if (existingUI == null)
                {
                    existingUI = dialogueObj.AddComponent<DialogueBoxUI>();
                    
                    // Use reflection to set Image and Text references directly
                    var imageField = typeof(DialogueBoxUI).GetField("dialogueBoxImage", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var textField = typeof(DialogueBoxUI).GetField("dialogueText", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (imageField != null && existingImage != null) 
                    {
                        imageField.SetValue(existingUI, existingImage);
                        Debug.Log($"DialogueBoxManager: Set dialogueBoxImage reference via reflection: {(existingImage != null ? "Success" : "Failed")}");
                    }
                    
                    if (textField != null && existingText != null) 
                    {
                        textField.SetValue(existingUI, existingText);
                        Debug.Log($"DialogueBoxManager: Set dialogueText reference via reflection: {(existingText != null ? "Success" : "Failed")}");
                    }
                    else
                    {
                        Debug.LogError("DialogueBoxManager: Could not find dialogueText field via reflection or existingText is null!");
                    }
                }
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
                text.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                
                // Check if we're in a World Space Canvas (scale < 1.0)
                // If so, scale up the font size significantly
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                float fontMultiplier = 1f;
                if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
                {
                    float canvasScale = parentCanvas.transform.localScale.x;
                    if (canvasScale < 1f && canvasScale > 0f)
                    {
                        // Scale font size inversely to canvas scale
                        fontMultiplier = 1f / canvasScale;
                    }
                }
                
                text.fontSize = Mathf.RoundToInt(textFontSize * fontMultiplier);
                text.color = textColor;
                text.alignment = TextAnchor.MiddleCenter;
                text.supportRichText = false;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                
                Debug.Log($"DialogueBoxManager: Created text with font size {text.fontSize} (base: {textFontSize}, multiplier: {fontMultiplier})");
                
                // Add DialogueBoxUI component
                DialogueBoxUI dialogueUI = dialogueObj.AddComponent<DialogueBoxUI>();
                
                // Set references using reflection (since fields are private)
                var imageField = typeof(DialogueBoxUI).GetField("dialogueBoxImage", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var textField = typeof(DialogueBoxUI).GetField("dialogueText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (imageField != null) 
                {
                    imageField.SetValue(dialogueUI, image);
                    Debug.Log($"DialogueBoxManager: Set dialogueBoxImage reference: {(image != null ? "Success" : "Failed")}");
                }
                
                if (textField != null) 
                {
                    textField.SetValue(dialogueUI, text);
                    Debug.Log($"DialogueBoxManager: Set dialogueText reference: {(text != null ? "Success" : "Failed")}");
                }
                else
                {
                    Debug.LogError("DialogueBoxManager: Could not find dialogueText field via reflection!");
                }
                
                // Force DialogueBoxUI to refresh component references
                // Call EnsureComponents via reflection or let Start() handle it
            }
            
            // Get or add DialogueBoxUI component
            currentDialogueBox = dialogueObj.GetComponent<DialogueBoxUI>();
            
            if (currentDialogueBox == null)
            {
                currentDialogueBox = dialogueObj.AddComponent<DialogueBoxUI>();
                
                // If we just added it, we need to set up the references
                // Find Image and Text components
                Image foundImage = dialogueObj.GetComponentInChildren<Image>(true);
                Text foundText = dialogueObj.GetComponentInChildren<Text>(true);
                
                if (foundImage != null || foundText != null)
                {
                    // Use reflection to set references
                    var imageField = typeof(DialogueBoxUI).GetField("dialogueBoxImage", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var textField = typeof(DialogueBoxUI).GetField("dialogueText", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (imageField != null && foundImage != null) 
                    {
                        imageField.SetValue(currentDialogueBox, foundImage);
                    }
                    
                    if (textField != null && foundText != null) 
                    {
                        textField.SetValue(currentDialogueBox, foundText);
                    }
                }
            }
            
            // Configure dialogue box
            currentDialogueBox.SetFont(textFont);
            // Don't overwrite font size if it was already set correctly for World Space
            // Calculate the correct font size for the current canvas
            Canvas canvasForFontSize = GetComponentInParent<Canvas>();
            float fontSizeMultiplier = 1f;
            if (canvasForFontSize != null && canvasForFontSize.renderMode == RenderMode.WorldSpace)
            {
                float canvasScale = canvasForFontSize.transform.localScale.x;
                if (canvasScale < 1f && canvasScale > 0f)
                {
                    fontSizeMultiplier = 1f / canvasScale;
                }
            }
            int finalFontSize = Mathf.RoundToInt(textFontSize * fontSizeMultiplier);
            currentDialogueBox.SetFontSize(finalFontSize);
            currentDialogueBox.SetTextColor(textColor);
            currentDialogueBox.SetPadding(textPadding);
            currentDialogueBox.SetScale(dialogueBoxScale);
            
            Debug.Log($"DialogueBoxManager: Configured dialogue box with font size {finalFontSize} (base: {textFontSize}, multiplier: {fontSizeMultiplier})");
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
        
        DialogueBoxUI dialogueBox = GetOrCreateDialogueBox();
        
        // Set sprite and text
        // If dialogueBoxSprites array has sprites, use them
        // Otherwise, use the sprite from the prefab's Image component (extracted from Sprite Renderer)
        if (dialogueBoxSprites != null && dialogueBoxSprites.Length > 0)
        {
            // Clamp sprite index
            spriteIndex = Mathf.Clamp(spriteIndex, 0, dialogueBoxSprites.Length - 1);
            dialogueBox.SetSprite(dialogueBoxSprites[spriteIndex]);
        }
        else
        {
            // No sprites in array, try to preserve existing sprite from Image component
            // The sprite should have been set during prefab instantiation (from Sprite Renderer)
            Image existingImage = dialogueBox.GetComponentInChildren<Image>(true);
            if (existingImage != null && existingImage.sprite != null)
            {
                // Sprite already exists, just ensure it's enabled and visible
                existingImage.enabled = true;
                existingImage.gameObject.SetActive(true);
                dialogueBox.SetSprite(existingImage.sprite); // This will ensure it's properly set
                Debug.Log($"DialogueBoxManager: Using existing sprite from Image component: {existingImage.sprite.name}");
            }
            else
            {
                Debug.LogWarning("DialogueBoxManager: No dialogue box sprites assigned and no sprite found in Image component! Please assign sprites in the Inspector or ensure the prefab has a Sprite Renderer.");
            }
        }
        
        // Ensure text component exists and is visible before setting text
        dialogueBox.SetText(text);
        
        // Debug: Verify text was set
        Debug.Log($"DialogueBoxManager: Showing dialogue '{text}' with sprite index {spriteIndex}");
        
        // Ensure dialogue box GameObject is active
        if (dialogueBox != null && dialogueBox.gameObject != null)
        {
            dialogueBox.gameObject.SetActive(true);
            
            // Ensure all child GameObjects are active (Image and Text)
            foreach (Transform child in dialogueBox.transform)
            {
                if (child != null && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
        
        // Show dialogue box
        if (useAnimations)
        {
            dialogueBox.Show();
        }
        else
        {
            dialogueBox.ShowImmediate();
        }
        
        Debug.Log($"DialogueBoxManager: Dialogue box shown. Active: {dialogueBox.gameObject.activeSelf}, Visible: {dialogueBox.IsVisible()}");
        
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

