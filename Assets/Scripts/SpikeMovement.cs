using UnityEngine;

public class SpikeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // Speed of up/down movement
    [SerializeField] private float moveDistance = 2f; // How far up and down the spike moves
    [SerializeField] private bool startMovingUp = true; // Whether to start moving up or down
    
    [Header("Sorting Order Settings")]
    [SerializeField] private int sortingOrderWhenUp = 10; // Order in layer when spike is up (visible above walls)
    [SerializeField] private int sortingOrderWhenDown = 0; // Order in layer when spike is down (hidden behind walls)
    
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private float currentTime = 0f;
    private bool isMovingUp;
    
    void Start()
    {
        // Store the starting position
        startPosition = transform.position;
        
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpikeMovement: SpriteRenderer component not found! Sorting order will not be updated.");
        }
        
        // Set initial movement direction
        isMovingUp = startMovingUp;
        
        // Initialize sorting order based on starting position
        UpdateSortingOrder();
    }
    
    void Update()
    {
        // Calculate movement using sine wave for smooth up/down motion
        // This creates a continuous back-and-forth movement
        currentTime += Time.deltaTime * moveSpeed;
        
        // Use sine wave: -1 to 1, then multiply by moveDistance
        float offset = Mathf.Sin(currentTime) * moveDistance;
        
        // Update position
        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + offset,
            startPosition.z
        );
        
        // Update sorting order based on current Y position
        UpdateSortingOrder();
    }
    
    private void UpdateSortingOrder()
    {
        if (spriteRenderer == null) return;
        
        // Compare current Y position to starting Y position
        // If current Y is higher than start, spike is "up"
        // If current Y is lower than start, spike is "down"
        float currentY = transform.position.y;
        float startY = startPosition.y;
        
        // Determine if spike is currently up or down
        // Using a small threshold to avoid flickering
        bool isCurrentlyUp = currentY > startY + 0.1f;
        
        // Set sorting order based on position
        if (isCurrentlyUp)
        {
            spriteRenderer.sortingOrder = sortingOrderWhenUp;
        }
        else
        {
            spriteRenderer.sortingOrder = sortingOrderWhenDown;
        }
    }
    
    // Optional: Method to reset position if needed
    public void ResetPosition()
    {
        transform.position = startPosition;
        currentTime = 0f;
        UpdateSortingOrder();
    }
}

