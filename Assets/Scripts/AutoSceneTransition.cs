using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class AutoSceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("The exact name of the next scene to load (must be in Build Settings).")]
    [SerializeField] private string nextSceneName = "Cutscene2";
    
    [Tooltip("If true, the countdown starts as soon as this scene loads.")]
    [SerializeField] private bool autoStartTimer = false;
    
    [Tooltip("Time in seconds before the next scene loads (used if autoStartTimer is true, or after Timeline finishes).")]
    [SerializeField] private float delayBeforeTransition = 0f;

    [Header("Timeline Settings (Optional)")]
    [Tooltip("Drag your PlayableDirector (Timeline) here. The next scene will load when it is finished.")]
    [SerializeField] private PlayableDirector timelineDirector;

    void Start()
    {
        Debug.Log($"[AutoSceneTransition] Initialized! Target Scene: '{nextSceneName}'");

        if (timelineDirector != null)
        {
            Debug.Log("[AutoSceneTransition] Timeline Director found. Waiting for timeline to finish...");
            // Subscribe to the timeline stopped event
            timelineDirector.stopped += OnTimelineStopped;
            
            // Start a backup coroutine just in case the stopped event fails
            StartCoroutine(TimelineBackupCheck());
        }
        else if (autoStartTimer)
        {
            Debug.Log($"[AutoSceneTransition] No timeline assigned. Timer started for {delayBeforeTransition} seconds.");
            StartCoroutine(TransitionRoutine());
        }
        else
        {
            Debug.Log("[AutoSceneTransition] Waiting for TriggerTransition() to be called manually.");
        }
    }

    private IEnumerator TimelineBackupCheck()
    {
        // Wait a frame so the timeline actually starts playing
        yield return null;
        
        while (timelineDirector != null)
        {
            // If the timeline reaches the very end of its duration, force a transition
            if (timelineDirector.time >= timelineDirector.duration - 0.05f && timelineDirector.duration > 0f)
            {
                Debug.Log("[AutoSceneTransition] Timeline backup check detected timeline reached end of duration!");
                OnTimelineStopped(timelineDirector);
                yield break; // Exit coroutine
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDestroy()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        if (director == timelineDirector)
        {
            Debug.Log("[AutoSceneTransition] Timeline Stopped Event Fired! Starting transition...");
            // Unsubscribe so it doesn't fire twice
            timelineDirector.stopped -= OnTimelineStopped; 
            StartCoroutine(TransitionRoutine());
        }
    }

    /// <summary>
    /// Call this method from Unity Events (like Timeline signals or Animation Events) 
    /// if you want to transition manually.
    /// </summary>
    public void TriggerTransition()
    {
        Debug.Log("[AutoSceneTransition] Manual Trigger Fired!");
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        if (delayBeforeTransition > 0f)
        {
            Debug.Log($"[AutoSceneTransition] Waiting for delay: {delayBeforeTransition} seconds...");
            yield return new WaitForSeconds(delayBeforeTransition);
        }
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[AutoSceneTransition] Attempting to Load Scene: '{nextSceneName}'");
            
            // Check if scene could be loaded
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError($"[AutoSceneTransition] ERROR: Scene '{nextSceneName}' cannot be loaded! Did you forget to add it to File -> Build Settings?");
            }
        }
        else
        {
            Debug.LogError("[AutoSceneTransition] Next Scene Name is empty!");
        }
    }
}
