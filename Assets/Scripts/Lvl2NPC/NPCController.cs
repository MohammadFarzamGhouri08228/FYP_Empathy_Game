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
    Fallen,
    Climbing
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
    private NPCLadderClimber ladderClimber;   // optional – null if not attached

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<NPCMotor>();
        checkpointMgr = GetComponent<NPCCheckpointManager>();
        ladderClimber = GetComponent<NPCLadderClimber>();
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
    /// Walk right → (climb any ladders along the way) →
    /// NPCCheckpointManager stops at checkpoint → idle.
    /// The reached checkpoint becomes the new home.
    /// </summary>
    private IEnumerator SuccessPath()
    {
        checkpointMgr.ConsumeCheckpoint();
        yield return StartCoroutine(WalkToNextCheckpoint());
        checkpointMgr.SetHome(checkpointMgr.LastCheckpointPosition);
        // State is already Idle (set by NPCCheckpointManager)
    }

    /// <summary>
    /// Key 2 -- Failure path.
    /// Walk right → (climb any ladders along the way) →
    /// NPCCheckpointManager stops at checkpoint → fallen →
    /// teleport back to home → idle.
    /// </summary>
    private IEnumerator FailurePath()
    {
        checkpointMgr.ConsumeCheckpoint();
        yield return StartCoroutine(WalkToNextCheckpoint());

        // Switch to fallen state (overrides the idle set by manager)
        CurrentState = NPCState.Fallen;
        yield return new WaitForSeconds(fallenDisplayTime);

        // Teleport back to the previous home (do NOT update home)
        motor.TeleportTo(checkpointMgr.HomePosition);
        CurrentState = NPCState.Idle;
    }

    // ═══════════════════════════════════════════
    //  Walk + Ladder Helpers
    // ═══════════════════════════════════════════

    /// <summary>
    /// Walk in walkDirection until the NPC reaches a checkpoint.
    /// If a ladder is encountered mid-walk, auto-climb it,
    /// hop off at the top, wait for landing, then resume walking.
    /// </summary>
    private IEnumerator WalkToNextCheckpoint()
    {
        CurrentState = NPCState.Walking;
        motor.Walk(walkDirection);

        while (true)
        {
            yield return null;

            // ── Checkpoint reached → stop ──
            if (checkpointMgr.ReachedCheckpoint)
                break;

            // ── Ladder encountered mid-walk → climb it ──
            if (ladderClimber != null && ladderClimber.IsAtLadder)
            {
                yield return StartCoroutine(HandleLadderClimb());

                // Resume walking after dismounting
                CurrentState = NPCState.Walking;
                motor.Walk(walkDirection);
            }
        }
    }

    /// <summary>
    /// Handle one complete ladder interaction:
    /// auto-climb → reach top → hop off → wait for landing.
    /// No player input needed; the NPC climbs by itself once
    /// it encounters a ladder during its walk sequence.
    /// </summary>
    private IEnumerator HandleLadderClimb()
    {
        // ── 1. Climb ──
        CurrentState = NPCState.Climbing;
        ladderClimber.BeginClimb();
        Debug.Log("[NPCController] NPC auto-climbing ladder.");

        // ── 2. Wait until the NPC reaches the exit height and hops off ──
        yield return new WaitUntil(() => ladderClimber.FinishedClimbing);

        // ── 3. Wait for the hop arc to play out and the NPC to land ──
        //       Short delay first so the NPC lifts off the ladder collider
        yield return new WaitForSeconds(0.15f);
        yield return new WaitUntil(() => motor.IsGrounded);

        // ── 4. Clean up ──
        ladderClimber.ConsumeClimb();
        Debug.Log("[NPCController] Ladder climb + hop complete. Resuming walk.");
    }
}
