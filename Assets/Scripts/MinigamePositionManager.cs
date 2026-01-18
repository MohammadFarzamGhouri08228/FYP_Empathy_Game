using UnityEngine;

/// <summary>
/// Static manager to persist player position across scene transitions for minigame teleportation.
/// </summary>
public static class MinigamePositionManager
{
    private static Vector3 savedPosition;
    private static bool hasSavedPosition = false;
    private static string savedSceneName = "";
    
    /// <summary>
    /// Saves the player's current position and scene name.
    /// </summary>
    /// <param name="position">The player's position to save</param>
    /// <param name="sceneName">The name of the scene the player is currently in</param>
    public static void SavePosition(Vector3 position, string sceneName)
    {
        savedPosition = position;
        savedSceneName = sceneName;
        hasSavedPosition = true;
        Debug.Log($"MinigamePositionManager: Saved position ({position.x:F2}, {position.y:F2}, {position.z:F2}) from scene '{sceneName}'");
    }
    
    /// <summary>
    /// Gets the saved position if available.
    /// </summary>
    /// <param name="position">Output parameter for the saved position</param>
    /// <param name="sceneName">Output parameter for the saved scene name</param>
    /// <returns>True if a position was saved, false otherwise</returns>
    public static bool TryGetSavedPosition(out Vector3 position, out string sceneName)
    {
        if (hasSavedPosition)
        {
            position = savedPosition;
            sceneName = savedSceneName;
            return true;
        }
        
        position = Vector3.zero;
        sceneName = "";
        return false;
    }
    
    /// <summary>
    /// Clears the saved position.
    /// </summary>
    public static void ClearSavedPosition()
    {
        hasSavedPosition = false;
        savedPosition = Vector3.zero;
        savedSceneName = "";
        Debug.Log("MinigamePositionManager: Cleared saved position");
    }
    
    /// <summary>
    /// Checks if there is a saved position.
    /// </summary>
    public static bool HasSavedPosition()
    {
        return hasSavedPosition;
    }
}
