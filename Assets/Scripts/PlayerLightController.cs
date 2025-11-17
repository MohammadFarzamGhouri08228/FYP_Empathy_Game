using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the player's flashlight light with smooth radius transitions.
/// Attach this to the player GameObject along with a Light2D component.
/// </summary>
public class PlayerLightController : MonoBehaviour
{
    [Header("Light Radius Settings")]
    [Tooltip("Minimum light radius (tight mode)")]
    [SerializeField] private float minRadius = 3f;
    
    [Tooltip("Maximum light radius (exploration mode)")]
    [SerializeField] private float maxRadius = 8f;
    
    [Tooltip("Current target radius (will smoothly lerp to this)")]
    [SerializeField] private float currentRadius = 5f;
    
    [Tooltip("Speed at which radius adjusts (units per second)")]
    [SerializeField] private float adjustSpeed = 2f;
    
    [Header("Light Reference")]
    [Tooltip("The Light2D component to control. Auto-assigned if null.")]
    [SerializeField] private Light2D playerLight;
    
    private void Start()
    {
        // Auto-assign Light2D if not set
        if (playerLight == null)
        {
            playerLight = GetComponent<Light2D>();
            
            if (playerLight == null)
            {
                Debug.LogError("PlayerLightController: No Light2D component found! Please add a Light2D component to this GameObject.");
                enabled = false;
                return;
            }
        }
        
        // Initialize current radius to match light's current radius if it's set
        if (playerLight.pointLightOuterRadius > 0)
        {
            currentRadius = playerLight.pointLightOuterRadius;
        }
        else
        {
            // Set initial radius
            currentRadius = Mathf.Clamp(currentRadius, minRadius, maxRadius);
            playerLight.pointLightOuterRadius = currentRadius;
        }
    }
    
    private void Update()
    {
        if (playerLight == null) return;
        
        // Smoothly lerp the light radius towards currentRadius
        float targetRadius = Mathf.Clamp(currentRadius, minRadius, maxRadius);
        float currentLightRadius = playerLight.pointLightOuterRadius;
        
        if (Mathf.Abs(currentLightRadius - targetRadius) > 0.01f)
        {
            float newRadius = Mathf.Lerp(currentLightRadius, targetRadius, adjustSpeed * Time.deltaTime);
            playerLight.pointLightOuterRadius = newRadius;
        }
        else
        {
            playerLight.pointLightOuterRadius = targetRadius;
        }
    }
    
    /// <summary>
    /// Sets the light to exploration mode (larger radius).
    /// </summary>
    public void SetExplorationMode()
    {
        currentRadius = maxRadius;
    }
    
    /// <summary>
    /// Sets the light to tight mode (smaller radius).
    /// </summary>
    public void SetTightMode()
    {
        currentRadius = minRadius;
    }
    
    /// <summary>
    /// Sets a custom radius (clamped between min and max).
    /// </summary>
    /// <param name="radius">Target radius</param>
    public void SetRadius(float radius)
    {
        currentRadius = Mathf.Clamp(radius, minRadius, maxRadius);
    }
    
    /// <summary>
    /// Gets the current target radius.
    /// </summary>
    public float GetCurrentRadius()
    {
        return currentRadius;
    }
}

