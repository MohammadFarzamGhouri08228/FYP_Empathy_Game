/*
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueBoxExample : MonoBehaviour
{
    [Header("Dialogue Manager Reference")]
    [Tooltip("Drag the DialogueBoxManager GameObject here, or leave null to auto-find")]
    [SerializeField] private DialogueBoxManager dialogueManager;
    
    [Header("Test Settings")]
    [Tooltip("Test dialogue messages")]
    [SerializeField] private string[] testMessages = {
        "Hello! This is a test message.",
        "This uses a different sprite!",
        "Watch out for bombs!",
        "Good luck on your journey!"
    };
    
    [Tooltip("Sprite indices to use for each message")]
    [SerializeField] private int[] spriteIndices = { 0, 1, 2, 0 };
    
    [Tooltip("Target to follow (optional)")]
    [SerializeField] private Transform followTarget;
    
    [Tooltip("Offset when following target")]
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1.5f, 0);
    
    private int currentMessageIndex = 0;
    
    void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueBoxManager>();
            
            if (dialogueManager == null)
            {
                Debug.LogError("DialogueBoxExample: DialogueBoxManager not found! Please assign it in the Inspector or create one in the scene.");
            }
        }
    }
    
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShowNextDialogue();
        }
        
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            HideDialogue();
        }
        
        if (Keyboard.current.digit1Key.wasPressedThisFrame) ShowDialogueWithSprite(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) ShowDialogueWithSprite(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) ShowDialogueWithSprite(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) ShowDialogueWithSprite(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) ShowDialogueWithSprite(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) ShowDialogueWithSprite(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) ShowDialogueWithSprite(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) ShowDialogueWithSprite(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) ShowDialogueWithSprite(8);
    }
    
    public void ShowNextDialogue()
    {
        if (dialogueManager == null) return;
        
        if (testMessages == null || testMessages.Length == 0)
        {
            Debug.LogWarning("DialogueBoxExample: No test messages configured!");
            return;
        }
        
        string message = testMessages[currentMessageIndex % testMessages.Length];
        int spriteIndex = (spriteIndices != null && spriteIndices.Length > currentMessageIndex) 
            ? spriteIndices[currentMessageIndex % spriteIndices.Length] 
            : 0;
        
        if (followTarget != null)
        {
            dialogueManager.ShowDialogueFollowing(message, followTarget, followOffset, spriteIndex);
        }
        else
        {
            dialogueManager.ShowDialogue(message, spriteIndex);
        }
        
        currentMessageIndex++;
    }
    
    public void ShowDialogueWithSprite(int spriteIndex)
    {
        if (dialogueManager == null) return;
        
        string message = $"This is sprite index {spriteIndex}!";
        dialogueManager.ShowDialogue(message, spriteIndex);
    }
    
    public void HideDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.HideDialogue();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowNextDialogue();
        }
    }
    
    public void ShowDialogueAtPosition(Vector3 worldPosition)
    {
        if (dialogueManager == null) return;
        
        dialogueManager.ShowDialogueAtPosition("Dialogue at position!", worldPosition);
    }
}
*/

