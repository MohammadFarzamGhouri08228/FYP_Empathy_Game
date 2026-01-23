//using JetBrains.Annotations;
//using UnityEngine;

//public class NormalKidMovementScript2 : MonoBehaviour
//{
//    public float moveSpeed = 5f;
//    public float jumpForce = 10f;
//    public Transform groundCheck;
//    public float groundCheckRadius = 0.2f;
//    public LayerMask groundLayer;

//    private Rigidbody2D rb;
//    private bool isGrounded;
//    void Start()
//    {
//        rb = GetComponent<Rigidbody2D>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        float moveinput = Input.GetAxis("Horizontal");
//        rb.linearVelocity = new Vector2(moveinput * moveSpeed, rb.linearVelocity.y);

//        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
//            Debug.Log("Space detected");
//        {
//            //press space to jump
//            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
//            //also when i press space i should get it on the console 
//            Debug.Log("Jumped!");
//        }
//    }

//    private void FixedUpdate()
//    {
//        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveJump : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // recommended for platformers
    }

    void Update()
    {
        // Ground check
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE DETECTED");
        }

        // Jump (Space)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("Jumped!");
        }
    }

    void FixedUpdate()
    {
        float moveX = 0f;

        if (Keyboard.current != null)
        {
            // Left / Right only
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                moveX = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                moveX = 1f;
        }

        // Apply X movement, keep Y from physics (gravity/jump)
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
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

