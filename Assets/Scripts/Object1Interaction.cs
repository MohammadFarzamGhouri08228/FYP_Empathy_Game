using UnityEngine;

public class Object1Interaction : MonoBehaviour
{
    [Header("Setup")]
    // Drag your InteractiveObject script here in the Inspector!
    // This is faster and safer than using "FindObject".
    public InteractiveObject receiverScript;

    private int interactionCount = 0;
    private string myID = "Object1"; 

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if Player touched us
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            SendData();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if Player hit us
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            SendData();
        }
    }

    private void SendData()
    {
        // If you forgot to drag the script in the Inspector, we try to find it automatically.
        if (receiverScript == null)
        {
            receiverScript = FindFirstObjectByType<InteractiveObject>();
        }

        if (receiverScript != null)
        {
            // --- THE HANDSHAKE ---
            receiverScript.ReceiveInteractionData(myID, interactionCount);
        }
        else
        {
            Debug.LogError("Error: Object1Interaction cannot find an 'InteractiveObject' to send data to!");
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