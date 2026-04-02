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
        else if (IsNPC(other.gameObject))
        {
            ApplyNPCDeathEffect(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.gameObject))
        {
            ApplyDeathEffect(collision.gameObject);
        }
        else if (IsNPC(collision.gameObject))
        {
            ApplyNPCDeathEffect(collision.gameObject);
        }
    }

    private bool IsNPC(GameObject obj)
    {
        return obj.GetComponentInParent<NPCBehaviour>() != null || 
               obj.GetComponentInParent<MimicNPC>() != null || 
               obj.CompareTag("NPC");
    }

    private void ApplyNPCDeathEffect(GameObject npcObj)
    {
        Debug.Log("[DeathLayer] NPC hit death layer. Teleporting to most recent checkpoint.");
        
        // Ensure we operate on the root NPC GameObject if the collider is on a child
        NPCBehaviour behaviour = npcObj.GetComponentInParent<NPCBehaviour>();
        if (behaviour != null) npcObj = behaviour.gameObject;
        else 
        {
            MimicNPC mimic = npcObj.GetComponentInParent<MimicNPC>();
            if (mimic != null) npcObj = mimic.gameObject;
        }

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnAtCheckpoint(npcObj);
            
            Rigidbody2D rb = npcObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        else
        {
            Debug.LogWarning("[DeathLayer] CheckpointManager instance not found! Cannot teleport NPC.");
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag("Player") || 
               obj.GetComponent<PlayerController>() != null || 
               obj.GetComponent<PlayerMovementController>() != null ||
               obj.GetComponent<Lvl2movement>() != null ||
               obj.GetComponent<DSmovementScript>() != null;
    }

    private void ApplyDeathEffect(GameObject playerObj)
    {
        Debug.Log("Player touches death layer");

        // Check if it is the DS (Distorted Self)
        DSmovementScript dsScript = playerObj.GetComponent<DSmovementScript>();
        if (dsScript != null)
        {
            Debug.Log("[DeathLayer] DS hit death layer. Teleporting to its most recent checkpoint.");
            dsScript.Respawn();
            return; // DS doesn't use the global checkpoint manager or health system
        }

        // 1. Teleport to most recent checkpoint
        if (CheckpointManager.Instance != null)
        {
            Debug.Log("[DeathLayer] Player hit death layer. Teleporting to most recent checkpoint.");
            CheckpointManager.Instance.RespawnAtCheckpoint(playerObj);
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
