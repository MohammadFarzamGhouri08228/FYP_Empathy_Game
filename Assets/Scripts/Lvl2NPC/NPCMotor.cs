/*
    NPCMotor.cs  –  NPC physical movement
    ─────────────────────────────────────
    Handles Rigidbody2D-based horizontal movement for the NPC.
    Exposes Walk / Stop methods called by NPCController.

    Attach to the NPC GameObject (requires Rigidbody2D).
    Works alongside: NPCController, NPCSpriteAnimator.
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCMotor : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Walk")]
    [SerializeField] private float walkSpeed = 3f;

    [Header("Physics")]
    [SerializeField] private float gravityScale = 5f;

    [Header("Ground Check")]
    [Tooltip("Layer(s) that count as ground (for detecting landing after a ladder hop).")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 15;

    // ═══════════════════════════════════════════
    //  Public Read-Only State
    // ═══════════════════════════════════════════

    /// <summary>True while the NPC is walking.</summary>
    public bool IsWalking { get; private set; }

    /// <summary>True while the NPC is climbing a ladder.</summary>
    public bool IsClimbing { get; private set; }

    /// <summary>Current walk direction (normalized). Zero when stopped.</summary>
    public Vector2 WalkDirection { get; private set; }

    /// <summary>True when the NPC is touching the ground (overlap-circle check).</summary>
    public bool IsGrounded => Physics2D.OverlapCircle(
        (Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer);

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float climbVelocity;               // vertical speed while climbing

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = sortingOrder;
    }

    void FixedUpdate()
    {
        if (IsClimbing)
        {
            // Climbing: no gravity, pure vertical movement
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, climbVelocity);
            return;
        }

        // Normal mode: enforce gravity
        rb.gravityScale = gravityScale;

        if (IsWalking)
        {
            // Apply horizontal walk velocity, preserve vertical (gravity)
            rb.linearVelocity = new Vector2(WalkDirection.x * walkSpeed, rb.linearVelocity.y);
        }
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Called by NPCController
    // ═══════════════════════════════════════════

    /// <summary>
    /// Start walking in the given direction.
    /// </summary>
    /// <param name="direction">Walk direction (will be normalized).</param>
    public void Walk(Vector2 direction)
    {
        WalkDirection = direction.normalized;
        IsWalking = true;
    }

    /// <summary>
    /// Stop all horizontal movement immediately.
    /// </summary>
    public void Stop()
    {
        IsWalking = false;
        WalkDirection = Vector2.zero;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    /// <summary>
    /// Begin climbing at the given vertical speed (positive = up).
    /// Disables gravity and horizontal movement.
    /// </summary>
    public void StartClimb(float speed)
    {
        IsWalking = false;
        WalkDirection = Vector2.zero;
        IsClimbing = true;
        climbVelocity = speed;
    }

    /// <summary>
    /// Stop climbing. Re-enables gravity and zeroes velocity.
    /// </summary>
    public void StopClimb()
    {
        IsClimbing = false;
        climbVelocity = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Hop off the ladder with an impulse (e.g. up-right).
    /// Ends climbing and re-enables gravity so the hop follows a natural arc.
    /// </summary>
    public void DismountHop(Vector2 hopVelocity)
    {
        IsClimbing = false;
        climbVelocity = 0f;
        rb.linearVelocity = hopVelocity;
        // Gravity is restored on the very next FixedUpdate (IsClimbing == false)
    }

    /// <summary>
    /// Teleport the NPC to a world position and zero out velocity.
    /// </summary>
    public void TeleportTo(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }
}
