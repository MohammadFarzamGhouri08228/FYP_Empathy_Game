using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Player Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite walkSprite1;
    [SerializeField] private Sprite walkSprite2;
    [SerializeField] private Sprite jumpSprite;
    [SerializeField] private Sprite hangSprite;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;
    private float buttonMoveInput = 0f; // Input from buttons (mobile)
    private float currentButtonMoveDirection = 0f; // Stores the last direction from a button click
    private bool isMovingWithButton = false; // Tracks if player is currently moving due to button click
    private float walkAnimationTimer = 0f;
    private bool isWalking = false;
    private bool isJumping = false;
    private bool isJumpingHorizontally = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Verify components exist
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D component not found!");
            return;
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController: SpriteRenderer component not found!");
            return;
        }
        
        // Create ground check point if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
            Debug.Log("PlayerController: GroundCheck created automatically");
        }
        
        // Set initial sprite
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }
    
    void Update()
    {
        // Safety check
        if (rb == null) return;
        
        // Get input using new Input System
        moveInput = 0f;
        
        // Check keyboard input using new Input System
        if (Keyboard.current != null)
        {
            // Left movement
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                moveInput = -1f;
            }
            // Right movement
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveInput = 1f;
            }
            
            // Jump input - Space key or Up Arrow
            if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && isGrounded)
            {
                Jump();
            }
        }
        
        // Combine keyboard input with button input (buttons take priority if pressed)
        if (isMovingWithButton && Mathf.Abs(buttonMoveInput) > 0.1f)
        {
            moveInput = buttonMoveInput;
        }
        
        // Check if grounded (check in Update for responsive jump)
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = false; // If no ground check, assume not grounded
        }
        
        // Update sprite based on state
        UpdateSprite();
    }
    
    void FixedUpdate()
    {
        // Safety check
        if (rb == null) return;
        
        // Move player in FixedUpdate for smooth physics
        // Apply movement regardless of grounded state (movement should work even in air)
        Vector2 newVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = newVelocity;
        
        // Debug: Log movement when input is detected
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            string direction = moveInput > 0 ? "RIGHT" : "LEFT";
            Debug.Log($"  → FixedUpdate: Applying movement - Direction: {direction}, Speed: {moveSpeed}, Velocity: {rb.linearVelocity}");
        }
        
        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (moveInput > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
        
        // Update walking state
        isWalking = Mathf.Abs(moveInput) > 0.1f && isGrounded;
        isJumping = !isGrounded;
        
        // Check if jumping horizontally (has horizontal movement or velocity while in air)
        // Check both input and velocity to handle momentum
        float horizontalVelocity = Mathf.Abs(rb.linearVelocity.x);
        isJumpingHorizontally = isJumping && (Mathf.Abs(moveInput) > 0.1f || horizontalVelocity > 0.5f);
    }
    
    void Jump()
    {
        // Safety check
        if (rb == null) return;
        
        // Apply jump force directly
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Debug.Log("PlayerController: Jump executed!");
    }
    
    void UpdateSprite()
    {
        if (isJumping)
        {
            // Check if jumping horizontally (left or right)
            if (isJumpingHorizontally && hangSprite != null)
            {
                // Show hang sprite when jumping left or right
                spriteRenderer.sprite = hangSprite;
            }
            else if (jumpSprite != null)
            {
                // Show regular jump sprite when jumping straight up
                spriteRenderer.sprite = jumpSprite;
            }
        }
        else if (isWalking)
        {
            // Animate walking
            walkAnimationTimer += Time.deltaTime;
            float walkCycleSpeed = 0.2f; // Adjust for faster/slower animation
            
            if (walkAnimationTimer >= walkCycleSpeed * 2)
            {
                walkAnimationTimer = 0f;
            }
            
            if (walkAnimationTimer < walkCycleSpeed && walkSprite1 != null)
            {
                spriteRenderer.sprite = walkSprite1;
            }
            else if (walkSprite2 != null)
            {
                spriteRenderer.sprite = walkSprite2;
            }
        }
        else
        {
            // Show idle sprite
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            walkAnimationTimer = 0f;
        }
    }
    
    // Public methods for button input (mobile controls)
    public void SetMoveInput(float inputDirection)
    {
        // If the input direction is different from the current moving direction, or if we are not moving
        if (inputDirection != 0f && inputDirection != currentButtonMoveDirection)
        {
            // Start new movement direction
            currentButtonMoveDirection = inputDirection;
            isMovingWithButton = true;
            buttonMoveInput = currentButtonMoveDirection;
            string direction = inputDirection > 0 ? "RIGHT" : "LEFT";
            Debug.Log($"  → PlayerController.SetMoveInput() called - Started moving {direction}");
            Debug.Log($"    - Input Value: {buttonMoveInput}");
            Debug.Log($"    - Will be applied in FixedUpdate()");
        }
        else if (inputDirection != 0f && inputDirection == currentButtonMoveDirection)
        {
            // If the same button is clicked again, stop movement (toggle)
            currentButtonMoveDirection = 0f;
            isMovingWithButton = false;
            buttonMoveInput = 0f;
            string direction = inputDirection > 0 ? "RIGHT" : "LEFT";
            Debug.Log($"  → PlayerController.SetMoveInput() called - Toggled OFF {direction}");
            Debug.Log($"    - Movement stopped");
        }
        else if (inputDirection == 0f)
        {
            // Explicitly stop movement
            currentButtonMoveDirection = 0f;
            isMovingWithButton = false;
            buttonMoveInput = 0f;
            Debug.Log($"  → PlayerController.SetMoveInput() called - Explicitly stopped movement");
        }
    }
    
    public void OnJumpButtonPressed()
    {
        Debug.Log($"  → PlayerController.OnJumpButtonPressed() called");
        Debug.Log($"    - IsGrounded: {isGrounded}");
        Debug.Log($"    - Rigidbody2D: {(rb != null ? "EXISTS" : "NULL - ERROR!")}");
        
        if (isGrounded)
        {
            Debug.Log($"    ✓ Player is grounded - Executing jump!");
            Jump();
        }
        else
        {
            Debug.LogWarning($"    ✗ Cannot jump - Player is NOT grounded!");
            Debug.LogWarning($"    → Player may be in the air or ground detection is not working correctly.");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

