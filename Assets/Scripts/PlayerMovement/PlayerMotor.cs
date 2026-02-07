/*
    PlayerMotor.cs  –  Core player controller
    ──────────────────────────────────────────
    Handles: horizontal movement (walk), jumping, input reading,
    ground detection, and coordination with other PlayerMovement components.

    Attach to the Player GameObject (requires Rigidbody2D).
    Works alongside: PlayerClimb, PlayerSpriteAnimator.
*/

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Walk")]
    [SerializeField] private float walkSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [Tooltip("Grace period after leaving ground where jump is still allowed")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("Buffer a jump input just before landing")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Ground Detection")]
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rendering")]
    [SerializeField] private int playerSortingOrder = 15;

    // ═══════════════════════════════════════════
    //  Public Read-Only State (used by other components)
    // ═══════════════════════════════════════════

    /// <summary>True when the player is standing on ground.</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>True when walking (grounded + horizontal input).</summary>
    public bool IsWalking { get; private set; }

    /// <summary>True when airborne and moving upward.</summary>
    public bool IsJumping { get; private set; }

    /// <summary>True when airborne and moving downward.</summary>
    public bool IsFalling { get; private set; }

    /// <summary>Current combined movement input (keyboard + mobile buttons).</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>1 = facing right, -1 = facing left.</summary>
    public float FacingDirection { get; private set; } = 1f;

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    // Cached components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerClimb playerClimb;

    // Coyote time & jump buffer
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpConsumed;

    // Mobile / UI button input
    private float buttonHorizontal;
    private bool buttonJumpRequested;

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerClimb = GetComponent<PlayerClimb>();

        rb.freezeRotation = true;

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = playerSortingOrder;
    }

    void Update()
    {
        // Always read input and ground state (other components depend on these)
        ReadInput();
        UpdateGroundCheck();

        // If climbing, let PlayerClimb handle jump / movement
        if (playerClimb != null && playerClimb.IsClimbing)
            return;

        HandleJumpInput();
        UpdateState();
    }

    void FixedUpdate()
    {
        // If climbing, PlayerClimb drives physics
        if (playerClimb != null && playerClimb.IsClimbing)
            return;

        ApplyMovement();
    }

    // ═══════════════════════════════════════════
    //  Input
    // ═══════════════════════════════════════════

    private void ReadInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            // Horizontal
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                input.x = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                input.x = 1f;

            // Vertical (primarily used for climbing, but always available)
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                input.y = 1f;
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                input.y = -1f;
        }

        // Mobile button override
        if (Mathf.Abs(buttonHorizontal) > 0.01f)
            input.x = buttonHorizontal;

        MoveInput = input;

        // Track facing direction (persists when input stops)
        if (input.x > 0.1f) FacingDirection = 1f;
        else if (input.x < -0.1f) FacingDirection = -1f;
    }

    // ═══════════════════════════════════════════
    //  Ground Detection
    // ═══════════════════════════════════════════

    private void UpdateGroundCheck()
    {
        IsGrounded = Physics2D.OverlapCircle(
            (Vector2)transform.position + groundCheckOffset,
            groundCheckRadius,
            groundLayer
        );

        if (IsGrounded)
        {
            coyoteCounter = coyoteTime;
            jumpConsumed = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    // ═══════════════════════════════════════════
    //  Jump
    // ═══════════════════════════════════════════

    private void HandleJumpInput()
    {
        bool jumpPressed = false;

        if (Keyboard.current != null)
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;

        // Mobile button jump
        if (buttonJumpRequested)
        {
            jumpPressed = true;
            buttonJumpRequested = false;
        }

        // Jump buffering
        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Execute jump when conditions are met
        if (jumpBufferCounter > 0f && coyoteCounter > 0f && !jumpConsumed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpConsumed = true;
        }
    }

    // ═══════════════════════════════════════════
    //  State Flags
    // ═══════════════════════════════════════════

    private void UpdateState()
    {
        bool hasHorizontalInput = Mathf.Abs(MoveInput.x) > 0.1f;

        IsWalking  = hasHorizontalInput && IsGrounded;
        IsJumping  = !IsGrounded && rb.linearVelocity.y > 0.1f;
        IsFalling  = !IsGrounded && rb.linearVelocity.y < -0.1f;
    }

    // ═══════════════════════════════════════════
    //  Physics
    // ═══════════════════════════════════════════

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * walkSpeed, rb.linearVelocity.y);
    }

    // ═══════════════════════════════════════════
    //  Public API  –  Mobile / UI Buttons
    // ═══════════════════════════════════════════

    /// <summary>
    /// Set horizontal input from a UI button.
    /// Pass -1 for left, 1 for right, or 0 to stop.
    /// </summary>
    public void SetButtonMoveInput(float direction)
    {
        buttonHorizontal = Mathf.Clamp(direction, -1f, 1f);
    }

    /// <summary>Request a jump from a UI button.</summary>
    public void RequestButtonJump()
    {
        buttonJumpRequested = true;
    }

    /// <summary>Access the Rigidbody2D (used by sibling components like PlayerClimb).</summary>
    public Rigidbody2D GetRigidbody() => rb;

    /// <summary>Access the SpriteRenderer (used by PlayerSpriteAnimator).</summary>
    public SpriteRenderer GetSpriteRenderer() => spriteRenderer;

    // ═══════════════════════════════════════════
    //  Editor Gizmos
    // ═══════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }
}
