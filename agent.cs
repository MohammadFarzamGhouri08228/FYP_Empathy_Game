using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System;

/// <summary>
/// General-purpose component for any object the player can interact with.
/// Automatically reports interactions to the AdaptiveBackend.
/// </summary>
public class InteractiveObject : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID for this object (e.g., 'Puzzle_Block_A', 'NPC_Guide'). Used by the AI backend.")]
    public string objectID;

    [Header("Interaction Settings")]
    [Tooltip("How many times has this been interacted with?")]
    [SerializeField] private int interactionCount = 0;

    [Tooltip("Should this object stop reporting after a certain number of uses? (-1 = infinite)")]
    public int maxInteractions = -1;

    [Header("Events")]
    [Tooltip("Unity Event triggered locally when interaction occurs (e.g., play sound, open door).")]
    public UnityEvent OnInteract;

    private void Start()
    {
        // Auto-generate ID if empty to prevent errors, though manual naming is better for analytics
        if (string.IsNullOrEmpty(objectID))
        {
            objectID = gameObject.name + "_" + System.Guid.NewGuid().ToString().Substring(0, 4);
        }
    }

    /// <summary>
    /// Call this method from your player controller, raycast logic, or UI button click.
    /// </summary>
    public void Interact()
    {
        // 1. Check constraints
        if (maxInteractions != -1 && interactionCount >= maxInteractions) return;

        // 2. Increment local state
        interactionCount++;

        // 3. Trigger local game logic (Visuals/Audio)
        OnInteract?.Invoke();

        // 4. Report to the AI Backend (The "Brain")
        if (AdaptiveBackend.Instance != null)
        {
            AdaptiveBackend.Instance.RecordInteraction(objectID, interactionCount);
        }
        else
        {
            Debug.LogWarning($"[InteractiveObject] '{objectID}' interacted, but AdaptiveBackend is missing!");
        }
    }

    /// <summary>
    /// Helper to reset state if the game loops or restarts level.
    /// </summary>
    public void ResetInteraction()
    {
        interactionCount = 0;
    }
}

/// <summary>
/// MAIN CONTROLLER: Orchestrates Data Processing, AI Training, and Gameplay Adaptation.
/// Singleton pattern ensures only one brain exists.
/// </summary>
public class AdaptiveBackend : MonoBehaviour
{
    public static AdaptiveBackend Instance;

    [Header("Configuration")]
    public string csvFileName = "Dataset_DownSyndrome_Profiles.csv"; // Renamed for clarity
    
    // The Neural Network
    private SimpleNeuralNet neuralNet;
    
    // Processed Real-World Data
    public DSStatistics MemoryStats { get; private set; }
    public DSStatistics SpatialStats { get; private set; }

    // --- INTERACTION TRACKING ---
    // Dictionary: [ObjectID] -> [InteractionCount]
    private Dictionary<string, int> interactionHistory = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else 
        {
            Destroy(gameObject);
            return;
        }

        // Initialize Neural Net: 5 Inputs (Big 5), 12 Hidden, 3 Outputs (Branches)
        neuralNet = new SimpleNeuralNet(5, 12, 3);
    }

    void Start()
    {
        ProcessDataset();
        StartCoroutine(TrainAdaptiveModel());
    }

    // =================================================================================
    // PART 1: PUBLIC API FOR GAME OBJECTS
    // =================================================================================

    /// <summary>
    /// Called by InteractiveObject.cs whenever a player does something.
    /// </summary>
    public void RecordInteraction(string objectId, int count)
    {
        // Update history
        if (interactionHistory.ContainsKey(objectId))
            interactionHistory[objectId] = count;
        else
            interactionHistory.Add(objectId, count);

        Debug.Log($"<color=cyan>[AdaptiveBackend]</color> Logged: {objectId} (x{count})");

        // Real-time evaluation: Did this specific interaction trigger a threshold?
        EvaluateProgression(objectId, count);
    }

    /// <summary>
    /// The "Brain" logic. Decides if the game should change based on input.
    /// </summary>
    private void EvaluateProgression(string objectId, int count)
    {
        // Example: Player keeps clicking a "Help" button or a specific puzzle piece
        if (objectId.Contains("Puzzle") && count > 5)
        {
            Debug.Log("<color=magenta>[Adaptation Triggered]</color> High repetition detected on puzzle. Lowering difficulty...");
            // Broadcast event or call GameManager to simplify the puzzle
            // EventManager.Trigger("SimplifyPuzzle"); 
        }

        // Example: Player interacts with "Social_NPC" frequently
        if (objectId == "NPC_Guide" && count > 3)
        {
             Debug.Log("<color=green>[Adaptation Triggered]</color> High social engagement. Unlocking new dialogue branch.");
        }
    }

    // =================================================================================
    // PART 2: DATA PROCESSING & AI (Unchanged core logic, cleaned up)
    // =================================================================================
    private void ProcessDataset()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"CSV File not found at: {filePath}. Using default fallback stats.");
            // Fallback to avoid crashes if file is missing during testing
            MemoryStats = new DSStatistics(new List<float> { 50f }); 
            SpatialStats = new DSStatistics(new List<float> { 50f });
            return;
        }

        List<float> memoryScores = new List<float>();
        List<float> spatialScores = new List<float>();
        string[] lines = File.ReadAllLines(filePath);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length > 9 && cols[1].Trim() == "Down")
            {
                if (float.TryParse(cols[9], out float mem)) memoryScores.Add(mem);
                if (float.TryParse(cols[11], out float spat)) spatialScores.Add(spat);
            }
        }

        MemoryStats = new DSStatistics(memoryScores);
        SpatialStats = new DSStatistics(spatialScores);
        Debug.Log($"DS Data Loaded. N={memoryScores.Count}");
    }

    private IEnumerator TrainAdaptiveModel()
    {
        // (Training logic remains the same as your provided code, omitted here for brevity 
        // unless you need the full training block reprinted)
        yield return null; 
        Debug.Log("<color=cyan>Adaptive Model Ready.</color>");
    }

    // =================================================================================
    // PART 3: PREDICTION
    // =================================================================================
    public int PredictBranch(double[] playerBigFive)
    {
        if (neuralNet == null) return 2; // Default to task-based if net not ready

        double[] outputs = neuralNet.FeedForward(playerBigFive);
        int maxIndex = 0;
        double maxVal = outputs[0];
        
        for(int i=1; i < outputs.Length; i++)
        {
            if(outputs[i] > maxVal) { maxVal = outputs[i]; maxIndex = i; }
        }
        return maxIndex; 
    }
}

// -------------------------------------------------------------------------
// HELPERS (Standardized)
// -------------------------------------------------------------------------
public class DSStatistics
{
    public float Mean, StdDev, Min, Max;
    public DSStatistics(List<float> data)
    {
        if (data == null || data.Count == 0) return;
        Mean = data.Average();
        Min = data.Min();
        Max = data.Max();
        if(data.Count > 1) 
            StdDev = Mathf.Sqrt(data.Sum(d => Mathf.Pow(d - Mean, 2)) / (data.Count - 1));
    }
}

public class SimpleNeuralNet
{
    // (Your existing Neural Net implementation goes here unchanged)
    // Included purely for compilation structure
    public SimpleNeuralNet(int i, int h, int o) { }
    public double[] FeedForward(double[] inputs) { return new double[3]; }
    public void Train(double[] inputs, double[] targets) { }
}