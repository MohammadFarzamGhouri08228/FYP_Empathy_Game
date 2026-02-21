using UnityEngine;

public class CandySpawner : MonoBehaviour
{
    public GameObject candyPrefab;
    public float spawnInterval = 1f; // How often to spawn (in seconds)
    public float xSpawnRange = 8f;   // The width of your spawn area!

    private int candiesSpawned = 0;
    public int totalCandiesToSpawn = 30;
    private float timer = 0f;

    void Update()
    {
        // Only spawn if game is active
        if (CandyGameManager.Instance != null && CandyGameManager.Instance.isGameActive)
        {
            timer += Time.deltaTime;
            
            // Time to spawn another candy
            if (timer >= spawnInterval && candiesSpawned < totalCandiesToSpawn)
            {
                SpawnCandy();
                timer = 0f;
            }
            
            // Check if we reached the max amount
            if (candiesSpawned >= totalCandiesToSpawn)
            {
                CandyGameManager.Instance.isGameActive = false; // Stop tracking score and spawning
                Invoke(nameof(CallGameOver), 3f); // Wait 3s before concluding to let final candies drop
            }
        }
    }

    void CallGameOver()
    {
        if (CandyGameManager.Instance != null)
        {
            CandyGameManager.Instance.GameOver();
        }
    }

    void SpawnCandy()
    {
        candiesSpawned++;
        float randomX = Random.Range(-xSpawnRange, xSpawnRange);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0f);
        Instantiate(candyPrefab, spawnPosition, Quaternion.identity);
    }
}
