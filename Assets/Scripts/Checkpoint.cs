using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActive = true; // Whether this checkpoint is active
    [SerializeField] private int checkpointID = 0; // Unique ID for this checkpoint (used for dialogue system)
    [SerializeField] private bool allowMultipleActivations = false; // Whether checkpoint can be activated multiple times

    [Header("Level Transition")]
    [SerializeField] private bool isFinalCheckpoint = false; // Check this for the very last checkpoint!
    [SerializeField] private string nextSceneName = "Level2"; // Scene to load when reached
    
    // Public getter for checkpoint ID (used by dialogue system)
    public int CheckpointID => checkpointID;
    
    [Header("Detection Settings")]
    [SerializeField] private bool useDistanceDetection = true; // Use distance-based detection instead of collider
    [SerializeField] private float detectionRadius = 1.5f; // Distance to detect player (if using distance detection)
    public bool playerIsClose;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer checkpointVisual; // Optional visual indicator
    [SerializeField] private Sprite activeSprite; // Sprite to show when checkpoint is active
    [SerializeField] private Sprite inactiveSprite; // Sprite to show when checkpoint is inactive
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Optional audio source for checkpoint sound
    [SerializeField] private AudioClip checkpointSound; // Sound to play when checkpoint is activated
    
    private bool hasBeenActivated = false; // Track if this checkpoint has been activated
    public bool HasBeenActivated => hasBeenActivated; // Public getter
    private CheckpointManager checkpointManager;
    private GameObject player; // Cache player reference

    [Header("Dialogue Content")]
    public DialogueLine[] dialogue;

    // Dialogue Tracking State
    private bool dialogueCompleted = false; 
    private bool hasDialogOpened = false;
    private string confirmedDirectionText = "";

    private CheckpointInteraction choiceInteraction; // Legacy component reference (kept for compatibility if needed)

    void Start()
    {
        choiceInteraction = GetComponent<CheckpointInteraction>();

        // Find CheckpointManager
        checkpointManager = CheckpointManager.Instance;
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }
        
        if (checkpointManager == null)
        {
            Debug.LogError($"Checkpoint {checkpointID}: CheckpointManager not found in scene! Please add CheckpointManager GameObject.");
            return;
        }
        
        // Find player
        FindPlayer();
        
        // Get visual component if not assigned
        if (checkpointVisual == null)
        {
            checkpointVisual = GetComponent<SpriteRenderer>();
        }
        
        // Get audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Setup collider if not using distance detection
        if (!useDistanceDetection)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                if (!col.isTrigger)
                {
                    Debug.LogWarning($"Checkpoint {checkpointID}: Collider is not set as trigger! Setting it now.");
                    col.isTrigger = true;
                }
            }
            else
            {
                Debug.LogWarning($"Checkpoint {checkpointID}: No Collider2D found! Please add a Collider2D component and set it as trigger, or enable 'Use Distance Detection'.");
            }
        }
        
        Debug.Log($"Checkpoint {checkpointID}: Initialized at position ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2}). Detection: {(useDistanceDetection ? "Distance-based" : "Collider-based")}");
        if (dialogue != null)
        {
            Debug.Log($"Checkpoint {checkpointID} Dialogue Content: {dialogue.Length} lines.");
        }
        else
        {
            Debug.LogError($"Checkpoint {checkpointID} has NULL dialogue array!");
        }
    }
    
    void Update()
    {
        // Use distance-based detection if enabled
        if (useDistanceDetection && isActive)
        {
            CheckPlayerDistance();
        }
    }

    private void HandleDialogueComplete()
    {
        if (!dialogueCompleted)
        {
            dialogueCompleted = true;
            float totalFrames = DialogueController.Instance != null ? DialogueController.Instance.dialogueTotalFrames : 0;
            float stillFrames = DialogueController.Instance != null ? DialogueController.Instance.dialogueStillFrames : 0;

            // Calculate listening ratio from velocity tracking
            float listenRatio = (totalFrames > 0) ? stillFrames / totalFrames : 1.0f;
            
            Debug.Log($"Checkpoint {checkpointID}: Dialogue Finished. Listen ratio: {listenRatio:F2}");
            if (AdaptiveBackend.Instance != null)
            {
                AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "DialogueInteraction", listenRatio);
            }
            if (checkpointManager != null)
            {
                checkpointManager.RecordDialogueInteraction(checkpointID, listenRatio);
                
                // Start movement tracking based on confirmed direction
                if (player != null && !string.IsNullOrEmpty(confirmedDirectionText))
                {
                    Vector2 expectedDir = Vector2.zero;
                    string lowerText = confirmedDirectionText.ToLower();
                    if (lowerText.Contains("left") || lowerText.Contains("back")) expectedDir = Vector2.left;
                    else if (lowerText.Contains("right") || lowerText.Contains("forward") || lowerText.Contains("ahead")) expectedDir = Vector2.right;
                    else if (lowerText.Contains("up") || lowerText.Contains("jump")) expectedDir = Vector2.up;
                    else if (lowerText.Contains("down")) expectedDir = Vector2.down;
                    
                    if (expectedDir != Vector2.zero)
                    {
                        checkpointManager.StartTrackingMovement(checkpointID, player.transform, expectedDir);
                    }
                }
            }
            
            // Reset for next potential activation
            confirmedDirectionText = "";
        }
    }

    private void HandleOptionSelection(int choiceIndex, ChoiceCategory selectedCategory, string optionText)
    {
        Debug.Log($"Checkpoint {checkpointID}: Option {choiceIndex + 1} chosen - Category: {selectedCategory}, Text: {optionText}");

        int optionNumber = choiceIndex + 1;
        PlayerPrefs.SetInt($"Checkpoint_{checkpointID}_SelectedOptionNumber", optionNumber);
        PlayerPrefs.SetString($"Checkpoint_{checkpointID}_SelectedCategory", selectedCategory.ToString());
        PlayerPrefs.Save();
        
        confirmedDirectionText = optionText;

        if (checkpointManager != null)
        {
            checkpointManager.RecordChoiceInteraction(checkpointID, selectedCategory);
        }

        if (AdaptiveBackend.Instance != null)
        {
            AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "ChoiceInteraction", selectedCategory.ToString());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // START CHANGES: Handle exit logic
            HandlePlayerExit();
            // END CHANGES
        }
    }
    
    // Handle exit logic to record 'Not Listening'
    private void HandlePlayerExit()
    {
        playerIsClose = false;
        
        // If dialogue was opened and NOT completed, record partial listening with velocity ratio
        if (hasDialogOpened && !dialogueCompleted && DialogueController.Instance != null)
        {
             float totalFrames = DialogueController.Instance.dialogueTotalFrames;
             float stillFrames = DialogueController.Instance.dialogueStillFrames;

             float partialRatio = (totalFrames > 0) ? (stillFrames / totalFrames) * 0.5f : 0.0f;
             
             Debug.Log($"Checkpoint {checkpointID}: Player left early. Partial listen ratio: {partialRatio:F2}");
             if (AdaptiveBackend.Instance != null)
             {
                 AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "DialogueInteraction", partialRatio);
             }
             if (checkpointManager != null)
             {
                 checkpointManager.RecordDialogueInteraction(checkpointID, partialRatio);
             }

             hasDialogOpened = false;
        }

        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.CloseDialogue();
        }
    }


    
    


    /// <summary>
    /// Finds the player GameObject using tags or controllers.
    /// </summary>
    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                player = pc.gameObject;
            }
            else
            {
                PlayerController2 pc2 = FindFirstObjectByType<PlayerController2>();
                if (pc2 != null)
                {
                    player = pc2.gameObject;
                }
            }
        }
    }

    /// <summary>
    /// Checks if player is within detection radius (for distance-based detection).
    /// </summary>
    ///private bool hasDialogOpened = false; // Track if dialogue has already auto-opened

    private void CheckPlayerDistance()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        // Update close status
        if (distance <= detectionRadius)
        {
            playerIsClose = true;
            
            // Only log and activate if not already done
            if (!hasBeenActivated)
            {
                Debug.Log($"Checkpoint {checkpointID}: Player detected within range! Distance: {distance:F2}");
                ActivateCheckpoint();
            }

            // Check if we actually have dialogue to show
            bool hasDialogueToPlay = dialogue != null && dialogue.Length > 0;

            // Auto-open dialogue if not opened before
            if (hasDialogueToPlay && !hasDialogOpened)
            {
                hasDialogOpened = true; 
                dialogueCompleted = false; 

                Debug.Log($"Checkpoint {checkpointID}: Starting dialogue via DialogueController");
                if (DialogueController.Instance != null)
                {
                    DialogueController.Instance.StartDialogue(
                        dialogue, 
                        HandleOptionSelection, 
                        HandleDialogueComplete
                    );
                }
            }
            else if (!hasDialogueToPlay && !hasDialogOpened)
            {
                // If there's no dialogue, mark it as completed so we don't block other features
                hasDialogOpened = true;
                dialogueCompleted = true;
            }

            // ONLY Start Choice Bubble UI if dialogue has FULLY completed (or if there was no dialogue)
            if (choiceInteraction != null && dialogueCompleted)
            {
                choiceInteraction.SetPlayerInRange(true);
            }
        }
        else
        {
            // Player moved away
            if (playerIsClose) // State change: Close -> Far
            {
                if (choiceInteraction != null)
                {
                    choiceInteraction.SetPlayerInRange(false);
                }

                // START CHANGES: Use HandlePlayerExit
                HandlePlayerExit();
                // END CHANGES
            }
        }
    }
    
    /// <summary>
    /// Called when player enters the checkpoint trigger zone (only used if not using distance detection).
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || useDistanceDetection) return;
        
        // Check if the colliding object is the player
        if (IsPlayer(other.gameObject))
        {
            Debug.Log($"Checkpoint {checkpointID}: Player entered trigger zone! Player position: ({other.transform.position.x:F2}, {other.transform.position.y:F2}, {other.transform.position.z:F2})");
            ActivateCheckpoint();
            playerIsClose = true;
            if (DialogueController.Instance != null) DialogueController.Instance.CloseDialogue();
        }
    }
    
    /// <summary>
    /// Checks if the GameObject is the player.
    /// </summary>
    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || 
               obj.GetComponent<PlayerController>() != null ||
               obj.GetComponent<PlayerController2>() != null;
    }
    
    /// <summary>
    /// Activates this checkpoint and registers it with CheckpointManager.
    /// </summary>
    private void ActivateCheckpoint()
    {
        if (hasBeenActivated && !allowMultipleActivations)
        {
            Debug.Log($"Checkpoint {checkpointID}: Already activated, skipping (allowMultipleActivations = false)");
            return; // Already activated, don't activate again
        }
        
        if (checkpointManager == null)
        {
            Debug.LogError($"Checkpoint {checkpointID}: Cannot activate - CheckpointManager is null!");
            return;
        }
        
        // Find player if not cached
        if (player == null)
        {
            FindPlayer();
        }
        
        // Register checkpoint position (use player's position when they pass through)
        Vector3 checkpointPosition;
        if (player != null)
        {
            checkpointPosition = player.transform.position;
            Debug.Log($"=== CHECKPOINT ENCOUNTERED ===");
            Debug.Log($"Checkpoint ID: {checkpointID}");
            Debug.Log($"Checkpoint GameObject Position: ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2})");
            Debug.Log($"Player Position (stored as checkpoint): ({checkpointPosition.x:F2}, {checkpointPosition.y:F2}, {checkpointPosition.z:F2})");
        }
        else
        {
            // Fallback to checkpoint's own position
            checkpointPosition = transform.position;
            Debug.LogWarning($"Checkpoint {checkpointID}: Player not found, using checkpoint's own position.");
            Debug.Log($"=== CHECKPOINT ENCOUNTERED ===");
            Debug.Log($"Checkpoint ID: {checkpointID}");
            Debug.Log($"Checkpoint Position (fallback): ({checkpointPosition.x:F2}, {checkpointPosition.y:F2}, {checkpointPosition.z:F2})");
        }
        
        // Register with CheckpointManager
        checkpointManager.RegisterCheckpoint(checkpointPosition);
        hasBeenActivated = true;
        
        // Notify manager of our ID for robust end-level evaluation
        checkpointManager.NotifyCheckpointReached(checkpointID);

        // WAIT FOR NPC AND DELAY IF FINAL CHECKPOINT
        if (isFinalCheckpoint)
        {
            Debug.Log($"<color=green>Final Checkpoint {checkpointID} Reached! Waiting for NPC before teleporting to {nextSceneName}...</color>");
            StartCoroutine(WaitForNPCAndTransition());
            return; // Exit out of the rest of the activation logic so no empty dialogues pop up!
        }
        
        // Trigger specific checkpoint event based on checkpoint ID for dialogue system
        GameEventType checkpointEvent = GetCheckpointEventType(checkpointID);
        if (checkpointEvent != GameEventType.BombEncountered) // Using BombEncountered as "invalid" since we don't have a "None"
        {
            GameEventManager.TriggerEvent(checkpointEvent, gameObject);
            Debug.Log($"Checkpoint {checkpointID}: Triggered {checkpointEvent} event for dialogue system.");
        }
        else
        {
            Debug.LogWarning($"Checkpoint {checkpointID}: No matching event type found! Please add Checkpoint{checkpointID}Reached to GameEventType enum.");
        }
        
        // Update visual
        UpdateVisualState();
        
        // Play sound
        PlayCheckpointSound();
        
        Debug.Log($"Checkpoint {checkpointID}: Successfully activated and registered!");
        Debug.Log($"=================================");
    }
    
    /// <summary>
    /// Updates the visual appearance of the checkpoint.
    /// </summary>
    private void UpdateVisualState()
    {
        if (checkpointVisual == null) return;
        
        if (hasBeenActivated)
        {
            if (activeSprite != null)
            {
                checkpointVisual.sprite = activeSprite;
            }
            checkpointVisual.color = activeColor;
        }
        else
        {
            if (inactiveSprite != null)
            {
                checkpointVisual.sprite = inactiveSprite;
            }
            checkpointVisual.color = inactiveColor;
        }
    }
    
    /// <summary>
    /// Plays the checkpoint activation sound.
    /// </summary>
    private void PlayCheckpointSound()
    {
        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }
    }
    
    /// <summary>
    /// Manually activate this checkpoint (useful for testing or special cases).
    /// </summary>
    public void Activate()
    {
        ActivateCheckpoint();
    }
    
    /// <summary>
    /// Enable or disable this checkpoint.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            hasBeenActivated = false;
            UpdateVisualState();
        }
    }
    
    /// <summary>
    /// Gets the corresponding GameEventType for this checkpoint based on its ID.
    /// </summary>
    private GameEventType GetCheckpointEventType(int id)
    {
        switch (id)
        {
            case 0: return GameEventType.Checkpoint0Reached;
            case 1: return GameEventType.Checkpoint1Reached;
            case 2: return GameEventType.Checkpoint2Reached;
            case 3: return GameEventType.Checkpoint3Reached;
            case 4: return GameEventType.Checkpoint4Reached;
            case 5: return GameEventType.Checkpoint5Reached;
            default:
                Debug.LogWarning($"Checkpoint ID {id} does not have a corresponding GameEventType. Add Checkpoint{id}Reached to the enum.");
                return GameEventType.BombEncountered; // Return a default (not ideal, but prevents errors)
        }
    }

    private IEnumerator WaitForNPCAndTransition()
    {
        // Try to find the NPC in the scene
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        if (npc == null)
        {
            NPCBehaviour npcBehavior = FindFirstObjectByType<NPCBehaviour>();
            if (npcBehavior != null) npc = npcBehavior.gameObject;
            else
            {
                DSmovementScript dsMovement = FindFirstObjectByType<DSmovementScript>();
                if (dsMovement != null) npc = dsMovement.gameObject;
            }
        }

        if (npc != null)
        {
            // Wait until the NPC gets close enough to the checkpoint
            while (Vector2.Distance(transform.position, npc.transform.position) > 4f)
            {
                yield return null; // Wait until next frame
            }
            Debug.Log("<color=green>NPC reached the final checkpoint!</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>NPC not found! Proceeding without waiting for NPC.</color>");
        }

        // Wait 3 seconds
        Debug.Log("<color=green>Both reached final checkpoint. Waiting 3 seconds before transition...</color>");
        yield return new WaitForSeconds(3f);

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
