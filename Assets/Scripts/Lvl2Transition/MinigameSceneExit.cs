/*
    MinigameSceneExit.cs  –  Lvl2Transition
    ─────────────────────────────────────────
    Attach this to the LAST Checkpoint GameObject in Minigame1.

    Watches the Checkpoint component on the same object. The moment
    it becomes activated it saves the Level2 arrival position to
    PlayerPrefs and loads the target scene.

    Does NOT depend on triggerCheckpointID matching anything — it
    reads the ID automatically from the sibling Checkpoint component.

    ── Inspector Setup ──────────────────────────────────────────────
    • Target Scene Name  : Exact name of the Level2 scene (must be
                           added to Build Settings).
    • Arrival Position   : World position the player should appear at
                           in Level2 (copy X/Y/Z from the Level2
                           checkpoint's Transform).
    • Transition Delay   : Seconds to wait before loading (real time,
                           unaffected by Time.timeScale pausing).
    ─────────────────────────────────────────────────────────────────
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameSceneExit : MonoBehaviour
{
    // ─── PlayerPrefs keys (shared with Lvl2EntryPoint) ───────────
    public const string KEY_ARRIVAL_X     = "Lvl2Arrival_X";
    public const string KEY_ARRIVAL_Y     = "Lvl2Arrival_Y";
    public const string KEY_ARRIVAL_Z     = "Lvl2Arrival_Z";
    public const string KEY_FROM_MINIGAME = "FromMinigame";

    // ─── Inspector ───────────────────────────────────────────────
    [Header("Destination")]
    [Tooltip("Exact scene name to load (must be in File > Build Settings).")]
    [SerializeField] private string targetSceneName = "Level2";

    [Tooltip("World position the player should appear at in Level2.")]
    [SerializeField] private Vector3 arrivalPosition = Vector3.zero;

    [Header("Transition")]
    [Tooltip("Seconds to wait before loading. Uses real time so it works even if the game is paused.")]
    [SerializeField] private float transitionDelay = 0.3f;

    // ─── State ───────────────────────────────────────────────────
    private Checkpoint _checkpoint;
    private bool _triggered = false;

    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _checkpoint = GetComponent<Checkpoint>();

        if (_checkpoint == null)
            Debug.LogError("[MinigameSceneExit] No Checkpoint component on this GameObject. " +
                           "Attach this script to the last Checkpoint object.");
    }

    void Start()
    {
        // Also subscribe to GameEventManager as a secondary trigger path
        GameEventManager.OnGameEvent += OnAnyGameEvent;

        Debug.Log($"[MinigameSceneExit] Ready. Will transition to '{targetSceneName}' " +
                  $"when Checkpoint (ID {_checkpoint?.CheckpointID}) activates.");
    }

    void OnDestroy()
    {
        GameEventManager.OnGameEvent -= OnAnyGameEvent;
    }

    // Poll every frame — this catches the activation even if Time.timeScale == 0
    // because Update still runs one frame after timeScale is set to 0.
    void Update()
    {
        if (_triggered || _checkpoint == null) return;

        // CheckpointManager.EndGame() pauses via Time.timeScale = 0 THEN the
        // GameEvent fires. Polling here ensures we catch it regardless of order.
        if (_checkpoint.HasBeenActivated)
            Trigger();
    }

    // ─── GameEvent secondary path ─────────────────────────────────

    private void OnAnyGameEvent(GameEventType eventType, GameObject source)
    {
        if (_triggered) return;
        // Only care about events from this exact checkpoint object
        if (source != gameObject) return;
        Trigger();
    }

    // ─── Core logic ──────────────────────────────────────────────

    private void Trigger()
    {
        _triggered = true;

        Debug.Log($"[MinigameSceneExit] Checkpoint activated. " +
                  $"Loading '{targetSceneName}' → {arrivalPosition}");

        SaveArrivalData();

        // Unpause before loading in case CheckpointManager froze time
        Time.timeScale = 1f;

        StartCoroutine(LoadAfterDelay());
    }

    private void SaveArrivalData()
    {
        PlayerPrefs.SetFloat(KEY_ARRIVAL_X, arrivalPosition.x);
        PlayerPrefs.SetFloat(KEY_ARRIVAL_Y, arrivalPosition.y);
        PlayerPrefs.SetFloat(KEY_ARRIVAL_Z, arrivalPosition.z);
        PlayerPrefs.SetInt(KEY_FROM_MINIGAME, 1);
        PlayerPrefs.Save();

        // This is a permanent level transition — clear the MinigameTeleport's
        // return-trip data so it doesn't override our arrival position in Level2.
        MinigamePositionManager.ClearSavedPosition();
    }

    private IEnumerator LoadAfterDelay()
    {
        if (transitionDelay > 0f)
            yield return new WaitForSecondsRealtime(transitionDelay);

        SceneManager.LoadScene(targetSceneName);
    }

    // ─── Editor Gizmo ────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(arrivalPosition, 0.4f);
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);
        Gizmos.DrawLine(transform.position, arrivalPosition);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(arrivalPosition + Vector3.up * 0.6f,
            $"Level2 arrival\n{arrivalPosition}");
#endif
    }
}
