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

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
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

        // Calculate where the camera wants to be
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera to that position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}