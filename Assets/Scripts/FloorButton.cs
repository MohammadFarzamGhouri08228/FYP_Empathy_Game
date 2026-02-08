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
    [SerializeField] private bool destroyAllBombs = true; // If true, destroys every BombInteraction in scene

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPressed) return; // Already pressed, do nothing

        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        isPressed = true;
        Debug.Log("FloorButton: Button pressed! Destroying bombs.");

        // Swap to pressed sprite
        if (spriteRenderer != null && onSprite != null)
        {
            spriteRenderer.sprite = onSprite;
        }

        // Destroy all bombs in the scene
        DestroyBombs();
    }

    private void DestroyBombs()
    {
        if (destroyAllBombs)
        {
            // Find and destroy every GameObject with BombInteraction
            BombInteraction[] bombs = FindObjectsByType<BombInteraction>(FindObjectsSortMode.None);
            Debug.Log($"FloorButton: Found {bombs.Length} bomb(s) to destroy.");

            foreach (BombInteraction bomb in bombs)
            {
                Destroy(bomb.gameObject);
            }
        }
        else if (!string.IsNullOrEmpty(bombTag))
        {
            // Only destroy bombs with the specified tag
            GameObject[] taggedBombs = GameObject.FindGameObjectsWithTag(bombTag);
            Debug.Log($"FloorButton: Found {taggedBombs.Length} tagged bomb(s) to destroy.");

            foreach (GameObject bomb in taggedBombs)
            {
                Destroy(bomb);
            }
        }
    }
}
