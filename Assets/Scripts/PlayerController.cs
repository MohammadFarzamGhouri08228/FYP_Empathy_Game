using UnityEngine;

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
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;
    private float walkAnimationTimer = 0f;
    private bool isWalking = false;
    private bool isJumping = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Create ground check point if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // Set initial sprite
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }
    
    void Update()
    {
        // Get input
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        
        // Update sprite based on state
        UpdateSprite();
    }
    
    void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Move player
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // Flip sprite based on movement direction
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }
        
        // Update walking state
        isWalking = Mathf.Abs(moveInput) > 0.1f && isGrounded;
        isJumping = !isGrounded;
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    void UpdateSprite()
    {
        if (isJumping)
        {
            // Show jump sprite
            if (jumpSprite != null)
            {
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
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

