using UnityEngine;

// This ensures Unity automatically adds a Rigidbody2D if you forgot!
[RequireComponent(typeof(Rigidbody2D))] 
public class KickableBall : MonoBehaviour
{
    [Header("Physics Settings")]
    public float kickForce = 5f;    // How fast it flies away
    public float upwardLift = 1.5f; // Gives it a little "pop" upward when hit

    private Rigidbody2D rb;

    void Start()
    {
        // Grab the Rigidbody2D component attached to the ball
        rb = GetComponent<Rigidbody2D>();
    }

    // This built-in Unity function triggers the exact moment two colliders hit each other
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object bumping into the ball is your player.
        // NOTE: Your player character MUST have its Tag set to "Player" in the Inspector!
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Calculate direction: from the Player TO the Ball
            Vector2 pushDirection = transform.position - collision.transform.position;
            
            // 2. Normalize it so the length is exactly 1 (keeps the kick strength consistent)
            pushDirection = pushDirection.normalized;

            // 3. Add a bit of upward direction so it doesn't just slide flat on the ground
            pushDirection.y += upwardLift;

            // 4. Apply the physics force! 
            // ForceMode2D.Impulse is crucial here—it applies a sudden burst of energy.
            rb.AddForce(pushDirection * kickForce, ForceMode2D.Impulse);
        }
    }
}