using UnityEngine;
using System.Collections;

public class BagInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 2.5f;
    [Tooltip("Optional child object (text / sprite) shown when the player can deposit the ball.")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Animation Settings")]
    [Tooltip("How much the bag bulges when a ball is inserted.")]
    [SerializeField] private Vector3 bulgeScale = new Vector3(1.3f, 0.8f, 1f);
    [Tooltip("How fast the bag animates.")]
    [SerializeField] private float animationSpeed = 7f;

    private Vector3 originalScale;
    private Transform playerTransform;

    void Start()
    {
        originalScale = transform.localScale;
        
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
        if (playerTransform == null) return;

        // Check if player is near AND holding the ball
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRange && BallPickup.hasBall;

        // Only show prompt if the player is in range AND actually carrying a ball
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(inRange);
        }

        // Press E to deposit the ball into the bag
        if (inRange && Input.GetKeyDown(interactKey))
        {
            InsertBall();
        }
    }

    private void InsertBall()
    {
        // Hide the prompt
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Find the currently held ball and consume it
        BallPickup heldBall = Object.FindObjectOfType<BallPickup>();
        if (heldBall != null)
        {
            heldBall.ConsumeBall();
        }

        // Play the squish/bulge animation via code so you don't have to make Animator clips
        StopAllCoroutines();
        StartCoroutine(SquishAnimation());

        // Add any score/logic here later!
        Debug.Log("Ball inserted into the bag!");
    }

    private IEnumerator SquishAnimation()
    {
        // Scale up (squash down and bulge wide)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localScale = Vector3.Lerp(originalScale, bulgeScale, t);
            yield return null;
        }

        // Scale back to normal
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localScale = Vector3.Lerp(bulgeScale, originalScale, t);
            yield return null;
        }

        // Ensure it exactly resets
        transform.localScale = originalScale;
    }

    void OnDrawGizmosSelected()
    {
        // Draw an orange sphere in the Scene view to visualize the interact range for the bag
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}