using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("The player transform to follow")]
    public Transform target;
    
    [Tooltip("How smoothly the camera catches up (lower is faster)")]
    public float smoothTime = 0.15f;
    
    [Tooltip("The offset from the player (Z should be -10 for 2D)")]
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("Zoom Settings")]
    [Tooltip("Lower value = more zoomed in (e.g., 3-5 is good for 2D)")]
    public float zoomLevel = 5f;

    [Header("Boundary Settings")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = zoomLevel;
        }

        // Automatically try to find the player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Apply zoom level every frame (allows changing it in inspector while playing)
        if (cam != null) cam.orthographicSize = zoomLevel;

        // Calculate where the camera wants to be
        Vector3 targetPosition = target.position + offset;

        // Apply bounds if enabled
        if (useBounds)
        {
            // Calculate half height and width of camera view to prevent showing outside bounds
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float minX = minBounds.x + camWidth;
            float maxX = maxBounds.x - camWidth;
            float minY = minBounds.y + camHeight;
            float maxY = maxBounds.y - camHeight;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        // Smoothly move the camera to that position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Visualize bounds in the editor
    private void OnDrawGizmos()
    {
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
            Gizmos.DrawWireCube(center, size);
        }
    }
}