using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuStartButton : MonoBehaviour
{
    [Header("Scene Navigation")]
    [Tooltip("Enter the exact name of the scene you want to load.")]
    public string sceneToLoad;

    [Header("Button Visuals")]
    [Tooltip("Slot to place the sprite for the start button.")]
    public Sprite buttonSprite;
    
    [Tooltip("Reference to the Image component of the button. If assigned, the script will apply the sprite above to this image.")]
    public Image buttonImage;

    private void Start()
    {
        // Automatically apply the assigned sprite to the button's Image component when the game starts
        if (buttonSprite != null && buttonImage != null)
        {
            buttonImage.sprite = buttonSprite;
        }
    }

    /// <summary>
    /// Call this method from the Button's OnClick event in the Unity Inspector.
    /// </summary>
    public void OnStartButtonClicked()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Loads the scene with the name specified in the inspector
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene to load is empty! Please enter a scene name in the inspector.");
        }
    }
}
