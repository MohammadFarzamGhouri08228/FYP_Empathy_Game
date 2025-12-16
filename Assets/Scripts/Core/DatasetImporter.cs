using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Manging the importing and parsing of dataset files.
/// Pure C# class: No Unity dependencies.
/// </summary>
public static class DatasetImporter
{
    /// <summary>
    /// Loads the CSV file from the specified full path.
    /// Returns a DatasetResult or throws an exception on failure.
    /// </summary>
    public static DatasetResult ImportFromCSV(string fullPath)
    {
        DatasetResult result = new DatasetResult();
        
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"CSV File not found at: {fullPath}");
        }

        try 
        {
            string[] lines = File.ReadAllLines(fullPath);
            
            // Skip header (i=1)
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] cols = lines[i].Split(',');
                
                // CSV Configuration based on: "Dataset path learning floor matrix task.csv"
                // Col 1: Group ("Down", "TD")
                // Col 9: WM_matr_sequential (Memory)
                // Col 11: Floor Matrix Map (Spatial)

                if (cols.Length > 11 && cols[1].Trim() == "Down")
                {
                    if (float.TryParse(cols[9], out float mem)) result.rawMemoryScores.Add(mem);
                    if (float.TryParse(cols[11], out float spat)) result.rawSpatialScores.Add(spat);
                }
            }

            // Calculate Stats
            result.MemoryStats = new DSStatistics(result.rawMemoryScores);
            result.SpatialStats = new DSStatistics(result.rawSpatialScores);

            return result;
        }
        catch (Exception)
        {
             throw; // Re-throw to let caller handle logging
        }
    }
}

public class DatasetResult
{
    public List<float> rawMemoryScores = new List<float>();
    public List<float> rawSpatialScores = new List<float>();
    
    public DSStatistics MemoryStats;
    public DSStatistics SpatialStats;
}

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
            StdDev = (float)Math.Sqrt(data.Sum(d => Math.Pow(d - Mean, 2)) / (data.Count - 1));
    }
}

