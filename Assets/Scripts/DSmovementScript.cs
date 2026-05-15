using UnityEngine;
using UnityEngine.InputSystem;

public class DSmovementScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private System.Collections.Generic.List<Vector3> visitedCheckpoints = new System.Collections.Generic.List<Vector3>();
    private Vector3 startPosition;

    void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    public void RegisterCheckpoint(Vector3 checkpointPos)
    {
        if (visitedCheckpoints.Count == 0 || visitedCheckpoints[visitedCheckpoints.Count - 1] != checkpointPos)
        {
            visitedCheckpoints.Add(checkpointPos);
            Debug.Log($"DS Registered Checkpoint at {checkpointPos}");
        }
    }

    public void Respawn()
    {
        if (visitedCheckpoints.Count > 0)
        {
            transform.position = visitedCheckpoints[visitedCheckpoints.Count - 1];
            Debug.Log($"DS Respawning at {transform.position}");
        }
        else
        {
            transform.position = startPosition;
            Debug.Log($"DS Respawning at start position {transform.position}");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float moveInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                moveInput = -1f;
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                moveInput = 1f;
        }
        
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}
