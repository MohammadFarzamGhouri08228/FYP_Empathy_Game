using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
/// <summary>
/// Simple dialogue controller that displays text on a triangle sprite using TextMeshPro.
/// Press 'P' to cycle through dialogue messages.
/// </summary>
public class SimpleDialogueController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("Array of dialogue messages to cycle through when pressing 'P'")]
    [SerializeField] private string[] dialogueMessages = {
        "Welcome to the maze!",
        "Find your way through...",
        "Watch out for dangers!",
        "Keep moving forward!"
    };
    
    [Header("References")]
    [Tooltip("The triangle GameObject. If null, will search for GameObject named 'triangle' or 'Triangle'")]
    [SerializeField] private GameObject triangleObject;
    
    [Tooltip("TextMeshProUGUI component for displaying text. If null, will search for it on triangle or its children")]
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    [Header("Input Settings")]
    //[Tooltip("Key to press to change dialogue (default: P)")]
    //[SerializeField] private KeyCode changeDialogueKey = KeyCode.P;
    
    [Header("Display Settings")]
    [Tooltip("Show dialogue automatically on start")]
    [SerializeField] private bool showOnStart = true;
    
    [Tooltip("Text position in world/screen space (X, Y)")]
    [SerializeField] private Vector2 textPosition = new Vector2(-6.82f, 5.42f);
    
    [Tooltip("Use world position (true) or local position relative to triangle (false)")]
    [SerializeField] private bool useWorldPosition = true;
    
    [Header("Text Wrapping Settings")]
    [Tooltip("Enable text wrapping for long sentences")]
    [SerializeField] private bool enableTextWrapping = true;
    
    [Tooltip("Maximum width for text before wrapping (in pixels). Set to 0 to use triangle bounds")]
    [SerializeField] private float maxTextWidth = 0f;
    
    [Tooltip("Enable auto-sizing text to fit within triangle")]
    [SerializeField] private bool autoSizeText = true;
    
    [Tooltip("Minimum font size when auto-sizing")]
    [SerializeField] private float minFontSize = 12f;
    
    [Tooltip("Maximum font size when auto-sizing")]
    [SerializeField] private float maxFontSize = 100f;
    
    [Tooltip("Padding around text (left, right, top, bottom)")]
    [SerializeField] private Vector4 textPadding = new Vector4(10f, 10f, 10f, 10f);
    
    private int currentDialogueIndex = 0;
    private RectTransform triangleRectTransform;
    private RectTransform textRectTransform;
    
    void Start()
    {
        // Find triangle GameObject if not assigned
        if (triangleObject == null)
        {
            triangleObject = GameObject.Find("triangle");
            if (triangleObject == null)
            {
                triangleObject = GameObject.Find("Triangle");
            }
            
            if (triangleObject == null)
            {
                Debug.LogError("SimpleDialogueController: Triangle GameObject not found! Please assign it in the Inspector or name it 'triangle' or 'Triangle'.");
                return;
            }
        }
        
        // Find TextMeshProUGUI component if not assigned
        if (textMeshPro == null)
        {
            // First try to find it on the triangle GameObject itself
            textMeshPro = triangleObject.GetComponent<TextMeshProUGUI>();
            
            // If not found, search in children
            if (textMeshPro == null)
            {
                textMeshPro = triangleObject.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            
            // If still not found, try searching in the Canvas children (since Text (TMP) is usually under Canvas)
            if (textMeshPro == null)
            {
                Canvas canvas = triangleObject.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                {
                    textMeshPro = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }
            
            // Also try searching the entire scene as a last resort
            if (textMeshPro == null)
            {
                textMeshPro = FindFirstObjectByType<TextMeshProUGUI>();
            }
            
            if (textMeshPro == null)
            {
                Debug.LogError("SimpleDialogueController: TextMeshProUGUI component not found! Please assign it in the Inspector or ensure there's a TextMeshProUGUI component in the scene.");
                return;
            }
        }
        
        // Ensure triangle is active
        if (triangleObject != null)
        {
            triangleObject.SetActive(true);
            
            // Get triangle's RectTransform for size calculations
            triangleRectTransform = triangleObject.GetComponent<RectTransform>();
            if (triangleRectTransform == null)
            {
                // Try to find it in children (might be on Canvas)
                Canvas canvas = triangleObject.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    triangleRectTransform = canvas.GetComponent<RectTransform>();
                }
            }
        }
        
        // Get text's RectTransform and configure TextMeshPro
        if (textMeshPro != null)
        {
            textRectTransform = textMeshPro.GetComponent<RectTransform>();
            
            // Set text position
            SetTextPosition();
            
            // Fix invalid font size (-99 or negative values)
            if (textMeshPro.fontSize <= 0)
            {
                textMeshPro.fontSize = 36f; // Set a default valid font size
            }
            
            // Ensure font asset is assigned (use default if not)
            if (textMeshPro.font == null)
            {
                // Try to load default TMP font
                TMP_FontAsset defaultFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()[0];
                if (defaultFont != null)
                {
                    textMeshPro.font = defaultFont;
                }
                else
                {
                    Debug.LogWarning("SimpleDialogueController: No TMP Font Asset found. Please assign one in the TextMeshPro component.");
                }
            }
            
            // Configure TextMeshPro for wrapping
            if (enableTextWrapping)
            {
                textMeshPro.enableWordWrapping = true;
                textMeshPro.overflowMode = TextOverflowModes.Truncate;
            }
            else
            {
                textMeshPro.enableWordWrapping = false;
            }
            
            // Set auto-sizing if enabled
            if (autoSizeText)
            {
                textMeshPro.enableAutoSizing = true;
                textMeshPro.fontSizeMin = minFontSize;
                textMeshPro.fontSizeMax = maxFontSize;
            }
            else
            {
                textMeshPro.enableAutoSizing = false;
            }
            
            // Set alignment to center by default
            textMeshPro.alignment = TextAlignmentOptions.Center;
            
            // Ensure text is visible
            textMeshPro.color = Color.white;
            
            // Set sorting order to ensure text appears above triangle
            // For UI Canvas, we need to set the Canvas sorting order
            Canvas canvas = textMeshPro.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Set canvas sorting order higher than triangle's sorting layer
                canvas.sortingOrder = 10; // Higher number = renders on top
            }
            
            // For Sprite Renderer (if triangle uses one), we can't directly control it from here
            // But we can ensure the text is on a higher layer
        }
        
        // Show initial dialogue if enabled
        if (showOnStart && dialogueMessages.Length > 0)
        {
            ShowCurrentDialogue();
        }
    }
    
    void Update()
    {
        // Check for 'P' key press to change dialogue (only on key down, not held)
        if (Keyboard.current != null && Keyboard.current.pKey.isPressed)
        {
            ChangeToNextDialogue();
        }
    }
    
    /// <summary>
    /// Sets the text position
    /// </summary>
    private void SetTextPosition()
    {
        if (textRectTransform == null) return;
        
        if (useWorldPosition)
        {
            // Set world position (for World Space Canvas or Screen Space)
            textRectTransform.position = new Vector3(textPosition.x, textPosition.y, textRectTransform.position.z);
        }
        else
        {
            // Set local position relative to triangle
            if (triangleObject != null)
            {
                textRectTransform.localPosition = new Vector3(textPosition.x, textPosition.y, textRectTransform.localPosition.z);
            }
            else
            {
                textRectTransform.anchoredPosition = textPosition;
            }
        }
        
        Debug.Log($"SimpleDialogueController: Set text position to ({textPosition.x}, {textPosition.y})");
    }
    
    /// <summary>
    /// Shows the current dialogue message
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (textMeshPro == null)
        {
            Debug.LogWarning("SimpleDialogueController: Cannot show dialogue - TextMeshPro is null!");
            return;
        }
        
        if (dialogueMessages == null || dialogueMessages.Length == 0)
        {
            Debug.LogWarning("SimpleDialogueController: No dialogue messages available!");
            return;
        }
        
        // Clamp index to valid range
        currentDialogueIndex = Mathf.Clamp(currentDialogueIndex, 0, dialogueMessages.Length - 1);
        
        // Set the text
        string message = dialogueMessages[currentDialogueIndex];
        textMeshPro.text = message;
        
        // Set position
        SetTextPosition();
        
        // Configure text wrapping and sizing based on triangle size
        ConfigureTextForTriangle();
        
        // Ensure TextMeshPro is enabled and visible
        textMeshPro.enabled = true;
        if (textMeshPro.gameObject != null)
        {
            textMeshPro.gameObject.SetActive(true);
        }
        
        Debug.Log($"SimpleDialogueController: Showing dialogue '{message}' (Index: {currentDialogueIndex})");
    }
    
    /// <summary>
    /// Configures text wrapping and sizing to fit within the triangle
    /// </summary>
    private void ConfigureTextForTriangle()
    {
        if (textMeshPro == null || textRectTransform == null) return;
        
        // Calculate available width for text
        float availableWidth = maxTextWidth;
        
        if (availableWidth <= 0f && triangleRectTransform != null)
        {
            // Use triangle's width minus padding
            availableWidth = triangleRectTransform.rect.width - textPadding.x - textPadding.y;
        }
        else if (availableWidth <= 0f && textRectTransform != null)
        {
            // Use text's own width
            availableWidth = textRectTransform.rect.width - textPadding.x - textPadding.y;
        }
        
        // Ensure we have a valid width
        if (availableWidth <= 0f)
        {
            availableWidth = 200f; // Default fallback width
        }
        
        // Set text width constraint
        textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, availableWidth);
        
        // Configure TextMeshPro for wrapping
        if (enableTextWrapping)
        {
            textMeshPro.enableWordWrapping = true;
            textMeshPro.overflowMode = TextOverflowModes.Truncate;
        }
        else
        {
            textMeshPro.enableWordWrapping = false;
        }
        
        // Set auto-sizing if enabled
        if (autoSizeText)
        {
            textMeshPro.enableAutoSizing = true;
            textMeshPro.fontSizeMin = minFontSize;
            textMeshPro.fontSizeMax = maxFontSize;
        }
        
        // Set padding on text RectTransform
        textRectTransform.offsetMin = new Vector2(textPadding.x, textPadding.z); // left, bottom
        textRectTransform.offsetMax = new Vector2(-textPadding.y, -textPadding.w); // right, top
        
        // Force TextMeshPro to update
        textMeshPro.ForceMeshUpdate();
    }
    
    /// <summary>
    /// Changes to the next dialogue message
    /// </summary>
    public void ChangeToNextDialogue()
    {
        if (dialogueMessages == null || dialogueMessages.Length == 0)
        {
            Debug.LogWarning("SimpleDialogueController: No dialogue messages available.");
            return;
        }
        
        // Move to next dialogue (loop back to start)
        currentDialogueIndex = (currentDialogueIndex + 1) % dialogueMessages.Length;
        
        // Show the new dialogue
        ShowCurrentDialogue();
    }
    
    /// <summary>
    /// Changes to a specific dialogue message by index
    /// </summary>
    public void ChangeToDialogue(int index)
    {
        if (dialogueMessages == null || dialogueMessages.Length == 0)
        {
            Debug.LogWarning("SimpleDialogueController: No dialogue messages available.");
            return;
        }
        
        if (index < 0 || index >= dialogueMessages.Length)
        {
            Debug.LogWarning($"SimpleDialogueController: Dialogue index {index} is out of range (0-{dialogueMessages.Length - 1}).");
            return;
        }
        
        currentDialogueIndex = index;
        ShowCurrentDialogue();
    }
    
    /// <summary>
    /// Adds a new dialogue message to the array
    /// </summary>
    public void AddDialogueMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("SimpleDialogueController: Cannot add empty dialogue message.");
            return;
        }
        
        System.Array.Resize(ref dialogueMessages, dialogueMessages.Length + 1);
        dialogueMessages[dialogueMessages.Length - 1] = message;
    }
    
    /// <summary>
    /// Gets the current dialogue message
    /// </summary>
    public string GetCurrentDialogue()
    {
        if (dialogueMessages != null && currentDialogueIndex >= 0 && currentDialogueIndex < dialogueMessages.Length)
        {
            return dialogueMessages[currentDialogueIndex];
        }
        return "";
    }
}

