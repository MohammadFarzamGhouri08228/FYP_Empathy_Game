/*
    NPCSpriteAnimator.cs  –  Sprite-based animation for the NPC
    ────────────────────────────────────────────────────────────
    Swaps sprites each frame based on the NPC's current state
    (idle, walking, fallen).

    Attach to the NPC GameObject alongside NPCMotor and NPCController.
    Assign your sprites in the Inspector.
*/

using UnityEngine;

[RequireComponent(typeof(NPCMotor))]
public class NPCSpriteAnimator : MonoBehaviour
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

    [Header("Climb  (2-frame cycle)")]
    [SerializeField] private Sprite climbSprite1;
    [SerializeField] private Sprite climbSprite2;
    [SerializeField] private float climbCycleSpeed = 0.2f;

    [Header("Fallen")]
    [SerializeField] private Sprite fallenSprite;

    // ═══════════════════════════════════════════
    //  Private Fields
    private NPCMotor motor;
    private NPCController controller;
    private SpriteRenderer spriteRenderer;
    private float animTimer;
    private Vector3 normalScale;
    private static readonly Vector3 fallenScale = new Vector3(0.1f, 0.1f, 1f);
    
    private Vector3 lastPosition;
    //  Unity Lifecycle
    // ═══════════════════════════════════════════

    void Awake()
    {
        motor = GetComponent<NPCMotor>();
        controller = GetComponent<NPCController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the original scale so we can restore it after fallen state
        normalScale = transform.localScale;
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (spriteRenderer == null) return;

        // Determine if walking by checking velocity (motor) OR position changes (Level 3 Sorter)
        bool isWalking = false;
        
        if (motor != null && motor.IsWalking)
        {
            isWalking = true;
            
            // Flip sprite based on walk direction (Level 2)
            // Skip this if NPCSorter is attached, because NPCSorter uses transform.localScale and they will double-flip!
            if (GetComponent<NPCSorter>() == null)
            {
                if (motor.WalkDirection.x > 0.1f)
                    spriteRenderer.flipX = false;
                else if (motor.WalkDirection.x < -0.1f)
                    spriteRenderer.flipX = true;
            }
        }
        else
        {
            // Level 3 Sorter override: check position delta
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved > 0.001f)
            {
                isWalking = true;
                // NPCSorter flips via localScale, so we don't need to touch flipX here
            }
        }
        
        lastPosition = transform.position;

        // Pick animation based on priority
        if (controller != null && controller.CurrentState == NPCState.Fallen)
        {
            transform.localScale = fallenScale;
            SetSprite(fallenSprite);
            animTimer = 0f;
        }
        else
        {
            // Restore normal scale for all non-fallen states
            // NOTE: Only restore if NPCSorter is NOT attached (as Sorter modifies localScale for flipping!)
            if (GetComponent<NPCSorter>() == null)
            {
                transform.localScale = normalScale;
            }

            if (controller != null && controller.CurrentState == NPCState.Climbing)
            {
                // Climbing animation
                AnimateCycle(climbSprite1, climbSprite2, climbCycleSpeed);
            }
            else if (isWalking)
            {
                // Walk animation
                AnimateCycle(walkSprite1, walkSprite2, walkCycleSpeed);
            }
            else
            {
                // Idle
                SetSprite(idleSprite);
                animTimer = 0f;
            }
        }
    }

    // ═══════════════════════════════════════════
    //  Animation Helpers
    // ═══════════════════════════════════════════

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
