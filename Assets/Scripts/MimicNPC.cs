using UnityEngine;
using System.Collections.Generic;

public class MimicNPC : MonoBehaviour
{
    [System.Serializable]
    public struct PlayerState
    {
        public Vector3 position;
        public bool flipX;
        public bool isWalking;
        public bool isJumping;

        public PlayerState(Vector3 pos, bool flip, bool walking, bool jumping)
        {
            position = pos;
            flipX = flip;
            isWalking = walking;
            isJumping = jumping;
        }
    }

    [Header("References")]
    public Transform playerTransform;
    public PlayerController playerController;
    private SpriteRenderer npcSprite;
    private Animator npcAnimator;

    [Header("Mimic Settings")]
    [Tooltip("How many frames of delay (e.g., 50 frames is ~1 second)")]
    public int delayFrames = 40;
    
    [Tooltip("Smoothing speed for position transitions")]
    public float lerpSpeed = 15f;

    [Header("Offset Settings")]
    [Tooltip("Horizontal distance to stay away from the player (negative = left)")]
    public float horizontalOffset = -1.5f;

    private Queue<PlayerState> stateBuffer = new Queue<PlayerState>();

    void Start()
    {
        npcSprite = GetComponent<SpriteRenderer>();
        npcAnimator = GetComponent<Animator>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 1. Record current player state
        RecordState();

        // 2. If we have reached the delay threshold, start mimicking
        if (stateBuffer.Count >= delayFrames)
        {
            ApplyState(stateBuffer.Dequeue());
        }
    }

    private void RecordState()
    {
        SpriteRenderer playerSprite = playerTransform.GetComponent<SpriteRenderer>();
        bool flip = playerSprite != null ? playerSprite.flipX : false;
        
        // Use the newly public PlayerController states
        bool walking = playerController != null ? playerController.isWalking : false;
        bool jumping = playerController != null ? playerController.isJumping : false;

        // Create a target position that is offset from the player
        Vector3 targetPos = playerTransform.position + new Vector3(horizontalOffset, 0, 0);

        PlayerState currentState = new PlayerState(
            targetPos, 
            flip, 
            walking, 
            jumping
        );

        stateBuffer.Enqueue(currentState);
    }

    private void ApplyState(PlayerState state)
    {
        // Smoothly move to the recorded position
        transform.position = Vector3.Lerp(transform.position, state.position, Time.fixedDeltaTime * lerpSpeed);

        // Mimic the flip
        if (npcSprite != null)
        {
            npcSprite.flipX = state.flipX;
        }

        // Mimic animations
        if (npcAnimator != null)
        {
            // If the NPC is very close to its target and the player isn't moving, 
            // we might want to force idle even if the recorded state said 'walking'
            // but for true mimicry, we follow the recorded state.
            bool isActuallyMoving = Vector3.Distance(transform.position, state.position) > 0.05f;
            bool shouldShowWalking = state.isWalking && isActuallyMoving;

            foreach (AnimatorControllerParameter param in npcAnimator.parameters)
            {
                if (param.name == "isWalking") npcAnimator.SetBool("isWalking", shouldShowWalking);
                if (param.name == "isJumping") npcAnimator.SetBool("isJumping", state.isJumping);
            }
        }
    }
}
