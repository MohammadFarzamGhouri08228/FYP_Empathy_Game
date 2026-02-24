using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCBehaviour : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private Vector2 jumpForce = new Vector2(10f, 15f);
    [SerializeField] private float hesitationTime = 1.5f;

    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Failure Settings")]
    [Tooltip("Weaker jump force used when the NPC fails — not enough to clear the gap")]
    [SerializeField] private Vector2 failJumpForce = new Vector2(3f, 5f);
    [Tooltip("Y position below which the NPC is considered fallen off the map")]
    [SerializeField] private float fallDeathY = -10f;

    [Header("Autonomous Mode — Learning")]
    [Tooltip("Starting probability of failure (high = NPC is unskilled at first)")]
    [SerializeField] [Range(0f, 1f)] private float initialFailProbability = 0.4f;
    [Tooltip("How much the fail probability decreases per attempt (NPC learns each try)")]
    [SerializeField] [Range(0f, 1f)] private float learningRate = 0.08f;
    [Tooltip("Minimum fail probability floor (NPC can never be perfect)")]
    [SerializeField] [Range(0f, 1f)] private float minFailProbability = 0.05f;
    [Tooltip("Seconds the NPC waits before auto-deciding after a checkpoint event")]
    [SerializeField] private float autoDecisionDelay = 1.5f;

    [Header("Debug — Respawn & Replay")]
    [Tooltip("Press this key to teleport the NPC back to its start position and replay all jumps to catch up to the player")]
    [SerializeField] private UnityEngine.InputSystem.Key debugRespawnKey = UnityEngine.InputSystem.Key.R;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool routineRunning = false;

    /// <summary>
    /// How many platform jumps the NPC has successfully completed.
    /// Jump 0 = the CP1 jump, jump 1 = the CP2 jump, etc.
    /// </summary>
    private int npcCompletedJumps = 0;

    /// <summary>
    /// Current fail probability — starts at initialFailProbability and decays with each attempt.
    /// </summary>
    private float currentFailProbability;

    /// <summary>
    /// Total jump attempts made (used for learning rate display).
    /// </summary>
    private int totalAttempts = 0;

    // =========================================================================
    //  JUMP CONFIGURATION TABLE
    //  Index 0 = jump triggered at CP1 (2nd checkpoint)
    //  Index 1 = jump triggered at CP2 (3rd checkpoint)
    //  Index 2+ = default parameters for later checkpoints
    // =========================================================================

    private struct JumpConfig
    {
        public float hesitation;
        public float walkDistance;
        public Vector2 successForce;
    }

    private JumpConfig GetJumpConfig(int jumpIndex)
    {
        switch (jumpIndex)
        {
            case 0: // CP1 — first jump
                return new JumpConfig { hesitation = 0.5f, walkDistance = moveDistance, successForce = jumpForce };
            case 1: // CP2 — second jump
                return new JumpConfig { hesitation = 1f, walkDistance = 4f, successForce = new Vector2(6f, 9f) };
            default: // CP3+ — default
                return new JumpConfig { hesitation = hesitationTime, walkDistance = moveDistance, successForce = jumpForce };
        }
    }

    // =========================================================================
    //  UNITY LIFECYCLE
    // =========================================================================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        currentFailProbability = initialFailProbability;
        Debug.Log($"NPCBehaviour: Initialized. Fail probability starts at {currentFailProbability:P0}. (ID: {gameObject.GetInstanceID()})");

        if (rb == null)
        {
            Debug.LogError("NPCBehaviour: Rigidbody2D IS MISSING!");
        }
        else
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning($"NPCBehaviour: Rigidbody2D was {rb.bodyType}, changing to Dynamic!");
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            if ((rb.constraints & RigidbodyConstraints2D.FreezePosition) != 0)
            {
                Debug.LogWarning("NPCBehaviour: Unfreezing Position constraints!");
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            Debug.Log($"NPCBehaviour Physics: Mass={rb.mass}, Gravity={rb.gravityScale}, Drag={rb.linearDamping}, Type={rb.bodyType}");
        }
    }

    void OnEnable()
    {
        GameEventManager.OnGameEvent += HandleGameEvent;
    }

    void OnDisable()
    {
        GameEventManager.OnGameEvent -= HandleGameEvent;
    }

    // =========================================================================
    //  UPDATE — Debug respawn key only
    // =========================================================================
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[(UnityEngine.InputSystem.Key)debugRespawnKey].wasPressedThisFrame)
        {
            Debug.Log($"<color=orange>[NPCBehaviour] Debug key '{debugRespawnKey}' pressed — respawning at start and replaying.</color>");
            StopAllCoroutines();
            routineRunning = false;
            npcCompletedJumps = 0;
            currentFailProbability = initialFailProbability;
            totalAttempts = 0;
            StartCoroutine(DebugReplayFromStart());
        }
    }

    // =========================================================================
    //  EVENT HANDLER — kicks off the jump loop if not already running
    // =========================================================================
    private void HandleGameEvent(GameEventType eventType, GameObject source)
    {
        Debug.Log($"NPCBehaviour: Received event {eventType}");

        switch (eventType)
        {
            // CP0: same platform, do nothing
            case GameEventType.Checkpoint0Reached:
                Debug.Log("NPCBehaviour: CP0 — same platform, NPC does nothing.");
                break;

            // CP1 onward: start the jump loop if not already running
            case GameEventType.Checkpoint1Reached:
            case GameEventType.Checkpoint2Reached:
            case GameEventType.Checkpoint3Reached:
            case GameEventType.Checkpoint4Reached:
            case GameEventType.Checkpoint5Reached:
                if (!routineRunning)
                {
                    Debug.Log($"NPCBehaviour: {eventType} — starting jump loop.");
                    StartCoroutine(JumpLoop());
                }
                else
                {
                    Debug.Log($"NPCBehaviour: {eventType} received but jump loop already running. It will catch up automatically.");
                }
                break;
        }
    }

    // =========================================================================
    //  JUMP LOOP — the core engine
    //  Keeps attempting jumps until the NPC has caught up to the player.
    //  After a failure + respawn, it loops back and retries.
    //  After a success, it checks if there's another jump to do.
    // =========================================================================
    private IEnumerator JumpLoop()
    {
        routineRunning = true;

        while (true)
        {
            // How many checkpoints has the player reached?
            int playerCPs = 0;
            if (CheckpointManager.Instance != null)
                playerCPs = CheckpointManager.Instance.GetCheckpointCount();

            // Jumps available = playerCPs - 1 (CP0 requires no jump)
            int jumpsAvailable = Mathf.Max(0, playerCPs - 1);

            Debug.Log($"<color=cyan>[NPCBehaviour] JumpLoop check: playerCPs={playerCPs}, " +
                      $"jumpsAvailable={jumpsAvailable}, npcCompletedJumps={npcCompletedJumps}</color>");

            if (npcCompletedJumps >= jumpsAvailable)
            {
                Debug.Log($"<color=green>[NPCBehaviour] NPC caught up! Completed {npcCompletedJumps}/{jumpsAvailable} jumps. " +
                          $"Waiting for next checkpoint event.</color>");
                break;
            }

            JumpConfig cfg = GetJumpConfig(npcCompletedJumps);

            // ── Inner retry loop: keep retrying THIS jump until the NPC succeeds ──
            bool jumpSucceeded = false;
            bool isRetry = false; // first attempt walks to edge; retries jump in place

            while (!jumpSucceeded)
            {
                // On retry for jump 0 (CP1): NPC spawned ON the checkpoint near the edge, so skip the walk.
                // For jump 1+ (CP2 onward): checkpoint is further from the edge, so always walk.
                float walkDist = (isRetry && npcCompletedJumps == 0) ? 0f : cfg.walkDistance;
                string attemptType = (isRetry && npcCompletedJumps == 0) ? "RETRY (jump in place)" : isRetry ? "RETRY (walk + jump)" : "FIRST (walk + jump)";

                Debug.Log($"<color=yellow>[NPCBehaviour] {attemptType} — jump {npcCompletedJumps + 1}/{jumpsAvailable}</color>");

                yield return new WaitForSeconds(autoDecisionDelay);

                float roll = Random.value;
                bool willFail = roll < currentFailProbability;

                Debug.Log($"<color=yellow>[NPCBehaviour] Decision: {(willFail ? "FAILURE" : "SUCCESS")} " +
                          $"(roll: {roll:F2} vs threshold: {currentFailProbability:F2}, attempt #{totalAttempts + 1})</color>");

                // NPC learns from each attempt — reduce fail probability
                totalAttempts++;
                currentFailProbability = Mathf.Max(minFailProbability, currentFailProbability - learningRate);
                Debug.Log($"<color=yellow>[NPCBehaviour] Learning! New fail probability: {currentFailProbability:P0}</color>");

                if (willFail)
                {
                    yield return StartCoroutine(FailureJumpRoutine(cfg.hesitation, walkDist));
                    Debug.Log("<color=cyan>[NPCBehaviour] Respawned after failure. Will retry in place...</color>");
                    yield return new WaitForSeconds(0.5f);
                    isRetry = true; // next attempt skips the walk
                }
                else
                {
                    yield return StartCoroutine(SuccessJumpRoutine(cfg.successForce, cfg.hesitation, walkDist));
                    npcCompletedJumps++;
                    jumpSucceeded = true;
                    Debug.Log($"<color=green>[NPCBehaviour] Jump {npcCompletedJumps} complete!</color>");
                    yield return new WaitForSeconds(0.5f);
                }
            }
            // ── Outer loop continues to check if there's another jump to do ──
        }

        routineRunning = false;
    }

    // =========================================================================
    //  SUCCESS PATH — walk forward, proper jump
    // =========================================================================
    private IEnumerator SuccessJumpRoutine(Vector2 force, float hesitation, float walkDist)
    {
        Debug.Log($"NPCBehaviour: [Success] Hesitating {hesitation}s");
        yield return new WaitForSeconds(hesitation);

        Debug.Log($"NPCBehaviour: [Success] Walking {walkDist} units right...");
        yield return StartCoroutine(WalkRight(walkDist));

        yield return new WaitForSeconds(0.2f);

        Debug.Log($"NPCBehaviour: [Success] Jumping with force {force}");
        Jump(force);
    }

    // =========================================================================
    //  FAILURE PATH — walk forward, weak jump, fall, respawn at player CP
    // =========================================================================
    private IEnumerator FailureJumpRoutine(float hesitation, float walkDist)
    {
        Debug.Log($"NPCBehaviour: [Failure] Hesitating {hesitation}s");
        yield return new WaitForSeconds(hesitation);

        Debug.Log($"NPCBehaviour: [Failure] Walking {walkDist} units right...");
        yield return StartCoroutine(WalkRight(walkDist));

        yield return new WaitForSeconds(0.2f);

        Debug.Log($"NPCBehaviour: [Failure] Weak jump with force {failJumpForce}");
        Jump(failJumpForce);

        // Wait for fall: either we drop below fallDeathY, or something else
        // (e.g. DeathLayer) respawns the NPC first.  Use a timeout so the
        // coroutine can never hang indefinitely.
        float fallTimer = 0f;
        float fallTimeout = 5f;
        bool fellOffMap = false;

        while (fallTimer < fallTimeout)
        {
            if (transform.position.y < fallDeathY)
            {
                fellOffMap = true;
                break;
            }
            fallTimer += Time.deltaTime;
            yield return null;
        }

        if (fellOffMap)
        {
            Debug.Log($"NPCBehaviour: [Failure] Fell below Y={fallDeathY}. Respawning ourselves...");
            RespawnAtPlayerCheckpoint();
        }
        else
        {
            // Something else already respawned the NPC (e.g. DeathLayer), or timeout.
            // Just make sure orientation and velocity are clean.
            Debug.Log("NPCBehaviour: [Failure] NPC was respawned externally or timed out. Cleaning up...");
            transform.rotation = startRotation;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // =========================================================================
    //  DEBUG — REPLAY FROM START
    // =========================================================================
    private IEnumerator DebugReplayFromStart()
    {
        routineRunning = true;

        // Teleport to start, reset everything
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        npcCompletedJumps = 0;
        Debug.Log($"<color=cyan>[NPCBehaviour] Respawned at start: {startPosition}</color>");

        yield return new WaitForSeconds(0.5f);

        routineRunning = false;

        // Kick off the normal jump loop — it will catch up to the player
        StartCoroutine(JumpLoop());
    }

    // =========================================================================
    //  SHARED HELPERS
    // =========================================================================

    private IEnumerator WalkRight(float distance)
    {
        float startX = transform.position.x;
        float targetX = startX + distance;

        while (transform.position.x < targetX)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        Debug.Log("NPCBehaviour: Walk complete.");
    }

    [ContextMenu("Force Jump Now")]
    public void Jump(Vector2? overrideForce = null)
    {
        if (rb != null)
        {
            Vector2 forceToUse = overrideForce ?? jumpForce;
            Debug.Log($"NPCBehaviour: Applying Jump Force {forceToUse} (Mass: {rb.mass})");
            rb.AddForce(forceToUse, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("NPCBehaviour: Rigidbody2D is null! Cannot jump.");
        }
    }

    private void RespawnAtPlayerCheckpoint()
    {
        Vector3 respawnPos;

        if (CheckpointManager.Instance != null)
        {
            respawnPos = CheckpointManager.Instance.GetMostRecentCheckpointPosition();
            Debug.Log($"<color=cyan>[NPCBehaviour] Respawning at player's checkpoint: {respawnPos}</color>");
        }
        else
        {
            respawnPos = transform.position;
            Debug.LogWarning("[NPCBehaviour] CheckpointManager not found! Cannot respawn.");
        }

        transform.position = respawnPos;
        transform.rotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        Debug.Log($"<color=cyan>[NPCBehaviour] Respawn complete. Velocity & rotation reset.</color>");
    }
}