using UnityEngine;
using System.Collections.Generic;

public class FireInteraction : MonoBehaviour
{
    [Header("Fire Animation Settings")]
    [Tooltip("OPTION 1: Drag fire sprite GameObjects here. Sprites will be extracted automatically from their SpriteRenderer components.")]
    [SerializeField] private GameObject[] fireSpriteObjects = new GameObject[0];
    
    [Tooltip("OPTION 2: Or directly assign sprite assets here. Click the + button to add elements, then drag sprites from Project window.")]
    [SerializeField] private Sprite[] fireSprites = new Sprite[0]; // Array to store all fire sprites (0 to n)
    
    [SerializeField] private float animationSpeed = 0.1f; // Time between sprite transitions
    
    [Header("Fire Settings")]
    [SerializeField] private int damageAmount = 1; // How much health to reduce
    
    private SpriteRenderer spriteRenderer;
    private int currentSpriteIndex = 0;
    private float animationTimer = 0f;
    
    private HeartManager heartManager;
    private float lastHitTime = 0f;
    [SerializeField] private float invincibilityDuration = 1f; // Prevent multiple hits in quick succession
    
    void Start()
    {
        // Get SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // If SpriteRenderer doesn't exist, try to add one automatically
        if (spriteRenderer == null)
        {
            Debug.LogWarning("FireInteraction: SpriteRenderer component not found! Attempting to add one automatically...");
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
            {
                Debug.LogError("FireInteraction: Failed to add SpriteRenderer component! Animation will not work.");
                return; // Exit early if we can't get/create a SpriteRenderer
            }
            else
            {
                Debug.Log("FireInteraction: SpriteRenderer component added successfully.");
            }
        }
        
        // Extract sprites from GameObjects if provided
        if (fireSpriteObjects != null && fireSpriteObjects.Length > 0)
        {
            ExtractSpritesFromGameObjects();
        }
        
        // Validate fire sprites array
        if (fireSprites == null || fireSprites.Length == 0)
        {
            Debug.LogWarning("FireInteraction: Fire sprites array is empty! Please assign sprites or GameObjects in the Inspector.");
        }
        else if (spriteRenderer != null) // Only set sprite if SpriteRenderer exists
        {
            // Set initial sprite
            spriteRenderer.sprite = fireSprites[0];
            Debug.Log($"FireInteraction: Initialized with {fireSprites.Length} fire sprites.");
        }
        
        // Find HeartManager in the scene
        heartManager = FindFirstObjectByType<HeartManager>();
        
        if (heartManager == null)
        {
            Debug.LogWarning("FireInteraction: HeartManager not found in scene! Hearts will not decrease on fire collision.");
        }
    }
    
    private void ExtractSpritesFromGameObjects()
    {
        // Create sprite array from GameObjects
        List<Sprite> spriteList = new List<Sprite>();
        
        foreach (GameObject obj in fireSpriteObjects)
        {
            if (obj != null)
            {
                SpriteRenderer objRenderer = obj.GetComponent<SpriteRenderer>();
                if (objRenderer != null && objRenderer.sprite != null)
                {
                    spriteList.Add(objRenderer.sprite);
                    Debug.Log($"FireInteraction: Extracted sprite '{objRenderer.sprite.name}' from GameObject '{obj.name}'");
                }
                else
                {
                    Debug.LogWarning($"FireInteraction: GameObject '{obj.name}' does not have a SpriteRenderer with a sprite assigned!");
                }
            }
        }
        
        // Convert list to array and assign to fireSprites
        if (spriteList.Count > 0)
        {
            fireSprites = spriteList.ToArray();
            Debug.Log($"FireInteraction: Successfully extracted {fireSprites.Length} sprites from GameObjects.");
        }
    }
    
    void Update()
    {
        // Animate fire sprites
        AnimateFire();
    }
    
    private void AnimateFire()
    {
        // Check if we have sprites to animate
        if (fireSprites == null || fireSprites.Length == 0 || spriteRenderer == null)
        {
            return;
        }
        
        // Update animation timer
        animationTimer += Time.deltaTime;
        
        // Check if it's time to switch to next sprite
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            
            // Move to next sprite (0 to n, then loop back to 0)
            currentSpriteIndex++;
            
            // Loop back to sprite 0 after reaching the last sprite
            if (currentSpriteIndex >= fireSprites.Length)
            {
                currentSpriteIndex = 0;
            }
            
            // Update sprite
            if (fireSprites[currentSpriteIndex] != null)
            {
                spriteRenderer.sprite = fireSprites[currentSpriteIndex];
            }
        }
    }
    
    // Detect trigger collisions (for objects with IsTrigger enabled)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleFireInteraction();
        }
    }
    
    // Detect regular collisions (for objects without IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleFireInteraction();
        }
    }
    
    // Detect continuous collisions (for objects staying in contact)
    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") || other.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleFireInteraction();
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Check if the colliding object is the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController2>() != null)
        {
            HandleFireInteraction();
        }
    }
    
    private void HandleFireInteraction()
    {
        // Check invincibility period
        if (Time.time - lastHitTime < invincibilityDuration)
        {
            return; // Still in invincibility period
        }
        
        Debug.Log("Fire interacted with! Reducing hearts.");
        
        // Reduce hearts if HeartManager is found
        if (heartManager != null)
        {
            heartManager.TakeDamage(damageAmount);
            Debug.Log($"Fire dealt {damageAmount} damage. Current health: {heartManager.currentHealth}/{heartManager.maxHealth}");
        }
        else
        {
            Debug.LogWarning("FireInteraction: Cannot reduce hearts - HeartManager not found!");
        }
        
        // Note: Fire does NOT disappear - it stays active and continues animating
        lastHitTime = Time.time;
    }
}

