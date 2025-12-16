using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

/// <summary>
/// MAIN CONTROLLER: Attaches to a GameObject (e.g., "GameManager").
/// Orchestrates Data Processing and AI Training on game initialization.
/// </summary>
public class AdaptiveBackend : MonoBehaviour
{
    public static AdaptiveBackend Instance;

    [Header("Configuration")]
    public string csvFileName = "Dataset path learning floor matrix task.csv";
    
    // The Neural Network (5 Inputs -> 12 Hidden -> 3 Outputs)
    private SimpleNeuralNet neuralNet;
    
    // Processed Real-World Data
    public DSStatistics MemoryStats { get; private set; }
    public DSStatistics SpatialStats { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize Neural Net: 5 Inputs (Big 5), 12 Hidden, 3 Outputs (Branch A, B, C)
        neuralNet = new SimpleNeuralNet(5, 12, 3);
    }

    void Start()
    {
        // 1. Process Real-World DS Data
        ProcessDataset();

        // 2. Train the Adaptive AI (Framework Initialization)
        StartCoroutine(TrainAdaptiveModel());
    }

    // =================================================================================
    // PART 1: REAL-WORLD DATA PROCESSING (CSV Parsing)
    // =================================================================================
    private void ProcessDataset()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, csvFileName);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"CSV File not found at: {filePath}");
            return;
        }

        List<float> memoryScores = new List<float>();
        List<float> spatialScores = new List<float>();

        string[] lines = File.ReadAllLines(filePath);
        
        // Skip header (i=1)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            
            // Check if Group is "Down" (Column 1 based on your CSV snippet)
            if (cols.Length > 9 && cols[1].Trim() == "Down")
            {
                // Column 9: WM_matr_sequential (Memory)
                if (float.TryParse(cols[9], out float mem)) memoryScores.Add(mem);

                // Column 11: Floor Matrix Map (Spatial)
                if (float.TryParse(cols[11], out float spat)) spatialScores.Add(spat);
            }
        }

        // Calculate Statistics
        MemoryStats = new DSStatistics(memoryScores);
        SpatialStats = new DSStatistics(spatialScores);

        Debug.Log($"<color=green>DS Data Loaded.</color> Avg Memory Capacity: {MemoryStats.Mean:F2}, Avg Spatial Score: {SpatialStats.Mean:F2}");
    }

    // =================================================================================
    // PART 2: ADAPTIVE AI TRAINING (Synthetic Framework)
    // =================================================================================
    private IEnumerator TrainAdaptiveModel()
    {
        Debug.Log("Initializing Adaptive Pathways (Training Neural Network)...");

        // A. GENERATE SYNTHETIC TRAINING DATA
        // We generate 1000 simulated player profiles to teach the AI the basic rules 
        // derived from the research paper framework.
        
        List<TrainingData> trainingSet = new List<TrainingData>();

        for (int i = 0; i < 1000; i++)
        {
            // Random Big 5 Profile: [Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism]
            double[] inputs = new double[5];
            for (int j = 0; j < 5; j++) inputs[j] = UnityEngine.Random.value;

            // Define "Ground Truth" Logic (The Rules we want the AI to learn)
            double[] targets = new double[3]; // [Exploration, LowStress, TaskBased]

            if (inputs[4] > 0.7) // High Neuroticism -> Needs Low Stress Branch
            {
                targets[0] = 0; targets[1] = 1; targets[2] = 0;
            }
            else if (inputs[0] > 0.6) // High Openness -> Needs Exploration Branch
            {
                targets[0] = 1; targets[1] = 0; targets[2] = 0;
            }
            else // Default -> Task Based Branch
            {
                targets[0] = 0; targets[1] = 0; targets[2] = 1;
            }

            trainingSet.Add(new TrainingData(inputs, targets));
        }

        // B. TRAINING LOOP (Backpropagation)
        // We train for 500 epochs across several frames to avoid freezing the game
        int epochs = 500;
        for (int e = 0; e < epochs; e++)
        {
            foreach (var data in trainingSet)
            {
                neuralNet.Train(data.inputs, data.targets);
            }
            
            if (e % 50 == 0) yield return null; // Wait a frame every 50 epochs
        }

        Debug.Log("<color=cyan>Adaptive Model Training Complete.</color>");
    }

    // =================================================================================
    // PUBLIC API: PREDICTION
    // =================================================================================
    public int PredictBranch(double[] playerBigFive)
    {
        double[] outputs = neuralNet.FeedForward(playerBigFive);
        
        // Find index of max value
        int maxIndex = 0;
        double maxVal = outputs[0];
        for(int i=1; i < outputs.Length; i++)
        {
            if(outputs[i] > maxVal)
            {
                maxVal = outputs[i];
                maxIndex = i;
            }
        }
        return maxIndex; // 0=Exploration, 1=LowStress, 2=TaskBased
    }

    // =================================================================================
    // PART 3: INTELLIGENT INTERACTION SYSTEM
    // =================================================================================
    
    // Dictionary to keep track of interactions: [ObjectID] -> [Count]
    private Dictionary<string, int> interactionHistory = new Dictionary<string, int>();

    /// <summary>
    /// Records an interaction from a game object and triggers an evaluation of game progression.
    /// </summary>
    /// <param name="objectId">Unique identifier for the object (e.g., "Object1")</param>
    /// <param name="count">Current interaction count for this object</param>
    public void RecordInteraction(string objectId, int count)
    {
        if (interactionHistory.ContainsKey(objectId))
        {
            interactionHistory[objectId] = count;
        }
        else
        {
            interactionHistory.Add(objectId, count);
        }

        Debug.Log($"<color=yellow>[AdaptiveBackend]</color> Received interaction from {objectId}. Total: {count}");
        
        // After recording, let's evaluate if this changes anything for the player
        EvaluateProgression();
    }

    /// <summary>
    /// Analyzes the current state of interactions and decides on game progression.
    /// </summary>
    private void EvaluateProgression()
    {
        // Example Logic:
        // If the player interacts with "Object1" more than 3 times, maybe they are stuck or very curious.
        
        if (interactionHistory.ContainsKey("Object1") && interactionHistory["Object1"] >= 3)
        {
            Debug.Log("<color=magenta>[Intelligent System]</color> Player shows high interest (or difficulty) with Object1. Adjusting game difficulty or providing a hint...");
            // TODO: Trigger actual game changes here (e.g., spawn a hint, lower enemy speed, etc.)
        }
    }
    // =================================================================================
    // DEBUG / VERIFICATION
    // =================================================================================
    
    void Update()
    {
        // Press 'I' to print current interaction history
        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintInteractionHistory();
        }
    }

    [ContextMenu("Print Interaction History")]
    public void PrintInteractionHistory()
    {
        Debug.Log("--- Current Interaction History ---");
        foreach (var kvp in interactionHistory)
        {
            Debug.Log($"Object: {kvp.Key}, Count: {kvp.Value}");
        }
        Debug.Log("-----------------------------------");
    }
}

