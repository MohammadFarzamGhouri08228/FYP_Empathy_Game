using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("Unique ID for this key (useful if you have multiple keys/doors)")]
    public string keyID = "default";

    [Header("Collection Settings")]
    [Tooltip("Time before key disappears after collection (for animation)")]
    public float collectionDelay = 0.1f;

    // Static counter for total keys collected
    public static int totalKeysCollected = 0;

    private bool isCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected && IsPlayer(other.gameObject))
        {
            CollectKey();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCollected && IsPlayer(collision.gameObject))
        {
            CollectKey();
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") ||
               obj.GetComponent<PlayerController>() != null;
    }

    private void CollectKey()
    {
        if (isCollected) return;

        isCollected = true;
        totalKeysCollected++;

        Debug.Log($"Key '{keyID}' collected! Total keys: {totalKeysCollected}");

        // Destroy the key object
        Destroy(gameObject, collectionDelay);
    }

    // Reset key counter (useful for game restarts)
    public static void ResetKeyCounter()
    {
        totalKeysCollected = 0;
    }

    // Get current key count
    public static int GetKeyCount()
    {
        return totalKeysCollected;
    }
}
