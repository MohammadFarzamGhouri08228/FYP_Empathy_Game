using UnityEngine;
using MagicPigGames; // From the InfinityPBR ProgressBar asset

/// <summary>
/// Displays the running empathy tally using the Horizontal Progress Bar asset.
/// Acts incrementally: positive empathy actions increase the bar,
/// negative empathy actions decrease the bar.
/// </summary>
public class EmpathyMeter : MonoBehaviour
{
    [Header("Meter Asset References")]
    [Tooltip("If you placed the Horizontal Progress Bar in the scene, assign it here.")]
    [SerializeField] private HorizontalProgressBar sceneProgressBar;

    [Tooltip("If the scene bar is empty, the meter will auto-spawn this prefab.")]
    [SerializeField] private HorizontalProgressBar progressBarPrefab;

    [Header("Empathy Tally")]
    [SerializeField][Range(0f, 1f)] private float initialEmpathy = 0.5f;

    [Header("Screen Placement (If Auto-spawned)")]
    [SerializeField] private Vector2 screenOffset = new Vector2(50f, -60f); // Top-center, shifted slightly right
    [SerializeField] private float barScale = 0.5f;

    // The single incremental tally
    private float currentEmpathy;
    private HorizontalProgressBar activeBar;
    private bool uiIsSetup = false;

    // Qualitative tier thresholds
    private static readonly string[] tiers = { "Disconnected", "Developing", "Attentive", "Empathetic", "Deeply Connected" };

    public float CurrentEmpathy => currentEmpathy;

    void Start()
    {
        // Load the global score if it exists, otherwise use initial
        currentEmpathy = PlayerPrefs.GetFloat("GlobalEmpathyScore", initialEmpathy);
        
        SetupMeterUI();
        UpdateMeterVisual();
    }

    private void SetupMeterUI()
    {
        // 1. If assigned in scene, just use it
        if (sceneProgressBar != null)
        {
            activeBar = sceneProgressBar;
            uiIsSetup = true;
            return;
        }

        // 2. Otherwise, spawn the prefab into a new Canvas
        if (progressBarPrefab != null)
        {
            Debug.Log("<color=cyan>[EmpathyMeter]</color> Spawning Horizontal Progress Bar prefab...");
            
            // Create a Screen-Space Overlay canvas to hold it
            GameObject canvasObj = new GameObject("EmpathyMeter_Canvas");
            canvasObj.transform.SetParent(transform);
            Canvas meterCanvas = canvasObj.AddComponent<Canvas>();
            meterCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            meterCanvas.sortingOrder = 100;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Spawn the prefab
            activeBar = Instantiate(progressBarPrefab, canvasObj.transform);
            
            // Bypass stale Unity Inspector values by forcing it here
            Vector2 actualOffset = new Vector2(180f, -60f); // Shifted right

            // Ensure the RectTransform is positioned properly
            RectTransform rt = activeBar.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = actualOffset;
                rt.localScale = new Vector3(barScale, barScale, 1f);
            }

            uiIsSetup = true;
        }
        else
        {
            Debug.LogWarning("<color=red>[EmpathyMeter]</color> No scene progress bar and no prefab assigned! Meter will not display.");
        }
    }

    // ========================================================================
    // PUBLIC API — Called by CheckpointManager
    // ========================================================================

    /// <summary>
    /// Increases the empathy tally by a set amount (simulating pushing "e").
    /// </summary>
    public void AddEmpathy(float amount)
    {
        currentEmpathy = Mathf.Clamp01(currentEmpathy + amount);
        PlayerPrefs.SetFloat("GlobalEmpathyScore", currentEmpathy); // Save globally
        PlayerPrefs.Save();
        
        Debug.Log($"<color=cyan>[EmpathyMeter]</color> Empathy INCREASED by {amount:F2}. New: {currentEmpathy:P0}");
        UpdateMeterVisual();
    }

    /// <summary>
    /// Reduces the empathy tally by a set amount (simulating pushing "q").
    /// </summary>
    public void ReduceEmpathy(float amount)
    {
        currentEmpathy = Mathf.Clamp01(currentEmpathy - amount);
        PlayerPrefs.SetFloat("GlobalEmpathyScore", currentEmpathy); // Save globally
        PlayerPrefs.Save();
        
        Debug.Log($"<color=cyan>[EmpathyMeter]</color> Empathy DECREASED by {amount:F2}. New: {currentEmpathy:P0}");
        UpdateMeterVisual();
    }

    /// <summary>
    /// Gets the qualitative tier name based on the current running tally.
    /// </summary>
    public string GetQualitativeTier()
    {
        if (currentEmpathy < 0.2f) return tiers[0];       // Disconnected
        if (currentEmpathy < 0.4f) return tiers[1];       // Developing
        if (currentEmpathy < 0.6f) return tiers[2];       // Attentive
        if (currentEmpathy < 0.8f) return tiers[3];       // Empathetic
        return tiers[4];                                  // Deeply Connected
    }

    private void UpdateMeterVisual()
    {
        if (!uiIsSetup || activeBar == null) return;
        
        // Use the asset's SetProgress method to trigger its internal visual transitions
        activeBar.SetProgress(currentEmpathy);
    }
}
