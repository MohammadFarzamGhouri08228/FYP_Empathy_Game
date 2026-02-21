using UnityEngine;
using TMPro;

public class CandyGameManager : MonoBehaviour
{
    public static CandyGameManager Instance;

    public int score = 0;
    public int winScoreThreshold = 10;
    public bool isGameActive = false;
    public bool isGameOver = false;

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
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
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
            if (score > winScoreThreshold)
            {
                gameOverText.text = "You Win!\nScore: " + score;
                gameOverText.color = Color.green;
            }
            else
            {
                gameOverText.text = "Game Over!\nScore: " + score;
                gameOverText.color = Color.red;
            }
        }
    }
}
