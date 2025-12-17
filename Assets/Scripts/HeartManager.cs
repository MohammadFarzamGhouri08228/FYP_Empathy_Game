
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class HeartManager : MonoBehaviour
{
    public int maxHealth = 4; 
    public int currentHealth;

    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public SpriteRenderer[] hearts; // size = 2 in inspector
    
    private StopwatchTimer gameTimer; // Reference to timer
    
    // Events for health changes (similar to HealthSystem)
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnHealthDepleted; // When health reaches 0

    void Start()
    {
        currentHealth = maxHealth;
        
        // Validate hearts array
        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogError("HeartManager: Hearts array is not assigned or empty! Please assign SpriteRenderer components in the Inspector.");
            return;
        }
        
        // Validate sprites
        if (fullHeart == null || halfHeart == null || emptyHeart == null)
        {
            Debug.LogError("HeartManager: Heart sprites are not assigned! Please assign Full Heart, Half Heart, and Empty Heart sprites in the Inspector.");
            return;
        }
        
        // Ensure all SpriteRenderers are enabled and visible
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].enabled = true;
                hearts[i].sortingOrder = 100; // Ensure hearts render on top
                //hearts[i].sortingLayerName = "Default"; // Make sure they're on a visible layer
                
                // Ensure hearts are at proper Z position (same as camera or slightly in front)
                Vector3 pos = hearts[i].transform.position;
                hearts[i].transform.position = new Vector3(pos.x, pos.y, 0f);
            }
            else
            {
                Debug.LogWarning($"HeartManager: Heart {i} SpriteRenderer is null!");
            }
        }
        
        UpdateHearts();
        
        // Find StopwatchTimer
        gameTimer = FindFirstObjectByType<StopwatchTimer>();
        
        // Invoke initial health event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        // TEMP DAMAGE TEST — press X to lose 1 health
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int dmg)
    {
        int previousHealth = currentHealth;
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();
        
        // Invoke health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Report to Agent if damage was taken
        if (dmg > 0)
        {
            float time = gameTimer != null ? gameTimer.GetTime() : 0f;
            AdaptiveBackend.Instance.ReceiveData("HeartManager", "LifeLost", time);
        }
        
        // Check if health depleted
        if (currentHealth <= 0)
        {
            OnHealthDepleted?.Invoke();
        }
    }

    public void UpdateHearts()
    {
        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogWarning("HeartManager: Cannot update hearts - array is null or empty!");
            return;
        }
        
        if (fullHeart == null || halfHeart == null || emptyHeart == null)
        {
            Debug.LogWarning("HeartManager: Cannot update hearts - sprites are not assigned!");
            return;
        }
        
        int health = currentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
            {
                Debug.LogWarning($"HeartManager: Heart {i} is null! Skipping...");
                continue;
            }
            
            // Ensure SpriteRenderer is enabled
            if (!hearts[i].enabled)
            {
                hearts[i].enabled = true;
            }
            
            // Update sprite based on health
            if (health >= 2)
            {
                hearts[i].sprite = fullHeart;
                health -= 2;
            }
            else if (health == 1)
            {
                hearts[i].sprite = halfHeart;
                health -= 1;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
            
            // Ensure sprite is assigned
            if (hearts[i].sprite == null)
            {
                Debug.LogWarning($"HeartManager: Heart {i} sprite is null after assignment!");
            }
        }
        
        Debug.Log($"HeartManager: Updated hearts display. Current health: {currentHealth}/{maxHealth}");
    }
}