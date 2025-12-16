using UnityEngine;
using System;

/// <summary>
/// Static event manager for handling game events
/// </summary>
public static class GameEventManager
{
    /// <summary>
    /// Event that gets triggered when a game event occurs
    /// Parameters: GameEventType eventType, GameObject source (optional)
    /// </summary>
    public static event Action<GameEventType, GameObject> OnGameEvent;
    
    /// <summary>
    /// Triggers a game event
    /// </summary>
    /// <param name="eventType">The type of event</param>
    /// <param name="source">The GameObject that triggered the event (optional)</param>
    public static void TriggerEvent(GameEventType eventType, GameObject source = null)
    {
        Debug.Log($"GameEventManager: Triggering event {eventType} from {source?.name ?? "Unknown"}");
        OnGameEvent?.Invoke(eventType, source);
    }
}
