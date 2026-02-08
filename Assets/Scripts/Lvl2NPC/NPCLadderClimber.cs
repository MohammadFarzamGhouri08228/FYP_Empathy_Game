/*
    NPCLadderClimber.cs  –  NPC ladder detection & climbing
    ────────────────────────────────────────────────────────
    Detects when the NPC enters a ladder trigger zone,
    pauses movement, and exposes an API for NPCController
    to orchestrate the climb.

    Flow:
      NPC walks → enters ladder trigger → stops →
      NPCController calls BeginClimb() → NPC climbs up →
      reaches exit height → hops off to the right →
      lands on platform → NPCController resumes walking.

    Exit-point design
    ─────────────────
    Each ladder can have an optional child Transform called
    "NPCExitPoint" (assigned via the ladder Inspector).
    • If set  → NPC climbs until it reaches that Y height,
                then hops off.
    • If null → NPC climbs to the top of the ladder's
                collider bounds + a small offset, then hops.

    Attach to the NPC alongside NPCMotor, NPCController.
*/

using UnityEngine;

[RequireComponent(typeof(NPCMotor))]
[RequireComponent(typeof(NPCController))]
public class NPCLadderClimber : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Climbing")]
    [Tooltip("Vertical speed while climbing (units / sec).")]
    [SerializeField] private float climbSpeed = 2.5f;

    [Header("Dismount Hop")]
    [Tooltip("Velocity applied when the NPC hops off at the top (X = rightward, Y = upward).")]
    [SerializeField] private Vector2 dismountHopForce = new Vector2(3f, 4f);

    [Header("Fallback Exit")]
    [Tooltip("If a ladder has no NPCExitPoint, exit this far above the collider top.")]
    [SerializeField] private float topExitOffset = 0.5f;

    // ═══════════════════════════════════════════
    //  Public Read-Only State
    // ═══════════════════════════════════════════

    /// <summary>True when the NPC has stopped at a ladder base and awaits input.</summary>
    public bool IsAtLadder { get; private set; }

    /// <summary>True while the NPC is actively climbing.</summary>
    public bool IsClimbing { get; private set; }

    /// <summary>True once the NPC has reached the exit point and dismounted.</summary>
    public bool FinishedClimbing { get; private set; }

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private NPCMotor motor;
    private NPCController controller;

    private ladder currentLadder;     // the ladder we're interacting with
    private float exitY;              // Y height at which the NPC dismounts
    private Vector3 exitPosition;     // full position to snap to on dismount
    private bool hasFullExitPos;      // true when an NPCExitPoint supplies X+Y

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<NPCMotor>();
        controller = GetComponent<NPCController>();
    }

    void Update()
    {
        if (!IsClimbing) return;

        // Check if NPC has reached (or passed) the exit height
        if (transform.position.y >= exitY)
        {
            FinishClimbing();
        }
    }

    // ═══════════════════════════════════════════
    //  Trigger Detection
    // ═══════════════════════════════════════════

    /// <summary>
    /// Fires when the NPC's collider enters a trigger (e.g. a ladder zone).
    /// Only reacts while the NPC is walking and not already on a ladder.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Guard – only detect while walking, and only once per ladder
        if (IsAtLadder || IsClimbing || FinishedClimbing) return;
        if (controller.CurrentState != NPCState.Walking) return;

        // Look for a ladder script on the trigger owner
        ladder ladderScript = other.GetComponent<ladder>();
        if (ladderScript == null) ladderScript = other.GetComponentInParent<ladder>();
        if (ladderScript == null) return;

        // ── Found a ladder ──
        currentLadder = ladderScript;

        // Determine exit point
        if (ladderScript.NPCExitPoint != null)
        {
            exitPosition = ladderScript.NPCExitPoint.position;
            exitY = exitPosition.y;
            hasFullExitPos = true;
        }
        else
        {
            // Fallback: top of the ladder's collider + offset
            exitY = other.bounds.max.y + topExitOffset;
            exitPosition = new Vector3(other.bounds.center.x, exitY, transform.position.z);
            hasFullExitPos = false;
        }

        // Snap NPC to ladder centre X
        Vector3 pos = transform.position;
        pos.x = other.bounds.center.x;
        transform.position = pos;

        // Stop horizontal walk and signal "at ladder"
        motor.Stop();
        IsAtLadder = true;

        Debug.Log($"[NPCLadderClimber] Reached ladder '{ladderScript.name}'. " +
                  $"Exit Y: {exitY:F2}  (full pos: {hasFullExitPos})");
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Called by NPCController
    // ═══════════════════════════════════════════

    /// <summary>Start the climb. Called by NPCController when a ladder is reached.</summary>
    public void BeginClimb()
    {
        if (!IsAtLadder) return;

        IsAtLadder = false;
        IsClimbing = true;
        motor.StartClimb(climbSpeed);

        Debug.Log("[NPCLadderClimber] Climbing started.");
    }

    /// <summary>Reset all flags so the climber is ready for the next ladder.</summary>
    public void ConsumeClimb()
    {
        IsAtLadder = false;
        IsClimbing = false;
        FinishedClimbing = false;
        currentLadder = null;
        hasFullExitPos = false;
    }

    // ═══════════════════════════════════════════
    //  Internal Helpers
    // ═══════════════════════════════════════════

    private void FinishClimbing()
    {
        IsClimbing = false;
        FinishedClimbing = true;

        // Hop off the ladder – gravity is re-enabled by DismountHop
        // so the NPC follows a natural arc and lands on the platform.
        motor.DismountHop(dismountHopForce);

        Debug.Log($"[NPCLadderClimber] Hopping off ladder with force {dismountHopForce}");
    }

    // ═══════════════════════════════════════════
    //  Editor Gizmos
    // ═══════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        if (currentLadder == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(exitPosition, 0.3f);
    }
}
