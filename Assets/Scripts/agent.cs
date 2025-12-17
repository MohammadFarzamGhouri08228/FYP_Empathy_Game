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
    // Processed Real-World Data
    private List<DataEntry> _datasetEntries;
    
    // Stats for scaling/normalization
    public float MeanMap, StdMap;
    public float MeanObs, StdObs;
    
    // --- DECISION MAKING STATE ---
    /// <summary>
    /// The current probability (0.0 to 1.0) that the player's behavior matches the DS profile.
    /// Updated via EvaluatePlayer().
    /// </summary>
    public float CurrentDSSimilarity { get; private set; } = 0.5f;

    /// <summary>
    /// Event fired whenever the player's profile is re-evaluated.
    /// Payload: The new similarity score.
    /// </summary>
    public event Action<float> OnPlayerProfileUpdated;

    // --- UNIVERSAL DATA STORAGE ---
    // Structure: [ObjectID] -> [DataLabel] -> [Value]
    // Example: "Object1" -> { "interactionCount": 5, "timeSpent": 12.4f }
    // --- DATA TRACKING LISTS ---
    private List<float> _checkpointTimes = new List<float>();
    private List<float> _lifeLostTimes = new List<float>();

    // --- UNIVERSAL DATA STORAGE ---
    // Structure: [ObjectID] -> [DataLabel] -> [Value]
    // Example: "Object1" -> { "interactionCount": 5, "timeSpent": 12.4f }
    private Dictionary<string, Dictionary<string, object>> gameDataLog = new Dictionary<string, Dictionary<string, object>>();

    // Private Constructor
    private AdaptiveBackend()
    {
        // Initialize Neural Net: 5 Inputs (Big 5), 12 Hidden, 3 Outputs (Branches)
        // Initialize Neural Net: 2 Inputs (Map, Obs), 5 Hidden, 1 Output (Probability of DS behavior)
        neuralNet = new SimpleNeuralNet(2, 5, 1);
        
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
        
        // 5. Rate Tracking
        if (dataValue is float timeStamp)
        {
            if (objectId == "CheckpointManager" && dataLabel.StartsWith("CheckpointReached"))
            {
                _checkpointTimes.Add(timeStamp);
                EvaluatePlayer();
            }
            if ((objectId == "HeartManager" || objectId == "HealthSystem") && dataLabel == "LifeLost")
            {
                _lifeLostTimes.Add(timeStamp);
                EvaluatePlayer();
            }
        }
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

    /// <summary>
    /// Calculates runtime metrics and feeds them into the Neural Network
    /// to categorize the player behavior.
    /// </summary>
    private void EvaluatePlayer()
    {
        // Metric 1: Average Time Between Checkpoints
        float avgCheckpointTime = 0f;
        if (_checkpointTimes.Count > 1)
        {
            List<float> deltas = new List<float>();
            // If the first checkpoint is at index 0, delta is from start (0.0). Usually index starts at 0.
            // But let's assume times are absolute.
            // Checkpoint times: [10.0, 25.0, 45.0] -> Deltas: [10.0, 15.0, 20.0]
            
            float previous = 0f;
            foreach(var t in _checkpointTimes)
            {
                deltas.Add(t - previous);
                previous = t;
            }
            avgCheckpointTime = deltas.Average();
        }
        else if (_checkpointTimes.Count == 1)
        {
             avgCheckpointTime = _checkpointTimes[0];
        }

        // Metric 2: Rate of Life Loss (Lives lost per minute)
        float lifeLossRate = 0f;
        float totalTime = 0f;
        // Use the latest event time as current total time
        if (_checkpointTimes.Count > 0) totalTime = Mathf.Max(totalTime, _checkpointTimes.Last());
        if (_lifeLostTimes.Count > 0) totalTime = Mathf.Max(totalTime, _lifeLostTimes.Last());
        
        if (totalTime > 0)
        {
            lifeLossRate = _lifeLostTimes.Count / (totalTime / 60f); // Lives per minute
        }

        Debug.Log($"<color=orange>[Metrics]</color> Avg CP Time: {avgCheckpointTime:F1}s | Life Loss Rate: {lifeLossRate:F1}/min");

        // MAP TO NEURAL NET INPUTS
        // NN expects "Floor Matrix Map" (Spatial) and "Floor Matrix Obs" (Observation/Memory).
        // Range roughly 1.0 - 5.0 (Map) and 1.0 - 6.0 (Obs).
        
        // Assumption: High Checkpoint Time (Slow) -> Low Map Score (Poor Spatial)
        // Assumption: High Life Loss Rate -> Low Obs Score (Poor Observation)
        
        // Simple Inverse Mapping (Tuning needed based on real data ranges)
        // Map: Base 5.0 - (Time / 10.0)
        float mappedMapScore = Mathf.Clamp(5.0f - (avgCheckpointTime / 15.0f), 1.0f, 5.0f);
        
        // Obs: Base 6.0 - (Rate * 2.0)
        float mappedObsScore = Mathf.Clamp(6.0f - (lifeLossRate * 2.0f), 1.0f, 6.0f);

        float dsSimilarity = PredictSimilarityToDS(mappedMapScore, mappedObsScore);
        
        // Update State
        CurrentDSSimilarity = dsSimilarity;
        OnPlayerProfileUpdated?.Invoke(CurrentDSSimilarity);
        
        Debug.Log($"<color=magenta>[Agent Evaluation]</color> Mapped Inputs: [Map:{mappedMapScore:F1}, Obs:{mappedObsScore:F1}] -> DS Similarity: {dsSimilarity:P1}");
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
            _datasetEntries = DatasetImporter.ImportFromCSV(fullPath);
            Debug.Log($"<color=green>[DataPipeline] Loaded {_datasetEntries.Count} data points.</color>");

            // Calculate Normalization Stats
            if (_datasetEntries.Count > 0)
            {
                MeanMap = _datasetEntries.Average(x => x.FloorMatrixMap);
                StdMap = CalculateStdDev(_datasetEntries.Select(x => x.FloorMatrixMap));
                
                MeanObs = _datasetEntries.Average(x => x.FloorMatrixObs);
                StdObs = CalculateStdDev(_datasetEntries.Select(x => x.FloorMatrixObs));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataPipeline] Failed to load dataset: {e.Message}");
            _datasetEntries = new List<DataEntry>();
        }
    }

    private float CalculateStdDev(IEnumerable<float> values)
    {
        float mean = values.Average();
        double sum = values.Sum(d => Math.Pow(d - mean, 2));
        return (float)Math.Sqrt(sum / (values.Count() - 1));
    }

    private void TrainAdaptiveModel()
    {
        if (_datasetEntries == null || _datasetEntries.Count == 0) return;

        Debug.Log("<color=yellow>[Training] Starting Neural Network Training on Floor Matrix Data...</color>");
        
        // Simple training loop (Epochs)
        int epochs = 1000;
        double initialError = 0;
        double finalError = 0;

        for (int i = 0; i < epochs; i++)
        {
            double totalSqError = 0;
            foreach (var data in _datasetEntries)
            {
                // Inputs: Normalized Map score, Normalized Obs score
                double[] inputs = new double[] 
                {
                    ZScore(data.FloorMatrixMap, MeanMap, StdMap),
                    ZScore(data.FloorMatrixObs, MeanObs, StdObs)
                };

                // Target: 1.0 for Down Syndrome behavior, 0.0 for TD
                double[] target = new double[] { data.IsDownSyndrome ? 1.0 : 0.0 };

                double error = neuralNet.Train(inputs, target); // Updated to return error
                totalSqError += error; 
            }

            if (i == 0) initialError = totalSqError / _datasetEntries.Count;
            if (i == epochs - 1) finalError = totalSqError / _datasetEntries.Count;
        }

        Debug.Log($"<color=cyan>[Training] Adaptive Model Trained on Floor Matrix Data.</color>");
        Debug.Log($"[Training Stats] Epochs: {epochs}");
        Debug.Log($"[Training Stats] Initial MSE: {initialError:F5}");
        Debug.Log($"[Training Stats] Final MSE: {finalError:F5}");
        Debug.Log($"[Training Stats] Improvement: {((initialError - finalError)/initialError * 100):F1}%");
    }

    private double ZScore(float val, float mean, float std)
    {
        if (std == 0) return 0;
        return (val - mean) / std;
    }

    // =================================================================================
    // PART 3: PREDICTION
    // =================================================================================
    public float PredictSimilarityToDS(float mapScore, float obsScore)
    {
        if (neuralNet == null) return 0.5f;

        double[] inputs = new double[]
        {
            ZScore(mapScore, MeanMap, StdMap),
            ZScore(obsScore, MeanObs, StdObs)
        };

        double[] output = neuralNet.FeedForward(inputs);
        return (float)output[0]; 
    }
}

