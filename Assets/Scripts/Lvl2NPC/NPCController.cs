/*
    NPCController.cs  –  NPC brain / command handler
    ─────────────────────────────────────────────────
    Listens for Key 1 and Key 2 presses and drives the NPC through
    walk-to-checkpoint behaviour sequences using coroutines.

    Key 1 (Success):  Walk right → reach checkpoint → idle (checkpoint becomes new home).
    Key 2 (Failure):  Walk right → reach checkpoint → fallen → teleport back to home → idle.

    Checkpoint detection and the concluding stop/idle step are handled
    by NPCCheckpointManager. This script only orchestrates the sequences.

    Attach to the NPC GameObject alongside NPCMotor, NPCCheckpointManager,
    and NPCSpriteAnimator.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════
//  State enum (shared with NPCSpriteAnimator)
// ═══════════════════════════════════════════

public enum NPCState
{
    Idle,
    Walking,
    Fallen
}

[RequireComponent(typeof(NPCMotor))]
[RequireComponent(typeof(NPCCheckpointManager))]
public class NPCController : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Walk Direction")]
    [Tooltip("Direction the NPC walks. Default is right. Change for future levels.")]
    [SerializeField] private Vector2 walkDirection = Vector2.right;

    [Header("Fallen State")]
    [Tooltip("How long the fallen sprite is shown before teleporting back")]
    [SerializeField] private float fallenDisplayTime = 2f;

    // ═══════════════════════════════════════════
    //  Public Read-Only State
    // ═══════════════════════════════════════════

    /// <summary>Current NPC behaviour state.</summary>
    public NPCState CurrentState { get; private set; } = NPCState.Idle;

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private NPCMotor motor;
    private NPCCheckpointManager checkpointMgr;

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<NPCMotor>();
        checkpointMgr = GetComponent<NPCCheckpointManager>();
    }

    void Update()
    {
        // Only accept commands while idle
        if (CurrentState != NPCState.Idle) return;

        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            StartCoroutine(SuccessPath());
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            StartCoroutine(FailurePath());
        }
    }

    // ═══════════════════════════════════════════
    //  Public API
    // ═══════════════════════════════════════════

    /// <summary>
    /// Set the NPC state. Called by NPCCheckpointManager when a checkpoint
    /// is reached (concluding stop → idle) and internally by coroutines.
    /// </summary>
    public void SetState(NPCState newState)
    {
        CurrentState = newState;
    }

    // ═══════════════════════════════════════════
    //  Behaviour Sequences
    // ═══════════════════════════════════════════

    /// <summary>
    /// Key 1 -- Success path.
    /// Walk right → NPCCheckpointManager stops at checkpoint → idle.
    /// The reached checkpoint becomes the new home.
    /// </summary>
    private IEnumerator SuccessPath()
    {
        // 1. Clear previous flag and start walking
        checkpointMgr.ConsumeCheckpoint();
        CurrentState = NPCState.Walking;
        motor.Walk(walkDirection);

        // 2. Wait until NPCCheckpointManager detects a checkpoint
        //    (it also stops the NPC and sets state to Idle)
        yield return new WaitUntil(() => checkpointMgr.ReachedCheckpoint);

        // 3. Update home to the new checkpoint position
        checkpointMgr.SetHome(checkpointMgr.LastCheckpointPosition);

        // State is already Idle (set by NPCCheckpointManager)
    }

    /// <summary>
    /// Key 2 -- Failure path.
    /// Walk right → NPCCheckpointManager stops at checkpoint → idle →
    /// show fallen sprite → teleport back to home → idle.
    /// </summary>
    private IEnumerator FailurePath()
    {
        // 1. Clear previous flag and start walking
        checkpointMgr.ConsumeCheckpoint();
        CurrentState = NPCState.Walking;
        motor.Walk(walkDirection);

        // 2. Wait until NPCCheckpointManager detects a checkpoint
        //    (it also stops the NPC and sets state to Idle)
        yield return new WaitUntil(() => checkpointMgr.ReachedCheckpoint);

        // 3. Switch to fallen state (overrides the idle set by manager)
        CurrentState = NPCState.Fallen;

        // 4. Hold the fallen sprite for a moment
        yield return new WaitForSeconds(fallenDisplayTime);

        // 5. Teleport back to the previous home (do NOT update home)
        motor.TeleportTo(checkpointMgr.HomePosition);

        // 6. Return to idle
        CurrentState = NPCState.Idle;
    }
}
