using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCBehaviour : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private Vector2 jumpForce = new Vector2(10f, 15f); // Increased default force
    [SerializeField] private float hesitationTime = 1.5f;

    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Debug.Log($"NPCBehaviour: Initialized. Waiting for GameEvents... (My ID: {gameObject.GetInstanceID()})");
        if (rb == null) 
        {
            Debug.LogError("NPCBehaviour: Rigidbody2D IS MISSING!");
        }
        else
        {
            // ENFORCE PHYSICS SETTINGS
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                Debug.LogWarning($"NPCBehaviour: Rigidbody2D was {rb.bodyType}, changing to Dynamic so it can jump!");
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            // Unfreeze position if frozen
            if ((rb.constraints & RigidbodyConstraints2D.FreezePosition) != 0)
            {
                Debug.LogWarning($"NPCBehaviour: Unfreezing Position constraints!");
                rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Keep rotation frozen typically
            }

            Debug.Log($"NPCBehaviour Physics Check: Mass={rb.mass}, Gravity={rb.gravityScale}, Drag={rb.linearDamping}, Type={rb.bodyType}");
        }
    }

    void OnEnable()
    {
        GameEventManager.OnGameEvent += HandleGameEvent;
    }

    void OnDisable()
    {
        GameEventManager.OnGameEvent -= HandleGameEvent;
    }

    private bool waitingForCheckpoint1Choice = false;
    private bool waitingForCheckpoint2Choice = false;

    void Update()
    {
        if (waitingForCheckpoint1Choice)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    Debug.Log("NPCBehaviour: Checkpoint 1 - Option 1 selected (Move then Jump). Removing prompt.");
                    waitingForCheckpoint1Choice = false; // Only 1 removes the prompt
                    StartCoroutine(PerformJumpRoutine(null, 0.5f)); // Small hesitation before moving
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                {
                    Debug.Log("NPCBehaviour: Checkpoint 1 - Option 2 selected (Jump immediately). Keeping prompt active for more jumps.");
                    // We DO NOT set waitingForCheckpoint1Choice = false here!
                    // This allows the player to keep pressing '2' to jump again until they hit next checkpoint.
                    Jump(); // Jump immediately
                }
            }
        }
        else if (waitingForCheckpoint2Choice)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                {
                    Debug.Log("NPCBehaviour: Checkpoint 2 - Option 1 selected (Move 4 units then Jump). Removing prompt.");
                    waitingForCheckpoint2Choice = false; // 1 always removes the prompt
                    // Optional hesitation (e.g., 1f), then move 4 units right, then jump
                    StartCoroutine(PerformJumpRoutine(new Vector2(6f, 9f), 1f, 4f)); 
                }
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                {
                    Debug.Log("NPCBehaviour: Checkpoint 2 - Option 2 selected (Direct jump without moving). Keeping prompt active for more jumps or '1'.");
                    // Jump immediately without moving (0 units), with a 1 second hesitation. 
                    // We DO NOT set waitingForCheckpoint2Choice to false, so this keeps listening for 1 or 2 again!
                    StartCoroutine(PerformJumpRoutine(new Vector2(6f, 9f), 1f, 0f)); 
                }
            }
        }
    }

    private void HandleGameEvent(GameEventType eventType, GameObject source)
    {
        Debug.Log($"NPCBehaviour: Received event {eventType}");
        
        if (eventType == GameEventType.Checkpoint1Reached)
        {
             Debug.Log($"NPCBehaviour: Checkpoint {eventType} reached! Waiting for player input: Press '1' for Move+Jump, '2' for Immediate Jump.");
             waitingForCheckpoint1Choice = true;
             waitingForCheckpoint2Choice = false;
        }
        else if (eventType == GameEventType.Checkpoint2Reached)
        {
            Debug.Log($"NPCBehaviour: Checkpoint {eventType} reached! Waiting for player input: Press '1' for Move+Jump, '2' for Immediate Jump.");
            waitingForCheckpoint1Choice = false; // Cancel the continuous option 2 jumps
            waitingForCheckpoint2Choice = true;
        }
        else if (eventType == GameEventType.Checkpoint3Reached ||
            eventType == GameEventType.Checkpoint4Reached ||
            eventType == GameEventType.Checkpoint5Reached)
        {
            Debug.Log($"NPCBehaviour: Checkpoint {eventType} reached! Preparing to jump (Default Force) in {hesitationTime} seconds...");
            waitingForCheckpoint1Choice = false; // Cancel the continuous option 2 jumps
            waitingForCheckpoint2Choice = false;
            StartCoroutine(PerformJumpRoutine(null));
        }
        else if (eventType == GameEventType.Checkpoint0Reached)
        {
            Debug.Log("NPCBehaviour: Checkpoint 0 reached - acting as Checkpoint 1 prompt again after a fall.");
            waitingForCheckpoint1Choice = true; // Wait for input again
        }
    }

    private IEnumerator PerformJumpRoutine(Vector2? customForce, float? customHesitation = null, float? customMoveDistance = null)
    {
        float timeToWait = customHesitation ?? hesitationTime;
        Debug.Log($"NPCBehaviour: Starting hesitation routine ({timeToWait}s)");
        // 1. Hesitate
        yield return new WaitForSeconds(timeToWait);
        
        float distanceToMove = customMoveDistance ?? moveDistance;
        // 2. Move Right
        Debug.Log($"NPCBehaviour: Moving {distanceToMove} units right at speed {moveSpeed}...");
        
        float startX = transform.position.x;
        float targetX = startX + distanceToMove;
        
        // Move loop
        while (transform.position.x < targetX)
        {
            // Calculate velocity
            Vector2 newVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            rb.linearVelocity = newVelocity;
            
            yield return new WaitForFixedUpdate();
        }

        // Stop movement
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        Debug.Log("NPCBehaviour: Movement complete. Stopping.");

        // Small pause before jump (optional, but looks better)
        yield return new WaitForSeconds(0.2f);

        // 3. Jump
        Debug.Log("NPCBehaviour: Attempting jump...");
        Jump(customForce);
    }

    [ContextMenu("Force Jump Now")]
    public void Jump(Vector2? overrideForce = null)
    {
        if (rb != null)
        {
            Vector2 forceToUse = overrideForce ?? jumpForce;
            Debug.Log($"NPCBehaviour: Applying Jump Force {forceToUse} (Mass: {rb.mass})");
            // Using Impulse for immediate jump force (ForceMode2D.Impulse is instant)
            rb.AddForce(forceToUse, ForceMode2D.Impulse);
        }
        else
        {
             Debug.LogError("NPCBehaviour: Rigidbody2D is null! Cannot jump.");
        }
    }
}
