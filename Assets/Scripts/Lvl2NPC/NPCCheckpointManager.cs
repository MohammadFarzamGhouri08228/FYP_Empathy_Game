/*
    NPCCheckpointManager.cs  –  NPC checkpoint tracking
    ────────────────────────────────────────────────────
    Detects when the NPC reaches a Checkpoint and tracks
    the NPC's home position and checkpoint history.

    Uses DISTANCE-BASED detection (like Checkpoint.cs does for the player)
    so it works regardless of whether checkpoints have colliders.
    Also supports trigger/collision detection as a fallback.

    Whenever the NPC reaches a checkpoint it is always
    stopped and transitioned to idle as a concluding step.

    Also reports the NPC's position to the central
    CheckpointManager so it can be tracked alongside
    the player's checkpoint data.

    Attach to the NPC GameObject alongside NPCMotor and NPCController.
*/

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMotor))]
public class NPCCheckpointManager : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Detection")]
    [Tooltip("How close the NPC must be to a checkpoint to trigger it (2D distance, ignores Z)")]
    [SerializeField] private float detectionRadius = 1.5f;

    // ═══════════════════════════════════════════
    //  Public Read-Only State
    // ═══════════════════════════════════════════

    /// <summary>True for one sequence after the NPC touches a checkpoint.
    /// Reset by calling ConsumeCheckpoint().</summary>
    public bool ReachedCheckpoint { get; private set; }

    /// <summary>World position of the most recently reached checkpoint.</summary>
    public Vector3 LastCheckpointPosition { get; private set; }

    /// <summary>The NPC's "safe" / home position (spawn or last success checkpoint).</summary>
    public Vector3 HomePosition { get; private set; }

    /// <summary>Total number of checkpoints the NPC has visited.</summary>
    public int CheckpointsReached { get; private set; }

    /// <summary>Ordered history of all checkpoint positions visited.</summary>
    public IReadOnlyList<Vector3> CheckpointHistory => checkpointHistory;

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private NPCMotor motor;
    private NPCController controller;
    private List<Vector3> checkpointHistory = new List<Vector3>();

    // Cache all checkpoints in the scene once at start
    private Checkpoint[] allCheckpoints;
    // Track which checkpoints the NPC has already visited (by instance ID)
    private HashSet<int> visitedCheckpointIDs = new HashSet<int>();

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<NPCMotor>();
        controller = GetComponent<NPCController>();
    }

    void Start()
    {
        // NPC's spawn position is the initial home
        HomePosition = transform.position;

        // Cache all checkpoints in the scene
        allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        Debug.Log($"[NPCCheckpointManager] Found {allCheckpoints.Length} checkpoints in scene.");
    }

    void Update()
    {
        // Only check distance while walking
        if (controller == null || controller.CurrentState != NPCState.Walking)
            return;

        // Already reached a checkpoint this sequence, don't detect another
        if (ReachedCheckpoint)
            return;

        CheckDistanceToCheckpoints();
    }

    // ═══════════════════════════════════════════
    //  Distance-Based Detection (primary)
    // ═══════════════════════════════════════════

    private void CheckDistanceToCheckpoints()
    {
        if (allCheckpoints == null) return;

        Vector2 npcPos2D = new Vector2(transform.position.x, transform.position.y);

        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp == null) continue;

            // Skip checkpoints we already visited
            int cpID = cp.GetInstanceID();
            if (visitedCheckpointIDs.Contains(cpID))
                continue;

            Vector2 cpPos2D = new Vector2(cp.transform.position.x, cp.transform.position.y);
            float distance = Vector2.Distance(npcPos2D, cpPos2D);

            if (distance <= detectionRadius)
            {
                OnCheckpointReached(cp);
                return; // Only one checkpoint per frame
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Trigger / Collision Detection (fallback)
    // ═══════════════════════════════════════════

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDetectCheckpointFromCollider(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDetectCheckpointFromCollider(collision.gameObject);
    }

    private void TryDetectCheckpointFromCollider(GameObject obj)
    {
        if (controller == null || controller.CurrentState != NPCState.Walking)
            return;
        if (ReachedCheckpoint)
            return;

        Checkpoint cp = obj.GetComponent<Checkpoint>();
        if (cp == null) cp = obj.GetComponentInParent<Checkpoint>();
        if (cp == null) cp = obj.GetComponentInChildren<Checkpoint>();

        if (cp == null) return;

        // Skip already visited
        if (visitedCheckpointIDs.Contains(cp.GetInstanceID()))
            return;

        OnCheckpointReached(cp);
    }

    // ═══════════════════════════════════════════
    //  Shared Checkpoint Logic
    // ═══════════════════════════════════════════

    private void OnCheckpointReached(Checkpoint cp)
    {
        Vector3 checkpointPos = cp.transform.position;
        Vector3 npcPos = transform.position;

        // Mark as visited so we don't trigger on it again
        visitedCheckpointIDs.Add(cp.GetInstanceID());

        LastCheckpointPosition = checkpointPos;
        CheckpointsReached++;
        checkpointHistory.Add(checkpointPos);
        ReachedCheckpoint = true;

        // ── Concluding step: always stop and go idle ──
        motor.Stop();
        controller.SetState(NPCState.Idle);

        // ── Report to central CheckpointManager ──
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterNPCCheckpoint(npcPos, checkpointPos);
        }

        Debug.Log($"[NPCCheckpointManager] Reached checkpoint {CheckpointsReached} (ID:{cp.CheckpointID}) at {checkpointPos}, NPC was at {npcPos}");
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Called by NPCController
    // ═══════════════════════════════════════════

    /// <summary>Clear the reached flag so the next walk sequence can begin.</summary>
    public void ConsumeCheckpoint()
    {
        ReachedCheckpoint = false;
    }

    /// <summary>Update the home position (called after a success path).</summary>
    public void SetHome(Vector3 position)
    {
        HomePosition = position;
    }

    /// <summary>Get the previous home position (one checkpoint back), or spawn if none.</summary>
    public Vector3 GetPreviousHome()
    {
        if (checkpointHistory.Count >= 2)
            return checkpointHistory[checkpointHistory.Count - 2];

        return HomePosition;
    }

    // ═══════════════════════════════════════════
    //  Editor Gizmos
    // ═══════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
