using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the intro sequence for Maze2 scene.
/// Freezes player movement and prevents timer from starting until all dialogues are complete.
/// </summary>
public class Maze2IntroController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the dialogue controller (SimpleDialogueController or FarDistanceDialogueController)")]
    [SerializeField] private MonoBehaviour dialogueController;
    
    [Tooltip("Reference to the player controller")]
    [SerializeField] private PlayerController2 playerController;
    
    [Tooltip("Reference to the stopwatch timer")]
    [SerializeField] private StopwatchTimer stopwatchTimer;
    
    [Header("UI Feedback (Optional)")]
    [Tooltip("Text to display prompting user to press arrow key (optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;
    
    [Tooltip("Instruction message to show")]
    [SerializeField] private string instructionMessage = "Press any arrow key to start...";
    
    private bool introComplete = false;
    private bool playerWasFrozen = false;
    
    // Store original player settings
    private float originalMoveSpeed;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController2>();
        }
        
        if (stopwatchTimer == null)
        {
            stopwatchTimer = FindFirstObjectByType<StopwatchTimer>();
        }
        
        if (dialogueController == null)
        {
            // Try to find SimpleDialogueController first
            dialogueController = FindFirstObjectByType<SimpleDialogueController>();
            
            // If not found, try FarDistanceDialogueController
            if (dialogueController == null)
            {
                dialogueController = FindFirstObjectByType<FarDistanceDialogueController>();
            }
        }
        
        // Verify required components
        if (playerController == null)
        {
            Debug.LogError("Maze2IntroController: PlayerController2 not found! Cannot freeze player.");
        }
        
        if (stopwatchTimer == null)
        {
            Debug.LogWarning("Maze2IntroController: StopwatchTimer not found! Timer control will be skipped.");
        }
        
        if (dialogueController == null)
        {
            Debug.LogWarning("Maze2IntroController: Dialogue controller not found! Intro sequence may not work correctly.");
        }
        
        // Start the intro sequence
        StartIntroSequence();
    }
    
    void Update()
    {
        // If game hasn't started, listen for arrow keys
        if (!introComplete)
        {
            if (Keyboard.current != null)
            {
                // Check for any arrow key press
                bool arrowKeyPressed = 
                    Keyboard.current.upArrowKey.wasPressedThisFrame ||
                    Keyboard.current.downArrowKey.wasPressedThisFrame ||
                    Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                    Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                    Keyboard.current.wKey.wasPressedThisFrame ||
                    Keyboard.current.aKey.wasPressedThisFrame ||
                    Keyboard.current.sKey.wasPressedThisFrame ||
                    Keyboard.current.dKey.wasPressedThisFrame;
                
                if (arrowKeyPressed)
                {
                    StartGame();
                }
            }
        }
    }
    
    /// <summary>
    /// Starts the intro sequence by freezing the player and stopping the timer
    /// </summary>
    private void StartIntroSequence()
    {
        Debug.Log("Maze2IntroController: Starting intro sequence...");
        
        // Freeze the player
        FreezePlayer();
        
        // Stop the timer (ensure it doesn't auto-start)
        if (stopwatchTimer != null)
        {
            stopwatchTimer.StopTimer();
            Debug.Log("Maze2IntroController: Timer stopped.");
        }
        
        // Show instruction text if available
        if (instructionText != null)
        {
            instructionText.text = instructionMessage;
            instructionText.gameObject.SetActive(true);
        }
        
        Debug.Log("Maze2IntroController: Player frozen. Waiting for arrow key press to start...");
    }
    
    /// <summary>
    /// Starts the game after arrow key is pressed
    /// </summary>
    private void StartGame()
    {
        if (introComplete) return; // Prevent multiple calls
        
        introComplete = true;
        
        Debug.Log("Maze2IntroController: Arrow key pressed! Starting game...");
        
        // Unfreeze the player
        UnfreezePlayer();
        
        // Start the timer
        if (stopwatchTimer != null)
        {
            stopwatchTimer.StartTimer();
            Debug.Log("Maze2IntroController: Timer started.");
        }
        
        // Hide instruction text
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
        
        Debug.Log("Maze2IntroController: Intro sequence complete! Player can now move and timer is running.");
    }
    
    /// <summary>
    /// Freezes the player by disabling the PlayerController2 component
    /// </summary>
    private void FreezePlayer()
    {
        if (playerController != null && playerController.enabled)
        {
            playerController.enabled = false;
            playerWasFrozen = true;
            Debug.Log("Maze2IntroController: Player frozen (controller disabled).");
        }
    }
    
    /// <summary>
    /// Unfreezes the player by re-enabling the PlayerController2 component
    /// </summary>
    private void UnfreezePlayer()
    {
        if (playerController != null && playerWasFrozen)
        {
            playerController.enabled = true;
            playerWasFrozen = false;
            Debug.Log("Maze2IntroController: Player unfrozen (controller enabled).");
        }
    }
    
    /// <summary>
    /// Public method to manually complete the intro (useful for debugging or skip functionality)
    /// </summary>
    [ContextMenu("Skip Intro")]
    public void SkipIntro()
    {
        StartGame();
    }
    
    /// <summary>
    /// Returns whether the intro sequence is complete
    /// </summary>
    public bool IsIntroComplete()
    {
        return introComplete;
    }
}