// -------------------------------------------------------------------------
// HELPERS (Standardized)
// -------------------------------------------------------------------------


public class SimpleNeuralNet
{
    private int inputNodes, hiddenNodes, outputNodes;
    private double[,] weightsIH;
    private double[,] weightsHO;
    private double[] hiddenLayer;
    private double[] outputs;

    private System.Random random;

    public SimpleNeuralNet(int i, int h, int o) 
    {
         inputNodes = i; hiddenNodes = h; outputNodes = o;
         weightsIH = new double[i, h];
         weightsHO = new double[h, o];
         hiddenLayer = new double[h];
         outputs = new double[o];
         random = new System.Random(12345);
         InitializeWeights();
    }

    private void InitializeWeights()
    {
        for(int i=0; i<inputNodes; i++)
            for(int h=0; h<hiddenNodes; h++) weightsIH[i,h] = (random.NextDouble() * 2) - 1; // -1 to 1

        for(int h=0; h<hiddenNodes; h++)
            for(int o=0; o<outputNodes; o++) weightsHO[h,o] = (random.NextDouble() * 2) - 1;
    }

    public double[] FeedForward(double[] inputs) 
    {
        // Input -> Hidden
        for(int h=0; h<hiddenNodes; h++)
        {
            double sum = 0;
            for(int i=0; i<inputNodes; i++) sum += inputs[i] * weightsIH[i,h];
            hiddenLayer[h] = Sigmoid(sum);
        }

        // Hidden -> Output
        for(int o=0; o<outputNodes; o++)
        {
            double sum = 0;
            for(int h=0; h<hiddenNodes; h++) sum += hiddenLayer[h] * weightsHO[h,o];
            outputs[o] = Sigmoid(sum);
        }
        return outputs;
    }

