using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowTarget : MonoBehaviour
{
    [Header("Layer Setting")]
    [Tooltip("If true, layer speeds are calculated automatically based on their order in the list (Element 0 = slowest).")]
    public bool autoCalculateSpeeds = true;
    [Tooltip("Speed multiplier for each layer. 0 = static relative to camera, 1 = moves with player speed + 10%.")]
    public float[] Layer_Speed = new float[7];
    public GameObject[] Layer_Objects = new GameObject[7];

    [Header("Target Settings")]
    public Transform target; // Acts as playerTransform
    
    private Transform _camera;
    private float[] startPos = new float[7];
    private float[] boundSizeX = new float[7];
    private float[] sizeX = new float[7];
    
    private float simulatedParallaxX;
    private Vector3 previousPlayerPos;

    void Start()
    {
        _camera = Camera.main.transform;
        
        // Find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            previousPlayerPos = target.position;
        }
        
        // Initialize simulated X to current camera X to prevent jumps
        simulatedParallaxX = _camera.position.x;

        // Auto-calculate speeds if enabled
        if (autoCalculateSpeeds && Layer_Objects.Length > 0)
        {
            for (int i = 0; i < Layer_Objects.Length; i++)
            {
                // Linearly distribute speeds from ~0.1 (farthest) to 1.0 (closest)
                // Layer 0 is background (minSpeed), Last Layer is foreground (1.0)
                float minSpeed = 0.1f; // Farthest layer moves at 10% of max speed
                
                if (Layer_Objects.Length > 1)
                {
                    float t = (float)i / (Layer_Objects.Length - 1);
                    Layer_Speed[i] = Mathf.Lerp(minSpeed, 1.0f, t);
                }
                else
                {
                    Layer_Speed[i] = 1f; // Single layer moves at full speed
                }
            }
        }

        // Initialize bounds for each layer individually
        for (int i = 0; i < Layer_Objects.Length; i++)
        {
            if (Layer_Objects[i] != null)
            {
                SpriteRenderer spriteRenderer = Layer_Objects[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    sizeX[i] = Layer_Objects[i].transform.localScale.x;
                    boundSizeX[i] = spriteRenderer.sprite.bounds.size.x;
                    startPos[i] = Layer_Objects[i].transform.position.x;
                }
            }
        }
    }

    void Update()
    {
        // Calculate player movement delta
        float deltaX = 0f;
        if (target != null)
        {
            deltaX = target.position.x - previousPlayerPos.x;
            previousPlayerPos = target.position;
        }

        // Apply 10% speed boost to the movement for parallax calculation
        // Only horizontal movement affects the parallax (stops on up/down)
        simulatedParallaxX += deltaX * 1.1f;

        for (int i = 0; i < Layer_Objects.Length; i++)
        {
            if (Layer_Objects[i] == null) continue;
            
            // Calculate parallax offset based on SIMULATED position (Player Speed + 10%)
            float parallaxOffset = simulatedParallaxX * Layer_Speed[i];
            
            // Set the new position
            // We still use _camera.position.y so layers track with camera vertically but don't parallax vertically
            Vector3 newPos = new Vector3(startPos[i] + parallaxOffset, _camera.position.y, Layer_Objects[i].transform.position.z);
            Layer_Objects[i].transform.position = newPos;
            
            // Calculate the wrap distance for this layer
            float wrapDistance = boundSizeX[i] * sizeX[i];
            
            // Calculate how far the layer has moved from camera (using actual positions)
            float distanceFromCamera = Layer_Objects[i].transform.position.x - _camera.position.x;
            
            // Wrap the layer when it goes too far left or right relative to the CAMERA
            if (distanceFromCamera > wrapDistance)
            {
                startPos[i] -= wrapDistance;
                // Re-apply position update immediately to prevent 1-frame gaps
                Layer_Objects[i].transform.position = new Vector3(startPos[i] + parallaxOffset, _camera.position.y, Layer_Objects[i].transform.position.z);
            }
            else if (distanceFromCamera < -wrapDistance)
            {
                startPos[i] += wrapDistance;
                Layer_Objects[i].transform.position = new Vector3(startPos[i] + parallaxOffset, _camera.position.y, Layer_Objects[i].transform.position.z);
            }
        }
    }
}
