using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset;
    
    [Header("Follow Settings")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = false;
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
        {
            // Try to find the player if target is not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = transform.position;

        if (followX) desiredPosition.x = target.position.x + offset.x;
        if (followY) desiredPosition.y = target.position.y + offset.y;
        if (followZ) desiredPosition.z = target.position.z + offset.z;

        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
