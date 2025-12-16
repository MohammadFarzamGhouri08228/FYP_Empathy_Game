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
            var result = DatasetImporter.ImportFromCSV(fullPath);

            var downGroup = result.FindAll(e => e.IsDownSyndrome);
            var tdGroup = result.FindAll(e => !e.IsDownSyndrome);

            Debug.Log("<color=green>[DatasetVerifier] Import Successful!</color>");
            Debug.Log($"[DatasetVerifier] Total Entries: {result.Count}");
            Debug.Log($"[DatasetVerifier] Down Syndrome Entries: {downGroup.Count}");
            Debug.Log($"[DatasetVerifier] TD Entries: {tdGroup.Count}");

            // Helper to calc mean
            float CalcMean(System.Collections.Generic.List<DataEntry> list, System.Func<DataEntry, float> selector)
            {
                if (list.Count == 0) return 0f;
                float sum = 0f;
                foreach (var item in list) sum += selector(item);
                return sum / list.Count;
            }

            Debug.Log($"[DatasetVerifier] DS Mean Map Score: {CalcMean(downGroup, e => e.FloorMatrixMap):F2}");
            Debug.Log($"[DatasetVerifier] TD Mean Map Score: {CalcMean(tdGroup, e => e.FloorMatrixMap):F2}");
            Debug.Log($"[DatasetVerifier] DS Mean Obs Score: {CalcMean(downGroup, e => e.FloorMatrixObs):F2}");
            Debug.Log($"[DatasetVerifier] TD Mean Obs Score: {CalcMean(tdGroup, e => e.FloorMatrixObs):F2}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DatasetVerifier] Import Failed: {e.Message}");
            Debug.LogException(e);
        }
    }
}
