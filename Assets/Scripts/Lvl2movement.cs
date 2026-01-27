/*
THIS IS OUR PLAYER MOVEMENT SCRIPT FOR LEVEL 2
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class Lvl2movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Climbing Sprites")]
    [SerializeField] public Sprite player_climb1;
    [SerializeField] public Sprite playerclimb_2;
    
    [Header("Rendering Settings")]
    [SerializeField] private int playerSortingOrder = 15; // Higher than spikes (which use max 10)
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    public bool isClimbing = false;
    private float climbAnimationTimer = 0f;
    
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
            }
            else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            {
                moveInput.x = 1f;
            }
            
            // Vertical movement (Up/Down)
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            {
                moveInput.y = 1f;
            }
            else if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            {
                moveInput.y = -1f;
            }
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (spriteRenderer == null) return;

        if (isClimbing)
        {
            // Handle climbing animation
            if (Mathf.Abs(moveInput.y) > 0.1f || Mathf.Abs(moveInput.x) > 0.1f)
            {
                climbAnimationTimer += Time.deltaTime;
                float climbCycleSpeed = 0.2f;

                if (climbAnimationTimer >= climbCycleSpeed * 2)
                {
                    climbAnimationTimer = 0f;
                }

                if (climbAnimationTimer < climbCycleSpeed && player_climb1 != null)
                {
                    spriteRenderer.sprite = player_climb1;
                }
                else if (playerclimb_2 != null)
                {
                    spriteRenderer.sprite = playerclimb_2;
                }
            }
            else if (player_climb1 != null)
            {
                spriteRenderer.sprite = player_climb1; // Idle on ladder
            }
        }
        else
        {
            // Flip sprite based on movement direction (only when not climbing)
            if (moveInput.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (moveInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;
        
        if (isClimbing)
        {
            // Apply movement without gravity
            rb.gravityScale = 0;
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            rb.gravityScale = 1;
            // Apply movement normalized for top-down or platformer movement
            Vector2 movement = moveInput.normalized * moveSpeed;
            rb.linearVelocity = movement;
        }
    }
}
