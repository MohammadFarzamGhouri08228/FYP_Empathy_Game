using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerGrounded : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;      // Set this to your "Ground" layer in Inspector
    public float groundCheckRadius = 0.2f; 
    public Vector2 groundCheckOffset = new Vector2(0, -0.5f);
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Ground Check Logic
        // We check if the circle at our feet overlaps with anything on the "groundLayer"
        isGrounded = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer);

        moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // Horizontal movement (Left/Right)
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
                Debug.Log("Jumped from ground!");
                // Using linearVelocity (Unity 2023+) or velocity
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // Optional: Visualizes the ground check in the Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
    }
}
