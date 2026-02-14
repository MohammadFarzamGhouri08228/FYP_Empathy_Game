using UnityEngine;
using System.Collections;

public enum LockRetractDirection
{
    Up,     // Top edge anchored – retracts upward
    Down,   // Bottom edge anchored – retracts downward
    Left,   // Left edge anchored – retracts leftward
    Right   // Right edge anchored – retracts rightward
}

/// <summary>
/// A gate/barrier that uses a Tiled sprite and retracts to size 0 when the player
/// has a Key and presses the interact button nearby.
/// One key unlocks ALL Lock instances in the scene.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Lock : MonoBehaviour
{
    [Header("Retract Settings")]
    [SerializeField] private LockRetractDirection retractDirection = LockRetractDirection.Up;

    [Tooltip("How fast the lock retracts (world-units per second).")]
    [SerializeField] private float retractSpeed = 4f;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Tooltip("How close the player must be to interact (measured from anchor edge).")]
    [SerializeField] private float interactRange = 2.5f;

    [Tooltip("Optional child object (text / sprite) shown when the player can interact.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Collider")]
    [SerializeField] private bool autoSizeCollider = true;

    // ---- runtime state ----
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private float currentSize;
    private Vector3 anchorWorld;      // the fixed edge that stays in place
    private bool isRetracting = false;
    private bool isAxisX;             // true = width (Left/Right), false = height (Up/Down)
    private Transform playerTransform;

    /// <summary>Read-only access to this lock's direction (used by the sequencer).</summary>
    public LockRetractDirection RetractDir => retractDirection;

    // ═══════════════════════════════════════════
    //  LIFECYCLE
    // ═══════════════════════════════════════════

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (col == null && autoSizeCollider)
            col = gameObject.AddComponent<BoxCollider2D>();

        isAxisX = (retractDirection == LockRetractDirection.Left ||
                   retractDirection == LockRetractDirection.Right);

        // Record the starting size from the current tiled sprite
        currentSize = isAxisX ? sr.size.x : sr.size.y;

        // Calculate the anchor (the edge that stays fixed)
        anchorWorld = CalculateAnchor();

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        // ---- Retract animation ----
        if (isRetracting)
        {
            currentSize = Mathf.MoveTowards(currentSize, 0f, retractSpeed * Time.deltaTime);
            ApplySize(currentSize);

            if (currentSize <= 0.001f)
                Destroy(gameObject);

            return;
        }

        // ---- Interaction check (distance-based) ----
        if (playerTransform == null) return;

        float dist = Vector2.Distance(anchorWorld, playerTransform.position);
        bool inRange = dist <= interactRange;

        // Show / hide prompt
        if (interactPrompt != null)
            interactPrompt.SetActive(inRange && Key.hasKey);

        // Unlock on key press
        if (inRange && Key.hasKey && Input.GetKeyDown(interactKey))
            UnlockAll();
    }

    // ═══════════════════════════════════════════
    //  ANCHOR CALCULATION
    // ═══════════════════════════════════════════

    private Vector3 CalculateAnchor()
    {
        Vector3 pos = transform.position;

        switch (retractDirection)
        {
            case LockRetractDirection.Up:
                // Anchor = top edge
                return pos + Vector3.up * (sr.size.y * 0.5f);

            case LockRetractDirection.Down:
                // Anchor = bottom edge
                return pos - Vector3.up * (sr.size.y * 0.5f);

            case LockRetractDirection.Left:
                // Anchor = left edge
                return pos - Vector3.right * (sr.size.x * 0.5f);

            case LockRetractDirection.Right:
                // Anchor = right edge
                return pos + Vector3.right * (sr.size.x * 0.5f);

            default:
                return pos;
        }
    }

    // ═══════════════════════════════════════════
    //  SIZE APPLICATION (keeps anchor edge fixed)
    // ═══════════════════════════════════════════

    private void ApplySize(float size)
    {
        // Clamp to a tiny value so the tiled renderer doesn't complain
        float safeSize = Mathf.Max(size, 0.01f);
        Vector2 spriteSize = sr.size;

        if (isAxisX)
        {
            spriteSize.x = safeSize;
            sr.size = spriteSize;

            if (retractDirection == LockRetractDirection.Left)
            {
                // Left anchor fixed → centre = left + halfWidth
                Vector3 pos = anchorWorld + Vector3.right * (size * 0.5f);
                pos.y = transform.position.y;
                pos.z = transform.position.z;
                transform.position = pos;
            }
            else // Right
            {
                // Right anchor fixed → centre = right − halfWidth
                Vector3 pos = anchorWorld - Vector3.right * (size * 0.5f);
                pos.y = transform.position.y;
                pos.z = transform.position.z;
                transform.position = pos;
            }
        }
        else
        {
            spriteSize.y = safeSize;
            sr.size = spriteSize;

            if (retractDirection == LockRetractDirection.Up)
            {
                // Top anchor fixed → centre = top − halfHeight
                Vector3 pos = anchorWorld - Vector3.up * (size * 0.5f);
                pos.x = transform.position.x;
                pos.z = transform.position.z;
                transform.position = pos;
            }
            else // Down
            {
                // Bottom anchor fixed → centre = bottom + halfHeight
                Vector3 pos = anchorWorld + Vector3.up * (size * 0.5f);
                pos.x = transform.position.x;
                pos.z = transform.position.z;
                transform.position = pos;
            }
        }

        // Keep collider in sync
        if (col != null && autoSizeCollider)
        {
            col.size = spriteSize;
            col.offset = Vector2.zero;
        }
    }

    // ═══════════════════════════════════════════
    //  PUBLIC HELPERS
    // ═══════════════════════════════════════════

    /// <summary>Begin retracting this individual lock.</summary>
    public void StartRetracting()
    {
        isRetracting = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // ═══════════════════════════════════════════
    //  UNLOCK ALL LOCKS (sequenced)
    // ═══════════════════════════════════════════

    public static void UnlockAll()
    {
        // Consume the key
        Key.hasKey = false;

        // Remove the key indicator floating above the player
        if (Key.keyIndicatorInstance != null)
        {
            Destroy(Key.keyIndicatorInstance);
            Key.keyIndicatorInstance = null;
        }

        Debug.Log("All locks unlocking (sequenced: Up → Left → Down → Right)");

        // Hide all prompts immediately
        Lock[] allLocks = FindObjectsByType<Lock>(FindObjectsSortMode.None);
        foreach (Lock lk in allLocks)
        {
            if (lk.interactPrompt != null)
                lk.interactPrompt.SetActive(false);
        }

        // Spawn a temporary object to run the sequenced coroutine
        // (it survives even as individual locks get destroyed)
        GameObject runner = new GameObject("_LockSequencer");
        LockSequencer seq = runner.AddComponent<LockSequencer>();
        seq.Run(allLocks);
    }

    // ═══════════════════════════════════════════
    //  GIZMOS (editor helpers)
    // ═══════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        // Interaction range sphere
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.35f);
        Vector3 gizmoCenter = Application.isPlaying ? anchorWorld : transform.position;
        Gizmos.DrawWireSphere(gizmoCenter, interactRange);

        // Retract direction arrow
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Gizmos.color = Color.red;
        Vector3 dir = retractDirection switch
        {
            LockRetractDirection.Up    => Vector3.up,
            LockRetractDirection.Down  => Vector3.down,
            LockRetractDirection.Left  => Vector3.left,
            LockRetractDirection.Right => Vector3.right,
            _ => Vector3.up
        };
        Gizmos.DrawRay(transform.position, dir * 1.5f);
    }
}

