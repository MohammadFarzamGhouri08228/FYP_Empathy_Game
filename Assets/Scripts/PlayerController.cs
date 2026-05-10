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
    
    [Header("Rendering Settings")]
    [SerializeField] private int playerSortingOrder = 15; // Higher than spikes (which use max 10)
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;
    private float buttonMoveInput = 0f; // Input from buttons (mobile)
    private float currentButtonMoveDirection = 0f; // Stores the last direction from a button click
    private bool isMovingWithButton = false; // Tracks if player is currently moving due to button click
    private float walkAnimationTimer = 0f;
    private float idleDelayTimer = 0f; // Small delay before switching to idle
    public bool isWalking = false;
    public bool isJumping = false;
    public bool IsOnSlope { get; set; }
    public Vector2 SlopeDownDirection { get; set; }
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
        
        // Set player sorting order to ensure they render above spikes and other obstacles
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = playerSortingOrder;
            Debug.Log($"PlayerController: Sorting order set to {playerSortingOrder}");
        }
        
        // If ground layer is set to nothing, ground detection will always fail
        // This causes the character to constantly be in a "Jumping" state and stops the walk animation from playing!
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("PlayerController: Ground Layer is set to 'Nothing'! This will break animations and jumping. Automatically falling back to 'Default' layer.");
            groundLayer = LayerMask.GetMask("Default");
        }
        
        // Also ensure "Ground" layer is included just in case
        int groundIdx = LayerMask.NameToLayer("Ground");
        if (groundIdx != -1)
        {
            groundLayer |= (1 << groundIdx);
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
            // First try with the assigned ground layer
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            
            // If still not grounded, fallback check against everything except "Player" and "Ignore Raycast"
            // This is especially useful for different levels with unassigned ground layers
            if (!isGrounded)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                int mask = ~((playerLayer != -1 ? 1 << playerLayer : 0) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
                isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);
            }
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
        
        // Update walking state regardless of whether we are grounded or jumping
        isWalking = Mathf.Abs(moveInput) > 0.1f;
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
            // Reset delay timer when walking starts
            idleDelayTimer = 0.15f; 

            // Animate walking
            walkAnimationTimer += Time.deltaTime;
            float walkCycleSpeed = 0.4f; // Slower animation, better transition delay
            
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
            // Wait briefly before showing idle sprite
            if (idleDelayTimer > 0f)
            {
                idleDelayTimer -= Time.deltaTime;
                // Keep the current walk sprite visible, so do nothing here
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

