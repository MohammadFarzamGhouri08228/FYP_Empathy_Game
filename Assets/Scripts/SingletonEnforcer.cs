using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any GameObject that might introduce a duplicate EventSystem or AudioListener.
/// On Awake, it checks if one already exists and destroys the duplicate.
/// </summary>
public class SingletonEnforcer : MonoBehaviour
{
    void Awake()
    {
        // ---- EventSystem: keep only one ----
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"[SingletonEnforcer] Destroying duplicate EventSystem on '{eventSystems[i].gameObject.name}'");
                Destroy(eventSystems[i].gameObject);
            }
        }

        // ---- AudioListener: keep only one ----
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++)
            {
                Debug.Log($"[SingletonEnforcer] Destroying duplicate AudioListener on '{listeners[i].gameObject.name}'");
                Destroy(listeners[i]);
            }
        }
    }
}
