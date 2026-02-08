using UnityEngine;

/// <summary>
/// Attach to a GameObject with a SpriteRenderer set to Draw Mode = Tiled.
/// The bridge expands to the RIGHT when activated and contracts back LEFT when deactivated.
/// The left edge stays anchored in place.
/// 
/// Also adds a BoxCollider2D so the player / NPC can walk on the expanded bridge.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ExpandableBridge : MonoBehaviour
{
    [Header("Bridge Settings")]
    [Tooltip("Maximum width the bridge can reach (in world units).")]
    [SerializeField] private float maxWidth = 10f;

    [Tooltip("Starting width of the bridge (set to your sprite's default tile width).")]
    [SerializeField] private float minWidth = 0.5f;

    [Tooltip("How fast the bridge expands / contracts (units per second).")]
    [SerializeField] private float expandSpeed = 6f;

    [Header("Collider")]
    [Tooltip("If true, automatically sizes a BoxCollider2D to match the bridge.")]
    [SerializeField] private bool autoSizeCollider = true;

    // ---- runtime state ----
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private float currentWidth;
    private float targetWidth;
    private Vector3 leftEdgeWorld;   // world position of the left edge (anchor point)

    // Public read-only access
    public bool IsFullyExpanded => Mathf.Approximately(currentWidth, maxWidth);
    public bool IsFullyContracted => Mathf.Approximately(currentWidth, minWidth);

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (col == null && autoSizeCollider)
            col = gameObject.AddComponent<BoxCollider2D>();

        // Start contracted
        currentWidth = minWidth;
        targetWidth = minWidth;

        // Record the left edge position so we can keep it fixed
        // SpriteRenderer centre is at transform.position, left edge = centre - halfWidth
        leftEdgeWorld = transform.position - Vector3.right * (currentWidth * 0.5f);

        ApplyWidth(currentWidth);
    }

    void Update()
    {
        if (Mathf.Approximately(currentWidth, targetWidth)) return;

        // Move current width toward target
        currentWidth = Mathf.MoveTowards(currentWidth, targetWidth, expandSpeed * Time.deltaTime);
        ApplyWidth(currentWidth);
    }

    /// <summary>Call this to start expanding the bridge.</summary>
    public void Expand()
    {
        targetWidth = maxWidth;
    }

    /// <summary>Call this to start contracting the bridge.</summary>
    public void Contract()
    {
        targetWidth = minWidth;
    }

    /// <summary>Instantly set the bridge to fully expanded.</summary>
    public void SetExpandedImmediate()
    {
        currentWidth = maxWidth;
        targetWidth = maxWidth;
        ApplyWidth(currentWidth);
    }

    /// <summary>Instantly set the bridge to fully contracted.</summary>
    public void SetContractedImmediate()
    {
        currentWidth = minWidth;
        targetWidth = minWidth;
        ApplyWidth(currentWidth);
    }

    // ---- internal ----

    private void ApplyWidth(float width)
    {
        // Update tiled sprite size (x = width, keep y the same)
        Vector2 size = sr.size;
        size.x = width;
        sr.size = size;

        // Reposition so the LEFT edge stays anchored
        // New centre = leftEdge + halfWidth
        Vector3 pos = leftEdgeWorld + Vector3.right * (width * 0.5f);
        pos.y = transform.position.y;
        pos.z = transform.position.z;
        transform.position = pos;

        // Resize collider to match
        if (col != null && autoSizeCollider)
        {
            col.size = new Vector2(width, sr.size.y);
            col.offset = Vector2.zero;
        }
    }

    // ---- Gizmos for easier scene editing ----
    void OnDrawGizmosSelected()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // Draw max-width outline
        Vector3 left = transform.position - Vector3.right * (sr.size.x * 0.5f);
        Vector3 right = left + Vector3.right * maxWidth;
        float h = sr.size.y;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Vector3 centre = (left + right) * 0.5f;
        Gizmos.DrawWireCube(centre, new Vector3(maxWidth, h, 0f));
    }
}
