using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    
    // Event for when health changes
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public event Action OnHealthDepleted; // When health reaches 0
    
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    
    private StopwatchTimer gameTimer;
    
    
    void Start()
    {
        currentHealth = maxHealth;
        gameTimer = FindFirstObjectByType<StopwatchTimer>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (currentHealth <= 0) return; // Already dead
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Health decreased! Current health: {currentHealth}/{maxHealth}");
        
        // Report to Agent
        float time = gameTimer != null ? gameTimer.GetTime() : 0f;
        AdaptiveBackend.Instance.ReceiveData("HealthSystem", "LifeLost", time);
        
        
        if (currentHealth <= 0)
        {
            OnHealthDepleted?.Invoke();
            Debug.Log("Health depleted! Player has no health left.");
        }
    }
    
    public void Heal(int amount = 1)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Health restored! Current health: {currentHealth}/{maxHealth}");
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Health reset to maximum!");
    }
}

