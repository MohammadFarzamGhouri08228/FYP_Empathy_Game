using UnityEngine;

/// <summary>
/// Ensures the Adaptive Backend is initialized and trained BEFORE the game scene fully starts.
/// This guarantees the agent is ready without needing to modify existing scene objects.
/// </summary>
public static class AgentBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAgent()
    {
        Debug.Log("[AgentBootstrapper] Initializing Adaptive Backend...");
        
        // Accessing the Instance triggers the constructor, which calls Initialize() -> TrainAdaptiveModel()
        var backend = AdaptiveBackend.Instance;
        
        Debug.Log("[AgentBootstrapper] Backend Initialized. Training should be complete.");
    }
}