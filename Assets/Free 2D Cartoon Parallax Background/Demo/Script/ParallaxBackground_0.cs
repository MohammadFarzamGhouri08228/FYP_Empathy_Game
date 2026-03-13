using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground_0 : MonoBehaviour
{
    // public bool Camera_Move; // Disabled as per request to not use set speed
    // public float Camera_MoveSpeed = 1.5f;
    [Header("Layer Setting")]
    public float[] Layer_Speed = new float[7];
    public GameObject[] Layer_Objects = new GameObject[7];

    [Header("Player Tracking")]
    public Transform playerTransform;
    
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
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform != null)
        {
            previousPlayerPos = playerTransform.position;
        }
        
        // Initialize simulated X to current camera X to prevent jumps
        simulatedParallaxX = _camera.position.x;

        // Initialize bounds for each layer individually
        for (int i = 0; i < 5; i++)
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
        if (playerTransform != null)
        {
            deltaX = playerTransform.position.x - previousPlayerPos.x;
            previousPlayerPos = playerTransform.position;
        }

        // Apply 10% speed boost to the movement for parallax calculation
        // Only horizontal movement affects the parallax (stops on up/down)
        simulatedParallaxX += deltaX * 1.1f;

        // Note: Camera_Move logic removed as requested ("shouldnt move with a set speed")
        
        for (int i = 0; i < 5; i++)
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
