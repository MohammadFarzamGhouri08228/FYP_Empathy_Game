using UnityEngine;
using System.Collections;

public class BagInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 2.5f;
    [Tooltip("Optional child object (text / sprite) shown when the player can deposit the ball.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Animation Settings")]
    [Tooltip("How long it takes for the ball to fly from the player into the bag.")]
    [SerializeField] private float flyDuration = 0.5f;
    [Tooltip("How long the bag vibrates after swallowing the ball.")]
    [SerializeField] private float vibrateDuration = 0.3f;
    [Tooltip("How intense the vibration is.")]
    [SerializeField] private float vibrateMagnitude = 0.05f;
    
    // We optionally take an offset for where the "opening" of the bag is
    [Tooltip("Offset from the bag's center where the ball should enter.")]
    [SerializeField] private Vector3 bagOpeningOffset = new Vector3(0, 0.5f, 0);

    private Transform playerTransform;
    private bool isAnimating = false; // Prevent multiple deposits at once

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null || isAnimating) return;

        // Check if player is near AND holding the ball
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRange && BallPickup.hasBall;

        // Only show prompt if the player is in range AND actually carrying a ball
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(inRange);
        }

        // Press E to deposit the ball into the bag
        if (inRange && Input.GetKeyDown(interactKey))
        {
            InsertBall();
        }
    }

    private void InsertBall()
    {
        if (isAnimating) return;

        // Hide the prompt
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Find the currently held ball to get its sprite before consuming
        BallPickup heldBall = Object.FindObjectOfType<BallPickup>();
        Sprite ballSprite = null;
        Vector3 startPos = playerTransform.position + new Vector3(0, 1.5f, 0); // Default if indicator is missing

        if (heldBall != null)
        {
            // If the indicator exists above the player's head, steal its position and sprite
            if (BallPickup.ballIndicatorInstance != null)
            {
                startPos = BallPickup.ballIndicatorInstance.transform.position;
                SpriteRenderer sr = BallPickup.ballIndicatorInstance.GetComponent<SpriteRenderer>();
                if (sr != null) ballSprite = sr.sprite;
            }

            // Immediately consume the ball (destroys the real ball and the player's overhead indicator)
            heldBall.ConsumeBall();
        }

        // Start the visual animation of the ball flying into the bag
        StartCoroutine(AnimateBallIntoBag(startPos, ballSprite));
    }

    private IEnumerator AnimateBallIntoBag(Vector3 startPosition, Sprite spriteToFly)
    {
        isAnimating = true;

        // 1. Create a temporary "dummy ball" to fly through the air
        GameObject dummyBall = new GameObject("DummyBall_FlyAnim");
        dummyBall.transform.position = startPosition;
        
        SpriteRenderer sr = dummyBall.AddComponent<SpriteRenderer>();
        sr.sprite = spriteToFly;
        sr.sortingOrder = 15; // Render on top of everything

        Vector3 targetPosition = transform.position + bagOpeningOffset;
        Vector3 initialScale = dummyBall.transform.localScale;
        
        // 2. Move the dummy ball from the player's head to the bag's opening, while shrinking
        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            
            // Smooth step for nicer movement
            float smoothT = t * t * (3f - 2f * t);

            // Lerp position
            dummyBall.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            // Shrink the ball as it goes "into" the bag
            dummyBall.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, smoothT);

            yield return null;
        }

        // Destroy the dummy ball once it's inside
        Destroy(dummyBall);

        // 3. Do a tiny bag vibrate to show it swallowed it
        yield return StartCoroutine(VibrateAnimation());

        isAnimating = false;

        // Add any score/logic here later!
        Debug.Log("Ball successfully inserted into the bag!");
    }

    private IEnumerator VibrateAnimation()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < vibrateDuration)
        {
            float x = Random.Range(-1f, 1f) * vibrateMagnitude;
            float y = Random.Range(-1f, 1f) * vibrateMagnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it exactly resets
        transform.localPosition = originalPos;
    }

    void OnDrawGizmosSelected()
    {
        // Draw an orange sphere in the Scene view to visualize the interact range for the bag
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);

        // Draw a small blue dot to show where the "bag opening" is targeted
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + bagOpeningOffset, 0.15f);
    }
}
