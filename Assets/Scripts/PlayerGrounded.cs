using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerGrounded : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite jumpSprite;
    public Sprite walkSprite1;
    public Sprite walkSprite2;
    public float walkAnimSpeed = 0.2f;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = false;
    private float walkTimer;
    private bool isWalkSprite1 = true;
    
    // Reference to check if UI buttons are moving the player
    private PlayerMovementController uiMovement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();
        uiMovement = GetComponent<PlayerMovementController>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // Horizontal movement (Left/Right) via Keyboard
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                moveInput.x = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveInput.x = 1f;
            }

            // Jump (Space) - Only if grounded
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false; // We jumped!
            }
        }

        // Update Sprite based on actual physical velocity (Works for both Keyboard and UI Buttons!)
        if (sr != null)
        {
            // Use velocity for animation so on-screen buttons work too!
            float currentSpeed = Mathf.Abs(rb.linearVelocity.x);

            // Flip sprite depending on physical direction
            if (rb.linearVelocity.x < -0.1f)
                sr.flipX = true;
            else if (rb.linearVelocity.x > 0.1f)
                sr.flipX = false;

            if (!isGrounded && Mathf.Abs(rb.linearVelocity.y) > 0.1f)
            {
                if (jumpSprite != null)
                    sr.sprite = jumpSprite;
                else
                    Debug.LogWarning("Jump Sprite is missing! Please assign it in the Inspector.");
            }
            else
            {
                if (currentSpeed > 0.1f)
                {
                    walkTimer += Time.deltaTime;
                    if (walkTimer >= walkAnimSpeed)
                    {
                        walkTimer = 0f;
                        isWalkSprite1 = !isWalkSprite1;
                    }
                    
                    if (isWalkSprite1 && walkSprite1 != null)
                        sr.sprite = walkSprite1;
                    else if (!isWalkSprite1 && walkSprite2 != null)
                        sr.sprite = walkSprite2;
                }
                else
                {
                    if (idleSprite != null)
                        sr.sprite = idleSprite;
                    else
                        Debug.LogWarning("Idle Sprite is missing! Please assign it in the Inspector.");
                }
            }
        }
    }

    void FixedUpdate()
    {
        // Only apply keyboard movement if the UI controller isn't actively moving the player
        // This prevents the two scripts from fighting each other!
        if (true)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
    }

    // --- FOOLPROOF GROUND DETECTION ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        // If we are touching anything solid, we are grounded
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // When we stop touching something, we are no longer grounded
        isGrounded = false;
    }
}
