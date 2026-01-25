using UnityEngine;
using System.Collections;

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

    private void HandleGameEvent(GameEventType eventType, GameObject source)
    {
        Debug.Log($"NPCBehaviour: Received event {eventType}");
        
        // Trigger jump for Checkpoint 1 onwards (ID > 0)
        // Assuming the same jump behavior for all for now. 
        // If different jumps are needed per checkpoint, we'll need a different structure (e.g. valid forces list).
        if (eventType == GameEventType.Checkpoint1Reached ||
            eventType == GameEventType.Checkpoint2Reached ||
            eventType == GameEventType.Checkpoint3Reached ||
            eventType == GameEventType.Checkpoint4Reached ||
            eventType == GameEventType.Checkpoint5Reached)
        {
            Debug.Log($"NPCBehaviour: Checkpoint {eventType} reached! Preparing to jump in {hesitationTime} seconds...");
            StartCoroutine(PerformJumpRoutine());
        }
        else if (eventType == GameEventType.Checkpoint0Reached)
        {
            Debug.Log("NPCBehaviour: Checkpoint 0 reached - ignoring jump as requested.");
        }
    }

    private IEnumerator PerformJumpRoutine()
    {
        Debug.Log($"NPCBehaviour: Starting hesitation routine ({hesitationTime}s)");
        // 1. Hesitate
        yield return new WaitForSeconds(hesitationTime);
        
        // 2. Move Right
        Debug.Log($"NPCBehaviour: Moving {moveDistance} units right at speed {moveSpeed}...");
        
        float startX = transform.position.x;
        float targetX = startX + moveDistance;
        
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
        Jump();
    }

    [ContextMenu("Force Jump Now")]
    public void Jump()
    {
        if (rb != null)
        {
            Debug.Log($"NPCBehaviour: Applying Jump Force {jumpForce} (Mass: {rb.mass})");
            // Using Impulse for immediate jump force (ForceMode2D.Impulse is instant)
            rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }
        else
        {
             Debug.LogError("NPCBehaviour: Rigidbody2D is null! Cannot jump.");
        }
    }
}
