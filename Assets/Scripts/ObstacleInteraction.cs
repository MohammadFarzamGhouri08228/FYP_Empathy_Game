using UnityEngine;

public class ObstacleInteraction : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private bool destroyOnContact = false;
    [SerializeField] private float invincibilityDuration = 1f; // Prevent multiple hits in quick succession
    
    private HealthSystem playerHealthSystem;
    private float lastHitTime = 0f;
    
    void Start()
    {
        // Find player's HealthSystem
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController2>()?.gameObject;
        }
        
        if (player != null)
        {
            playerHealthSystem = player.GetComponent<HealthSystem>();
        }
        
        if (playerHealthSystem == null)
        {
            Debug.LogWarning($"ObstacleInteraction on {gameObject.name}: Player HealthSystem not found! Make sure player has HealthSystem component.");
        }
    }
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (IsPlayer(other.gameObject))
        {
            HandleObstacleContact();
        }
    }
    
    // Detect regular collisions (for objects without IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (IsPlayer(collision.gameObject))
        {
            HandleObstacleContact();
        }
    }
    
    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || obj.GetComponent<PlayerController2>() != null;
    }
    
    private void HandleObstacleContact()
    {
        // Check invincibility period
        if (Time.time - lastHitTime < invincibilityDuration)
        {
            return; // Still in invincibility period
        }
        
        if (playerHealthSystem != null)
        {
            playerHealthSystem.TakeDamage(damageAmount);
            lastHitTime = Time.time;
            Debug.Log($"Player hit obstacle: {gameObject.name}. Health decreased by {damageAmount}.");
        }
        else
        {
            Debug.LogWarning($"Obstacle {gameObject.name} hit player, but HealthSystem not found!");
        }
        
        // Destroy obstacle if configured
        if (destroyOnContact)
        {
            Destroy(gameObject);
        }
    }
}

