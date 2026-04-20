using UnityEngine;

using System.Collections;
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
    
    [Header("Last Checkpoint Evaluation")]
    [SerializeField] private int totalCheckpointsInLevel = 5; // Total number of checkpoints in the level
    [SerializeField] private string nextSceneName = "Level2"; // Scene to load after the last checkpoint
    
    [Header("Empathy Meter")]
    [Tooltip("Drag the EmpathyMeter UI component here. If null, meter won't update but everything else works.")]
    [SerializeField] private EmpathyMeter empathyMeter;

    private HealthSystem playerHealthSystem;
    private HeartManager playerHeartManager; // Support for HeartManager as well
    private int previousHealth; // Track previous health to detect 2-life loss
    private int initialMaxHealth; // Track initial max health to calculate total lives lost
    private Transform playerTransform;
    private Rigidbody2D playerRigidbody;
    private StopwatchTimer gameTimer; // Reference to timer
    private bool lastCheckpointEvaluated = false; // Prevent multiple evaluations
    
    // Dialogue interaction tracking: maps checkpointID -> listening ratio (0.0 to 1.0)
    private Dictionary<int, float> dialogueResults = new Dictionary<int, float>();
    
    // Choice interaction tracking: maps checkpointID -> ChoiceCategory
    private Dictionary<int, ChoiceCategory> choiceResults = new Dictionary<int, ChoiceCategory>();
    
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

        // Find StopwatchTimer
        gameTimer = FindFirstObjectByType<StopwatchTimer>();
        if (gameTimer == null) Debug.LogWarning("CheckpointManager: StopwatchTimer not found! Timestamps will be 0.");

        // Auto-find EmpathyMeter if not assigned — or create one automatically
        if (empathyMeter == null)
        {
            empathyMeter = FindFirstObjectByType<EmpathyMeter>();
        }
        if (empathyMeter == null)
        {
            Debug.Log("<color=cyan>[CheckpointManager]</color> No EmpathyMeter found in scene — auto-creating one...");
            GameObject meterObj = new GameObject("EmpathyMeter_AutoCreated");
            empathyMeter = meterObj.AddComponent<EmpathyMeter>();
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
        
        // Report to Agent
        float time = gameTimer != null ? gameTimer.GetTime() : 0f;
        AdaptiveBackend.Instance.ReceiveData("CheckpointManager", $"CheckpointReached_{mostRecentCheckpointIndex}", time);
        
        // Print all checkpoint coordinates
        Debug.Log($"All Checkpoint Coordinates:");
        for (int i = 0; i < checkpointPositions.Length; i++)
        {
            Vector3 cp = checkpointPositions[i];
            string marker = (i == mostRecentCheckpointIndex) ? " <-- MOST RECENT" : "";
            Debug.Log($"  [{i}] ({cp.x:F2}, {cp.y:F2}, {cp.z:F2}){marker}");
        }
        Debug.Log($"=============================");
        
        // Old array length check logic left here just in case,
        // but robust completion is now handled by NotifyCheckpointReached based on ID
        if (checkpointPositions.Length >= totalCheckpointsInLevel && !lastCheckpointEvaluated)
        {
            Debug.Log($"<color=green>=== LAST CHECKPOINT REACHED (Array full) ===</color>");
            EvaluatePlayerAtLastCheckpoint();
            lastCheckpointEvaluated = true;
        }
    }
    
    /// <summary>
    /// Notifies the manager which checkpoint ID was just reached.
    /// Used as a robust method to determine if the level end was reached.
    /// </summary>
    public void NotifyCheckpointReached(int checkpointID)
    {
        // For Example: total=5 means IDs 0,1,2,3,4. 
        if (checkpointID >= totalCheckpointsInLevel - 1 && !lastCheckpointEvaluated)
        {
            Debug.Log($"<color=green>=== LAST CHECKPOINT REACHED (ID: {checkpointID}) ===</color>");
            EvaluatePlayerAtLastCheckpoint();
            lastCheckpointEvaluated = true;
        }
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
    /// Teleports a specific GameObject to the most recent checkpoint.
    /// Used for resetting both the player and the NPC based on shared checkpoints.
    /// </summary>
    public void RespawnAtCheckpoint(GameObject targetObj)
    {
        if (targetObj == null) 
        {
            RespawnAtCheckpoint(); // Default to player
            return;
        }

        Vector3 respawnPosition = GetMostRecentCheckpointPosition();
        targetObj.transform.position = respawnPosition;
        
        Rigidbody2D rb = targetObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        Debug.Log($"[CheckpointManager] Respawned {targetObj.name} at {respawnPosition}");
    }
    
    /// <summary>
    /// Tracks NPC checkpoints. Called by NPCCheckpointManager.
    /// </summary>
    public void RegisterNPCCheckpoint(Vector3 npcPos, Vector3 checkpointPos)
    {
        Debug.Log($"[CheckpointManager] NPC Reached Checkpoint at {checkpointPos}");
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
    
    // ========================================================================
    // METRIC TRACKING — Dialogue (Metric 1: Listening)
    // ========================================================================

    /// <summary>
    /// Records dialogue interaction result for a checkpoint.
    /// Called by Checkpoint script when dialogue is completed or player leaves early.
    /// 
    /// The listenRatio is a continuous value from 0.0 to 1.0:
    ///   1.0 = player was perfectly still during entire dialogue
    ///   0.0 = player was moving the entire time / walked away immediately
    /// 
    /// Only the first result per checkpoint is recorded (prevents duplicates).
    /// </summary>
    public void RecordDialogueInteraction(int checkpointID, float listenRatio)
    {
        if (!dialogueResults.ContainsKey(checkpointID))
        {
            dialogueResults[checkpointID] = Mathf.Clamp01(listenRatio);
            string status = listenRatio >= 0.5f ? "Listened" : "Not Listened";
            Debug.Log($"<color=yellow>[Dialogue Metric]</color> Checkpoint {checkpointID}: {status} (ratio: {listenRatio:F2})");
            
            // Update empathy meter
            RecalculateAndPushMetrics();
        }
        else
        {
            Debug.Log($"<color=yellow>[Dialogue Metric]</color> Checkpoint {checkpointID}: Already recorded, skipping.");
        }
    }

    /// <summary>
    /// Backwards-compatible overload: converts bool to float ratio.
    /// true = 1.0 (listened), false = 0.0 (didn't listen)
    /// </summary>
    public void RecordDialogueInteraction(int checkpointID, bool listened)
    {
        RecordDialogueInteraction(checkpointID, listened ? 1.0f : 0.0f);
    }

    // ========================================================================
    // METRIC TRACKING — Choices (Metric 2: Clarification, Metric 3: Reinforcement)
    // ========================================================================

    /// <summary>
    /// Records a choice interaction result for a checkpoint.
    /// Called by CheckpointInteraction when the player makes (or doesn't make) a choice.
    /// Only the first result per checkpoint is recorded.
    /// </summary>
    public void RecordChoiceInteraction(int checkpointID, ChoiceCategory category)
    {
        if (!choiceResults.ContainsKey(checkpointID))
        {
            choiceResults[checkpointID] = category;
            Debug.Log($"<color=yellow>[Choice Metric]</color> Checkpoint {checkpointID}: {category}");

            // Update empathy meter
            RecalculateAndPushMetrics();
        }
        else
        {
            Debug.Log($"<color=yellow>[Choice Metric]</color> Checkpoint {checkpointID}: Already recorded, skipping.");
        }
    }

    // ========================================================================
    // METRIC CALCULATION — Push to Empathy Meter
    // ========================================================================

    /// <summary>
    /// Recalculates all three metric scores from tracked data and pushes them to the EmpathyMeter.
    /// Called after each new interaction record.
    /// </summary>
    private void RecalculateAndPushMetrics()
    {
        // ─── Metric 1: Listening Rate ───
        float listeningRate = 0.5f; // Default neutral
        if (dialogueResults.Count > 0)
        {
            float totalListenRatio = 0f;
            foreach (var kvp in dialogueResults)
            {
                totalListenRatio += kvp.Value;
            }
            listeningRate = totalListenRatio / dialogueResults.Count;
        }

        // ─── Metric 2: Visual Clarification Score ───
        // HighEmpathy choices increase score, LowEmpathy choices decrease it
        float clarificationScore = 0.5f; // Default neutral
        int clarificationRelevantCount = 0;
        int highEmpathyCount = 0;
        foreach (var kvp in choiceResults)
        {
            if (kvp.Value == ChoiceCategory.HighEmpathy || kvp.Value == ChoiceCategory.LowEmpathy)
            {
                clarificationRelevantCount++;
                if (kvp.Value == ChoiceCategory.HighEmpathy)
                {
                    highEmpathyCount++;
                }
            }
        }
        if (clarificationRelevantCount > 0)
        {
            clarificationScore = (float)highEmpathyCount / clarificationRelevantCount;
        }

        // ─── Metric 3: Positive Reinforcement Score ───
        // PositiveReinforcement increases score, NegativeReinforcement decreases it
        float reinforcementScore = 0.5f; // Default neutral
        int reinforcementRelevantCount = 0;
        int positiveCount = 0;
        foreach (var kvp in choiceResults)
        {
            if (kvp.Value == ChoiceCategory.PositiveReinforcement || kvp.Value == ChoiceCategory.NegativeReinforcement)
            {
                reinforcementRelevantCount++;
                if (kvp.Value == ChoiceCategory.PositiveReinforcement)
                {
                    positiveCount++;
                }
            }
        }
        if (reinforcementRelevantCount > 0)
        {
            reinforcementScore = (float)positiveCount / reinforcementRelevantCount;
        }

        // ─── Push to Meter ───
        if (empathyMeter != null)
        {
            empathyMeter.UpdateListeningScore(listeningRate);
            empathyMeter.UpdateClarificationScore(clarificationScore);
            empathyMeter.UpdateReinforcementScore(reinforcementScore);
        }

        // Report to AdaptiveBackend
        if (AdaptiveBackend.Instance != null)
        {
            AdaptiveBackend.Instance.ReceiveData("EmpathyMetrics", "ListeningRate", listeningRate);
            AdaptiveBackend.Instance.ReceiveData("EmpathyMetrics", "ClarificationScore", clarificationScore);
            AdaptiveBackend.Instance.ReceiveData("EmpathyMetrics", "ReinforcementScore", reinforcementScore);
        }
    }

    // ========================================================================
    // EVALUATION — End of Level
    // ========================================================================

    /// <summary>
    /// Evaluates player performance at the last checkpoint using AI backend.
    /// If dissimilarity > 50%, gives confident answer. Otherwise not confident.
    /// Ends the game after evaluation.
    /// </summary>
    private void EvaluatePlayerAtLastCheckpoint()
    {
        Debug.Log($"<color=cyan>=== EVALUATING PLAYER PERFORMANCE ===</color>");
        
        // Print all empathy metrics
        PrintAllMetrics();
        
        // Get backend instance and trigger evaluation
        AdaptiveBackend backend = AdaptiveBackend.Instance;
        
        // Trigger evaluation (calculates from tracked checkpoint/life data)
        backend.EvaluatePlayer();
        
        // Get similarity value (0.0 to 1.0)
        float similarity = backend.CurrentDSSimilarity;
        
        // Calculate dissimilarity as percentage (0-100)
        float dissimilarity = (1f - similarity) * 100f;
        
        Debug.Log($"<color=yellow>[Checkpoint Evaluation]</color> Similarity to DS: {similarity:F2} ({similarity * 100f:F1}%)");
        Debug.Log($"<color=yellow>[Checkpoint Evaluation]</color> Dissimilarity Value: {dissimilarity:F1}%");
        
        // Simple threshold: > 50% dissimilarity = confident, <= 50% = not confident
        if (dissimilarity > 50f)
        {
            // Confident answer
            Debug.Log($"<color=green>╔═══════════════════════════════════════╗</color>");
            Debug.Log($"<color=green>║     CONFIDENT ANSWER!                ║</color>");
            Debug.Log($"<color=green>║     \"ya ve dii iit\"                  ║</color>");
            Debug.Log($"<color=green>╚═══════════════════════════════════════╝</color>");
        }
        else
        {
            // Not confident answer
            Debug.Log($"<color=red>╔═══════════════════════════════════════╗</color>");
            Debug.Log($"<color=red>║     NOT CONFIDENT ANSWER             ║</color>");
            Debug.Log($"<color=red>║     \"n-no i lost\"                    ║</color>");
            Debug.Log($"<color=red>╚═══════════════════════════════════════╝</color>");
        }
        
        Debug.Log($"<color=cyan>===========================================</color>");
        
        // End the game
        EndGame();
    }
    
    /// <summary>
    /// Prints ALL empathy metrics to the console: listening, clarification, reinforcement, and composite.
    /// </summary>
    private void PrintAllMetrics()
    {
        Debug.Log($"<color=magenta>╔═══════════════════════════════════════════════╗</color>");
        Debug.Log($"<color=magenta>║       EMPATHY METRICS — LEVEL SUMMARY        ║</color>");
        Debug.Log($"<color=magenta>╠═══════════════════════════════════════════════╣</color>");
        
        // ── Metric 1: Listening ──
        Debug.Log($"<color=magenta>║</color>  <color=white>─── METRIC 1: LISTENING ───</color>");
        float totalListenRatio = 0f;
        int listenedCount = 0;
        int totalDialogue = dialogueResults.Count;
        
        for (int i = 0; i < totalCheckpointsInLevel; i++)
        {
            if (dialogueResults.ContainsKey(i))
            {
                float ratio = dialogueResults[i];
                totalListenRatio += ratio;
                if (ratio >= 0.5f) listenedCount++;
                string status = ratio >= 0.5f ? "<color=green>LISTENED</color>" : "<color=red>NOT LISTENED</color>";
                Debug.Log($"<color=magenta>║</color>  Checkpoint {i}: {status} (ratio: {ratio:F2})");
            }
            else
            {
                Debug.Log($"<color=magenta>║</color>  Checkpoint {i}: <color=grey>NO DATA</color>");
            }
        }
        float listeningRate = (totalDialogue > 0) ? totalListenRatio / totalDialogue : 0f;
        Debug.Log($"<color=magenta>║</color>  <color=white>LISTENING RATE: {listeningRate:P0}</color>");
        
        // ── Metric 2: Visual Clarification ──
        Debug.Log($"<color=magenta>║</color>");
        Debug.Log($"<color=magenta>║</color>  <color=white>─── METRIC 2: VISUAL CLARIFICATION ───</color>");
        int highEmpathy = 0, lowEmpathy = 0;
        foreach (var kvp in choiceResults)
        {
            if (kvp.Value == ChoiceCategory.HighEmpathy) highEmpathy++;
            else if (kvp.Value == ChoiceCategory.LowEmpathy) lowEmpathy++;
        }
        int totalClarification = highEmpathy + lowEmpathy;
        float clarificationRate = totalClarification > 0 ? (float)highEmpathy / totalClarification : 0f;
        Debug.Log($"<color=magenta>║</color>  High Empathy choices: {highEmpathy}");
        Debug.Log($"<color=magenta>║</color>  Low Empathy choices: {lowEmpathy}");
        Debug.Log($"<color=magenta>║</color>  <color=white>CLARIFICATION RATE: {clarificationRate:P0}</color>");
        
        // ── Metric 3: Positive Reinforcement ──
        Debug.Log($"<color=magenta>║</color>");
        Debug.Log($"<color=magenta>║</color>  <color=white>─── METRIC 3: POSITIVE REINFORCEMENT ───</color>");
        int positive = 0, negative = 0;
        foreach (var kvp in choiceResults)
        {
            if (kvp.Value == ChoiceCategory.PositiveReinforcement) positive++;
            else if (kvp.Value == ChoiceCategory.NegativeReinforcement) negative++;
        }
        int totalReinforcement = positive + negative;
        float reinforcementRate = totalReinforcement > 0 ? (float)positive / totalReinforcement : 0f;
        Debug.Log($"<color=magenta>║</color>  Positive choices: {positive}");
        Debug.Log($"<color=magenta>║</color>  Negative choices: {negative}");
        Debug.Log($"<color=magenta>║</color>  <color=white>REINFORCEMENT RATE: {reinforcementRate:P0}</color>");
        
        // ── Composite ──
        Debug.Log($"<color=magenta>╠═══════════════════════════════════════════════╣</color>");
        if (empathyMeter != null)
        {
            Debug.Log($"<color=magenta>║</color>  <color=white>COMPOSITE EMPATHY SCORE: {empathyMeter.CompositeScore:P0}</color>");
            Debug.Log($"<color=magenta>║</color>  <color=white>TIER: {empathyMeter.GetQualitativeTier()}</color>");
        }
        else
        {
            float composite = listeningRate * 0.4f + clarificationRate * 0.3f + reinforcementRate * 0.3f;
            Debug.Log($"<color=magenta>║</color>  <color=white>COMPOSITE EMPATHY SCORE: {composite:P0}</color>");
        }
        Debug.Log($"<color=magenta>╚═══════════════════════════════════════════════╝</color>");

        // ── All choice results ──
        Debug.Log($"<color=magenta>║</color>  <color=white>ALL CHOICES:</color>");
        foreach (var kvp in choiceResults)
        {
            Debug.Log($"<color=magenta>║</color>    Checkpoint {kvp.Key}: {kvp.Value}");
        }
    }

    /// <summary>
    /// Ends the game after last checkpoint evaluation.
    /// </summary>
    private void EndGame()
    {
        Debug.Log($"<color=magenta>╔═══════════════════════════════════════╗</color>");
        Debug.Log($"<color=magenta>║          GAME ENDED!                 ║</color>");
        Debug.Log($"<color=magenta>║   Last Checkpoint Reached            ║</color>");
        Debug.Log($"<color=magenta>╚═══════════════════════════════════════╝</color>");
        
        // Stop game timer if it exists
        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }
        
        // Ensure time is flowing normally before transition
        Time.timeScale = 1f;
        
        // Teleport to the next level
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"<color=green>Teleporting to next level: {nextSceneName}</color>");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}

