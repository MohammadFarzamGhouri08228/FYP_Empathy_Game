using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
/// Controls dialogue box that appears at a far distance from the player.
/// The dialogue remains visible as the camera moves, and changes when 'P' is pressed.
/// </summary>
public class FarDistanceDialogueController : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("Array of dialogue messages to cycle through when pressing 'P'")]
    [SerializeField] private string[] dialogueMessages = {
        "Welcome to the maze!",
        "Find your way through...",
        "Watch out for dangers!",
        "Keep moving forward!"
    };
    
    [Header("Position Settings")]
    [Tooltip("How to position the dialogue: Fixed (world position), FollowCamera (follows camera with offset), or FollowPlayer (follows player with offset)")]
    [SerializeField] private DialoguePositionMode positionMode = DialoguePositionMode.FollowCamera;
    
    [Tooltip("World position where the dialogue box will appear (only used if Position Mode is Fixed)")]
    [SerializeField] private Vector3 dialogueWorldPosition = new Vector3(50f, 10f, 0f);
    
    [Tooltip("Reference to the player transform (optional, auto-finds if not set)")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("Reference to the camera transform (optional, auto-finds Main Camera if not set)")]
    [SerializeField] private Transform cameraTransform;
    
    [Tooltip("Offset from camera/player position (used when Position Mode is FollowCamera or FollowPlayer)")]
    [SerializeField] private Vector3 followOffset = new Vector3(30f, 5f, 0f);
    
    [Tooltip("If true and using Fixed mode, position will be relative to player's starting position")]
    [SerializeField] private bool useRelativePosition = false;
    
    [Tooltip("Offset from player position if using relative positioning (only used with Fixed mode)")]
    [SerializeField] private Vector3 relativeOffset = new Vector3(30f, 5f, 0f);
    
    [Header("Dialogue Box Manager")]
    [Tooltip("Reference to DialogueBoxManager. If null, will try to find one in scene.")]
    [SerializeField] private DialogueBoxManager dialogueManager;
    
    [Header("Input Settings")]
    //[Tooltip("Key to press to change dialogue (default: P)")]
    //[SerializeField] private KeyCode changeDialogueKey = KeyCode.P;
    
    [Header("Display Settings")]
    [Tooltip("Sprite index to use for dialogue box (from DialogueBoxManager's sprite array)")]
    [SerializeField] private int dialogueSpriteIndex = 0;
    
    [Tooltip("Show dialogue automatically on start")]
    [SerializeField] private bool showOnStart = true;
    
    private int currentDialogueIndex = 0;
    private Vector3 initialPlayerPosition;
    private Canvas worldSpaceCanvas;
    
    /// <summary>
    /// Enum for dialogue positioning modes
    /// </summary>
    public enum DialoguePositionMode
    {
        Fixed,           // Fixed world position
        FollowCamera,    // Follows camera with offset (always visible)
        FollowPlayer     // Follows player with offset (always visible)
    }
    
    void Start()
    {
        // Find or create dialogue manager
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueBoxManager>();
            
            if (dialogueManager == null)
            {
                Debug.LogError("FarDistanceDialogueController: DialogueBoxManager not found! Please add one to the scene.");
                return;
            }
        }
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Try to find by component
                PlayerController2 playerController = FindFirstObjectByType<PlayerController2>();
                if (playerController != null)
                {
                    playerTransform = playerController.transform;
                }
            }
        }
        
        // Find camera if not assigned (needed for FollowCamera mode)
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                // Try to find any camera
                Camera anyCamera = FindFirstObjectByType<Camera>();
                if (anyCamera != null)
                {
                    cameraTransform = anyCamera.transform;
                }
            }
        }
        
        // Store initial player position for relative positioning
        if (playerTransform != null && useRelativePosition && positionMode == DialoguePositionMode.Fixed)
        {
            initialPlayerPosition = playerTransform.position;
        }
        
        // Ensure world space canvas exists
        EnsureWorldSpaceCanvas();
        
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
        
        // Update dialogue position if following camera or player
        if (positionMode == DialoguePositionMode.FollowCamera || positionMode == DialoguePositionMode.FollowPlayer)
        {
            UpdateFollowingPosition();
        }
    }
    
    /// <summary>
    /// Updates the dialogue position when following camera or player
    /// </summary>
    private void UpdateFollowingPosition()
    {
        if (worldSpaceCanvas == null || dialogueManager == null) return;
        
        Vector3 targetPosition = Vector3.zero;
        Transform targetTransform = null;
        
        if (positionMode == DialoguePositionMode.FollowCamera)
        {
            if (cameraTransform != null)
            {
                targetTransform = cameraTransform;
                targetPosition = cameraTransform.position + followOffset;
            }
            else
            {
                Debug.LogWarning("FarDistanceDialogueController: Camera transform not found for FollowCamera mode.");
                return;
            }
        }
        else if (positionMode == DialoguePositionMode.FollowPlayer)
        {
            if (playerTransform != null)
            {
                targetTransform = playerTransform;
                targetPosition = playerTransform.position + followOffset;
            }
            else
            {
                Debug.LogWarning("FarDistanceDialogueController: Player transform not found for FollowPlayer mode.");
                return;
            }
        }
        
        // Update canvas position
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.position = targetPosition;
        }
        
        // Update dialogue position if it's currently showing
        if (dialogueManager != null && dialogueManager.IsDialogueVisible())
        {
            dialogueManager.ShowDialogueAtPosition(
                dialogueMessages[currentDialogueIndex], 
                targetPosition, 
                dialogueSpriteIndex
            );
        }
    }
    
    /// <summary>
    /// Ensures a World Space Canvas exists for the dialogue
    /// </summary>
    private void EnsureWorldSpaceCanvas()
    {
        // Check if dialogue manager's canvas is world space
        Canvas existingCanvas = dialogueManager.GetComponentInParent<Canvas>();
        
        if (existingCanvas != null)
        {
            if (existingCanvas.renderMode == RenderMode.WorldSpace)
            {
                worldSpaceCanvas = existingCanvas;
                Debug.Log("FarDistanceDialogueController: Using existing World Space Canvas.");
                return;
            }
            else
            {
                // Convert existing Screen Space canvas to World Space
                Debug.Log($"FarDistanceDialogueController: Converting existing {existingCanvas.renderMode} canvas to World Space.");
                ConvertCanvasToWorldSpace(existingCanvas);
                worldSpaceCanvas = existingCanvas;
                return;
            }
        }
        
        // Check if DialogueBoxManager will create a canvas (it might not have run Awake yet)
        // We'll create our own World Space canvas and the manager will use it
        GameObject canvasObj = new GameObject("WorldSpaceDialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        worldSpaceCanvas = canvasObj.GetComponent<Canvas>();
        worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
        
        // Set canvas scale for 2D (typically 0.01 for world space UI in 2D)
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Position canvas at dialogue position
        canvasObj.transform.position = GetDialoguePosition();
        
        // Move dialogue manager to be a child of the world space canvas
        // This way DialogueBoxManager's EnsureCanvas will detect it and use it
        dialogueManager.transform.SetParent(canvasObj.transform, false);
        
        // IMPORTANT: Adjust text settings for World Space Canvas
        // Since canvas scale is 0.01, we need much larger font size to be visible
        // The DialogueBoxManager will handle this, but we should ensure it's set correctly
        AdjustTextForWorldSpace();
        
        Debug.Log("FarDistanceDialogueController: Created World Space Canvas for far distance dialogue.");
    }
    
    /// <summary>
    /// Converts an existing canvas to World Space
    /// </summary>
    private void ConvertCanvasToWorldSpace(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Change render mode to World Space
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Adjust canvas settings for World Space
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            // Set appropriate size and scale for 2D World Space
            canvasRect.sizeDelta = new Vector2(100, 100);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            // Position at dialogue position
            canvasRect.position = GetDialoguePosition();
        }
        
        // Update CanvasScaler for World Space
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
        }
        
        Debug.Log("FarDistanceDialogueController: Successfully converted canvas to World Space.");
    }
    
    /// <summary>
    /// Gets the world position where dialogue should appear
    /// </summary>
    private Vector3 GetDialoguePosition()
    {
        if (positionMode == DialoguePositionMode.FollowCamera)
        {
            if (cameraTransform != null)
            {
                return cameraTransform.position + followOffset;
            }
            return followOffset; // Fallback
        }
        else if (positionMode == DialoguePositionMode.FollowPlayer)
        {
            if (playerTransform != null)
            {
                return playerTransform.position + followOffset;
            }
            return followOffset; // Fallback
        }
        else // Fixed mode
        {
            if (useRelativePosition && playerTransform != null)
            {
                return initialPlayerPosition + relativeOffset;
            }
            return dialogueWorldPosition;
        }
    }
    
    /// <summary>
    /// Adjusts text settings for World Space Canvas (larger font size needed)
    /// </summary>
    private void AdjustTextForWorldSpace()
    {
        if (dialogueManager == null) return;
        
        // Use reflection to access private fields and adjust text size for world space
        // World Space Canvas with scale 0.01 needs font size ~2400 to appear as size 24
        var textFontSizeField = typeof(DialogueBoxManager).GetField("textFontSize", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (textFontSizeField != null)
        {
            int currentSize = (int)textFontSizeField.GetValue(dialogueManager);
            // If font size is small (like 24), scale it up for world space (0.01 scale = 100x multiplier)
            if (currentSize < 100)
            {
                int worldSpaceSize = currentSize * 100; // Scale up for 0.01 canvas scale
                textFontSizeField.SetValue(dialogueManager, worldSpaceSize);
                Debug.Log($"FarDistanceDialogueController: Adjusted text font size from {currentSize} to {worldSpaceSize} for World Space Canvas.");
            }
        }
    }
    
    /// <summary>
    /// Shows the current dialogue message
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (dialogueManager == null || dialogueMessages == null || dialogueMessages.Length == 0)
        {
            Debug.LogWarning("FarDistanceDialogueController: Cannot show dialogue - manager or messages not set up.");
            return;
        }
        
        // Clamp index to valid range
        currentDialogueIndex = Mathf.Clamp(currentDialogueIndex, 0, dialogueMessages.Length - 1);
        
        string message = dialogueMessages[currentDialogueIndex];
        Vector3 position = GetDialoguePosition();
        
        // Show dialogue at the specified world position
        dialogueManager.ShowDialogueAtPosition(message, position, dialogueSpriteIndex);
        
        // Verify text is set (debug)
        if (dialogueManager.IsDialogueVisible())
        {
            Debug.Log($"FarDistanceDialogueController: Showing dialogue '{message}' at position {position}");
        }
        else
        {
            Debug.LogWarning($"FarDistanceDialogueController: Dialogue called but not visible! Message: '{message}'");
        }
    }
    
    /// <summary>
    /// Changes to the next dialogue message
    /// </summary>
    public void ChangeToNextDialogue()
    {
        if (dialogueMessages == null || dialogueMessages.Length == 0)
        {
            Debug.LogWarning("FarDistanceDialogueController: No dialogue messages available.");
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
            Debug.LogWarning("FarDistanceDialogueController: No dialogue messages available.");
            return;
        }
        
        if (index < 0 || index >= dialogueMessages.Length)
        {
            Debug.LogWarning($"FarDistanceDialogueController: Dialogue index {index} is out of range (0-{dialogueMessages.Length - 1}).");
            return;
        }
        
        currentDialogueIndex = index;
        ShowCurrentDialogue();
    }
    
    /// <summary>
    /// Sets the dialogue position at runtime
    /// </summary>
    public void SetDialoguePosition(Vector3 newPosition)
    {
        dialogueWorldPosition = newPosition;
        
        // Update canvas position if it exists
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.position = newPosition;
        }
        
        // Update dialogue position if it's currently showing
        if (dialogueManager != null && dialogueManager.IsDialogueVisible())
        {
            ShowCurrentDialogue();
        }
    }
    
    /// <summary>
    /// Adds a new dialogue message to the array
    /// </summary>
    public void AddDialogueMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("FarDistanceDialogueController: Cannot add empty dialogue message.");
            return;
        }
        
        System.Array.Resize(ref dialogueMessages, dialogueMessages.Length + 1);
        dialogueMessages[dialogueMessages.Length - 1] = message;
    }
}

