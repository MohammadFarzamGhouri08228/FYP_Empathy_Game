using UnityEngine;

public class SlopeHandler : MonoBehaviour
{
    [Header("Slope Settings")]
    [Tooltip("Force applied to the player to make them slip down.")]
    [SerializeField] private float slideForce = 10f;
    
    [Tooltip("If true, automatically calculates slide direction based on the slope's angle (contact normal).")]
    [SerializeField] private bool usePhysicsNormal = true;
    
    [Tooltip("Manually define slide direction if not using physics normal. (e.g. (1, -1) for down-right)")]
    [SerializeField] private Vector2 manualSlideDirection = new Vector2(1, -1);

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check for both player controller types
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Lvl2movement playerLvl2 = collision.gameObject.GetComponent<Lvl2movement>();
            
            bool isPlayer = player != null || playerLvl2 != null;
            
            if (isPlayer)
            {
                // Notify player controller that we are on a slope
                // Logic moved lower to capture slideDir
                
                Vector2 slideDir;

                if (usePhysicsNormal && collision.contactCount > 0)
                {
                    // Get the normal of the surface we are standing on
                    Vector2 normal = collision.GetContact(0).normal;
                    
                    // Calculate the tangent (slope direction)
                    // The tangent is perpendicular to the normal: (-y, x)
                    Vector2 tangent = new Vector2(normal.y, -normal.x);
                    
                    // Ensure the tangent points downwards (y should be negative)
                    if (tangent.y > 0)
                    {
                        tangent = -tangent;
                    }
                    
                    slideDir = tangent;
                }
                else
                {
                    // Use manual direction
                    slideDir = manualSlideDirection.normalized;
                }
                
                // Assign slope info to players
                if (player != null) 
                {
                    player.IsOnSlope = true;
                    player.SlopeDownDirection = slideDir;
                }
                if (playerLvl2 != null) 
                {
                    playerLvl2.IsOnSlope = true;
                    playerLvl2.SlopeDownDirection = slideDir;
                }
                
                // Apply the slide force to the player's Rigidbody
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Only apply force if player is trying to stop (or always, for extra slippery)
                    // Adding force always makes it harder to climb up
                    rb.AddForce(slideDir * slideForce);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.IsOnSlope = false;
            }

            Lvl2movement playerLvl2 = collision.gameObject.GetComponent<Lvl2movement>();
            if (playerLvl2 != null)
            {
                playerLvl2.IsOnSlope = false;
            }
        }
    }
}
