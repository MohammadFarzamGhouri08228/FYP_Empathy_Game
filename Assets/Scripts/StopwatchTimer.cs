using UnityEngine;
using TMPro;

public class StopwatchTimer : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI timeText;
    
    [Header("Settings")]
    public bool autoStart = false;
    
    private float currentTime = 0f;
    private bool isRunning = false;
    
    void Start()
    {
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
