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
    /// <summary>
    /// Loads the CSV file from the specified full path.
    /// Returns a list of DataEntry objects.
    /// </summary>
    public static List<DataEntry> ImportFromCSV(string fullPath)
    {
        List<DataEntry> entries = new List<DataEntry>();
        
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
                
                // CSV Configuration
                // Col 1: Group ("Down", "TD")
                // Col 11: Floor Matrix Map
                // Col 12: Floor Matrix Obs

                if (cols.Length > 12)
                {
                    string groupRaw = cols[1].Trim();
                    bool isDown = groupRaw.Equals("Down", StringComparison.OrdinalIgnoreCase);

                    // Parse Scores
                    float mapScore = 0f;
                    float obsScore = 0f;
                    
                    bool hasMap = float.TryParse(cols[11], out mapScore);
                    bool hasObs = float.TryParse(cols[12], out obsScore);

                    if (hasMap && hasObs)
                    {
                        entries.Add(new DataEntry
                        {
                            IsDownSyndrome = isDown,
                            FloorMatrixMap = mapScore,
                            FloorMatrixObs = obsScore
                        });
                    }
                }
            }

            return entries;
        }
        catch (Exception)
        {
             throw; // Re-throw to let caller handle logging
        }
    }
}

public class DataEntry
{
    public bool IsDownSyndrome;
    public float FloorMatrixMap;
    public float FloorMatrixObs;
}

