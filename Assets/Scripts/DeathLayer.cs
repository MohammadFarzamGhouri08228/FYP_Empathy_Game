using UnityEngine;

public class DeathLayer : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private bool reduceLives = false; // Set to true if you still want life reduction
    [SerializeField] private int damageAmount = 2;
    
    private HeartManager heartManager;
    private HealthSystem healthSystem;

    private void FindManagers()
    {
        if (heartManager == null)
            heartManager = FindFirstObjectByType<HeartManager>();

        if (healthSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                healthSystem = player.GetComponent<HealthSystem>();
                if (heartManager == null)
                    heartManager = player.GetComponent<HeartManager>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other.gameObject))
        {
            ApplyDeathEffect(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            ApplyDeathEffect(collision.gameObject);
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || 
               obj.GetComponent<PlayerController>() != null || 
               obj.GetComponent<PlayerMovementController>() != null;
    }

    private void ApplyDeathEffect(GameObject playerObj)
    {
        Debug.Log("Player touches death layer");
        // 1. Teleport to most recent checkpoint
        if (CheckpointManager.Instance != null)
        {
            Debug.Log("[DeathLayer] Player hit death layer. Teleporting to most recent checkpoint.");
            CheckpointManager.Instance.RespawnAtCheckpoint();
        }
        else
        {
            Debug.LogWarning("[DeathLayer] CheckpointManager instance not found! Cannot teleport.");
        }

        // 2. Reduce lives (only if explicitly enabled via inspector)
        if (reduceLives)
        {
            FindManagers();
            bool damageDealt = false;

            if (heartManager != null)
            {
                heartManager.TakeDamage(damageAmount);
                damageDealt = true;
            }
            else if (healthSystem != null)
            {
                healthSystem.TakeDamage(damageAmount);
                damageDealt = true;
            }

            if (!damageDealt)
            {
                Debug.LogWarning("[DeathLayer] Could not find HeartManager or HealthSystem to reduce lives.");
            }
        }
    }
}
