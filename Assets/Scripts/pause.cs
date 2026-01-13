using UnityEngine;
using UnityEngine.SceneManagement;

public class pause : MonoBehaviour
{

    [SerializeField] GameObject pauseMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Stop the game time
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Stop the game time
    }
    public void Home(int sceneID)
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Stop the game time
        SceneManager.LoadScene(sceneID); // Load the scene with the given ID    
    }
}