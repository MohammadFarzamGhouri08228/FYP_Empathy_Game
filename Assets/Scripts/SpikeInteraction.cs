using UnityEngine;

public class SpikeInteraction : MonoBehaviour
{
    [Header("Spike Settings")]
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
            Debug.LogWarning("SpikeInteraction: HeartManager not found in scene! Hearts will not decrease on spike collision.");
        }
    }
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleSpikeInteraction();
        }
    }
    
    // Detect regular collisions (for objects without IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleSpikeInteraction();
        }
    }
    
    private void HandleSpikeInteraction()
    {
        // Check invincibility period
        if (Time.time - lastHitTime < invincibilityDuration)
        {
            return; // Still in invincibility period
        }
        
        Debug.Log("Spike interacted with! Reducing hearts.");
        
        // Reduce hearts if HeartManager is found
        if (heartManager != null)
        {
            heartManager.TakeDamage(damageAmount);
            Debug.Log($"Spike dealt {damageAmount} damage. Current health: {heartManager.currentHealth}/{heartManager.maxHealth}");
        }
        else
        {
            Debug.LogWarning("SpikeInteraction: Cannot reduce hearts - HeartManager not found!");
        }
        
        // Note: Spikes do NOT get destroyed or disabled - they remain active
        // This is the key difference from BombInteraction
        
        lastHitTime = Time.time;
    }
}

