using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System;


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
        string fullPath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        Debug.Log($"[DataPipeline] Loading from: {fullPath}");

        try 
        {
            var result = DatasetImporter.ImportFromCSV(fullPath);
            
            MemoryStats = result.MemoryStats;
            SpatialStats = result.SpatialStats;
            
            Debug.Log($"<color=green>[DataPipeline] Loaded {result.rawMemoryScores.Count} profiles.</color>");
            Debug.Log($"Stats -> Avg Memory: {MemoryStats.Mean:F2}, Avg Spatial: {SpatialStats.Mean:F2}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataPipeline] Failed to load dataset: {e.Message}. Using fallback.");
            MemoryStats = new DSStatistics(new List<float> { 50f }); 
            SpatialStats = new DSStatistics(new List<float> { 50f });
        }
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


public class SimpleNeuralNet
{
    public SimpleNeuralNet(int i, int h, int o) { }
    public double[] FeedForward(double[] inputs) { return new double[3]; }
    public void Train(double[] inputs, double[] targets) { }
}