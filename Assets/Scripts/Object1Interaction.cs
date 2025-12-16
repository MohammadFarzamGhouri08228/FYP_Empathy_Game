using UnityEngine;

public class Object1Interaction : MonoBehaviour
{
    private int interactionCount = 0;
    private string myID = "Object1"; 

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleInteraction(other.gameObject);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleInteraction(collision.gameObject);
    }

    private void HandleInteraction(GameObject other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;

            // --- SENDING GENERIC DATA ---
            if (AdaptiveBackend.Instance != null)
            {
                // We send: ID ("Object1"), Label ("interactionCount"), and Value (interactionCount)
                AdaptiveBackend.Instance.ReceiveData(myID, "interactionCount", interactionCount);
            }
            // ----------------------------
            
            Debug.Log($"Object1 count: {interactionCount}");
        }
    }
}






// using UnityEngine;

// public class Object1Interaction : MonoBehaviour
// {
//     private int interactionCount = 0;
    
//     // Detect trigger collisions (for objects with IsTrigger enabled)
//     void OnTriggerEnter2D(Collider2D other)
//     {
//         // Check if the colliding object is the player
//         if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
//         {
//             interactionCount++;
//             Debug.Log($"Object1 interaction count: {interactionCount}");
//         }
//     }
    
//     // Detect regular collisions (for objects without IsTrigger)
//     void OnCollisionEnter2D(Collision2D collision)
//     {
//         // Check if the colliding object is the player
//         if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
//         {
//             interactionCount++;
//             Debug.Log($"Object1 interaction count: {interactionCount}");
//         }
//     }
// }