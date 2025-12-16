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
            // UPDATED: Now uses the generic ReceiveData method
            AdaptiveBackend.Instance.ReceiveData(objectID, "interactionCount", interactionCount);
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
/// <summary>
/// MAIN CONTROLLER: Orchestrates Data Processing, AI Training, and Gameplay Adaptation.
/// Singleton pattern ensures only one brain exists.
/// Pure C# Class - Not a MonoBehaviour.
/// </summary>
public class AdaptiveBackend
{
    // Lazy-loaded Singleton
    private static AdaptiveBackend _instance;
    public static AdaptiveBackend Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AdaptiveBackend();
            }
            return _instance;
        }
    }

    [Header("Configuration")]
    public string csvFileName = "Dataset path learning floor matrix task.csv"; 
    
    // The Neural Network
    private SimpleNeuralNet neuralNet;
    
    // Processed Real-World Data
    public DSStatistics MemoryStats { get; private set; }
    public DSStatistics SpatialStats { get; private set; }

    // --- UNIVERSAL DATA STORAGE ---
    // Structure: [ObjectID] -> [DataLabel] -> [Value]
    // Example: "Object1" -> { "interactionCount": 5, "timeSpent": 12.4f }
    private Dictionary<string, Dictionary<string, object>> gameDataLog = new Dictionary<string, Dictionary<string, object>>();

    // Private Constructor
    private AdaptiveBackend()
    {
        // Initialize Neural Net: 5 Inputs (Big 5), 12 Hidden, 3 Outputs (Branches)
        neuralNet = new SimpleNeuralNet(5, 12, 3);
        
        // Start initialization logic
        Initialize();
    }

    private void Initialize()
    {
        ProcessDataset();
        // Replacing Coroutine with direct call since it was just a dummy wait
        TrainAdaptiveModel(); 
    }

    // =================================================================================
    // PART 1: PUBLIC API FOR GAME OBJECTS (UPDATED)
    // =================================================================================

    /// <summary>
    /// Universal entry point for ANY object to send ANY data.
    /// </summary>
    /// <param name="objectId">Who is sending data? (e.g. "Object1", "PuzzleManager")</param>
    /// <param name="dataLabel">What is this data? (e.g. "interactionCount", "score", "time_taken")</param>
    /// <param name="dataValue">The actual data (int, float, bool, string, etc.)</param>
    public void ReceiveData(string objectId, string dataLabel, object dataValue)
    {
        // 1. Ensure the object exists in our log
        if (!gameDataLog.ContainsKey(objectId))
        {
            gameDataLog[objectId] = new Dictionary<string, object>();
        }

        // 2. Update or Add the specific data point
        if (gameDataLog[objectId].ContainsKey(dataLabel))
        {
            gameDataLog[objectId][dataLabel] = dataValue;
        }
        else
        {
            gameDataLog[objectId].Add(dataLabel, dataValue);
        }

        // 3. Debug Log (Shows what type of data was received)
        Debug.Log($"<color=cyan>[Backend]</color> received from {objectId}: [{dataLabel} = {dataValue}]");

        // 4. Trigger Analysis
        EvaluateData(objectId, dataLabel, dataValue);
    }

    /// <summary>
    /// Analyzes incoming data. Handles specific logic for Object1 and generic logic for others.
    /// </summary>
    private void EvaluateData(string objectId, string label, object value)
    {
        // --- LOGIC FOR INTEGERS (Counts, Scores) ---
        if (value is int intVal)
        {
            // Specific Logic for Object1
            if (objectId == "Object1" && label == "interactionCount")
            {
                if (intVal > 10) 
                {
                    Debug.Log($"<color=green>[Agent Decision]</color> Object1 usage high ({intVal}). Adjusting Game Parameter...");
                    // Add your difficulty adjustment code here
                }
            }

            // Generic Logic for Puzzles
            if (objectId.Contains("Puzzle") && label == "interactionCount" && intVal > 5)
            {
                Debug.Log("<color=magenta>[Adaptation Triggered]</color> High repetition on puzzle. Lowering difficulty...");
            }
        }

        // --- LOGIC FOR FLOATS (Timers, Durations) ---
        else if (value is float floatVal)
        {
            if (label == "timeTaken" && floatVal > 60f)
            {
                Debug.Log("Player is taking a long time. Triggering hint system.");
            }
        }
    }

    // =================================================================================
    // PART 2: DATA PROCESSING & AI (Preserved from your code)
    // =================================================================================
    private void ProcessDataset()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        
        Debug.Log($"[DataPipeline] Attempting to load dataset from: {filePath}");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[DataPipeline] CSV File NOT FOUND at: {filePath}. Using default fallback stats (50/50).");
            MemoryStats = new DSStatistics(new List<float> { 50f }); 
            SpatialStats = new DSStatistics(new List<float> { 50f });
            return;
        }

        List<float> memoryScores = new List<float>();
        List<float> spatialScores = new List<float>();

        try 
        {
            string[] lines = File.ReadAllLines(filePath);
            Debug.Log($"[DataPipeline] File loaded. Total lines: {lines.Length}");

            int loadedCount = 0;
            
            // Skip header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] cols = lines[i].Split(',');
                
                // DATASET MAPPING: 
                // Col 1: Group ("Down", "TD")
                // Col 9: WM_matr_sequential (Memory)
                // Col 11: Floor Matrix Map (Spatial)
                
                if (cols.Length > 11 && cols[1].Trim() == "Down")
                {
                    if (float.TryParse(cols[9], out float mem)) memoryScores.Add(mem);
                    if (float.TryParse(cols[11], out float spat)) spatialScores.Add(spat);
                    
                    loadedCount++;
                    
                    if (loadedCount <= 3)
                    {
                         Debug.Log($"[DataPipeline] Sample #{loadedCount}: Memory={mem}, Spatial={spat} (Row {i})");
                    }
                }
            }

            // Calculate Statistics
            MemoryStats = new DSStatistics(memoryScores);
            SpatialStats = new DSStatistics(spatialScores);

            Debug.Log($"<color=green>[DataPipeline] SUCCESS!</color> Loaded {loadedCount} 'Down' syndrome profiles.");
            Debug.Log($"Stats -> Avg Memory: {MemoryStats.Mean:F2}, Avg Spatial: {SpatialStats.Mean:F2}");
        }
        catch (Exception e)
        {
             Debug.LogError($"[DataPipeline] CRITICAL ERROR parsing CSV: {e.Message}");
        }
    }

    public void VerifyDataIntegrity()
    {
        Debug.Log("--- Starting Manual Data Verification ---");
        ProcessDataset();
        Debug.Log("--- Verification Complete ---");
    }

    private void TrainAdaptiveModel()
    {
        // Was a coroutine waiting for null in original code. 
        // In pure C#, we just execute logic.
        Debug.Log("<color=cyan>Adaptive Model Ready.</color>");
    }

    // =================================================================================
    // PART 3: PREDICTION
    // =================================================================================
    public int PredictBranch(double[] playerBigFive)
    {
        if (neuralNet == null) return 2; 

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
    public SimpleNeuralNet(int i, int h, int o) { }
    public double[] FeedForward(double[] inputs) { return new double[3]; }
    public void Train(double[] inputs, double[] targets) { }
}