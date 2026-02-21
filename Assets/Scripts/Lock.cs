using UnityEngine;
using System.Collections;

public enum LockRetractDirection
{
    Up,     // Top edge anchored – retracts upward
    Down,   // Bottom edge anchored – retracts downward
    Left,   // Left edge anchored – retracts leftward
    Right   // Right edge anchored – retracts rightward
}

public enum LockMode
{
    TiledSprite,    // Resizes SpriteRenderer.size (requires Draw Mode = Tiled)
    ScaleTransform  // Scales transform.localScale (works for Meshes/Prefabs)
}

/// <summary>
/// A gate/barrier that retracts to size 0 when the player has a Key.
/// Can work with Tiled Sprites OR by scaling any object (like a door mesh).
/// One key unlocks ALL Lock instances in the scene in a specific sequence.
[RequireComponent(typeof(SpriteRenderer))]
/// </summary>
public class Lock : MonoBehaviour
{
    [Header("Retract Settings")]
    [SerializeField] private LockMode mode = LockMode.TiledSprite;
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
    [Tooltip("Only used for TiledSprite mode.")]
    [SerializeField] private bool autoSizeCollider = true;

    // ---- runtime state ----
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private float currentSize;  // Current width or height
    private Vector3 anchorWorld; // The fixed edge that stays in place
    private Vector3 initialScale; // Used for ScaleTransform mode
    private bool isRetracting = false;
    private bool isAxisX;       // true = width (Left/Right), false = height (Up/Down)
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

        if (mode == LockMode.TiledSprite)
        {
            if (sr == null) Debug.LogError("Lock set to TiledSprite but no SpriteRenderer found!", this);
            if (col == null && autoSizeCollider)
                col = gameObject.AddComponent<BoxCollider2D>();
        }

        isAxisX = (retractDirection == LockRetractDirection.Left ||
                   retractDirection == LockRetractDirection.Right);

        // Record starting size
        if (mode == LockMode.TiledSprite && sr != null)
        {
            currentSize = isAxisX ? sr.size.x : sr.size.y;
        }
        else // ScaleTransform
        {
            initialScale = transform.localScale;
            // For scaling, we track size in local units relative to initial scale
            // But to keep speed consistent, we'll track the visual world size approximately
            // For simplicity, we'll just track 0..1 percentage for scaling
            currentSize = 1f; // 1.0 = 100% scale
        }

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
            if (mode == LockMode.TiledSprite)
            {
                currentSize = Mathf.MoveTowards(currentSize, 0f, retractSpeed * Time.deltaTime);
                ApplySizeTiled(currentSize);
                if (currentSize <= 0.001f) Destroy(gameObject);
            }
            else // ScaleTransform
            {
                // Retract speed is in units/sec. Convert to scale/sec based on approximate object size.
                // Assuming object is roughly 1 unit big for speed calc, or just use speed as "scales per second"
                float scaleSpeed = retractSpeed * 0.5f; // Adjust multiplier as needed
                currentSize = Mathf.MoveTowards(currentSize, 0f, scaleSpeed * Time.deltaTime);
                ApplySizeScaled(currentSize);
                if (currentSize <= 0.001f) Destroy(gameObject);
            }
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
        Vector3 size = Vector3.one;

