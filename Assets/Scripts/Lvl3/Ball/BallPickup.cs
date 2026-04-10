using UnityEngine;

public class BallPickup : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
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
        if (isCollected || playerTransform == null) return;

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

        // Create a new GameObject to hold the sprite above the player's head
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
