using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the composite empathy score as a visual meter on screen.
/// Driven programmatically by CheckpointManager — NOT by arrow keys.
/// 
/// AUTO-CREATES its own UI if no references are assigned in the Inspector.
/// Just add this component to any GameObject (or let CheckpointManager auto-create it)
/// and the meter will appear on screen automatically.
/// 
/// The meter combines three sub-scores:
///   1. Listening Rate      — Did the player stop moving to listen to NPC dialogue?
///   2. Clarification Score  — Did the player choose visual/concrete (DS-friendly) response options?
///   3. Reinforcement Score  — Did the player validate the NPC's feelings after errors?
/// </summary>
public class EmpathyMeter : MonoBehaviour
{
    [Header("UI References (auto-created if left empty)")]
    [Tooltip("The fill bar Image (Image Type = Filled, Fill Method = Horizontal)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Background/border Image for the meter frame")]
    [SerializeField] private Image borderImage;

    [Tooltip("Label showing the current score percentage")]
    [SerializeField] private TMP_Text scoreLabel;

    [Tooltip("Label showing a qualitative description")]
    [SerializeField] private TMP_Text qualitativeLabel;

    [Tooltip("Label for the meter title")]
    [SerializeField] private TMP_Text titleLabel;

    [Header("Appearance")]
    [Tooltip("Fill color gradient from low empathy (left) to high empathy (right)")]
    [SerializeField] private Gradient fillGradient;

    [Tooltip("How fast the meter bar animates to its target")]
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Meter Size & Position")]
    [SerializeField] private float meterWidth = 220f;
    [SerializeField] private float meterHeight = 28f;
    [Tooltip("Offset from top-right corner (negative X = left, negative Y = down)")]
    [SerializeField] private Vector2 screenOffset = new Vector2(-30f, -60f);

    [Header("Metric Weights (should sum to 1.0)")]
    [SerializeField][Range(0f, 1f)] private float listeningWeight = 0.4f;
    [SerializeField][Range(0f, 1f)] private float clarificationWeight = 0.3f;
    [SerializeField][Range(0f, 1f)] private float reinforcementWeight = 0.3f;

    [Header("Initial Values")]
    [SerializeField][Range(0f, 1f)] private float initialFill = 0.5f;

    // Current sub-scores (0.0 to 1.0 range)
    private float listeningScore;
    private float clarificationScore;
    private float reinforcementScore;

    // Internal display state
    private float targetFill;
    private float displayedFill;
    private bool hasReceivedAnyScore = false;

    // Qualitative tier thresholds
    private static readonly string[] tiers = { "Disconnected", "Developing", "Attentive", "Empathetic", "Deeply Connected" };

    // Auto-created canvas reference
    private Canvas meterCanvas;

    /// <summary>
    /// The current composite empathy score (0.0 to 1.0).
    /// </summary>
    public float CompositeScore
    {
        get
        {
            if (!hasReceivedAnyScore) return initialFill;
            return Mathf.Clamp01(
                listeningWeight * listeningScore +
                clarificationWeight * clarificationScore +
                reinforcementWeight * reinforcementScore
            );
        }
    }

    void Awake()
    {
        SetupGradient();
    }

    void Start()
    {
        // Auto-create UI if fill image isn't assigned
        if (fillImage == null)
        {
            CreateMeterUI();
        }

        // Initialize display
        listeningScore = initialFill;
        clarificationScore = initialFill;
        reinforcementScore = initialFill;
        displayedFill = initialFill;
        targetFill = initialFill;

        UpdateVisuals(true);
        Debug.Log("<color=cyan>[EmpathyMeter]</color> Initialized. Meter is visible on screen.");
    }