// -------------------------------------------------------------------------
// HELPER CLASSES
// -------------------------------------------------------------------------

public class DSStatistics
{
    public float Mean;
    public float StdDev;
    public float Min;
    public float Max;

    public DSStatistics(List<float> data)
    {
        if (data.Count == 0) return;
        
        Mean = data.Average();
        Min = data.Min();
        Max = data.Max();
        
        // Calculate StdDev
        float sumSquares = data.Sum(d => Mathf.Pow(d - Mean, 2));
        StdDev = Mathf.Sqrt(sumSquares / (data.Count - 1));
    }
}

struct TrainingData
{
    public double[] inputs;
    public double[] targets;
    public TrainingData(double[] i, double[] t) { inputs = i; targets = t; }
}

// -------------------------------------------------------------------------
// C# NEURAL NETWORK IMPLEMENTATION (From Scratch)
// -------------------------------------------------------------------------
public class SimpleNeuralNet
{
    int inputNodes, hiddenNodes, outputNodes;
    double[,] weights_ih, weights_ho;
    double[] bias_h, bias_o;
    double learningRate = 0.1;

    public SimpleNeuralNet(int i, int h, int o)
    {
        inputNodes = i; hiddenNodes = h; outputNodes = o;
        
        weights_ih = new double[i, h];
        weights_ho = new double[h, o];
        bias_h = new double[h];
        bias_o = new double[o];

        InitializeWeights();
    }

