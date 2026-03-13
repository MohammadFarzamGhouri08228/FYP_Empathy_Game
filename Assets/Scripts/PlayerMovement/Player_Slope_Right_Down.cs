/*
    Player_Slope_Right_Down.cs  –  Right-slope sliding
    ───────────────────────────────────────────────────
    Detects when the player is standing on the "Slope_R" layer
    and automatically slides them to the right with a slide sprite.

    Attach to the Player GameObject alongside PlayerMotor.
    Assign the "Slope_R" layer in the Inspector.
*/

using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class Player_Slope_Right_Down : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  Inspector Settings
    // ═══════════════════════════════════════════

    [Header("Slope Detection")]
    [Tooltip("Layer that triggers the right-slide (set to Slope_R)")]
    [SerializeField] private LayerMask slopeLayer;
    [SerializeField] private Vector2 slopeCheckOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float slopeCheckRadius = 0.25f;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 7f;

    [Header("Sprite")]
    [SerializeField] private Sprite slideSprite;

    // ═══════════════════════════════════════════
    //  Public State
    // ═══════════════════════════════════════════

    /// <summary>True while the player is sliding on a right slope.</summary>
    public bool IsSliding { get; private set; }

    // ═══════════════════════════════════════════
    //  Private Fields
    // ═══════════════════════════════════════════

    private PlayerMotor motor;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // ═══════════════════════════════════════════
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckSlope();
    }

    void FixedUpdate()
    {
        if (!IsSliding) return;

        rb.linearVelocity = new Vector2(slideSpeed, rb.linearVelocity.y);
    }

    void LateUpdate()
    {
        if (!IsSliding) return;

        if (spriteRenderer != null && slideSprite != null)
        {
            spriteRenderer.sprite = slideSprite;
            spriteRenderer.flipX = false;
        }
    }

    // ═══════════════════════════════════════════
    //  Slope Detection
    // ═══════════════════════════════════════════

    private void CheckSlope()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + slopeCheckOffset,
            slopeCheckRadius,
            slopeLayer
        );

        IsSliding = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != gameObject && !hit.isTrigger)
            {
                IsSliding = true;
                break;
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Editor Gizmos
    // ═══════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsSliding ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere((Vector2)transform.position + slopeCheckOffset, slopeCheckRadius);
    }
}
