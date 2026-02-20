using UnityEngine;

public class CandySpawner : MonoBehaviour
{
    public GameObject candyPrefab;
    public float spawnInterval = 1f; // How often to spawn (in seconds)
    public float xSpawnRange = 8f;   // The width of your spawn area!

    void Start()
    {
        // Spawns a candy repeatedly every 'spawnInterval' seconds
        InvokeRepeating(nameof(SpawnCandy), 0f, spawnInterval);
    }

    void SpawnCandy()
    {
        // THIS IS THE LINE THAT RANDOMIZES THE POSITION
        // It picks a random number between -8 and 8 (or whatever you set xSpawnRange to)
        float randomX = Random.Range(-xSpawnRange, xSpawnRange);

        // Sets the spawn point at the spawner's Y, but the new random X
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0f);

        // Creates the candy prefab at that location
        Instantiate(candyPrefab, spawnPosition, Quaternion.identity);
    }
}
