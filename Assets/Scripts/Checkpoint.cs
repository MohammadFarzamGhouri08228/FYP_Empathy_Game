using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActive = true; // Whether this checkpoint is active
    [SerializeField] private int checkpointID = 0; // Optional ID for debugging/identification
    [SerializeField] private bool allowMultipleActivations = false; // Whether checkpoint can be activated multiple times
    
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
    
    void Start()
    {
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
        
        Debug.Log($"Checkpoint {checkpointID}: Initialized at position ({transform.position.x:F2}, {transform.position.y:F2}, {transform.position.z:F2}). Detection: {(useDistanceDetection ? "Distance-based" : "Collider-based")}");
    }
    
    void Update()
    {
        // Use distance-based detection if enabled
        if (useDistanceDetection && isActive && !hasBeenActivated)
        {
            CheckPlayerDistance();
        }
    }
    
    /// <summary>
    /// Finds the player GameObject.
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
            Debug.Log($"Checkpoint {checkpointID}: Player detected within range! Distance: {distance:F2}, Position: ({player.transform.position.x:F2}, {player.transform.position.y:F2}, {player.transform.position.z:F2})");
            ActivateCheckpoint();
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
}

