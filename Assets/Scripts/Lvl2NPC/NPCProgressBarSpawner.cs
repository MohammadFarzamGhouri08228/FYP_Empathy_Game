using UnityEngine;
using MagicPigGames;

public class NPCProgressBarSpawner : MonoBehaviour
{
    [Header("Progress Bar Source")]
    [Tooltip("If assigned, this specific instance in the scene will be moved. If empty, a new one will be spawned from the Prefab below.")]
    [SerializeField] private HorizontalProgressBar sceneInstance;

    [Tooltip("The Horizontal Progress Bar prefab to spawn (ignored if Scene Instance is assigned).")]
    [SerializeField] private HorizontalProgressBar progressBarPrefab;

    [Header("Positioning")]
    [Tooltip("Offset from the NPC's center.")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    [Tooltip("If we create a new World Space Canvas, what scale should it have? Adjust this if the bar is too big/small.")]
    [SerializeField] private float canvasScale = 0.01f;

    private HorizontalProgressBar activeBar;
    private Transform containerTransform; // The object we move (either the bar or its canvas)

    void Start()
    {
        if (sceneInstance != null)
        {
            activeBar = sceneInstance;
            SetupExistingInstance();
        }
        else if (progressBarPrefab != null)
        {
            SpawnProgressBar();
        }
        else
        {
            Debug.LogError("[NPCProgressBarSpawner] No Prefab or Scene Instance assigned!");
        }
    }

    private void SetupExistingInstance()
    {
        // Check if it's already on a World Space Canvas
        Canvas c = activeBar.GetComponentInParent<Canvas>();
        if (c != null && c.renderMode == RenderMode.WorldSpace)
        {
            // Perfect, just follow using the canvas or the bar
            // If the canvas contains other things, we might not want to move the whole canvas.
            // But usually we do.
            // Let's assume we move the bar's root object (which might be the canvas or the bar itself).
            // If the bar is a child of a large static canvas, moving it is tricky without reparenting.
            // For now, let's assume we move the bar itself, but this requires the parent canvas to be World Space.
            
            containerTransform = activeBar.transform; 
            // If we move the RectTransform, it works in World Space canvas.
        }
        else
        {
            // It's on an Overlay canvas or no canvas. We need to hijack it.
            // We'll create a world canvas and reparent it.
            CreateCanvasAndReparent(activeBar.transform);
        }
    }

    private void SpawnProgressBar()
    {
        // Check if the prefab itself has a Canvas that is World Space
        Canvas prefabCanvas = progressBarPrefab.GetComponent<Canvas>();
        bool hasWorldCanvas = prefabCanvas != null && prefabCanvas.renderMode == RenderMode.WorldSpace;

        if (hasWorldCanvas)
        {
            // Instantiate directly
            activeBar = Instantiate(progressBarPrefab, transform.position + offset, Quaternion.identity);
            containerTransform = activeBar.transform;
        }
        else
        {
            // Instantiate first, then reparent
            // Wait, we can't instantiate a UI element without a canvas, it might warn or not show.
            // But we can instantiate it, then immediately reparent.
            // Or create canvas first.
            
            GameObject canvasObj = new GameObject("NPC_ProgressCanvas_" + gameObject.name);
            SetupCanvas(canvasObj);
            
            containerTransform = canvasObj.transform;
            containerTransform.position = transform.position + offset;

            activeBar = Instantiate(progressBarPrefab, containerTransform);
            activeBar.transform.localPosition = Vector3.zero;
        }
    }

    private void CreateCanvasAndReparent(Transform barTransform)
    {
        GameObject canvasObj = new GameObject("NPC_ProgressCanvas_" + gameObject.name);
        SetupCanvas(canvasObj);
        
        containerTransform = canvasObj.transform;
        containerTransform.position = transform.position + offset;
        
        // Reparent the existing bar
        barTransform.SetParent(containerTransform, false);
        barTransform.localPosition = Vector3.zero;
        
        // Ensure it's active
        barTransform.gameObject.SetActive(true);
    }

    private void SetupCanvas(GameObject canvasObj)
    {
        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.sortingOrder = 20; 
        canvasObj.transform.localScale = new Vector3(canvasScale, canvasScale, 1f);
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
