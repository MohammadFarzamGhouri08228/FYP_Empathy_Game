using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple script to load scenes. Attach this to a button and assign the scene name in the Inspector.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName = "Maze2"; // Name of the scene to load
    
    /// <summary>
    /// Loads the scene specified in sceneName.
    /// Call this method from a button's OnClick event.
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: Scene name is not set! Please assign a scene name in the Inspector.");
            return;
        }
        
        Debug.Log($"SceneLoader: Loading scene '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Loads a scene by name. Useful if you want to load different scenes from different buttons.
    /// </summary>
    /// <param name="name">Name of the scene to load</param>
    public void LoadSceneByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("SceneLoader: Scene name is empty!");
            return;
        }
        
        Debug.Log($"SceneLoader: Loading scene '{name}'");
        SceneManager.LoadScene(name);
    }
}
