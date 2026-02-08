using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private float pressDistance = 0.15f; // How far the button moves down
    [SerializeField] private float pressSpeed = 8f; // How fast the button presses down

    [Header("Visuals (Optional)")]
    [SerializeField] private Sprite onSprite;  // Sprite when pressed
    [SerializeField] private Sprite offSprite; // Sprite when not pressed

    [Header("Bomb Settings")]
    [SerializeField] private string bombTag = ""; // Optional: only destroy bombs with this tag
    [SerializeField] private bool destroyAllBombs = false; // If true, destroys every BombInteraction in scene

    [Header("Bridge Settings")]
    [Tooltip("Assign an ExpandableBridge to make it expand while the player stands on this button.")]
    [SerializeField] private ExpandableBridge bridge;

    [Tooltip("If true, the button stays pressed permanently after first activation (one-shot). " +
             "If false, the button releases when the player steps off (hold mode).")]
    [SerializeField] private bool oneShot = false;

    private Vector3 unpressedPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        unpressedPosition = transform.position;
        pressedPosition = unpressedPosition + Vector3.down * pressDistance;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set initial sprite
        if (spriteRenderer != null && offSprite != null)
        {
            spriteRenderer.sprite = offSprite;
        }
    }

    void Update()
    {
        // Smoothly move toward target position
        Vector3 target = isPressed ? pressedPosition : unpressedPosition;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * pressSpeed);
    }

    // --- Trigger callbacks ---

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPressed) return; // Already pressed, do nothing

        if (other.CompareTag("Player"))
        {
            PressButton();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isPressed) return;
        if (oneShot) return; // One-shot buttons stay pressed forever

        if (other.CompareTag("Player"))
        {
            ReleaseButton();
        }
    }

    // --- Press / Release ---

    private void PressButton()
    {
        isPressed = true;
        Debug.Log("FloorButton: Button pressed!");

        // Swap to pressed sprite
        if (spriteRenderer != null && onSprite != null)
            spriteRenderer.sprite = onSprite;

        // Expand bridge
        if (bridge != null)
            bridge.Expand();

        // Destroy bombs (if configured)
        if (destroyAllBombs || !string.IsNullOrEmpty(bombTag))
            DestroyBombs();
    }

    private void ReleaseButton()
    {
        isPressed = false;
        Debug.Log("FloorButton: Button released!");

        // Swap back to unpressed sprite
        if (spriteRenderer != null && offSprite != null)
            spriteRenderer.sprite = offSprite;

        // Contract bridge
        if (bridge != null)
            bridge.Contract();
    }

    // --- Bomb logic (unchanged) ---

    private void DestroyBombs()
    {
        if (destroyAllBombs)
        {
            BombInteraction[] bombs = FindObjectsByType<BombInteraction>(FindObjectsSortMode.None);
            Debug.Log($"FloorButton: Found {bombs.Length} bomb(s) to destroy.");

            foreach (BombInteraction bomb in bombs)
            {
                Destroy(bomb.gameObject);
            }
        }
        else if (!string.IsNullOrEmpty(bombTag))
        {
            GameObject[] taggedBombs = GameObject.FindGameObjectsWithTag(bombTag);
            Debug.Log($"FloorButton: Found {taggedBombs.Length} tagged bomb(s) to destroy.");

            foreach (GameObject bomb in taggedBombs)
            {
                Destroy(bomb);
            }
        }
    }
}
