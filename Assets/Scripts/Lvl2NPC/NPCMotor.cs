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

    [Header("Rendering")]
    [SerializeField] private int sortingOrder = 15;

    // ═══════════════════════════════════════════
    //  Public Read-Only State
    // ═══════════════════════════════════════════

    /// <summary>True while the NPC is walking.</summary>
    public bool IsWalking { get; private set; }

    /// <summary>Current walk direction (normalized). Zero when stopped.</summary>
    public Vector2 WalkDirection { get; private set; }

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

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
        // Always enforce gravity
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
    /// Teleport the NPC to a world position and zero out velocity.
    /// </summary>
    public void TeleportTo(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }
}