    void InitializeWeights()
    {
        System.Random rnd = new System.Random();
        for (int i = 0; i < inputNodes; i++)
            for (int h = 0; h < hiddenNodes; h++)
                weights_ih[i, h] = rnd.NextDouble() * 2 - 1;

        for (int h = 0; h < hiddenNodes; h++)
            for (int o = 0; o < outputNodes; o++)
                weights_ho[h, o] = rnd.NextDouble() * 2 - 1;
                
        for(int h=0; h<hiddenNodes; h++) bias_h[h] = rnd.NextDouble() * 2 - 1;
        for(int o=0; o<outputNodes; o++) bias_o[o] = rnd.NextDouble() * 2 - 1;
    }

    public double[] FeedForward(double[] inputs)
    {
        // Input -> Hidden
        double[] hidden = new double[hiddenNodes];
        for (int h = 0; h < hiddenNodes; h++)
        {
            double sum = 0;
            for (int i = 0; i < inputNodes; i++) sum += inputs[i] * weights_ih[i, h];
            hidden[h] = Sigmoid(sum + bias_h[h]);
        }

        // Hidden -> Output
        double[] outputs = new double[outputNodes];
        for (int o = 0; o < outputNodes; o++)
        {
            double sum = 0;
            for (int h = 0; h < hiddenNodes; h++) sum += hidden[h] * weights_ho[h, o];
            outputs[o] = Sigmoid(sum + bias_o[o]);
        }
        return outputs;
    }

    public void Train(double[] inputs, double[] targets)
    {
        // 1. Feed Forward
        double[] hidden = new double[hiddenNodes];
        for (int h = 0; h < hiddenNodes; h++)
        {
            double sum = 0;
            for (int i = 0; i < inputNodes; i++) sum += inputs[i] * weights_ih[i, h];
            hidden[h] = Sigmoid(sum + bias_h[h]);
        }

        double[] outputs = new double[outputNodes];
        for (int o = 0; o < outputNodes; o++)
        {
            double sum = 0;
            for (int h = 0; h < hiddenNodes; h++) sum += hidden[h] * weights_ho[h, o];
            outputs[o] = Sigmoid(sum + bias_o[o]);
        }

        // 2. Backpropagation
        
        // Output Layer Errors
        double[] output_errors = new double[outputNodes];
        for (int o = 0; o < outputNodes; o++)
            output_errors[o] = targets[o] - outputs[o];

        // Calculate Gradients for Output Weights
        for (int o = 0; o < outputNodes; o++)
        {
            double gradient = outputs[o] * (1 - outputs[o]) * output_errors[o] * learningRate;
            for (int h = 0; h < hiddenNodes; h++)
            {
                weights_ho[h, o] += gradient * hidden[h];
            }
            bias_o[o] += gradient;
        }

        // Hidden Layer Errors
        double[] hidden_errors = new double[hiddenNodes];
        for (int h = 0; h < hiddenNodes; h++)
        {
            double sum = 0;
            for (int o = 0; o < outputNodes; o++) sum += output_errors[o] * weights_ho[h, o];
            hidden_errors[h] = sum;
        }

        // Calculate Gradients for Hidden Weights
        for (int h = 0; h < hiddenNodes; h++)
        {
            double gradient = hidden[h] * (1 - hidden[h]) * hidden_errors[h] * learningRate;
            for (int i = 0; i < inputNodes; i++)
            {
                weights_ih[i, h] += gradient * inputs[i];
            }
            bias_h[h] += gradient;
        }
    }

    private double Sigmoid(double x) => 1.0 / (1.0 + System.Math.Exp(-x));
}