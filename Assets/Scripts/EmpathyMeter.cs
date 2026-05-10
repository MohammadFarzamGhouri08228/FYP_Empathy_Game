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
    [Tooltip("Drag the Horizontal Progress Bar that is already placed in the Canvas here.")]
    [SerializeField] private HorizontalProgressBar sceneProgressBar;

    [Header("Empathy Tally")]
    [SerializeField][Range(0f, 1f)] private float initialEmpathy = 0.5f;

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
        if (sceneProgressBar != null)
        {
            activeBar = sceneProgressBar;
            uiIsSetup = true;
            Debug.Log("<color=cyan>[EmpathyMeter]</color> Using scene-placed progress bar.");
            return;
        }

        Debug.LogError("<color=red>[EmpathyMeter]</color> Scene Progress Bar is not assigned! " +
            "Drag your Horizontal Progress Bar from the Canvas into the 'Scene Progress Bar' field in the Inspector.");
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
