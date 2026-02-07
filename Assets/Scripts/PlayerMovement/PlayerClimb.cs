/*
    PlayerClimb.cs  –  Ladder climbing
    ──────────────────────────────────
    Manages the climbing state, climb movement, and detach logic.

    Attach to the Player GameObject alongside PlayerMotor.
    The LadderZone component (on ladder objects) calls EnterLadder / ExitLadder.
*/

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerClimb : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 4f;
    [Tooltip("Cooldown after detaching before the player can re-attach to a ladder")]
    [SerializeField] private float detachCooldown = 0.5f;

    // ═══════════════════════════════════════════
    //  Public State
    // ═══════════════════════════════════════════

    /// <summary>True while the player is attached to a ladder.</summary>
    public bool IsClimbing { get; private set; }

    /// <summary>Time remaining before the player can re-attach to a ladder.</summary>
    public float CooldownRemaining { get; private set; }

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private PlayerMotor motor;
    private Rigidbody2D rb;

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Tick cooldown
        if (CooldownRemaining > 0f)
            CooldownRemaining -= Time.deltaTime;

        if (!IsClimbing) return;

        // ── Detach conditions ──

        // 1) Press Space to jump off the ladder
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Detach();
            return;
        }

        // 2) Touching ground while not pressing up  →  step off ladder naturally
        if (motor.IsGrounded && motor.MoveInput.y <= 0f)
        {
            Detach();
        }
    }

    void FixedUpdate()
    {
        if (!IsClimbing) return;

        // No gravity while on the ladder
        rb.gravityScale = 0f;

        // Move in all directions at climb speed
        rb.linearVelocity = motor.MoveInput * climbSpeed;
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Called by LadderZone
    // ═══════════════════════════════════════════

    /// <summary>
    /// Attach the player to a ladder.
    /// Called by LadderZone.OnTriggerEnter2D / OnTriggerStay2D.
    /// </summary>
    /// <param name="ladderCenterX">The world-space X center of the ladder collider.</param>
    public void EnterLadder(float ladderCenterX)
    {
        if (CooldownRemaining > 0f) return;

        IsClimbing = true;
        rb.gravityScale = 0f;

        // Snap player horizontally to the ladder centre
        Vector3 pos = transform.position;
        pos.x = ladderCenterX;
        transform.position = pos;
    }

    /// <summary>
    /// Detach the player from the ladder.
    /// Called by LadderZone.OnTriggerExit2D or internally when pressing Space / touching ground.
    /// </summary>
    public void ExitLadder()
    {
        if (!IsClimbing) return;

        IsClimbing = false;
        rb.gravityScale = 1f;
    }

    // ═══════════════════════════════════════════
    //  Internal
    // ═══════════════════════════════════════════

    /// <summary>Detach with cooldown (used when the player chooses to leave).</summary>
    private void Detach()
    {
        ExitLadder();
        CooldownRemaining = detachCooldown;
    }
}
