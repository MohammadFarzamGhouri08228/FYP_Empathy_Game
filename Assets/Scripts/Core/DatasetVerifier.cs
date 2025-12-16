using UnityEngine;
using UnityEditor;
using System.IO;

public class DatasetVerifier
{
    [MenuItem("Tools/Verify Dataset")]
    public static void Verify()
    {
        string fileName = "Dataset path learning floor matrix task.csv";
        string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);

        Debug.Log($"[DatasetVerifier] Verifying dataset at: {fullPath}");

        try
        {
            DatasetResult result = DatasetImporter.ImportFromCSV(fullPath);

            Debug.Log("<color=green>[DatasetVerifier] Import Successful!</color>");
            Debug.Log($"[DatasetVerifier] Memory Stats: Mean={result.MemoryStats.Mean:F2}, StdDev={result.MemoryStats.StdDev:F2}, Count={result.rawMemoryScores.Count}");
            Debug.Log($"[DatasetVerifier] Spatial Stats: Mean={result.SpatialStats.Mean:F2}, StdDev={result.SpatialStats.StdDev:F2}, Count={result.rawSpatialScores.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DatasetVerifier] Import Failed: {e.Message}");
            Debug.LogException(e);
        }
    }
}
