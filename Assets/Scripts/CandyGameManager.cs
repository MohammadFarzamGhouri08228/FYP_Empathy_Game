using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CandyGameManager : MonoBehaviour
{
    public static CandyGameManager Instance;

    public int score = 0;
    public int winScoreThreshold = 10;
    public bool isGameActive = false;
    public bool isGameOver = false;

    [Header("Level Transition")]
    public string nextCutsceneName;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI gameOverText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreUI();
        
        if (instructionsText != null)
        {
            instructionsText.text = "Press SPACE to Start!";
            instructionsText.gameObject.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Wait for player to press space to start the game
        if (!isGameActive && !isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        isGameActive = true;
        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(false);
        }
    }

    public void AddScore(int amount)
    {
        if (!isGameActive) return;
        score += amount;
        UpdateScoreUI();
        Debug.Log("Current Score: " + score);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
            Debug.Log("[UI OK] Text updated to: " + score);
        }
        else
        {
            Debug.LogError("[UI BROKEN] scoreText is NULL! Drag your TextMeshPro into the Score Text slot on GameManager.");
        }
    }

    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameActive = false;
        isGameOver = true;
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            // Player won! (Got 7 or more candies)
            if (score >= 7)
            {
                gameOverText.text = "You Win!\nScore: " + score;
                gameOverText.color = Color.green;
                
                // Wait 3 seconds, then load the next cutscene!
                Invoke(nameof(LoadNextScene), 3f);
            }
            else // Player lost! (Got less than 7 candies)
            {
                gameOverText.text = "Game Over!\nScore: " + score + "\nRestarting...";
                gameOverText.color = Color.red;
                
                // Call the RestartLevel function after 2 seconds so the player can read the Game Over text
                Invoke(nameof(RestartLevel), 2f);
            }
        }
        else
        {
            // If the text UI is missing, we still want it to restart or transition
            if (score < 7) Invoke(nameof(RestartLevel), 2f);
            else Invoke(nameof(LoadNextScene), 3f);
        }
    }

    private void RestartLevel()
    {
        // Reloads the exact scene that is currently active
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextCutsceneName))
        {
            SceneManager.LoadScene(nextCutsceneName);
        }
        else
        {
            Debug.LogWarning("Next Cutscene Name is empty! Please type the name of the next scene in the CandyGameManager inspector.");
        }
    }
}
