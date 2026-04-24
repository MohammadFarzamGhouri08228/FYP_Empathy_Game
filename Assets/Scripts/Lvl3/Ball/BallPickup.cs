using UnityEngine;
using System.Collections;

public class BallPickup : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.D;
    [SerializeField] private float interactRange = 2.5f;
    [Tooltip("Optional child object (text / sprite) shown when the player can interact.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Ball Indicator")]
    [Tooltip("Drag the Sprite/Image here that you want to show above the player's head when carrying the ball.")]
    [SerializeField] private Sprite ballIndicatorSprite;
    [Tooltip("Offset from the player's pivot where the indicator floats.")]
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 1.5f, 0f);

    public static bool hasBall = false;
    public static GameObject ballIndicatorInstance;

    // Used to prevent picking up multiple balls in the exact same frame
    private static int lastPickupFrame = -1;
    private static bool isShaking = false;

    private Transform playerTransform;
    private bool isCollected = false;

    // References to components so we can hide/show the ball instead of destroying it
    private Renderer[] allRenderers;
    private Collider2D[] allColliders;
    private Rigidbody2D rb;

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

        // Cache components
        allRenderers = GetComponentsInChildren<Renderer>();
        allColliders = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // If the player is carrying the ball, wait for drop input
        if (isCollected)
        {
            if (Input.GetKeyDown(dropKey))
            {
                DropBall();
            }
            return;
        }

        // Otherwise, check if player is close enough to pick it up
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRange;

        if (interactPrompt != null)
        {
            // Only show the prompt if we are in range and not already carrying a ball
            // (or if we are, we can still show it to let them know they can interact, but it will shake instead)
            interactPrompt.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
            if (hasBall)
            {
                // Trigger screen shake if we try to pick up another ball while holding one!
                // But ensure we don't shake on the exact same frame we just picked one up (if 2 balls are next to each other)
                if (lastPickupFrame != Time.frameCount)
                {
                    StartCoroutine(ScreenShake());
                }
            }
            else
            {
                CollectBall(playerTransform.gameObject);
            }
        }
    }

    private void CollectBall(GameObject player)
    {
        // Double check to absolutely ensure we don't pick up two balls at once
        if (isCollected || hasBall) return;
        
        lastPickupFrame = Time.frameCount; // Record the frame so other balls know we picked one up right now
        isCollected = true;
        hasBall = true;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        // 1. Hide the ball and disable its physics
        foreach (var r in allRenderers) r.enabled = false;
        foreach (var c in allColliders) c.enabled = false;
        if (rb != null) rb.simulated = false;

        // 2. Parent it to the player so it follows them around invisibly
        transform.SetParent(player.transform);
        transform.localPosition = Vector3.zero; // Center it on the player

        // 3. Create a new GameObject to hold the sprite above the player's head
        if (ballIndicatorSprite != null)
        {
            ballIndicatorInstance = new GameObject("BallIndicator");
            ballIndicatorInstance.transform.SetParent(player.transform);
            ballIndicatorInstance.transform.localPosition = indicatorOffset;
            
            // Add a SpriteRenderer and assign the image
            SpriteRenderer sr = ballIndicatorInstance.AddComponent<SpriteRenderer>();
            sr.sprite = ballIndicatorSprite;
            sr.sortingOrder = 10; // Make sure it renders in front of other things
        }
    }

    private void DropBall()
    {
        if (!isCollected) return;
        isCollected = false;
        hasBall = false;

        // 1. Remove the indicator above the player's head
        if (ballIndicatorInstance != null)
        {
            Destroy(ballIndicatorInstance);
            ballIndicatorInstance = null;
        }

        // 2. Unparent the ball and place it slightly above the player's position
        transform.SetParent(null);
        transform.position = playerTransform.position + new Vector3(0, 0.5f, 0);

        // 3. Re-enable graphics and physics
        foreach (var r in allRenderers) r.enabled = true;
        foreach (var c in allColliders) c.enabled = true;
        if (rb != null) 
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero; // Reset any leftover momentum
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>Call this method when the ball is inserted into a bag.</summary>
    public void ConsumeBall()
    {
        isCollected = false;
        hasBall = false;

        if (ballIndicatorInstance != null)
        {
            Destroy(ballIndicatorInstance);
            ballIndicatorInstance = null;
        }

        // Destroy the actual ball object completely
        Destroy(gameObject);
    }

    /// <summary>Call on scene restart / game over to clear ball state.</summary>
    public static void ResetBall()
    {
        hasBall = false;
        isShaking = false;
        if (ballIndicatorInstance != null)
        {
            Destroy(ballIndicatorInstance);
            ballIndicatorInstance = null;
        }
    }

    // A simple screen shake effect
    private IEnumerator ScreenShake()
    {
        if (isShaking) yield break;
        isShaking = true;

        Camera mainCam = Camera.main;
        if (mainCam == null) 
        {
            isShaking = false;
            yield break;
        }

        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.2f;   // How long the shake lasts
        float magnitude = 0.15f; // How intense the shake is

        while (elapsed < duration)
        {
            if (mainCam != null)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mainCam != null)
        {
            mainCam.transform.localPosition = originalPos;
        }
        isShaking = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a green sphere in the Scene view to visualize the interact range
        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
