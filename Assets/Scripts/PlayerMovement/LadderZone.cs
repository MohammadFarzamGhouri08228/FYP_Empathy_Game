/*
    LadderZone.cs  –  Ladder trigger detection
    ───────────────────────────────────────────
    Place this component on every Ladder GameObject.
    The GameObject must have a Collider2D set to "Is Trigger".

    This script only tells PlayerClimb that the player is
    near a ladder. The actual grab/release is handled by
    the player pressing Space (see PlayerClimb).
*/

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LadderZone : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // ═══════════════════════════════════════════
    //  Trigger Callbacks
    // ═══════════════════════════════════════════

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerClimb climb = GetPlayerClimb(other);
        if (climb == null) return;

        Bounds bounds = GetLadderBounds();
        climb.SetNearLadder(bounds.center.x, bounds.min.y, bounds.max.y);

        if (showDebugLogs)
            Debug.Log($"[LadderZone] {other.name} near ladder '{gameObject.name}'");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerClimb climb = GetPlayerClimb(other);
        if (climb == null) return;

        // Keep the near-ladder state fresh (in case Enter was missed)
        Bounds bounds = GetLadderBounds();
        climb.SetNearLadder(bounds.center.x, bounds.min.y, bounds.max.y);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerClimb climb = GetPlayerClimb(other);
        if (climb == null) return;

        // Only clear the "near" flag – does NOT force the player off the ladder.
        // The player lets go by pressing Space.
        climb.ClearNearLadder();

        if (showDebugLogs)
            Debug.Log($"[LadderZone] {other.name} left ladder zone '{gameObject.name}'");
    }

    // ═══════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════

    /// <summary>
    /// Try to get the PlayerClimb component from the colliding object.
    /// Checks the "Player" tag first, then falls back to component search.
    /// </summary>
    private PlayerClimb GetPlayerClimb(Collider2D col)
    {
        // Quick reject: not the player
        if (!col.CompareTag("Player") && col.GetComponent<PlayerMotor>() == null)
            return null;

        PlayerClimb c = col.GetComponent<PlayerClimb>();
        if (c == null)
            c = col.GetComponentInParent<PlayerClimb>();

        return c;
    }

    /// <summary>Returns the world-space bounds of this ladder's collider.</summary>
    private Bounds GetLadderBounds()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            return col.bounds;

        // Fallback: a small bounds around the transform
        return new Bounds(transform.position, Vector3.one);
    }
}
