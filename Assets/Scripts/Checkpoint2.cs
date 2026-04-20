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
    // DIALOGUE SYSTEM — Same infrastructure as Checkpoint.cs (Level 1)
    // Fill in dialogue text via Unity Inspector for each checkpoint.
    // ========================================================================
    
    [Header("Dialogue UI")]
    [Tooltip("The dialogue panel GameObject (drag from Canvas). Leave null if this checkpoint has no dialogue.")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    [TextArea(2, 5)]
    public string[] dialogue;
    private int index;

    public GameObject portraitImage;
    public GameObject nameTitle;

    public GameObject contButton;
    public float wordSpeed = 0.03f;
    
    [Header("Text to Speech")]
    public ElevenLabsTTS ttsSystem;
    public bool readDialogueAloud = true;

    // Dialogue tracking state
    private bool dialogueCompleted = false;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private bool hasDialogOpened = false;

    // Velocity-based listening metric
    private float dialogueStillFrames = 0f;
    private float dialogueTotalFrames = 0f;
    private Rigidbody2D playerRb;
    private const float STILL_VELOCITY_THRESHOLD = 0.15f;

    private CheckpointInteraction choiceInteraction;

    void Start()
    {
        choiceInteraction = GetComponent<CheckpointInteraction>();

        // Automatically find the TTS system if it's not assigned
        if (ttsSystem == null)
        {
            ttsSystem = FindFirstObjectByType<ElevenLabsTTS>();
        }

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
        
        // Cache player Rigidbody2D for velocity tracking
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }

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
        
        // Initialize visual state
        UpdateVisualState();

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // Warn if dialogue panel is assigned but no dialogue text
        if (dialogPanel != null && (dialogue == null || dialogue.Length == 0))
        {
            Debug.LogWarning($"Checkpoint2 {checkpointID}: dialogPanel is assigned but dialogue array is empty! Fill in dialogue text via Inspector.");
        }
        
        if (contButton == null && dialogPanel != null)
        {
            Debug.LogWarning($"Checkpoint2 {checkpointID}: 'contButton' not assigned!");
        }
        
        Debug.Log($"Checkpoint2 {checkpointID}: Initialized at position ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2}). Detection: {(useDistanceDetection ? "Distance-based" : "Collider-based")}. Dialogue: {(dialogue != null && dialogue.Length > 0 ? $"{dialogue.Length} lines" : "none")}");
    }
    
    void Update()
    {
        // Use distance-based detection if enabled
        if (useDistanceDetection && isActive)
        {
            CheckPlayerDistance();
        }

        // Only process dialogue input if THIS checkpoint owns the active dialogue
        if (isDialogueActive && dialogPanel != null && dialogPanel.activeInHierarchy)
        {
            // Velocity tracking for listening metric (during NPC lines)
            if (isTyping && index < dialogue.Length && !IsPlayerDialogue(dialogue[index]))
            {
                dialogueTotalFrames++;
                if (playerRb != null && playerRb.linearVelocity.magnitude < STILL_VELOCITY_THRESHOLD)
                {
                    dialogueStillFrames++;
                }
                else if (playerRb == null)
                {
                    dialogueStillFrames++; // No rigidbody — assume still
                }
            }

            // Allow advancing text with E key
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (index < dialogue.Length && !IsPlayerDialogue(dialogue[index]))
                {
                    if (dialogText.maxVisibleCharacters < dialogText.textInfo.characterCount)
                    {
                        dialogText.maxVisibleCharacters = dialogText.textInfo.characterCount;
                    }
                    else
                    {
                        NextLine();
                    }
                }
            }
        }
    }

    // ========================================================================
    // DIALOGUE METHODS (same pattern as Checkpoint.cs)
    // ========================================================================

    public void zeroText()
    {
        if (dialogText != null) dialogText.text = "";
        index = 0;
        if (dialogText != null) dialogText.maxVisibleCharacters = 0;
        if (dialogPanel != null) dialogPanel.SetActive(false);
        isDialogueActive = false;
        
        if (readDialogueAloud && ttsSystem != null)
        {
            ttsSystem.StopSpeaking();
        }
    }

    IEnumerator Typing()
    {
        isTyping = true;
        
        if (contButton != null) contButton.SetActive(false);
        
        bool showUI = ShouldShowUI(index);
        if (portraitImage != null && nameTitle != null)
        {
            portraitImage.SetActive(showUI);
            nameTitle.SetActive(showUI);
        }

        // Play TTS for this line
        if (readDialogueAloud && ttsSystem != null && !string.IsNullOrWhiteSpace(dialogue[index]))
        {
            string spokenText = dialogue[index];
            if (spokenText.StartsWith("Musa:"))
            {
                spokenText = spokenText.Substring(5).Trim();
            }
            ttsSystem.Speak(spokenText);
        }

        dialogText.text = dialogue[index];
        dialogText.maxVisibleCharacters = 0;
        dialogText.ForceMeshUpdate();
        
        yield return null;

        int totalVisibleCharacters = dialogText.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            dialogText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(wordSpeed);
        }
        
        isTyping = false;
        
        if (IsPlayerDialogue(dialogue[index]))
        {
            yield return new WaitForSeconds(1.2f);
            NextLine();
        }
        else
        {
            if (contButton != null) contButton.SetActive(true);
        }
    }

    public void NextLine()
    {
        if (isTyping) return;
        
        if (contButton != null) contButton.SetActive(false);

        if (!playerIsClose)
        {
            zeroText();
            return;
        }

        if (index < dialogue.Length - 1)
        {
            index++;
            StartCoroutine(Typing());
        }
        else
        {
            // Dialogue finished — record listening metric
            if (!dialogueCompleted)
            {
                dialogueCompleted = true;
                
                float listenRatio = (dialogueTotalFrames > 0)
                    ? dialogueStillFrames / dialogueTotalFrames
                    : 1.0f;
                
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
            zeroText();
        }
    }

    private void HandlePlayerExit()
    {
        playerIsClose = false;
        
        // Record partial listening if dialogue was active but not completed
        if (dialogPanel != null && (dialogPanel.activeInHierarchy || isDialogueActive) && !dialogueCompleted)
        {
            float partialRatio = (dialogueTotalFrames > 0)
                ? (dialogueStillFrames / dialogueTotalFrames) * 0.5f
                : 0.0f;
            
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
            dialogueStillFrames = 0f;
            dialogueTotalFrames = 0f;
        }

        // Always close dialogue when player exits
        if (dialogPanel != null && (dialogPanel.activeInHierarchy || isDialogueActive))
        {
            StopAllCoroutines();
            zeroText();
        }
    }

    private bool ShouldShowUI(int dialogueIndex)
    {
        if (dialogue == null || dialogueIndex < 0 || dialogueIndex >= dialogue.Length) return false;
        return !IsPlayerDialogue(dialogue[dialogueIndex]);
    }

    private bool IsPlayerDialogue(string line)
    {
        if (string.IsNullOrEmpty(line)) return false;
        return line.TrimStart().StartsWith("Musa:");
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
            bool hasDialogueToPlay = dialogPanel != null && dialogue != null && dialogue.Length > 0;

            // Auto-open dialogue if not active, NOT opened before, and not currently shown
            if (hasDialogueToPlay && !dialogPanel.activeInHierarchy && !hasDialogOpened)
            {
                hasDialogOpened = true;
                dialogPanel.SetActive(true);
                isDialogueActive = true;
                dialogueCompleted = false;
                dialogueStillFrames = 0f;
                dialogueTotalFrames = 0f;
                
                // Assign button listener for THIS checkpoint instance
                if (contButton != null)
                {
                    Button btn = contButton.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(NextLine);
                    }
                }

                // Show first line with typing effect
                index = 0;
                Debug.Log($"Checkpoint2 {checkpointID}: Starting dialogue typing for: '{dialogue[index]}'");
                StartCoroutine(Typing());
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
            zeroText();
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
