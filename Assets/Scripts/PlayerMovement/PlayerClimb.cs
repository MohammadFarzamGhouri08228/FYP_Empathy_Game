/*
    PlayerClimb.cs  –  Ladder climbing
    ──────────────────────────────────
    Manages the climbing state, climb movement, and detach logic.

    Climbing is a Space-bar toggle:
      • Press Space near a ladder  →  grab on
      • Press Space while climbing →  let go

    Attach to the Player GameObject alongside PlayerMotor.
    The LadderZone component (on ladder objects) calls SetNearLadder / ClearNearLadder.
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
    [SerializeField] private float detachCooldown = 0.3f;

    // ═══════════════════════════════════════════
    //  Public State
    // ═══════════════════════════════════════════

    /// <summary>True while the player is attached to a ladder.</summary>
    public bool IsClimbing { get; private set; }

    /// <summary>True when the player is inside a ladder trigger zone.</summary>
    public bool IsNearLadder { get; private set; }

    /// <summary>Time remaining before the player can re-attach to a ladder.</summary>
    public float CooldownRemaining { get; private set; }

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private PlayerMotor motor;
    private Rigidbody2D rb;
    private float nearLadderCenterX;
    private float ladderTopY;
    private float ladderBottomY;

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

        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (IsClimbing)
        {
            // ── Currently climbing: Space to let go ──
            if (spacePressed)
            {
                Detach();
            }
        }
        else
        {
            // ── Not climbing: Space near a ladder to grab on ──
            if (spacePressed && IsNearLadder && CooldownRemaining <= 0f)
            {
                GrabLadder();
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsClimbing) return;

        // No gravity while on the ladder
        rb.gravityScale = 0f;

        // Build desired velocity
        Vector2 climbVelocity = motor.MoveInput * climbSpeed;

        // Block upward movement if at the top of the ladder
        if (transform.position.y >= ladderTopY && climbVelocity.y > 0f)
            climbVelocity.y = 0f;

        // Block downward movement if at the bottom of the ladder
        if (transform.position.y <= ladderBottomY && climbVelocity.y < 0f)
            climbVelocity.y = 0f;

        rb.linearVelocity = climbVelocity;
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Called by LadderZone
    // ═══════════════════════════════════════════

    /// <summary>
    /// Mark that the player is inside a ladder trigger zone.
    /// Called by LadderZone.OnTriggerEnter2D / OnTriggerStay2D.
    /// </summary>
    public void SetNearLadder(float ladderCenterX, float bottomY, float topY)
    {
        IsNearLadder = true;
        nearLadderCenterX = ladderCenterX;
        ladderBottomY = bottomY;
        ladderTopY = topY;
    }

    /// <summary>
    /// Mark that the player has left the ladder trigger zone.
    /// Called by LadderZone.OnTriggerExit2D.
    /// Also detaches the player if they are currently climbing.
    /// </summary>
    public void ClearNearLadder()
    {
        IsNearLadder = false;

        // Force detach – since grabbing requires Space, there is
        // no risk of flickering when the player falls back into the trigger.
        if (IsClimbing)
            Detach();
    }

    // ═══════════════════════════════════════════
    //  Internal
    // ═══════════════════════════════════════════

    /// <summary>Grab the ladder and start climbing.</summary>
    private void GrabLadder()
    {
        IsClimbing = true;
        rb.gravityScale = 0f;

        // Kill all momentum so the player doesn't drift on the ladder
        rb.linearVelocity = Vector2.zero;

        // Snap player horizontally to the ladder centre
        Vector3 pos = transform.position;
        pos.x = nearLadderCenterX;
        transform.position = pos;
    }

    /// <summary>Detach with cooldown.</summary>
    private void Detach()
    {
        if (!IsClimbing) return;

        IsClimbing = false;
        rb.gravityScale = 1f;

        // Kill vertical velocity so the player doesn't shoot up/down
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        CooldownRemaining = detachCooldown;
    }
}
