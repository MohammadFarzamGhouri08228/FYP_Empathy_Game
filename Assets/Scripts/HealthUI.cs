using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Display Settings")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite halfHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private GameObject heartPrefab; // Optional: if you want to use a prefab
    [SerializeField] private Transform heartContainer; // Parent object to hold hearts
    [SerializeField] private Vector2 heartSpacing = new Vector2(50f, 0f); // Space between hearts
    [SerializeField] private Vector2 heartSize = new Vector2(50f, 50f);
    [SerializeField] private Vector2 startPosition = new Vector2(50f, -50f); // Top-left position
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1.2f);
    
    private HealthSystem healthSystem;
    private List<Image> heartImages = new List<Image>();
    private Canvas canvas;
    
    void Start()
    {
        // Find or create canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        // Find or create heart container
        if (heartContainer == null)
        {
            GameObject containerObj = new GameObject("HealthContainer", typeof(RectTransform));
            containerObj.transform.SetParent(canvas.transform, false);
            heartContainer = containerObj.transform;
            
            RectTransform rectTransform = containerObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f); // Top-left
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = startPosition;
        }
        
        // Find HealthSystem on player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController2>()?.gameObject;
        }
        
        if (player != null)
        {
            healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                healthSystem = player.AddComponent<HealthSystem>();
            }
        }
        else
        {
            Debug.LogError("HealthUI: Player not found! Make sure player has 'Player' tag or PlayerController2 component.");
            return;
        }
        
        // Subscribe to health changes
        healthSystem.OnHealthChanged += UpdateHealthDisplay;
        
        // Initialize hearts display
        InitializeHearts();
    }
    
    void InitializeHearts()
    {
        if (healthSystem == null) return;
        
        int maxHealth = healthSystem.MaxHealth;
        
        // Clear existing hearts
        foreach (Image heart in heartImages)
        {
            if (heart != null) Destroy(heart.gameObject);
        }
        heartImages.Clear();
        
        // Create heart images
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(Image));
            heartObj.transform.SetParent(heartContainer, false);
            
            RectTransform rectTransform = heartObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = heartSize;
            rectTransform.anchoredPosition = new Vector2(startPosition.x + (i * heartSpacing.x), startPosition.y - (i * heartSpacing.y));
            
            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = fullHeartSprite != null ? fullHeartSprite : CreateDefaultHeartSprite();
            heartImage.preserveAspect = true;
            
            heartImages.Add(heartImage);
        }
        
        // Update display with current health
        UpdateHealthDisplay(healthSystem.CurrentHealth, healthSystem.MaxHealth);
    }
    
    void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (heartImages.Count != maxHealth)
        {
            InitializeHearts();
            return;
        }
        
        for (int i = 0; i < maxHealth; i++)
        {
            Image heartImage = heartImages[i];
            if (heartImage == null) continue;
            
            // Determine heart state: Full, Half, or Empty
            if (i < currentHealth)
            {
                // Full heart
                heartImage.sprite = fullHeartSprite != null ? fullHeartSprite : CreateDefaultHeartSprite();
                heartImage.color = Color.white;
            }
            else if (i == currentHealth && currentHealth % 1 != 0)
            {
                // Half heart (if health system supports fractional health)
                heartImage.sprite = halfHeartSprite != null ? halfHeartSprite : (fullHeartSprite != null ? fullHeartSprite : CreateDefaultHeartSprite());
                heartImage.color = Color.white;
            }
            else
            {
                // Empty heart
                heartImage.sprite = emptyHeartSprite != null ? emptyHeartSprite : CreateDefaultHeartSprite();
                heartImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Dimmed
            }
            
            // Animate heart change (animate the heart that just changed state)
            if (i == currentHealth - 1 || i == currentHealth)
            {
                StartCoroutine(AnimateHeart(heartImage));
            }
        }
    }
    
    System.Collections.IEnumerator AnimateHeart(Image heartImage)
    {
        if (heartImage == null) yield break;
        
        RectTransform rectTransform = heartImage.GetComponent<RectTransform>();
        Vector3 originalScale = Vector3.one;
        Color originalColor = heartImage.color;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Scale animation
            float scale = scaleCurve.Evaluate(t);
            rectTransform.localScale = originalScale * scale;
            
            // Optional: Fade animation when losing health
            if (heartImage.sprite == emptyHeartSprite || (heartImage.color.a < 1f))
            {
                float alpha = Mathf.Lerp(1f, 0.5f, t);
                heartImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
        heartImage.color = originalColor;
    }
    
    Sprite CreateDefaultHeartSprite()
    {
        // Create a simple colored square as fallback if no sprite is assigned
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Color heartColor = new Color(1f, 0.2f, 0.2f); // Red
        
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = heartColor;
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
}

