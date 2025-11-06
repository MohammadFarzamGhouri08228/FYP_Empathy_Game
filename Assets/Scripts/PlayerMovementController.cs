using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private bool isMovingRight = false;
    private bool isMovingLeft = false;
    private bool isJumping = false;
    private bool isGrounded = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure Rigidbody2D exists
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is missing! Please add one to the Player GameObject.");
        }
    }
    
    void Update()
    {
        // Check if player is on the ground
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // Fallback if no ground check is set
            isGrounded = true;
        }
    }
    
    void FixedUpdate()
    {
        // Handle horizontal movement
        float horizontalMove = 0f;
        
        if (isMovingRight)
        {
            horizontalMove = 1f;
        }
        else if (isMovingLeft)
        {
            horizontalMove = -1f;
        }
        
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalMove * moveSpeed, rb.linearVelocity.y);
        
        // Handle jumping (only if grounded)
        if (isJumping && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = false; // Reset jump flag after applying jump
        }
    }
    
    // Methods to be called by UI buttons
    public void OnRightButtonDown()
    {
        isMovingRight = true;
        Debug.Log("PlayerMovementController: OnRightButtonDown() - Started moving RIGHT");
    }
    
    public void OnRightButtonUp()
    {
        isMovingRight = false;
        Debug.Log("PlayerMovementController: OnRightButtonUp() - Stopped moving RIGHT");
    }
    
    public void OnLeftButtonDown()
    {
        isMovingLeft = true;
        Debug.Log("PlayerMovementController: OnLeftButtonDown() - Started moving LEFT");
    }
    
    public void OnLeftButtonUp()
    {
        isMovingLeft = false;
        Debug.Log("PlayerMovementController: OnLeftButtonUp() - Stopped moving LEFT");
    }
    
    public void OnJumpButtonDown()
    {
        isJumping = true;
        Debug.Log("PlayerMovementController: OnJumpButtonDown() - Jump requested");
    }
    
    public void OnJumpButtonUp()
    {
        isJumping = false;
        Debug.Log("PlayerMovementController: OnJumpButtonUp() - Jump state reset");
    }
    
    // Optional: Visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

