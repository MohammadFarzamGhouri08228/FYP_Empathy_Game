/*
    LadderZone.cs  –  Ladder trigger detection
    ───────────────────────────────────────────
    Place this component on every Ladder GameObject.
    The GameObject must have a Collider2D set to "Is Trigger".

    When the player enters the trigger, this script tells
    PlayerClimb to start climbing. When the player exits, it
    tells PlayerClimb to stop.
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

        float centerX = GetLadderCenterX();
        climb.EnterLadder(centerX);

        if (showDebugLogs)
            Debug.Log($"[LadderZone] {other.name} entered ladder '{gameObject.name}'");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerClimb climb = GetPlayerClimb(other);
        if (climb == null) return;

        // Re-attach if the player left the ladder (e.g. pressed Space)
        // but is still inside the trigger and cooldown has expired
        if (!climb.IsClimbing && climb.CooldownRemaining <= 0f)
        {
            float centerX = GetLadderCenterX();
            climb.EnterLadder(centerX);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerClimb climb = GetPlayerClimb(other);
        if (climb == null) return;

        climb.ExitLadder();

        if (showDebugLogs)
            Debug.Log($"[LadderZone] {other.name} exited ladder '{gameObject.name}'");
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

    /// <summary>Returns the world-space X center of this ladder's collider.</summary>
    private float GetLadderCenterX()
    {
        Collider2D col = GetComponent<Collider2D>();
        return col != null ? col.bounds.center.x : transform.position.x;
    }
}
