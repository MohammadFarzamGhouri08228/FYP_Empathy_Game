using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CandyMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    // Score is now tracked by CandyGameManager

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite jumpSprite;
    private SpriteRenderer sr;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded; // We will track this automatically now!

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Move Left/Right
        moveInput = Input.GetAxisRaw("Horizontal");

        // 2. Jump (if we are on the ground and press Space)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; // We just jumped, so we leave the ground
        }

        // Update Sprite
        if (sr != null)
        {
            if (isGrounded && idleSprite != null)
            {
                sr.sprite = idleSprite;
            }
            else if (!isGrounded && jumpSprite != null)
            {
                sr.sprite = jumpSprite;
            }
        }
    }

    void FixedUpdate()
    {
        // Apply the left/right movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // 3. This automatically detects when we land on the solid floor!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If it's a candy that hit us physically (not a trigger), collect it!
        if (collision.gameObject.CompareTag("candy"))
        {
            if (CandyGameManager.Instance != null) CandyGameManager.Instance.AddScore(1);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("bomb"))
        {
            if (CandyGameManager.Instance != null) CandyGameManager.Instance.AddScore(-2);
            Destroy(collision.gameObject);
        }
        else 
        {
            // As long as we bumped into something solid OTHER than a candy, we can jump again
            isGrounded = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // When we leave a solid surface (like jumping or falling)
        if (!collision.gameObject.CompareTag("candy") && !collision.gameObject.CompareTag("bomb"))
        {
            isGrounded = false;
        }
    }

    // 4. This collects candies when we touch them (candies must be Triggers!)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D fired! We hit something with tag: " + collision.gameObject.tag);

        if (collision.gameObject.CompareTag("candy"))
        {
            if (CandyGameManager.Instance != null) CandyGameManager.Instance.AddScore(1);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("bomb"))
        {
            if (CandyGameManager.Instance != null) CandyGameManager.Instance.AddScore(-2);
            Destroy(collision.gameObject);
        }
    }
}
