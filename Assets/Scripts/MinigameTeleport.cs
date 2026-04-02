using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles teleportation to minigame scene and back to the original position.
/// </summary>
public class MinigameTeleport : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string minigameSceneName = "Background"; // Name of the minigame scene
    [SerializeField] private string returnSceneName = "Level2"; // Name of the scene to return to
    
    [Header("Interaction Settings")]
    [SerializeField] private bool canInteractMultipleTimes = false; // If false, deactivates after first use
    [SerializeField] private bool isActive = true; // Whether this portal is currently active
    
    private bool hasBeenUsed = false;
    private GameObject player;
    
    void Start()
    {
        // Check if we're returning from minigame and restore player position
        // Use coroutine to wait for scene to fully load
        StartCoroutine(RestorePlayerPositionIfNeededCoroutine());
    }
    
    /// <summary>
    /// Detects when player enters the trigger zone.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        // Check if Player touched us
        if (IsPlayer(other.gameObject))
        {
            HandlePlayerInteraction(other.gameObject);
        }
    }
    
    /// <summary>
    /// Detects when player collides with the portal.
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;
        
        // Check if Player hit us
        if (IsPlayer(collision.gameObject))
        {
            HandlePlayerInteraction(collision.gameObject);
        }
    }
    
    /// <summary>
    /// Checks if the given GameObject is the player.
    /// </summary>
    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || 
               obj.GetComponent<PlayerController2>() != null ||
               obj.GetComponent<PlayerController>() != null;
    }
    
    /// <summary>
    /// Handles the player interaction with the portal.
    /// </summary>
    private void HandlePlayerInteraction(GameObject playerObj)
    {
        // Prevent multiple uses if not allowed
        if (hasBeenUsed && !canInteractMultipleTimes)
        {
            Debug.Log("MinigameTeleport: Portal already used and multiple uses not allowed.");
            return;
        }
        
        player = playerObj;
        
        // Save player's current position before teleporting
        Vector3 playerPosition = player.transform.position;
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        MinigamePositionManager.SavePosition(playerPosition, currentSceneName);
        Debug.Log($"MinigameTeleport: Saved player position ({playerPosition.x:F2}, {playerPosition.y:F2}, {playerPosition.z:F2}) from scene '{currentSceneName}'");
        
        // Load the minigame scene
        Debug.Log($"MinigameTeleport: Loading minigame scene '{minigameSceneName}'");
        SceneManager.LoadScene(minigameSceneName);
        
        // Mark as used
        hasBeenUsed = true;
        
        // Deactivate if multiple uses not allowed
        if (!canInteractMultipleTimes)
        {
            isActive = false;
            Debug.Log("MinigameTeleport: Portal deactivated after use.");
        }
    }
    
    /// <summary>
    /// Coroutine to restore player position if returning from minigame.
    /// Waits for scene to fully load before attempting restoration.
    /// </summary>
    private IEnumerator RestorePlayerPositionIfNeededCoroutine()
    {
        // Wait a frame to ensure scene is fully loaded
        yield return null;
        
        // Check if we have a saved position and we're in the return scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (currentSceneName == returnSceneName && MinigamePositionManager.HasSavedPosition())
        {
            Vector3 savedPosition;
            string savedSceneName;
            
            if (MinigamePositionManager.TryGetSavedPosition(out savedPosition, out savedSceneName))
            {
                // Try to find player with retries (in case it takes a moment to spawn)
                GameObject playerObj = null;
                int maxRetries = 10;
                int retryCount = 0;
                
                while (playerObj == null && retryCount < maxRetries)
                {
                    // Find player in the scene
                    playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj == null)
                    {
                        // Try alternative methods to find player
                        PlayerController2 playerController2 = FindFirstObjectByType<PlayerController2>();
                        if (playerController2 != null)
                        {
                            playerObj = playerController2.gameObject;
                        }
                        else
                        {
                            PlayerController playerController = FindFirstObjectByType<PlayerController>();
                            if (playerController != null)
                            {
                                playerObj = playerController.gameObject;
                            }
                        }
                    }
                    
                    if (playerObj == null)
                    {
                        retryCount++;
                        yield return new WaitForSeconds(0.1f); // Wait 0.1 seconds before retrying
                    }
                }
                
                if (playerObj != null)
                {
                    // Restore player position
                    playerObj.transform.position = savedPosition;
                    Debug.Log($"MinigameTeleport: Restored player position to ({savedPosition.x:F2}, {savedPosition.y:F2}, {savedPosition.z:F2})");
                    
                    // Reset velocity if Rigidbody2D exists
                    Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        Debug.Log("MinigameTeleport: Reset player velocity.");
                    }
                    
                    // Clear saved position
                    MinigamePositionManager.ClearSavedPosition();
                }
                else
                {
                    Debug.LogWarning("MinigameTeleport: Player not found when trying to restore position after retries!");
                }
            }
        }
    }
}
