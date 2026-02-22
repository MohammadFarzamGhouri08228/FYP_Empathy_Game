/*
    PlayerSpriteAnimator.cs  –  Sprite-based animation
    ───────────────────────────────────────────────────
    Swaps sprites each frame based on the player's current state
    (idle, walk, jump, fall, climb).

    Attach to the Player GameObject alongside PlayerMotor.
    Assign your sprites in the Inspector.

    NOTE: If you later switch to Unity's Animator / Animation system,
    you can replace this script without touching the movement code.
*/

using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Sprite Slots
    // ═══════════════════════════════════════════

    [Header("Idle")]
    [SerializeField] private Sprite idleSprite;

    [Header("Walk  (2-frame cycle)")]
    [SerializeField] private Sprite walkSprite1;
    [SerializeField] private Sprite walkSprite2;
    [SerializeField] private float walkCycleSpeed = 0.2f;

    [Header("Jump / Fall")]
    [SerializeField] private Sprite jumpSprite;
    [Tooltip("Optional – if empty, jumpSprite is used for falling too")]
    [SerializeField] private Sprite fallSprite;

    [Header("Climb  (2-frame cycle)")]
    [SerializeField] private Sprite climbSprite1;
    [SerializeField] private Sprite climbSprite2;
    [SerializeField] private float climbCycleSpeed = 0.15f;

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private PlayerMotor motor;
    private PlayerClimb playerClimb;
    private Player_Slope_Right_Down slopeRight;
    private SpriteRenderer spriteRenderer;
    private float animTimer;

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        playerClimb = GetComponent<PlayerClimb>();
        slopeRight = GetComponent<Player_Slope_Right_Down>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (spriteRenderer == null || motor == null) return;

        // Flip sprite based on facing direction (skip while climbing)
        if (playerClimb == null || !playerClimb.IsClimbing)
        {
            spriteRenderer.flipX = motor.FacingDirection < 0f;
        }

        // Pick the right animation based on priority
        if (slopeRight != null && slopeRight.IsSliding)
        {
            return; // Player_Slope_Right_Down handles the slide sprite directly
        }
        else if (playerClimb != null && playerClimb.IsClimbing)
        {
            AnimateClimb();
        }
        else if (motor.IsJumping || motor.IsFalling)
        {
            AnimateAirborne();
        }
        else if (motor.IsWalking)
        {
            AnimateCycle(walkSprite1, walkSprite2, walkCycleSpeed);
        }
        else
        {
            // Idle
            SetSprite(idleSprite);
            animTimer = 0f;
        }
    }

    // ═══════════════════════════════════════════
    //  Animation Helpers
    // ═══════════════════════════════════════════

    private void AnimateClimb()
    {
        // Only animate when pressing up or down
        if (Mathf.Abs(motor.MoveInput.y) > 0.1f)
        {
            AnimateCycle(climbSprite1, climbSprite2, climbCycleSpeed);
        }
        else
        {
            // Idle on ladder
            SetSprite(climbSprite1);
            animTimer = 0f;
        }
    }

    private void AnimateAirborne()
    {
        if (motor.IsFalling && fallSprite != null)
            SetSprite(fallSprite);
        else
            SetSprite(jumpSprite);

        animTimer = 0f;
    }

    /// <summary>Two-frame sprite cycle.</summary>
    private void AnimateCycle(Sprite a, Sprite b, float speed)
    {
        animTimer += Time.deltaTime;

        if (animTimer >= speed * 2f)
            animTimer = 0f;

        SetSprite(animTimer < speed ? a : b);
    }

    private void SetSprite(Sprite s)
    {
        if (s != null)
            spriteRenderer.sprite = s;
    }
}
