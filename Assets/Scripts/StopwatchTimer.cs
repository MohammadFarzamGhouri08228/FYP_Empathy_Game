using UnityEngine;
using TMPro;

public class StopwatchTimer : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI timeText;
    
    [Header("Settings")]
    public bool autoStart = false;
    
    [Header("Maze2 Intro Control")]
    [Tooltip("If true, timer will wait for Maze2IntroController to complete before auto-starting")]
    public bool waitForIntroComplete = false;
    
    private float currentTime = 0f;
    private bool isRunning = false;
    private Maze2IntroController introController;
    
    void Start()
    {
        // Check if we should wait for intro controller
        if (waitForIntroComplete)
        {
            introController = FindFirstObjectByType<Maze2IntroController>();
            if (introController != null)
            {
                Debug.Log("StopwatchTimer: Waiting for intro sequence to complete...");
                UpdateTimeDisplay();
                return; // Don't auto-start, wait for intro controller to call StartTimer()
            }
            else
            {
                Debug.LogWarning("StopwatchTimer: waitForIntroComplete is true but Maze2IntroController not found!");
            }
        }
        
        if (autoStart)
        {
            StartTimer();
        }
        else
        {
            UpdateTimeDisplay();
        }
    }
    
    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimeDisplay();
        }
    }
    
    public void StartTimer()
    {
        isRunning = true;
    }
    
    public void StopTimer()
    {
        isRunning = false;
    }
    
    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimeDisplay();
    }
    
    public float GetTime()
    {
        return currentTime;
    }
    
    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
