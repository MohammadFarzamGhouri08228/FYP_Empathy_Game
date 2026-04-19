using System.Collections.Generic;
using UnityEngine;

public class NPCSorter : MonoBehaviour
{
    [Header("Sorting Stats")]
    public int maxCapacity = 3; // He can carry 3 at a time! (Player maybe only carries 1)
    public int currentBallsHeld = 0;
    public float moveSpeed = 3f;

    [Header("Targets")]
    public Transform blueBag;
    private Transform currentTargetBall;

    // A list of all the balls in the scene
    private List<GameObject> allBlueBalls = new List<GameObject>();
    
    private Vector3 originalScale;

    private void Start()
    {
        // Save the original size of the NPC so we don't accidentally make him huge!
        originalScale = transform.localScale;

        // Find all balls at the start
        allBlueBalls = new List<GameObject>(GameObject.FindGameObjectsWithTag("Blue Ball"));
        FindNextBall();
    }

    private void Update()
    {
        // If his hands are full OR there are no balls left, go to the bag
        if (currentBallsHeld >= maxCapacity || allBlueBalls.Count == 0)
        {
            MoveTowards(blueBag);
        }
        // Otherwise, if he has a target ball, go to it
        else if (currentTargetBall != null)
        {
            MoveTowards(currentTargetBall);
        }
    }

    private void MoveTowards(Transform target)
    {
        if (target == null) return;

        // Move the NPC horizontally towards the target
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), step);
        
        // Optional: Flip sprite to face movement direction
        if (target.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); // Face right
        else if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); // Face left
    }

    private void FindNextBall()
    {
        // Clean up the list to remove any balls that were destroyed
        allBlueBalls.RemoveAll(ball => ball == null);

        if (allBlueBalls.Count > 0)
        {
            // For simplicity, just target the first ball in the list
            // (You can upgrade this later to find the *closest* ball)
            currentTargetBall = allBlueBalls[0].transform;
        }
        else
        {
            currentTargetBall = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        // If he touches a Blue Ball and his hands aren't full
        if (other.CompareTag("Blue Ball") && currentBallsHeld < maxCapacity)
        {
            currentBallsHeld++;
            
            // Remove from list and destroy the object
            allBlueBalls.Remove(other);
            Destroy(other);
            
            // Find the next ball
            FindNextBall();
        }
        
        // If he touches the Blue Bag and he is carrying balls
        else if (other.CompareTag("Blue Bag") && currentBallsHeld > 0)
        {
            Debug.Log("NPC deposited " + currentBallsHeld + " balls into the bag! Much faster than the player.");
            
            // Here you could add to the Bag's score
            // BagScript.AddScore(currentBallsHeld);

            currentBallsHeld = 0; // Hands are empty now
            FindNextBall();       // Look for more balls
        }
    }
}
