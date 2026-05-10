using UnityEngine;
using MagicPigGames;

public class NPCProgressBarSpawner : MonoBehaviour
{
    [Header("Progress Bar Source")]
    [Tooltip("Drag the Horizontal Progress Bar that is already placed in the Canvas here.")]
    [SerializeField] private HorizontalProgressBar sceneInstance;

    [Header("Positioning")]
    [Tooltip("Offset from the NPC's center (used to follow NPC in world space).")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private HorizontalProgressBar activeBar;
    private Transform containerTransform; // The object we move (either the bar or its canvas)

    void Start()
    {
        if (sceneInstance != null)
        {
            activeBar = sceneInstance;
            containerTransform = activeBar.transform;
            Debug.Log($"[NPCProgressBarSpawner] Using scene-placed progress bar on '{gameObject.name}'.");
        }
        else
        {
            Debug.LogError($"[NPCProgressBarSpawner] No Scene Instance assigned on '{gameObject.name}'! " +
                "Drag the Horizontal Progress Bar from the Canvas into the 'Scene Instance' field.");
        }
    }


    public void ReduceProgress(float amount)
    {
        if (activeBar == null) return;
        
        float current = activeBar.Progress;
        float next = Mathf.Clamp01(current - amount);
        activeBar.SetProgress(next);
        
        Debug.Log($"[NPCProgressBarSpawner] Reduced progress by {amount}. New: {next}");
    }

    void LateUpdate()
    {
        if (containerTransform != null)
        {
            // Make the UI element follow the NPC
            containerTransform.position = transform.position + offset;
        }
    }

    void OnDestroy()
    {
        if (containerTransform != null)
        {
            Destroy(containerTransform.gameObject);
        }
    }

    void OnEnable()
    {
        if (containerTransform != null) containerTransform.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (containerTransform != null) containerTransform.gameObject.SetActive(false);
    }
}
