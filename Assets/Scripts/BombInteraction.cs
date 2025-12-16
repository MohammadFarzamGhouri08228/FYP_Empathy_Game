using UnityEngine;

public class BombInteraction : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private bool destroyBomb = true; // If false, just disable it
    [SerializeField] private int damageAmount = 1; // How much health to reduce
    
    private HeartManager heartManager;
    private float lastHitTime = 0f;
    [SerializeField] private float invincibilityDuration = 1f; // Prevent multiple hits in quick succession
    
    void Start()
    {
        // Find HeartManager in the scene
        heartManager = FindFirstObjectByType<HeartManager>();
        
        if (heartManager == null)
        {
            Debug.LogWarning("BombInteraction: HeartManager not found in scene! Hearts will not decrease on bomb collision.");
        }
    }
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleBombInteraction();
        }
    }
    
    // Detect regular collisions (for objects without IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleBombInteraction();
        }
    }
    
    private void HandleBombInteraction()
    {
        // Check invincibility period
        if (Time.time - lastHitTime < invincibilityDuration)
        {
            Debug.Log($"BombInteraction: Still in invincibility period ({Time.time - lastHitTime:F2}s / {invincibilityDuration}s)");
            return; // Still in invincibility period
        }
        
        Debug.Log("BombInteraction: Bomb interacted with! Reducing hearts.");
        
        // Trigger bomb encounter event for dialogue system
        Debug.Log($"BombInteraction: About to trigger GameEventManager.TriggerEvent(BombEncountered)");
        GameEventManager.TriggerEvent(GameEventType.BombEncountered, gameObject);
        Debug.Log("BombInteraction: GameEventManager.TriggerEvent called successfully");
        
        // Reduce hearts if HeartManager is found
        if (heartManager != null)
        {
            heartManager.TakeDamage(damageAmount);
            Debug.Log($"Bomb dealt {damageAmount} damage. Current health: {heartManager.currentHealth}/{heartManager.maxHealth}");
        }
        else
        {
            Debug.LogWarning("BombInteraction: Cannot reduce hearts - HeartManager not found!");
        }
        
        // Remove the bomb
        if (destroyBomb)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
        
        lastHitTime = Time.time;
    }
}

