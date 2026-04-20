using UnityEngine;
using TMPro; 

public class CheckpointInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject interactionBubble; 
    public GameObject choicesContainer; 
    public TextMeshPro[] dialogueOptions; 

    [Header("Choice Tracking")]
    [Tooltip("Which checkpoint this interaction belongs to (should match Checkpoint.checkpointID)")]
    [SerializeField] private int checkpointID = 0;
    
    [Tooltip("Empathy category for each dialogue option. Must match dialogueOptions array length.")]
    [SerializeField] private ChoiceCategory[] optionCategories;

    [Header("Text to Speech")]
    public ElevenLabsTTS ttsSystem;
    public bool readOptionsAloud = true;

    [Header("Styling")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow; 

    private bool isPlayerInRange = false;
    private bool isShowingChoices = false; 
    private bool hasMadeChoice = false; // Tracks if we locked in an answer
    private int selectedIndex = 0; 

    void Start()
    {
        // Automatically find the TTS system if it's not assigned
        if (ttsSystem == null)
        {
            ttsSystem = FindFirstObjectByType<ElevenLabsTTS>();
        }

        // Auto-sync checkpointID from Checkpoint component if present
        Checkpoint cp = GetComponent<Checkpoint>();
        if (cp != null)
        {
            checkpointID = cp.CheckpointID;
        }
        else
        {
            Checkpoint2 cp2 = GetComponent<Checkpoint2>();
            if (cp2 != null)
            {
                checkpointID = cp2.CheckpointID;
            }
        }

        // Initialize optionCategories array if not set in Inspector
        if (optionCategories == null || optionCategories.Length != dialogueOptions.Length)
        {
            if (dialogueOptions != null && dialogueOptions.Length > 0)
            {
                Debug.LogWarning($"CheckpointInteraction (CP {checkpointID}): optionCategories length doesn't match dialogueOptions. Initializing all as Neutral.");
                optionCategories = new ChoiceCategory[dialogueOptions.Length];
            }
        }

        ResetInteraction();
    }

    void Update()
    {
        // 1. Trigger the choices to appear
        if (isPlayerInRange && !isShowingChoices && !hasMadeChoice && Input.GetKeyDown(KeyCode.Space))
        {
            ShowChoices();
            return; 
        }

        // 2. Listen for 1, 2, 3, or Space ONLY if choices are showing and a choice hasn't been made yet
        if (isShowingChoices && !hasMadeChoice)
        {
            HandleNavigation();
        }
    }

    private void HandleNavigation()
    {
        // Check for top-row numbers OR numpad numbers
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetSelection(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetSelection(2);
        }

        // Confirm Choice
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteChoice();
        }
    }

    private void SetSelection(int index)
    {
        // Safety check to ensure we don't try to select an option that doesn't exist
        if (index < dialogueOptions.Length)
        {
            selectedIndex = index;
            UpdateColors();
        }
    }

    private void ShowChoices()
    {
        isShowingChoices = true;
        interactionBubble.SetActive(false);
        choicesContainer.SetActive(true);
        
        selectedIndex = 0; // Default highlight to the first option
        UpdateColors();
    }

    private void UpdateColors()
    {
        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            if (i == selectedIndex)
                dialogueOptions[i].color = highlightColor;
            else
                dialogueOptions[i].color = normalColor;
        }
    }

    private void ExecuteChoice()
    {
        hasMadeChoice = true; // Lock in the state so input stops processing

        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            if (i == selectedIndex)
            {
                // Make the chosen text bold and revert its color to normal
                dialogueOptions[i].fontStyle = FontStyles.Bold;
                dialogueOptions[i].color = normalColor; 
            }
            else
            {
                // Turn off the GameObjects of the options we didn't pick
                dialogueOptions[i].gameObject.SetActive(false); 
            }
        }

        // Determine the category of the selected choice
        ChoiceCategory selectedCategory = ChoiceCategory.Neutral;
        if (optionCategories != null && selectedIndex < optionCategories.Length)
        {
            selectedCategory = optionCategories[selectedIndex];
        }

        Debug.Log($"CheckpointInteraction (CP {checkpointID}): Player confirmed Option {selectedIndex + 1} — Category: {selectedCategory}");
        
        // Report choice to CheckpointManager for metric tracking
        CheckpointManager manager = CheckpointManager.Instance;
        if (manager != null)
        {
            manager.RecordChoiceInteraction(checkpointID, selectedCategory);
        }

        // Report to AdaptiveBackend
        if (AdaptiveBackend.Instance != null)
        {
            AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "ChoiceInteraction", selectedCategory.ToString());
        }
        
        // At this point, the chosen text stays on screen until the player walks away
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If Checkpoint.cs is attached, let it manage the range to sync with dialogue!
        if (GetComponent<Checkpoint>() != null) return; 
        if (GetComponent<Checkpoint2>() != null) return; // Also defer to Checkpoint2

        if (collision.CompareTag("Player"))
        {
            SetPlayerInRange(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (GetComponent<Checkpoint>() != null) return; 
        if (GetComponent<Checkpoint2>() != null) return;

        if (collision.CompareTag("Player"))
        {
            SetPlayerInRange(false);
        }
    }
    
    // Expose range setting for distance-based detection from Checkpoint.cs / Checkpoint2.cs
    public void SetPlayerInRange(bool inRange)
    {
        if (isPlayerInRange == inRange) return; // Prevent spamming when called in Update()

        isPlayerInRange = inRange;
        if (inRange)
        {
            if (!isShowingChoices && !hasMadeChoice) 
            {
                interactionBubble.SetActive(true);

                // Read the interaction bubble text if TTS is available
                if (readOptionsAloud && ttsSystem != null)
                {
                    TextMeshPro bubbleText = interactionBubble.GetComponentInChildren<TextMeshPro>();
                    if (bubbleText != null && !string.IsNullOrWhiteSpace(bubbleText.text))
                    {
                        ttsSystem.Speak(bubbleText.text);
                    }
                }
            }
        }
        else
        {
            // Player walked away without making a choice — track as lowest empathy
            if (isShowingChoices && !hasMadeChoice)
            {
                Debug.Log($"CheckpointInteraction (CP {checkpointID}): Player walked away without choosing. Recording as NoChoice.");
                
                CheckpointManager manager = CheckpointManager.Instance;
                if (manager != null)
                {
                    manager.RecordChoiceInteraction(checkpointID, ChoiceCategory.Neutral); // Walked away = neutral/no engagement
                }

                if (AdaptiveBackend.Instance != null)
                {
                    AdaptiveBackend.Instance.ReceiveData($"Checkpoint_{checkpointID}", "ChoiceInteraction", "NoChoice_WalkedAway");
                }
            }

            ResetInteraction();
        }
    }

    private void ResetInteraction()
    {
        isPlayerInRange = false;
        isShowingChoices = false;
        hasMadeChoice = false; 
        
        if (interactionBubble != null) interactionBubble.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);

        // CRITICAL: We must reset the disabled and bolded text objects back to normal
        // Otherwise, they will stay hidden or bold the next time we approach the checkpoint!
        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            if (dialogueOptions[i] != null)
            {
                dialogueOptions[i].gameObject.SetActive(true);
                dialogueOptions[i].fontStyle = FontStyles.Normal;
                dialogueOptions[i].color = normalColor;
            }
        }
    }
}