        if (mode == LockMode.TiledSprite && sr != null)
        {
            size = new Vector3(sr.size.x, sr.size.y, 0);
        }
        else
        {
            // For meshes, use Renderer bounds or estimation
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) size = r.bounds.size;
        }

        switch (retractDirection)
        {
            case LockRetractDirection.Up:    return pos + Vector3.up * (size.y * 0.5f);
            case LockRetractDirection.Down:  return pos - Vector3.up * (size.y * 0.5f);
            case LockRetractDirection.Left:  return pos - Vector3.right * (size.x * 0.5f);
            case LockRetractDirection.Right: return pos + Vector3.right * (size.x * 0.5f);
            default: return pos;
        }
    }

    // ═══════════════════════════════════════════
    //  SIZE APPLICATION
    // ═══════════════════════════════════════════

    // Original logic for Tiled Sprites
        // Clamp to a tiny value so the tiled renderer doesn't complain
    private void ApplySizeTiled(float size)
    {
        float safeSize = Mathf.Max(size, 0.01f);
        Vector2 spriteSize = sr.size;

        if (isAxisX)
        {
            spriteSize.x = safeSize;
            sr.size = spriteSize;

            Vector3 pos = anchorWorld;
            if (retractDirection == LockRetractDirection.Left)
                pos += Vector3.right * (size * 0.5f);
            else // Right
                pos -= Vector3.right * (size * 0.5f);

            pos.y = transform.position.y;
            pos.z = transform.position.z;
            transform.position = pos;
        }
        else
        {
            spriteSize.y = safeSize;
            sr.size = spriteSize;

            Vector3 pos = anchorWorld;
            if (retractDirection == LockRetractDirection.Up)
                pos -= Vector3.up * (size * 0.5f);
            else // Down
                pos += Vector3.up * (size * 0.5f);

            pos.x = transform.position.x;
        // Keep collider in sync
            pos.z = transform.position.z;
            transform.position = pos;
        }

        if (col != null && autoSizeCollider)
        {
            col.size = spriteSize;
            col.offset = Vector2.zero;
        }
    }

    // New logic for Scaling Prefabs (Door Meshes)
    private void ApplySizeScaled(float scalePercent)
    {
        // 1. Scale the object
        Vector3 newScale = initialScale;
        if (isAxisX) newScale.x *= scalePercent;
        else         newScale.y *= scalePercent;
        
        transform.localScale = newScale;

        // 2. Reposition to keep anchor fixed
        // We need the current world size
        Vector3 currentWorldSize = Vector3.zero;
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null) currentWorldSize = r.bounds.size;

        // If bounds are unreliable (e.g. rotated), we can estimate from scale
        // But let's try a simpler pivot shift approach:
        // Center moves by half the delta size
        // This part is tricky without accurate bounds, so for prefabs we often rely on
        // the pivot being in the center. 
        // A simpler way: just Lerp position from Start -> Anchor based on (1-scale)
        // BUT we need the Anchor to be accurate.

        // Re-calculate position based on Anchor and Current Half-Size
        Vector3 pos = anchorWorld;
        
        if (retractDirection == LockRetractDirection.Up)
            pos -= Vector3.up * (currentWorldSize.y * 0.5f);
        else if (retractDirection == LockRetractDirection.Down)
            pos += Vector3.up * (currentWorldSize.y * 0.5f);
        else if (retractDirection == LockRetractDirection.Left)
            pos += Vector3.right * (currentWorldSize.x * 0.5f);
        else // Right
            pos -= Vector3.right * (currentWorldSize.x * 0.5f);

        // Preserve Z and non-moving axis
        if (isAxisX) pos.y = transform.position.y;
        else         pos.x = transform.position.x;
        
        transform.position = pos;
    /// <summary>Begin retracting this individual lock.</summary>
    }

    // ═══════════════════════════════════════════
    //  PUBLIC HELPERS
    // ═══════════════════════════════════════════

    // ═══════════════════════════════════════════
    //  UNLOCK ALL LOCKS (sequenced)
    // ═══════════════════════════════════════════
        // Consume the key


        // Remove the key indicator floating above the player
    public void StartRetracting()
    {
        isRetracting = true;
        if (interactPrompt != null)
        // Hide all prompts immediately
            interactPrompt.SetActive(false);
    }

    public static void UnlockAll()
    {
        Key.hasKey = false;
        // Spawn a temporary object to run the sequenced coroutine
        // (it survives even as individual locks get destroyed)
        if (Key.keyIndicatorInstance != null)
        {
            Destroy(Key.keyIndicatorInstance);
            Key.keyIndicatorInstance = null;
        }

        Debug.Log("All locks unlocking (sequenced: Up → Left → Down → Right)");

        Lock[] allLocks = FindObjectsByType<Lock>(FindObjectsSortMode.None);
        // Interaction range sphere
        foreach (Lock lk in allLocks)
        {
            if (lk.interactPrompt != null)
        // Retract direction arrow
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

                lk.interactPrompt.SetActive(false);
        }

        GameObject runner = new GameObject("_LockSequencer");
        LockSequencer seq = runner.AddComponent<LockSequencer>();
        seq.Run(allLocks);
    }

    // ═══════════════════════════════════════════
// ═════════════════════════════════════════════════════════
//  Helper: runs the sequenced unlock coroutine on a
//  temporary GameObject so it survives lock destruction.
    // Order in which direction groups retract
// ═════════════════════════════════════════════════════════

    //  GIZMOS
    // ═══════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.35f);
        Vector3 gizmoCenter = Application.isPlaying ? anchorWorld : transform.position;
        Gizmos.DrawWireSphere(gizmoCenter, interactRange);

        Gizmos.color = Color.red;
        Vector3 dir = retractDirection switch
        {
            LockRetractDirection.Up    => Vector3.up,
            // Start retracting every lock that matches this direction
            LockRetractDirection.Down  => Vector3.down,
            LockRetractDirection.Left  => Vector3.left,
            LockRetractDirection.Right => Vector3.right,
            _ => Vector3.up
        };
        Gizmos.DrawRay(transform.position, dir * 1.5f);
    }
}

public class LockSequencer : MonoBehaviour
{
            // Wait until every lock in this group has been destroyed
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

        // All groups done – clean up the sequencer
    {
        foreach (LockRetractDirection dir in order)
        {
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
        Destroy(gameObject);
    }
}
