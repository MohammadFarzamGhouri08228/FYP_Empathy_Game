using UnityEngine;

public class ladder : MonoBehaviour
{
    [Header("Climbing Status")]
    [SerializeField] private bool showDebugLogs = false;

    [Header("NPC Ladder Settings")]
    [Tooltip("Where the NPC should dismount. Create a child Transform and place it at the desired exit position (e.g. on the platform at the top). If left empty, the NPC exits at the top of the collider.")]
    [SerializeField] private Transform npcExitPoint;

    /// <summary>Read by NPCLadderClimber to know where to dismount.</summary>
    public Transform NPCExitPoint => npcExitPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Something entered ladder trigger: {other.name} on Ladder Object: {gameObject.name}");
        if (IsPlayer(other.gameObject))
        {
            Lvl2movement player = GetPlayerController(other.gameObject);
            
            // Only attach if cooldown is finished
            if (player != null && player.climbCooldown <= 0)
            {
                // Use collider bounds center if available, otherwise transform position
                Collider2D ladderCollider = GetComponent<Collider2D>();
                float targetX = (ladderCollider != null) ? ladderCollider.bounds.center.x : transform.position.x;

                if (showDebugLogs) Debug.Log($"Ladder Position (Transform): {transform.position}, Target X (Bounds): {targetX}");
                
                // Snap player to center of ladder
                Vector3 newPos = player.transform.position;
                newPos.x = targetX;
                player.transform.position = newPos;

                player.isClimbing = true;
                Debug.Log("Player entered ladder: Climbing ON");
            }
            else
            {
                if (showDebugLogs) Debug.LogWarning("Found player tag/component but Lvl2movement is null or cooldown active!");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        
        // Ensure isClimbing remains true while inside the trigger
        if (IsPlayer(other.gameObject))
        {
            Lvl2movement player = GetPlayerController(other.gameObject);
            if (player != null && !player.isClimbing && player.climbCooldown <= 0)
            {
                player.isClimbing = true;
                
                // Optional: Re-snap if needed, but usually once on enter is enough unless they drift
                // Vector3 newPos = player.transform.position;
                // newPos.x = GetComponent<Collider2D>().bounds.center.x;
                // player.transform.position = newPos;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            Lvl2movement player = GetPlayerController(other.gameObject);
            if (player != null)
            {
                player.isClimbing = false;
                if (showDebugLogs) Debug.Log("Player exited ladder: Climbing OFF");
            }
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || obj.GetComponent<Lvl2movement>() != null;
    }

    private Lvl2movement GetPlayerController(GameObject obj)
    {
        Lvl2movement pc = obj.GetComponent<Lvl2movement>();
        if (pc == null)
        {
            pc = obj.GetComponentInParent<Lvl2movement>();
        }
        return pc;
    }
}
