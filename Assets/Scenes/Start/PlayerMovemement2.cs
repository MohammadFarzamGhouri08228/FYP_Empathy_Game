using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D component not found!");
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