    public double Train(double[] inputs, double[] targets) 
    {
        double[] output = FeedForward(inputs);

        // Calculate Output Errors (Target - Output)
        double[] outputErrors = new double[outputNodes];
        double totalSqError = 0;

        for(int o=0; o<outputNodes; o++) 
        {
            double error = targets[o] - output[o];
            outputErrors[o] = error;
            totalSqError += error * error;
        }

        // Backprop Output -> Hidden
        for(int h=0; h<hiddenNodes; h++)
        {
            for(int o=0; o<outputNodes; o++)
            {
                // Delta = Error * Derivative * HiddenInput
                double delta = outputErrors[o] * SigmoidDerivative(output[o]) * hiddenLayer[h];
                weightsHO[h,o] += delta * 0.1; // Learning Rate
            }
        }

        // Calculate Hidden Errors
        double[] hiddenErrors = new double[hiddenNodes];
        for(int h=0; h<hiddenNodes; h++)
        {
            double err = 0;
            for(int o=0; o<outputNodes; o++) err += outputErrors[o] * weightsHO[h,o];
            hiddenErrors[h] = err;
        }

        // Backprop Hidden -> Input
        for(int i=0; i<inputNodes; i++)
        {
            for(int h=0; h<hiddenNodes; h++)
            {
                double delta = hiddenErrors[h] * SigmoidDerivative(hiddenLayer[h]) * inputs[i];
                weightsIH[i,h] += delta * 0.1;
            }
        }

        return totalSqError; // Return error for logging
    }

    private double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
    private double SigmoidDerivative(double x) => x * (1.0 - x); // For sigmoid output
}