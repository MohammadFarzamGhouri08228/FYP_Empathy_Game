using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Rendering Settings")]
    [SerializeField] private int playerSortingOrder = 15; // Higher than spikes (which use max 10)
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D component not found!");
        }
        
        // Set player sorting order to ensure they render above spikes and other obstacles
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = playerSortingOrder;
            Debug.Log($"PlayerController2: Sorting order set to {playerSortingOrder}");
        }
        else
        {
            Debug.LogWarning("PlayerController2: SpriteRenderer component not found! Player may not render correctly.");
        }
    }
    
    void Update()
    {
        if (rb == null) return;
        
        // Get input for both X and Y axes
        moveInput = Vector2.zero;
        
        if (Keyboard.current != null)
        {
            // Horizontal movement (Left/Right)
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            {
                moveInput.x = -1f;
                if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                {
                    Debug.Log("Player moving LEFT");
                }
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveInput.x = 1f;
                if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                {
                    Debug.Log("Player moving RIGHT");
                }
            }
            
            // Vertical movement (Up/Down)
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                moveInput.y = 1f;
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                {
                    Debug.Log("Player moving UP");
                }
            }
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                moveInput.y = -1f;
                if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                {
                    Debug.Log("Player moving DOWN");
                }
            }
        }
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;
        
        // Apply movement
        Vector2 movement = moveInput.normalized * moveSpeed;
        rb.linearVelocity = movement;
    }
}
