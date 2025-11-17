using UnityEngine;

/// <summary>
/// Alternative fog-of-war solution using Sprite Mask.
/// Creates a dark overlay with a circular hole that follows the player.
/// Use this if URP 2D lights are not available.
/// </summary>
public class FogOfWarSpriteMask : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("Radius of the visible area around the player")]
    [SerializeField] private float visibleRadius = 5f;
    
    [Tooltip("Color of the fog overlay (dark area)")]
    [SerializeField] private Color fogColor = new Color(0, 0, 0, 0.9f);
    
    [Header("References")]
    [Tooltip("The player transform to follow")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("The sprite renderer for the fog overlay")]
    [SerializeField] private SpriteRenderer fogRenderer;
    
    [Tooltip("The sprite mask component")]
    [SerializeField] private SpriteMask spriteMask;
    
    private Material fogMaterial;
    private static readonly int RadiusProperty = Shader.PropertyToID("_Radius");
    private static readonly int PlayerPosProperty = Shader.PropertyToID("_PlayerPos");
    
    private void Start()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("FogOfWarSpriteMask: Player not found! Assign playerTransform or tag your player as 'Player'.");
            }
        }
        
        SetupFogOverlay();
    }
    
    private void SetupFogOverlay()
    {
        // This script assumes you've set up the fog overlay manually in the scene.
        // See the setup instructions in the documentation.
        
        if (fogRenderer == null)
        {
            fogRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteMask == null)
        {
            spriteMask = GetComponent<SpriteMask>();
        }
    }
    
    private void Update()
    {
        if (playerTransform == null || fogRenderer == null) return;
        
        // Update mask position to follow player
        if (spriteMask != null)
        {
            transform.position = playerTransform.position;
        }
        
        // If using a shader-based approach, update shader properties
        if (fogMaterial != null)
        {
            fogMaterial.SetFloat(RadiusProperty, visibleRadius);
            fogMaterial.SetVector(PlayerPosProperty, playerTransform.position);
        }
    }
    
    /// <summary>
    /// Sets the visible radius around the player.
    /// </summary>
    public void SetVisibleRadius(float radius)
    {
        visibleRadius = Mathf.Max(0.1f, radius);
        
        if (spriteMask != null)
        {
            // Adjust mask scale based on radius
            float scale = visibleRadius * 2f;
            spriteMask.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}

