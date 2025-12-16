using UnityEngine;
//using UnityEngine.EventSystems;

public class Object1Interaction : MonoBehaviour
{
    private int interactionCount = 0;
    private AdaptiveBackend adaptiveBackend;

    void Start()
    {
        // 1. Try to find the singleton instance
        if (AdaptiveBackend.Instance != null)
        {
            adaptiveBackend = AdaptiveBackend.Instance;
        }
        else
        {
            // 2. Fallback: Find it in the scene if Instance isn't set yet (though Awake should have run)
            adaptiveBackend = FindObjectOfType<AdaptiveBackend>();
        }

        if (adaptiveBackend == null)
        {
            Debug.LogWarning("[Object1Interaction] AdaptiveBackend not found in the scene. Intelligent features will be disabled.");
        }
        else
        {
            Debug.Log("[Object1Interaction] Successfully connected to AdaptiveBackend.");
        }
    }
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            Debug.Log($"Object1 interaction count: {interactionCount}");
            
            // Report to Intelligent System
            if (adaptiveBackend != null)
            {
                adaptiveBackend.RecordInteraction("Object1", interactionCount);
            }
        }
    }
    
    // Detect regular collisions (for objects without IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            Debug.Log($"Object1 interaction count: {interactionCount}");

            // Report to Intelligent System
            if (adaptiveBackend != null)
            {
                adaptiveBackend.RecordInteraction("Object1", interactionCount);
            }
        }
    }
}

