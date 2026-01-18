using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles returning from minigame scene to the original level.
/// Attach this to a GameObject in the minigame scene (e.g., a button or trigger).
/// </summary>
public class MinigameReturn : MonoBehaviour
{
    [Header("Return Settings")]
    [SerializeField] private string returnSceneName = "Level2"; // Name of the scene to return to
    
    /// <summary>
    /// Returns to the original level scene.
    /// Call this method when the minigame ends (e.g., from a button click or trigger).
    /// </summary>
    public void ReturnToLevel()
    {
        if (string.IsNullOrEmpty(returnSceneName))
        {
            Debug.LogError("MinigameReturn: Return scene name is not set! Please assign a scene name in the Inspector.");
            return;
        }
        
        Debug.Log($"MinigameReturn: Returning to scene '{returnSceneName}'");
        SceneManager.LoadScene(returnSceneName);
    }
    
    /// <summary>
    /// Returns to a specific scene by name.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void ReturnToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("MinigameReturn: Scene name is empty!");
            return;
        }
        
        Debug.Log($"MinigameReturn: Returning to scene '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }
}
