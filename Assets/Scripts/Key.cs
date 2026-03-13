using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Indicator")]
    [Tooltip("A small sprite prefab to show above the player's head when carrying the key.")]
    [SerializeField] private GameObject keyIndicatorPrefab;

    [Tooltip("Offset from the player's pivot where the indicator floats.")]
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 1.5f, 0f);

    // --- Static state (read by Lock.cs) ---
    public static bool hasKey = false;
    public static GameObject keyIndicatorInstance;

    private bool isCollected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCollected && IsPlayer(other.gameObject))
            CollectKey(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCollected && IsPlayer(collision.gameObject))
            CollectKey(collision.gameObject);
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") ||
               obj.GetComponent<PlayerController>() != null;
    }

    private void CollectKey(GameObject player)
    {
        if (isCollected) return;
        isCollected = true;
        hasKey = true;

        Debug.Log("Key collected! Carry it to a lock and press E to unlock.");

        // Show indicator above player's head
        if (keyIndicatorPrefab != null)
        {
            keyIndicatorInstance = Instantiate(keyIndicatorPrefab, player.transform);
            keyIndicatorInstance.transform.localPosition = indicatorOffset;
        }

        // Remove the pickup from the scene
        Destroy(gameObject);
    }

    /// <summary>Call on scene restart / game over to clear key state.</summary>
    public static void ResetKey()
    {
        hasKey = false;
        if (keyIndicatorInstance != null)
        {
            Destroy(keyIndicatorInstance);
            keyIndicatorInstance = null;
        }
    }
}
