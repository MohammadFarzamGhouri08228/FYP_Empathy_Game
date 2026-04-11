using UnityEngine;

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
            interactPrompt.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
            CollectBall(playerTransform.gameObject);
        }
    }

    private void CollectBall(GameObject player)
    {
        if (isCollected) return;
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
            rb.velocity = Vector2.zero; // Reset any leftover momentum
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
        if (ballIndicatorInstance != null)
        {
            Destroy(ballIndicatorInstance);
            ballIndicatorInstance = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a green sphere in the Scene view to visualize the interact range
        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
