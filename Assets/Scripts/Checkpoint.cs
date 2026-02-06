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
    
    // Public getter for checkpoint ID (used by dialogue system)
    public int CheckpointID => checkpointID;
    
    [Header("Detection Settings")]
    [SerializeField] private bool useDistanceDetection = true; // Use distance-based detection instead of collider
    [SerializeField] private float detectionRadius = 1.5f; // Distance to detect player (if using distance detection)
    
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
    private CheckpointManager checkpointManager;
    private GameObject player; // Cache player reference

    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public string[] dialogue;
    private int index;

    public GameObject contButton;
    public float wordSpeed = 0.02f;
    public bool playerIsClose;
    
    void Start()
    {
        wordSpeed = 0.05f; // Ensure fast typing speed

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
        
        // Initialize visual state
        UpdateVisualState();

        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        // Ensure the continue button has the event listener attached
        if (contButton != null)
        {
            Button btn = contButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(NextLine);
            }
            else
            {
                Debug.LogWarning($"Checkpoint {checkpointID}: 'contButton' assigned but no Button component found!");
            }
        }
        
        Debug.Log($"Checkpoint {checkpointID}: Initialized at position ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2}). Detection: {(useDistanceDetection ? "Distance-based" : "Collider-based")}");
        if (dialogue != null)
        {
            Debug.Log($"Checkpoint {checkpointID} Dialogue Content: {string.Join(" | ", dialogue)}");
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

        // Allow advancing text with E key if dialogue is already active
        if (dialogPanel.activeInHierarchy && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogText.maxVisibleCharacters < dialogText.textInfo.characterCount)
            {
                // Instant finish typing
                dialogText.maxVisibleCharacters = dialogText.textInfo.characterCount;
            }
            else
            {
                NextLine();
            }
        }

        if (dialogPanel.activeInHierarchy && dialogText.textInfo != null && dialogText.maxVisibleCharacters >= dialogText.textInfo.characterCount)
        {
            if(contButton != null) contButton.SetActive(true);
        }
    }


    public void zeroText()
    {
        dialogText.text = "";
        index = 0;
        dialogText.maxVisibleCharacters = 0;
        dialogPanel.SetActive(false);
    }

    IEnumerator Typing()
    {
        dialogText.text = dialogue[index];
        dialogText.maxVisibleCharacters = 0;
        dialogText.ForceMeshUpdate(); // Ensure textInfo is updated
        
        // Wait one frame to ensure UI is ready
        yield return null;

        int totalVisibleCharacters = dialogText.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            dialogText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine()
    {
        contButton.SetActive(false);

        if (index < dialogue.Length - 1)
        {
            StartCoroutine(Typing());
            index++;
        }
        else
        {
            zeroText();
        }
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerIsClose = true;
    //     }
    // }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            zeroText();
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
    private bool hasDialogOpened = false; // Track if dialogue has already auto-opened

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

            // Auto-open dialogue if not active, NOT opened before, and not currently shown
            if (!dialogPanel.activeInHierarchy && !hasDialogOpened)
            {
                hasDialogOpened = true; // Mark as opened so it doesn't auto-pop again
                dialogPanel.SetActive(true);
                
                // Show first line with typing effect
                index = 0;
                if (dialogue != null && dialogue.Length > 0)
                {
                    Debug.Log($"Checkpoint {checkpointID}: Starting dialogue typing for: '{dialogue[index]}'");
                    StartCoroutine(Typing());
                }
                else
                {
                    Debug.LogWarning($"Checkpoint {checkpointID}: Dialogue empty or null!");
                }
            }
        }
        else
        {
            // Player moved away
            if (playerIsClose) // State change: Close -> Far
            {
                playerIsClose = false;
                if (dialogPanel.activeInHierarchy)
                {
                    zeroText();
                }
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
}

