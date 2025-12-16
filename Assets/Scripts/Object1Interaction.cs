using UnityEngine;

public class Object1Interaction : MonoBehaviour
{
    private int interactionCount = 0;
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if Player touched us
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            Debug.Log($"Object1 interaction count: {interactionCount}");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if Player hit us
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            interactionCount++;
            Debug.Log($"Object1 interaction count: {interactionCount}");
        }
    }

    private void SendData()
    {
        // Send data directly to the global Agent (AdaptiveBackend)
        // Since AdaptiveBackend is a pure C# singleton, it is always accessible.
        AdaptiveBackend.Instance.ReceiveData(myID, "interactionCount", interactionCount);
    }
}
