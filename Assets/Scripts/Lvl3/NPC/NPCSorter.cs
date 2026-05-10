using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSorter : MonoBehaviour
{
    [System.Serializable]
    public class HeldBall
    {
        public string bagTag;
        public GameObject indicator;
    }

    [Header("Sorting Stats")]
    public int maxCapacity = 3; 
    public float moveSpeed = 3f;
    public bool canSort = true; // Use this to start/stop the NPC externally (Phase UI)

    [Header("Ball Tags")]
    public string targetBallTag = "Blue Ball";
    public string targetBagTag = "Blue Bag";
    public Transform targetBag;

    [Header("Secondary Ball Tags (Optional)")]
    public string secondaryBallTag = "Red Ball";
    public string secondaryBagTag = "Red Bag";
    public Transform secondaryBag;

    [Header("Visual Indicators")]
    public Vector3 indicatorStartOffset = new Vector3(0f, 1.5f, 0f);
    public Vector3 indicatorSpacing = new Vector3(0.5f, 0f, 0f);

    private List<HeldBall> inventory = new List<HeldBall>();
    private bool isDepositingAnimation = false;
    private List<GameObject> allTargetBalls = new List<GameObject>();
    private Transform currentTargetBall;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;

        // Auto-assign Bags if missing
        AutoAssignBag(ref targetBag, targetBagTag);
        AutoAssignBag(ref secondaryBag, secondaryBagTag);

        // Find all interactive balls (Blue and Red)
        UpdateGlobalBallList();
    }

    private void AutoAssignBag(ref Transform bagTransform, string bagTag)
    {
        if (bagTransform == null && !string.IsNullOrEmpty(bagTag))
        {
            GameObject[] bags = GameObject.FindGameObjectsWithTag(bagTag);
            foreach (var b in bags)
            {
                if (Mathf.Abs(b.transform.position.y - transform.position.y) < 4.0f)
                {
                    bagTransform = b.transform;
                    break;
                }
            }
        }
    }

    private void UpdateGlobalBallList()
    {
        allTargetBalls.Clear();
        foreach (GameObject ball in GameObject.FindGameObjectsWithTag(targetBallTag))
        {
            if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 4.0f)
                allTargetBalls.Add(ball);
        }
        
        if (!string.IsNullOrEmpty(secondaryBallTag))
        {
            foreach (GameObject ball in GameObject.FindGameObjectsWithTag(secondaryBallTag))
            {
                if (Mathf.Abs(ball.transform.position.y - transform.position.y) < 4.0f)
                    allTargetBalls.Add(ball);
            }
        }
    }

    private void Update()
    {
        if (isDepositingAnimation || !canSort) return;

        allTargetBalls.RemoveAll(b => b == null);

        // Decide Mode: Collect or Deposit?
        if (inventory.Count >= maxCapacity || (allTargetBalls.Count == 0 && inventory.Count > 0))
        {
            // DEPOSIT MODE
            // Find which bag to go to based on the FIRST item in our hands
            string targetBagForDeposit = inventory[0].bagTag;
            Transform bagTrans = GetBagTransformForTag(targetBagForDeposit);

            MoveTowards(bagTrans);

            if (bagTrans != null && Mathf.Abs(transform.position.x - bagTrans.position.x) < 0.1f)
            {
                StartCoroutine(DepositBallsRoutine(targetBagForDeposit, bagTrans));
            }
        }
        else if (allTargetBalls.Count > 0)
        {
            // COLLECT MODE
            FindClosestBall();

            if (currentTargetBall != null)
            {
                MoveTowards(currentTargetBall);

                if (Mathf.Abs(transform.position.x - currentTargetBall.position.x) < 0.1f)
                {
                    HandleCollision(currentTargetBall.gameObject);
                }
            }
        }
    }

    private void MoveTowards(Transform target)
    {
        if (target == null) return;

        NPCMotor motor = GetComponent<NPCMotor>();
        if (motor != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            motor.Walk(new Vector2(dir.x, 0));
        }
        else
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(target.position.x, transform.position.y), step);
        }

        // Flip sprite manually if NPCSpriteAnimator isn't doing it natively
        if (target.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else if (target.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    private void FindClosestBall()
    {
        currentTargetBall = null;
        float minDistance = float.MaxValue;
        
        foreach (var ball in allTargetBalls)
        {
            float dist = Vector2.Distance(transform.position, ball.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                currentTargetBall = ball.transform;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleCollision(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision.gameObject);

    private void HandleCollision(GameObject other)
    {
        if (isDepositingAnimation) return;

        // Check if picking up a ball
        if (inventory.Count < maxCapacity)
        {
            if (other.CompareTag(targetBallTag) || (!string.IsNullOrEmpty(secondaryBallTag) && other.CompareTag(secondaryBallTag)))
            {
                string bagForThisBall = GetBagTagForBall(other.tag);
                
                Sprite ballSprite = null;
                SpriteRenderer ballSR = other.GetComponent<SpriteRenderer>();
                if (ballSR != null) ballSprite = ballSR.sprite;

                allTargetBalls.Remove(other);
                Destroy(other);

                CreateIndicator(ballSprite, bagForThisBall);
                return;
            }
        }

        // Check if dropped off at a bag
        if (inventory.Count > 0)
        {
            if (other.CompareTag(targetBagTag) || other.CompareTag(secondaryBagTag))
            {
                if (HasBallForBag(other.tag))
                {
                    Transform bagTrans = GetBagTransformForTag(other.tag);
                    StartCoroutine(DepositBallsRoutine(other.tag, bagTrans));
                }
            }
        }
    }

    private void CreateIndicator(Sprite sprite, string bagTag)
    {
        GameObject indicator = new GameObject("NPCBallIndicator");
        indicator.transform.SetParent(transform);
        
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        
        HeldBall hb = new HeldBall { bagTag = bagTag, indicator = indicator };
        inventory.Add(hb);

        UpdateIndicatorPositions();
    }

    private void UpdateIndicatorPositions()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            float offsetX = (i * indicatorSpacing.x) - ((inventory.Count - 1) * indicatorSpacing.x / 2f);
            inventory[i].indicator.transform.localPosition = indicatorStartOffset + new Vector3(offsetX, 0, 0);
        }
    }

    private IEnumerator DepositBallsRoutine(string targetBagTagForDeposit, Transform bagTransform)
    {
        isDepositingAnimation = true;

        if (GetComponent<NPCMotor>() != null) GetComponent<NPCMotor>().Stop();

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].bagTag == targetBagTagForDeposit)
            {
                GameObject indicator = inventory[i].indicator;
                inventory.RemoveAt(i);
                
                StartCoroutine(AnimateIndicatorIntoBag(indicator, bagTransform));
                UpdateIndicatorPositions();
                
                yield return new WaitForSeconds(0.15f);
            }
        }

        yield return new WaitForSeconds(0.35f);
        isDepositingAnimation = false;
        
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        bool areBallsRemaining = false;

        if (FindObjectsOfType<BallPickup>().Length > 0)
            areBallsRemaining = true;

        if (IsHoldingBalls() || IsDepositing())
            areBallsRemaining = true;

        if (!areBallsRemaining)
        {
            Debug.Log("LEVEL COMPLETE! All balls sorted - Triggering Scene Loader!");
            SceneLoader loader = FindObjectOfType<SceneLoader>();
            if (loader != null) loader.LoadScene();
        }
    }

    private IEnumerator AnimateIndicatorIntoBag(GameObject indicator, Transform destination)
    {
        indicator.transform.SetParent(null);
        
        Vector3 startPos = indicator.transform.position;
        Vector3 targetPos = destination.position + new Vector3(0, 0.5f, 0);
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

    // --- Helpers ---
    private string GetBagTagForBall(string ballTag)
    {
        if (ballTag == targetBallTag) return targetBagTag;
        if (ballTag == secondaryBallTag) return secondaryBagTag;
        return null;
    }

    private Transform GetBagTransformForTag(string bagTag)
    {
        if (bagTag == targetBagTag) return targetBag;
        if (bagTag == secondaryBagTag) return secondaryBag;
        return null;
    }

    private bool HasBallForBag(string bagTag)
    {
        foreach (var b in inventory) if (b.bagTag == bagTag) return true;
        return false;
    }

    public bool IsHoldingBalls() => inventory.Count > 0;
    public bool IsDepositing() => isDepositingAnimation;
}