// ═════════════════════════════════════════════════════════
//  Helper: runs the sequenced unlock coroutine on a
//  temporary GameObject so it survives lock destruction.
// ═════════════════════════════════════════════════════════

public class LockSequencer : MonoBehaviour
{
    // Order in which direction groups retract
    private static readonly LockRetractDirection[] order =
    {
        LockRetractDirection.Up,
        LockRetractDirection.Left,
        LockRetractDirection.Down,
        LockRetractDirection.Right
    };

    public void Run(Lock[] locks)
    {
        StartCoroutine(UnlockSequence(locks));
    }

    private IEnumerator UnlockSequence(Lock[] locks)
    {
        foreach (LockRetractDirection dir in order)
        {
            // Start retracting every lock that matches this direction
            bool anyInGroup = false;
            foreach (Lock lk in locks)
            {
                if (lk != null && lk.RetractDir == dir)
                {
                    lk.StartRetracting();
                    anyInGroup = true;
                }
            }

            if (!anyInGroup) continue;

            // Wait until every lock in this group has been destroyed
            bool groupDone = false;
            while (!groupDone)
            {
                groupDone = true;
                foreach (Lock lk in locks)
                {
                    if (lk != null && lk.RetractDir == dir)
                    {
                        groupDone = false;
                        break;
                    }
                }
                yield return null;
            }
        }

        // All groups done – clean up the sequencer
        Destroy(gameObject);
    }
}