    void Update()
    {
        targetFill = CompositeScore;

        // Smooth lerp towards target
        if (!Mathf.Approximately(displayedFill, targetFill))
        {
            displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * smoothSpeed);

            if (Mathf.Abs(displayedFill - targetFill) < 0.001f)
            {
                displayedFill = targetFill;
            }

            UpdateVisuals(false);
        }
    }

    // ========================================================================
    // AUTO-CREATE UI — No Unity Editor setup needed!
    // ========================================================================

    private void CreateMeterUI()
    {
        Debug.Log("<color=cyan>[EmpathyMeter]</color> No UI references assigned — auto-creating meter UI...");

        // ── Create dedicated Canvas (Screen Space Overlay, renders on top of everything) ──
        GameObject canvasObj = new GameObject("EmpathyMeter_Canvas");
        canvasObj.transform.SetParent(transform);
        meterCanvas = canvasObj.AddComponent<Canvas>();
        meterCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        meterCanvas.sortingOrder = 100; // Render above everything

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Container panel (anchored to top-right) ──
        GameObject container = CreateUIObject("MeterContainer", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 1); // Top-right
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(1, 1);
        containerRect.anchoredPosition = screenOffset;
        containerRect.sizeDelta = new Vector2(meterWidth, meterHeight + 40f); // Extra height for labels

        // ── Title label ("Empathy") ──
        GameObject titleObj = CreateUIObject("TitleLabel", container.transform);
        titleLabel = titleObj.AddComponent<TextMeshProUGUI>();
        titleLabel.text = "Empathy";
        titleLabel.fontSize = 14f;
        titleLabel.fontStyle = FontStyles.Bold;
        titleLabel.color = new Color(0.9f, 0.9f, 0.95f, 0.9f);
        titleLabel.alignment = TextAlignmentOptions.Left;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(0.7f, 1);
        titleRect.pivot = new Vector2(0, 1);
        titleRect.anchoredPosition = new Vector2(0, 0);
        titleRect.sizeDelta = new Vector2(0, 18f);

        // ── Score label (percentage, top-right of bar) ──
        GameObject scoreLabelObj = CreateUIObject("ScoreLabel", container.transform);
        scoreLabel = scoreLabelObj.AddComponent<TextMeshProUGUI>();
        scoreLabel.text = "50%";
        scoreLabel.fontSize = 13f;
        scoreLabel.color = new Color(0.85f, 0.85f, 0.9f, 0.85f);
        scoreLabel.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreLabelObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.7f, 1);
        scoreRect.anchorMax = new Vector2(1, 1);
        scoreRect.pivot = new Vector2(1, 1);
        scoreRect.anchoredPosition = new Vector2(0, 0);
        scoreRect.sizeDelta = new Vector2(0, 18f);

        // ── Background (dark rounded bar) ──
        GameObject bgObj = CreateUIObject("MeterBackground", container.transform);
        borderImage = bgObj.AddComponent<Image>();
        borderImage.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.anchoredPosition = new Vector2(0, 0);
        bgRect.sizeDelta = new Vector2(0, -20f); // Leave room for labels above
        // Add slight outline effect
        Outline bgOutline = bgObj.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
        bgOutline.effectDistance = new Vector2(1, 1);

        // ── Fill bar ──
        GameObject fillObj = CreateUIObject("MeterFill", bgObj.transform);
        fillImage = fillObj.AddComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = initialFill;
        fillImage.color = Color.green; // Will be overridden by gradient
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = new Vector2(3, 3); // Inner padding
        fillRect.offsetMax = new Vector2(-3, -3);

        // ── Tier label (below the bar) ──
        GameObject tierObj = CreateUIObject("TierLabel", container.transform);
        qualitativeLabel = tierObj.AddComponent<TextMeshProUGUI>();
        qualitativeLabel.text = "Attentive";
        qualitativeLabel.fontSize = 11f;
        qualitativeLabel.fontStyle = FontStyles.Italic;
        qualitativeLabel.color = new Color(0.7f, 0.75f, 0.8f, 0.7f);
        qualitativeLabel.alignment = TextAlignmentOptions.Center;
        RectTransform tierRect = tierObj.GetComponent<RectTransform>();
        tierRect.anchorMin = new Vector2(0, 0);
        tierRect.anchorMax = new Vector2(1, 0);
        tierRect.pivot = new Vector2(0.5f, 1);
        tierRect.anchoredPosition = new Vector2(0, -2f);
        tierRect.sizeDelta = new Vector2(0, 16f);

        Debug.Log("<color=cyan>[EmpathyMeter]</color> UI auto-created successfully.");
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    // ========================================================================
    // GRADIENT SETUP
    // ========================================================================

    private void SetupGradient()
    {
        if (fillGradient == null || fillGradient.colorKeys.Length <= 2)
        {
            fillGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.85f, 0.2f, 0.2f),  0f),     // Red at 0%
                new GradientColorKey(new Color(0.90f, 0.55f, 0.15f), 0.25f), // Orange at 25%
                new GradientColorKey(new Color(0.95f, 0.80f, 0.15f), 0.45f), // Yellow at 45%
                new GradientColorKey(new Color(0.45f, 0.85f, 0.35f), 0.65f), // Green at 65%
                new GradientColorKey(new Color(0.25f, 0.70f, 0.95f), 0.85f), // Blue at 85%
                new GradientColorKey(new Color(0.65f, 0.45f, 0.95f), 1f)     // Purple at 100% (exceptional)
            };
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };
            fillGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    // ========================================================================
    // PUBLIC API — Called by CheckpointManager
    // ========================================================================

    /// <summary>
    /// Updates the listening sub-score (Metric 1).
    /// Value should be 0.0 (never listened) to 1.0 (always listened perfectly).
    /// </summary>
    public void UpdateListeningScore(float score)
    {
        hasReceivedAnyScore = true;
        listeningScore = Mathf.Clamp01(score);
        Debug.Log($"<color=cyan>[EmpathyMeter]</color> Listening: {listeningScore:P0} | Composite: {CompositeScore:P0}");
    }

    /// <summary>
    /// Updates the visual clarification sub-score (Metric 2).
    /// Value should be 0.0 (always abstract/harsh) to 1.0 (always visual/empathetic).
    /// </summary>
    public void UpdateClarificationScore(float score)
    {
        hasReceivedAnyScore = true;
        clarificationScore = Mathf.Clamp01(score);
        Debug.Log($"<color=cyan>[EmpathyMeter]</color> Clarification: {clarificationScore:P0} | Composite: {CompositeScore:P0}");
    }

    /// <summary>
    /// Updates the positive reinforcement sub-score (Metric 3).
    /// Value should be 0.0 (always blaming) to 1.0 (always validating).
    /// </summary>
    public void UpdateReinforcementScore(float score)
    {
        hasReceivedAnyScore = true;
        reinforcementScore = Mathf.Clamp01(score);
        Debug.Log($"<color=cyan>[EmpathyMeter]</color> Reinforcement: {reinforcementScore:P0} | Composite: {CompositeScore:P0}");
    }

    /// <summary>
    /// Gets the qualitative tier name based on composite score.
    /// </summary>
    public string GetQualitativeTier()
    {
        float score = CompositeScore;
        if (score < 0.2f) return tiers[0];       // Disconnected
        if (score < 0.4f) return tiers[1];        // Developing
        if (score < 0.6f) return tiers[2];        // Attentive
        if (score < 0.8f) return tiers[3];        // Empathetic
        return tiers[4];                           // Deeply Connected
    }

    // ========================================================================
    // INTERNAL — Visuals
    // ========================================================================

    private void UpdateVisuals(bool immediate)
    {
        float fill = immediate ? CompositeScore : displayedFill;

        if (fillImage != null)
        {
            fillImage.fillAmount = fill;
            fillImage.color = fillGradient.Evaluate(fill);
        }

        if (scoreLabel != null)
        {
            scoreLabel.text = $"{Mathf.RoundToInt(fill * 100)}%";
        }

        if (qualitativeLabel != null)
        {
            qualitativeLabel.text = GetQualitativeTier();
        }
    }
}
