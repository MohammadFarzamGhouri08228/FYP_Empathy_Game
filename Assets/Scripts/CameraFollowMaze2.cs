using UnityEngine;

public class CameraFollowMaze2 : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // The player to follow
    [SerializeField] private bool autoFindPlayer = true; // Automatically find player if target is not set
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 0.125f; // How smoothly the camera follows (lower = smoother)
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player (Z should be -10 for 2D)
    
    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false; // Enable to limit camera movement within bounds
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        // If target is not set and auto-find is enabled, try to find the player
        if (target == null && autoFindPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraFollowMaze2: Found player automatically");
            }
            else
            {
                // Try to find by component name
                PlayerController2 playerController = FindObjectOfType<PlayerController2>();
                if (playerController != null)
                {
                    target = playerController.transform;
                    Debug.Log("CameraFollowMaze2: Found player by PlayerController2 component");
                }
                else
                {
                    Debug.LogWarning("CameraFollowMaze2: Player not found! Please assign the target manually in the inspector.");
                }
            }
        }
    }
    
    void LateUpdate()
    {
        // Only follow if target is assigned
        if (target == null)
            return;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Apply bounds if enabled
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;
    }
    
    // Method to set target at runtime (useful if player spawns later)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

