using UnityEngine;

public class ladder : MonoBehaviour
{
    [Header("Climbing Status")]
    [SerializeField] private bool showDebugLogs = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Something entered ladder trigger: {other.name}");
        if (IsPlayer(other.gameObject))
        {
            Lvl2movement player = GetPlayerController(other.gameObject);
            if (player != null)
            {
                player.isClimbing = true;
                Debug.Log("Player entered ladder: Climbing ON");
            }
            else
            {
                Debug.LogWarning("Found player tag/component but Lvl2movement is null!");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Ensure isClimbing remains true while inside the trigger
        if (IsPlayer(other.gameObject))
        {
            Lvl2movement player = GetPlayerController(other.gameObject);
            if (player != null && !player.isClimbing)
            {
                player.isClimbing = true;
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
