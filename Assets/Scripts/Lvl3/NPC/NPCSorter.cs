using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSorter : MonoBehaviour
{
    [Header("Sorting Stats")]
    public int maxCapacity = 3; // He can carry 3 at a time! (Player maybe only carries 1)
    public int currentBallsHeld = 0;
    public float moveSpeed = 3f;

    [Header("Targeting Setup")]
    [Tooltip("The tag of the balls this NPC should pick up (e.g. 'Red Ball')")]
    public string targetBallTag = "Blue Ball";
    [Tooltip("The tag of the bag this NPC should drop balls into (e.g. 'Red Bag')")]
    public string targetBagTag = "Blue Bag";
    
    [Tooltip("Drag the actual bag GameObject here that you want the NPC to walk towards")]
    public Transform targetBag;
    private Transform currentTargetBall;

    [Header("Phase 2 Targeting (Optional)")]
    [Tooltip("Once phase 1 is done, does he sort a second color?")]
    public string secondaryBallTag = "Red Ball";
    public string secondaryBagTag = "Red Bag";
    public Transform secondaryBag;
    
    private bool isSortingSecondary = false;

    [Header("Visual Indicators")]
    [Tooltip("Where the first ball floats above the NPC's head")]
    public Vector3 indicatorStartOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("How much space between each ball above his head")]
    public Vector3 indicatorSpacing = new Vector3(0.5f, 0f, 0f);
    
    private List<GameObject> carriedIndicators = new List<GameObject>();
    private bool isDepositing = false;

    // A list of all the target balls in the scene
    private List<GameObject> allTargetBalls = new List<GameObject>();
    
    private Vector3 originalScale;

    private void Start()
    {
        // Save the original size of the NPC so we don't accidentally make him huge!
        originalScale = transform.localScale;

        // Auto-assign the target bag if it was left empty
        if (targetBag == null)
        {
            GameObject[] bags = GameObject.FindGameObjectsWithTag(targetBagTag);
            foreach (var b in bags)
            {
                if (Mathf.Abs(b.transform.position.y - transform.position.y) < 4.0f)
                {
                    targetBag = b.transform;
                    break;
                }
            }
            if (targetBag == null) Debug.LogError($"NPCSorter: targetBag not assigned AND no bag on this floor with tag '{targetBagTag}'.");
        }

        // Find all balls with the specified tag at the start (ONLY ON THIS FLOOR)
        allTargetBalls = new List<GameObject>();
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag(targetBallTag))
        {
            if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 4.0f)
            {
                allTargetBalls.Add(ball);
            }
        }
        
        FindNextBall();
    }

    private void Update()
    {
        if (isDepositing) return; // Don't move while doing the deposit animation

        // If his hands are full OR there are no balls left, go to the bag
        if (currentBallsHeld >= maxCapacity || (allTargetBalls.Count == 0 && currentBallsHeld > 0))
        {
            MoveTowards(targetBag);
            
            // Failsafe: If he reached the bag's X position but the bag doesn't have a Physics Collider!
            if (targetBag != null && Mathf.Abs(transform.position.x - targetBag.position.x) < 0.1f)
            {
                if (!isDepositing)
                {
                    Debug.Log("Failsafe: NPC arrived at Bag coordinates. Triggering deposit!");
                    StartCoroutine(DepositBallsRoutine());
                }
            }
        }
        // Otherwise, if he has a target ball, go to it
        else if (currentTargetBall != null)
        {
            MoveTowards(currentTargetBall);
            
            // Failsafe: If he reached the ball's X position but the ball doesn't have a Collider!
            if (currentTargetBall != null && Mathf.Abs(transform.position.x - currentTargetBall.position.x) < 0.1f)
            {
                if (!isDepositing)
                {
                    Debug.Log("Failsafe: NPC arrived at Ball coordinates. Triggering pickup!");
                    HandleCollision(currentTargetBall.gameObject);
                }
            }
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
        allTargetBalls.RemoveAll(ball => ball == null);

        if (allTargetBalls.Count > 0)
        {
            // For simplicity, just target the first ball in the list
            currentTargetBall = allTargetBalls[0].transform;
        }
        else
        {
            // No more balls for the current phase!
            // Crucial fix: Only switch to phase 2 if he has already dropped off his current balls!
            if (currentBallsHeld == 0 && !isSortingSecondary && !string.IsNullOrEmpty(secondaryBallTag))
            {
                Debug.Log("NPC finished first color entirely. Switching to secondary color!");
                isSortingSecondary = true;
                
                // Swap targets to Phase 2
                targetBallTag = secondaryBallTag;
                targetBagTag = secondaryBagTag;
                
                if (secondaryBag != null) {
                    targetBag = secondaryBag.transform;
                } else {
                    GameObject[] bags = GameObject.FindGameObjectsWithTag(secondaryBagTag);
                    foreach (var b in bags)
                    {
                        if (Mathf.Abs(b.transform.position.y - transform.position.y) < 4.0f)
                        {
                            targetBag = b.transform;
                            break;
                        }
                    }
                }

                // Find the new color balls (ONLY ON THIS FLOOR)
                allTargetBalls = new List<GameObject>();
                foreach (GameObject ball in GameObject.FindGameObjectsWithTag(targetBallTag))
                {
                    if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 4.0f)
                    {
                        allTargetBalls.Add(ball);
                    }
                }
                
                if (allTargetBalls.Count > 0)
                {
                    currentTargetBall = allTargetBalls[0].transform;
                }
                else
                {
                    currentTargetBall = null; // Literally nothing left to do
                }
            }
            else
            {
                currentTargetBall = null; // Completely finished all phases
            }
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
        if (isDepositing) return; // Ignore collisions while depositing

        // If he touches a Target Ball and his hands aren't full
        if (other.CompareTag(targetBallTag) && currentBallsHeld < maxCapacity)
        {
            // Steal the sprite before destroying it
            Sprite ballSprite = null;
            SpriteRenderer ballSR = other.GetComponent<SpriteRenderer>();
            if (ballSR != null) ballSprite = ballSR.sprite;

            // Remove from list and destroy the object
            allTargetBalls.Remove(other);
            Destroy(other);

            // Create an indicator above his head
            CreateIndicator(ballSprite);

            currentBallsHeld++;
            
            // Find the next ball
            FindNextBall();
        }
        
        // If he touches the Target Bag and he is carrying balls
        else if (other.CompareTag(targetBagTag) && currentBallsHeld > 0)
        {
            Debug.Log("NPC deposited " + currentBallsHeld + " balls into the bag! Much faster than the player.");
            
            // Start the visual drop-off animation
            StartCoroutine(DepositBallsRoutine());
        }
    }

    private void CreateIndicator(Sprite sprite)
    {
        GameObject indicator = new GameObject("NPCBallIndicator");
        indicator.transform.SetParent(transform);
        
        // Stack them horizontally based on how many he is currently holding
        // Center them slightly so they look nice
        float offsetX = (currentBallsHeld * indicatorSpacing.x) - ((maxCapacity - 1) * indicatorSpacing.x / 2f);
        indicator.transform.localPosition = indicatorStartOffset + new Vector3(offsetX, 0, 0);
        
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10; // Keep in front of NPC
        
        carriedIndicators.Add(indicator);
    }

    private IEnumerator DepositBallsRoutine()
    {
        isDepositing = true;

        // Animate each ball flying into the bag one by one, very quickly!
        for (int i = carriedIndicators.Count - 1; i >= 0; i--)
        {
            GameObject indicator = carriedIndicators[i];
            carriedIndicators.RemoveAt(i);
            
            StartCoroutine(AnimateIndicatorIntoBag(indicator));
            
            // Wait just a tiny bit between each ball for a satisfying "pop pop pop" effect
            yield return new WaitForSeconds(0.15f);
        }

        // Wait for the last animation to finish
        yield return new WaitForSeconds(0.35f);

        currentBallsHeld = 0; // Hands are empty now
        isDepositing = false;
        FindNextBall();       // Look for more balls
    }

    private IEnumerator AnimateIndicatorIntoBag(GameObject indicator)
    {
        indicator.transform.SetParent(null); // Detach from NPC
        
        Vector3 startPos = indicator.transform.position;
        Vector3 targetPos = targetBag.position + new Vector3(0, 0.5f, 0); // Bag opening offset
        Vector3 initialScale = indicator.transform.localScale;
        
        float flyDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            float smoothT = t * t * (3f - 2f * t);

            indicator.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            indicator.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, smoothT);
            
            yield return null;
        }

        Destroy(indicator);
    }
}
