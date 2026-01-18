using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 90f;
    
    [Header("Coin Collection")]
    [Tooltip("Value added to coin count when collected (editable in Unity)")]
    public int coinValue = 1;
    
    [Header("Collection Settings")]
    [Tooltip("Time before coin disappears after collection (for animation)")]
    public float collectionDelay = 0.1f;
    
    // Static counter for total coins collected (accessible from anywhere)
    public static int totalCoinsCollected = 0;
    
    private bool isCollected = false;
    
    void Update()
    {
        // Rotate the coin continuously
        if (!isCollected)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if Player touched the coin
        if (!isCollected && (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null))
        {
            CollectCoin();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if Player hit the coin
        if (!isCollected && (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController>() != null))
        {
            CollectCoin();
        }
    }
    
    private void CollectCoin()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Add to total coins collected
        totalCoinsCollected += coinValue;
        
        Debug.Log($"Coin collected! Total coins: {totalCoinsCollected}");
        
        // Optional: Send data to backend if needed
        // AdaptiveBackend.Instance.ReceiveData("CoinManager", "coinsCollected", totalCoinsCollected);
        
        // Disable the coin (make it disappear)
        // You can also use Destroy(gameObject, collectionDelay) if you want a delay
        gameObject.SetActive(false);
    }
    
    // Public method to reset the coin counter (useful for game restarts)
    public static void ResetCoinCounter()
    {
        totalCoinsCollected = 0;
    }
    
    // Public method to get the current coin count
    public static int GetCoinCount()
    {
        return totalCoinsCollected;
    }
}

