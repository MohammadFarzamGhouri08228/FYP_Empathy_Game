using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Checkpoint2 : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActive = true; // Whether this checkpoint is active
    [SerializeField] private int checkpointID = 0; // Unique ID for this checkpoint
    [SerializeField] private bool allowMultipleActivations = false; // Whether checkpoint can be activated multiple times

    [Header("Level Transition")]
    [SerializeField] private bool isFinalCheckpoint = false; // Check this for the very last checkpoint!
    [SerializeField] private string nextSceneName = "Level2"; // Scene to load when reached
    
    // Public getter for checkpoint ID
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

    // ========================================================================
    // DIALOGUE SYSTEM — Now utilizes DialogueController
    // Fill in dialogue text and options via Unity Inspector.
    // ========================================================================
    
    [Header("Dialogue Content")]
    public DialogueLine[] dialogue;

    // Dialogue tracking state
    private bool dialogueCompleted = false;
    private bool hasDialogOpened = false;

    private CheckpointInteraction choiceInteraction;

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
            Debug.LogError($"Checkpoint2 {checkpointID}: CheckpointManager not found in scene! Please add CheckpointManager GameObject.");
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
                    Debug.LogWarning($"Checkpoint2 {checkpointID}: Collider is not set as trigger! Setting it now.");
                    col.isTrigger = true;
                }
            }
            else
            {
                Debug.LogWarning($"Checkpoint2 {checkpointID}: No Collider2D found! Please add a Collider2D component and set it as trigger, or enable 'Use Distance Detection'.");
            }
        }
        
        Debug.Log($"Checkpoint2 {checkpointID}: Initialized at position ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2}). Detection: {(useDistanceDetection ? "Distance-based" : "Collider-based")}");
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

            float listenRatio = (totalFrames > 0) ? stillFrames / totalFrames : 1.0f;
            
            Debug.Log($"Checkpoint2 {checkpointID}: Dialogue Finished. Listen ratio: {listenRatio:F2}");
            if (AdaptiveBackend.Instance != null)
            {
                AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "DialogueInteraction", listenRatio);
            }
            if (checkpointManager != null)
            {
                checkpointManager.RecordDialogueInteraction(checkpointID, listenRatio);
            }
        }
    }

    private void HandleOptionSelection(int choiceIndex, ChoiceCategory selectedCategory)
    {
        Debug.Log($"Checkpoint2 {checkpointID}: Option {choiceIndex + 1} chosen - Category: {selectedCategory}");

        int optionNumber = choiceIndex + 1;
        PlayerPrefs.SetInt($"Checkpoint_{checkpointID}_SelectedOptionNumber", optionNumber);
        PlayerPrefs.SetString($"Checkpoint_{checkpointID}_SelectedCategory", selectedCategory.ToString());
        PlayerPrefs.Save();

        if (checkpointManager != null)
        {
            checkpointManager.RecordChoiceInteraction(checkpointID, selectedCategory);
        }

        if (AdaptiveBackend.Instance != null)
        {
            AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "ChoiceInteraction", selectedCategory.ToString());
        }
    }

    private void HandlePlayerExit()
    {
        playerIsClose = false;
        
        if (hasDialogOpened && !dialogueCompleted && DialogueController.Instance != null)
        {
            float totalFrames = DialogueController.Instance.dialogueTotalFrames;
            float stillFrames = DialogueController.Instance.dialogueStillFrames;

            float partialRatio = (totalFrames > 0) ? (stillFrames / totalFrames) * 0.5f : 0.0f;
            
            Debug.Log($"Checkpoint2 {checkpointID}: Player left early. Partial listen ratio: {partialRatio:F2}");
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

    // ========================================================================
    // DETECTION & ACTIVATION
    // ========================================================================

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (choiceInteraction != null)
            {
                choiceInteraction.SetPlayerInRange(false);
            }
            HandlePlayerExit();
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

    private void CheckPlayerDistance()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        
        if (distance <= detectionRadius)
        {
            playerIsClose = true;
            
            if (!hasBeenActivated)
            {
                Debug.Log($"Checkpoint2 {checkpointID}: Player detected within range! Distance: {distance:F2}");
                ActivateCheckpoint();
            }

            // Check if we have dialogue to show
            bool hasDialogueToPlay = dialogue != null && dialogue.Length > 0;

            // Auto-open dialogue if not active, NOT opened before
            if (hasDialogueToPlay && !hasDialogOpened)
            {
                hasDialogOpened = true;
                dialogueCompleted = false;
                
                Debug.Log($"Checkpoint2 {checkpointID}: Starting dialogue via DialogueController");
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
                hasDialogOpened = true;
                dialogueCompleted = true;
            }

            // Show Choice Bubble ONLY after dialogue has completed (or if there was no dialogue)
            if (choiceInteraction != null && dialogueCompleted)
            {
                choiceInteraction.SetPlayerInRange(true);
            }
        }
        else
        {
            // Player moved away
            if (playerIsClose)
            {
                if (choiceInteraction != null)
                {
                    choiceInteraction.SetPlayerInRange(false);
                }

                HandlePlayerExit();
            }
        }
    }
    
    /// <summary>
    /// Called when player enters the checkpoint trigger zone (only used if not using distance detection).
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || useDistanceDetection) return;
        
        if (IsPlayer(other.gameObject))
        {
            Debug.Log($"Checkpoint2 {checkpointID}: Player entered trigger zone! Player position: ({other.transform.position.x:F2}, {other.transform.position.y:F2}, {other.transform.position.z:F2})");
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
            Debug.Log($"Checkpoint2 {checkpointID}: Already activated, skipping (allowMultipleActivations = false)");
            return;
        }
        
        if (checkpointManager == null)
        {
            Debug.LogError($"Checkpoint2 {checkpointID}: Cannot activate - CheckpointManager is null!");
            return;
        }
        
        if (player == null)
        {
            FindPlayer();
        }
        
        Vector3 checkpointPosition;
        if (player != null)
        {
            checkpointPosition = player.transform.position;
            Debug.Log($"=== CHECKPOINT ENCOUNTERED ===");
            Debug.Log($"Checkpoint2 ID: {checkpointID}");
            Debug.Log($"Checkpoint2 GameObject Position: ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2})");
            Debug.Log($"Player Position (stored as checkpoint): ({checkpointPosition.x:F2}, {checkpointPosition.y:F2}, {checkpointPosition.z:F2})");
        }
        else
        {
            checkpointPosition = transform.position;
            Debug.LogWarning($"Checkpoint2 {checkpointID}: Player not found, using checkpoint's own position.");
            Debug.Log($"=== CHECKPOINT ENCOUNTERED ===");
            Debug.Log($"Checkpoint2 ID: {checkpointID}");
            Debug.Log($"Checkpoint2 Position (fallback): ({checkpointPosition.x:F2}, {checkpointPosition.y:F2}, {checkpointPosition.z:F2})");
        }
        
        checkpointManager.RegisterCheckpoint(checkpointPosition);
        hasBeenActivated = true;
        
        checkpointManager.NotifyCheckpointReached(checkpointID);

        // IMMEDIATE TELEPORT IF FINAL CHECKPOINT
        if (isFinalCheckpoint)
        {
            Debug.Log($"<color=green>Final Checkpoint2 {checkpointID} Reached! Teleporting to {nextSceneName}...</color>");
            
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            return;
        }
        
        // Trigger checkpoint event for dialogue system
        GameEventType checkpointEvent = GetCheckpointEventType(checkpointID);
        if (checkpointEvent != GameEventType.BombEncountered)
        {
            GameEventManager.TriggerEvent(checkpointEvent, gameObject);
            Debug.Log($"Checkpoint2 {checkpointID}: Triggered {checkpointEvent} event for dialogue system.");
        }
        else
        {
            Debug.LogWarning($"Checkpoint2 {checkpointID}: No matching event type found! Please add Checkpoint{checkpointID}Reached to GameEventType enum.");
        }
        
        UpdateVisualState();
        PlayCheckpointSound();
        
        Debug.Log($"Checkpoint2 {checkpointID}: Successfully activated and registered!");
        Debug.Log($"=================================");
    }
    
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
    
    private void PlayCheckpointSound()
    {
        if (audioSource != null && checkpointSound != null)
        {
            audioSource.PlayOneShot(checkpointSound);
        }
    }
    
    public void Activate()
    {
        ActivateCheckpoint();
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            hasBeenActivated = false;
            UpdateVisualState();
        }
    }
    
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
                return GameEventType.BombEncountered;
        }
    }
}
