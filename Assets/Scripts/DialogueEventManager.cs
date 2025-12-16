using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages dialogue triggered by game events
/// Listens to GameEventManager and displays appropriate dialogue using TextMeshPro
/// </summary>
public class DialogueEventManager : MonoBehaviour
{
    [Header("TextMeshPro Reference")]
    [Tooltip("Reference to TextMeshProUGUI component that displays dialogue. If null, will search for it in the scene.")]
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    [Header("Event Dialogue Settings")]
    [Tooltip("Dialogue messages for each event type (including checkpoints)")]
    [SerializeField] private EventDialogueData[] eventDialogues;
    
    [Header("Display Settings")]
    [Tooltip("Hide dialogue text on start (only show when event is triggered)")]
    [SerializeField] private bool hideOnStart = true;
    
    [Tooltip("Duration to show dialogue before clearing (0 = don't auto-clear)")]
    [SerializeField] private float dialogueDisplayDuration = 3f;
    
    [Tooltip("If true, will restore previous dialogue text after clearing event dialogue")]
    [SerializeField] private bool restorePreviousText = false;
    
    private Dictionary<GameEventType, string> dialogueDictionary;
    private string previousText = ""; // Store previous text to restore
    
    void Start()
    {
        Debug.Log("DialogueEventManager: Start() called");
        
        // Find TextMeshPro if not assigned
        if (textMeshPro == null)
        {
            Debug.Log("DialogueEventManager: TextMeshPro not assigned, searching...");
            textMeshPro = FindFirstObjectByType<TextMeshProUGUI>();
            
            if (textMeshPro == null)
            {
                Debug.LogError("DialogueEventManager: TextMeshProUGUI not found! Please assign one in the Inspector or ensure one exists in the scene.");
                return;
            }
            else
            {
                Debug.Log($"DialogueEventManager: Found TextMeshPro: {textMeshPro.name}");
            }
        }
        else
        {
            Debug.Log($"DialogueEventManager: Using assigned TextMeshPro: {textMeshPro.name}");
        }
        
        // Hide dialogue text on start if enabled
        if (hideOnStart && textMeshPro != null)
        {
            textMeshPro.text = "";
            textMeshPro.gameObject.SetActive(false);
            Debug.Log("DialogueEventManager: Dialogue text hidden on start.");
        }
        
        // Store initial text if we want to restore it
        if (restorePreviousText)
        {
            previousText = textMeshPro.text;
        }
        
        // Build dialogue dictionary from serialized data
        dialogueDictionary = new Dictionary<GameEventType, string>();
        if (eventDialogues != null && eventDialogues.Length > 0)
        {
            Debug.Log($"DialogueEventManager: Processing {eventDialogues.Length} event dialogue entries...");
            foreach (var eventDialogue in eventDialogues)
            {
                if (eventDialogue != null && !string.IsNullOrEmpty(eventDialogue.dialogueText))
                {
                    dialogueDictionary[eventDialogue.eventType] = eventDialogue.dialogueText;
                    Debug.Log($"DialogueEventManager: Added dialogue for {eventDialogue.eventType}: '{eventDialogue.dialogueText}'");
                }
                else
                {
                    Debug.LogWarning("DialogueEventManager: Skipping null or empty event dialogue entry");
                }
            }
        }
        else
        {
            Debug.LogWarning("DialogueEventManager: No event dialogues configured! Please add entries in the Inspector.");
        }
        
        // Subscribe to game events
        GameEventManager.OnGameEvent += HandleGameEvent;
        Debug.Log("DialogueEventManager: Subscribed to GameEventManager.OnGameEvent");
        
        Debug.Log($"DialogueEventManager: Initialized successfully with {dialogueDictionary.Count} event dialogues.");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from game events
        GameEventManager.OnGameEvent -= HandleGameEvent;
    }
    
    /// <summary>
    /// Handles game events and displays appropriate dialogue
    /// </summary>
    private void HandleGameEvent(GameEventType eventType, GameObject source)
    {
        Debug.Log($"DialogueEventManager: HandleGameEvent called for {eventType} from {source?.name ?? "Unknown"}");
        
        if (dialogueDictionary == null)
        {
            Debug.LogError("DialogueEventManager: dialogueDictionary is null! Initialization may have failed.");
            return;
        }
        
        // Handle all events the same way (including checkpoint events)
        if (dialogueDictionary.ContainsKey(eventType))
        {
            string dialogueText = dialogueDictionary[eventType];
            Debug.Log($"DialogueEventManager: Found dialogue text for {eventType}: '{dialogueText}'");
            ShowDialogue(dialogueText);
            Debug.Log($"DialogueEventManager: Showing dialogue for event {eventType}: '{dialogueText}'");
        }
        else
        {
            Debug.LogWarning($"DialogueEventManager: No dialogue found for event type {eventType}. Available keys: {string.Join(", ", dialogueDictionary.Keys)}");
            Debug.LogWarning($"DialogueEventManager: Make sure to add an entry for {eventType} in the Event Dialogue Settings!");
        }
    }
    
    /// <summary>
    /// Shows dialogue using TextMeshPro
    /// </summary>
    private void ShowDialogue(string text)
    {
        Debug.Log($"DialogueEventManager: ShowDialogue called with text: '{text}'");
        
        if (textMeshPro == null)
        {
            Debug.LogError("DialogueEventManager: Cannot show dialogue - TextMeshPro is null!");
            return;
        }
        
        Debug.Log($"DialogueEventManager: TextMeshPro found: {textMeshPro.name}, current text: '{textMeshPro.text}'");
        
        // Store current text if we want to restore it
        if (restorePreviousText && string.IsNullOrEmpty(previousText))
        {
            previousText = textMeshPro.text;
            Debug.Log($"DialogueEventManager: Stored previous text: '{previousText}'");
        }
        
        // Show the dialogue text
        textMeshPro.text = text;
        textMeshPro.gameObject.SetActive(true);
        
        Debug.Log($"DialogueEventManager: Set TextMeshPro text to: '{textMeshPro.text}', GameObject active: {textMeshPro.gameObject.activeSelf}");
        
        // Auto-clear after duration if set
        if (dialogueDisplayDuration > 0f)
        {
            Debug.Log($"DialogueEventManager: Starting auto-clear coroutine for {dialogueDisplayDuration} seconds");
            StartCoroutine(AutoClearDialogue(dialogueDisplayDuration));
        }
    }
    
    /// <summary>
    /// Test method to manually trigger dialogue (for debugging)
    /// </summary>
    [ContextMenu("Test Bomb Dialogue")]
    public void TestBombDialogue()
    {
        Debug.Log("DialogueEventManager: TestBombDialogue called");
        GameEventManager.TriggerEvent(GameEventType.BombEncountered, gameObject);
    }
    
    /// <summary>
    /// Coroutine to auto-clear dialogue after duration
    /// </summary>
    private System.Collections.IEnumerator AutoClearDialogue(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (textMeshPro != null)
        {
            if (restorePreviousText && !string.IsNullOrEmpty(previousText))
            {
                // Restore previous text
                textMeshPro.text = previousText;
                previousText = ""; // Clear stored text
            }
            else
            {
                // Just clear the text
                textMeshPro.text = "";
            }
        }
    }
}

/// <summary>
/// Serializable class for event dialogue data
/// </summary>
[System.Serializable]
public class EventDialogueData
{
    public GameEventType eventType;
    [TextArea(2, 5)]
    public string dialogueText;
}
