using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private Vector3[] checkpointPositions = new Vector3[0]; // Array to store checkpoint positions (similar to hearts array)
    [SerializeField] private int mostRecentCheckpointIndex = -1; // -1 means no checkpoint has been passed yet
    
    [Header("Player Reference")]
    [SerializeField] private GameObject player; // Player GameObject (auto-found if not assigned)
    
    [Header("Respawn Settings")]
    [SerializeField] private bool resetHealthOnRespawn = false; // Whether to reset health to max on respawn
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero; // Fallback position if no checkpoint exists
    
    private HealthSystem playerHealthSystem;
    private HeartManager playerHeartManager; // Support for HeartManager as well
    private int previousHealth; // Track previous health to detect 2-life loss
    private int initialMaxHealth; // Track initial max health to calculate total lives lost
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;
    
    // Singleton pattern for easy access
    private static CheckpointManager instance;
    public static CheckpointManager Instance => instance;
    
    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple CheckpointManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>()?.gameObject;
            }
            if (player == null)
            {
                Debug.LogError("CheckpointManager: Player not found! Please assign player GameObject or ensure it has 'Player' tag.");
                return;
            }
        }
        
        // Get player components
        playerTransform = player.GetComponent<Transform>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        
        if (playerTransform == null)
        {
            Debug.LogError("CheckpointManager: Player Transform not found!");
            return;
        }
        
        // Find and subscribe to HealthSystem or HeartManager
        playerHealthSystem = player.GetComponent<HealthSystem>();
        playerHeartManager = FindFirstObjectByType<HeartManager>(); // HeartManager might be on a different GameObject
        
        if (playerHealthSystem != null)
        {
            previousHealth = playerHealthSystem.CurrentHealth;
            initialMaxHealth = playerHealthSystem.MaxHealth;
            playerHealthSystem.OnHealthChanged += OnHealthChanged;
            Debug.Log($"CheckpointManager: Subscribed to HealthSystem events. Initial Health: {previousHealth}/{initialMaxHealth}");
        }
        else if (playerHeartManager != null)
        {
            previousHealth = playerHeartManager.currentHealth;
            initialMaxHealth = playerHeartManager.maxHealth;
            playerHeartManager.OnHealthChanged += OnHealthChanged;
            Debug.Log($"CheckpointManager: Subscribed to HeartManager events. Initial Health: {previousHealth}/{initialMaxHealth}");
        }
        else
        {
            Debug.LogWarning("CheckpointManager: Neither HealthSystem nor HeartManager found! Checkpoint respawn on health loss will not work.");
            Debug.LogWarning("CheckpointManager: Please ensure player has HealthSystem component OR HeartManager exists in scene.");
        }
        
        // Initialize checkpoint positions list if empty
        if (checkpointPositions == null || checkpointPositions.Length == 0)
        {
            checkpointPositions = new Vector3[0];
            Debug.Log("CheckpointManager: Initialized empty checkpoint positions array.");
        }
        
        // Set default spawn position to player's starting position if not set
        if (defaultSpawnPosition == Vector3.zero && playerTransform != null)
        {
            defaultSpawnPosition = playerTransform.position;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnHealthChanged -= OnHealthChanged;
        }
        if (playerHeartManager != null)
        {
            playerHeartManager.OnHealthChanged -= OnHealthChanged;
        }
    }
    
    /// <summary>
    /// Called when player's health changes. Detects when 2 lives are lost (multiples of 2).
    /// </summary>
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        // Calculate total lives lost from max health
        int totalLivesLost = maxHealth - currentHealth;
        
        // Also check if health dropped by 2 or more in this single event
        int healthLostThisEvent = previousHealth - currentHealth;
        
        Debug.Log($"CheckpointManager: Health changed! Current: {currentHealth}/{maxHealth}, Previous: {previousHealth}, Total Lost: {totalLivesLost}, Lost This Event: {healthLostThisEvent}");
        
        // Only trigger respawn when total lives lost is a multiple of 2 (2, 4, 6, etc.)
        // OR if they lost 2+ lives in a single damage event
        bool isMultipleOfTwo = (totalLivesLost > 0 && totalLivesLost % 2 == 0);
        bool lostTwoOrMoreThisEvent = (healthLostThisEvent >= 2);
        
        if (isMultipleOfTwo || lostTwoOrMoreThisEvent)
        {
            Debug.Log($"=== CHECKPOINT RESPAWN TRIGGERED ===");
            Debug.Log($"Total Lives Lost: {totalLivesLost} (Multiple of 2: {isMultipleOfTwo})");
            Debug.Log($"Lives Lost This Event: {healthLostThisEvent}");
            Debug.Log($"Current Health: {currentHealth}/{maxHealth}");
            RespawnAtCheckpoint();
        }
        else
        {
            Debug.Log($"CheckpointManager: Lives lost ({totalLivesLost}) is not a multiple of 2. No respawn.");
        }
        
        // Update previous health for next check
        previousHealth = currentHealth;
    }
    
    /// <summary>
    /// Registers a new checkpoint position. Called by Checkpoint script when player passes through.
    /// </summary>
    public void RegisterCheckpoint(Vector3 position)
    {
        // Add position to array (similar to how hearts array works)
        int oldLength = checkpointPositions != null ? checkpointPositions.Length : 0;
        System.Array.Resize(ref checkpointPositions, oldLength + 1);
        mostRecentCheckpointIndex = checkpointPositions.Length - 1;
        checkpointPositions[mostRecentCheckpointIndex] = position;
        
        Debug.Log($"=== CHECKPOINT REGISTERED ===");
        Debug.Log($"Checkpoint Position: ({position.x:F2}, {position.y:F2}, {position.z:F2})");
        Debug.Log($"Total Checkpoints: {checkpointPositions.Length}");
        Debug.Log($"Most Recent Checkpoint Index: {mostRecentCheckpointIndex}");
        
        // Print all checkpoint coordinates
        Debug.Log($"All Checkpoint Coordinates:");
        for (int i = 0; i < checkpointPositions.Length; i++)
        {
            Vector3 cp = checkpointPositions[i];
            string marker = (i == mostRecentCheckpointIndex) ? " <-- MOST RECENT" : "";
            Debug.Log($"  [{i}] ({cp.x:F2}, {cp.y:F2}, {cp.z:F2}){marker}");
        }
        Debug.Log($"=============================");
    }
    
    /// <summary>
    /// Teleports player to the most recent checkpoint position.
    /// </summary>
    public void RespawnAtCheckpoint()
    {
        Debug.Log($"=== RESPAWN AT CHECKPOINT ===");
        
        if (playerTransform == null)
        {
            Debug.LogError("CheckpointManager: Cannot respawn - player Transform is null!");
            return;
        }
        
        Vector3 respawnPosition;
        
        // Check if we have a valid checkpoint
        if (mostRecentCheckpointIndex >= 0 && mostRecentCheckpointIndex < checkpointPositions.Length)
        {
            respawnPosition = checkpointPositions[mostRecentCheckpointIndex];
            Debug.Log($"CheckpointManager: Respawning at checkpoint index {mostRecentCheckpointIndex}");
            Debug.Log($"Respawn Position: ({respawnPosition.x:F2}, {respawnPosition.y:F2}, {respawnPosition.z:F2})");
            Debug.Log($"Current Player Position: ({playerTransform.position.x:F2}, {playerTransform.position.y:F2}, {playerTransform.position.z:F2})");
        }
        else
        {
            // No checkpoint yet, use default spawn position
            respawnPosition = defaultSpawnPosition;
            Debug.LogWarning($"CheckpointManager: No checkpoint available! mostRecentCheckpointIndex = {mostRecentCheckpointIndex}, checkpointPositions.Length = {checkpointPositions.Length}");
            Debug.LogWarning($"Respawning at default position: ({respawnPosition.x:F2}, {respawnPosition.y:F2}, {respawnPosition.z:F2})");
        }
        
        // Teleport player
        playerTransform.position = respawnPosition;
        Debug.Log($"Player teleported to: ({playerTransform.position.x:F2}, {playerTransform.position.y:F2}, {playerTransform.position.z:F2})");
        
        // Reset velocity if Rigidbody2D exists
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            Debug.Log("CheckpointManager: Reset player velocity.");
        }
        else
        {
            Debug.LogWarning("CheckpointManager: Player Rigidbody2D not found - velocity not reset.");
        }
        
        // Reset health if configured
        if (resetHealthOnRespawn)
        {
            if (playerHealthSystem != null)
            {
                playerHealthSystem.ResetHealth();
                previousHealth = playerHealthSystem.CurrentHealth;
                Debug.Log($"CheckpointManager: Reset player health to maximum: {previousHealth}");
            }
            else if (playerHeartManager != null)
            {
                playerHeartManager.currentHealth = playerHeartManager.maxHealth;
                previousHealth = playerHeartManager.currentHealth;
                playerHeartManager.UpdateHearts();
                Debug.Log($"CheckpointManager: Reset player health to maximum: {previousHealth}");
            }
        }
        
        Debug.Log($"=============================");
    }
    
    /// <summary>
    /// Gets the most recent checkpoint position. Returns default spawn if no checkpoint exists.
    /// </summary>
    public Vector3 GetMostRecentCheckpointPosition()
    {
        if (mostRecentCheckpointIndex >= 0 && mostRecentCheckpointIndex < checkpointPositions.Length)
        {
            return checkpointPositions[mostRecentCheckpointIndex];
        }
        return defaultSpawnPosition;
    }
    
    /// <summary>
    /// Gets the number of checkpoints that have been passed.
    /// </summary>
    public int GetCheckpointCount()
    {
        return checkpointPositions != null ? checkpointPositions.Length : 0;
    }
    
    /// <summary>
    /// Manually set the default spawn position (useful for setting starting position).
    /// </summary>
    public void SetDefaultSpawnPosition(Vector3 position)
    {
        defaultSpawnPosition = position;
        Debug.Log($"CheckpointManager: Default spawn position set to {position}");
    }
}

