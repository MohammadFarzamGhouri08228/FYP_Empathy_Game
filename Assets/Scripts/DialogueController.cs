using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public class DialogueOption
{
    public string optionText;
    public ChoiceCategory category;
    [Tooltip("The index of the DialogueLine to jump to. Set to -1 to end the dialogue.")]
    public int nextDialogueIndex = -1;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    [Tooltip("If true, this line will display options instead of the continue button.")]
    public bool isQuestion;
    public DialogueOption[] options;
}

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("Dialogue UI References")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public GameObject contButton;

    [Header("Speaker UI - NPC")]
    public GameObject portraitImage;
    public GameObject nameTitle;

    [Header("Speaker UI - Player")]
    public GameObject playerPortraitImage;
    public GameObject playerNameTitle;

    [Header("Options UI")]
    public GameObject optionsPanel;
    public GameObject choiceButtonPrefab;

    [Header("Settings")]
    public float wordSpeed = 0.03f;
    public ElevenLabsTTS ttsSystem;
    public bool readDialogueAloud = true;

    // State
    private DialogueLine[] currentDialogue;
    private int index;
    private bool isTyping;
    private Action onDialogueComplete;
    private Action<int, ChoiceCategory> onOptionSelected;

    // Empathy tracking
    public float dialogueStillFrames { get; private set; }
    public float dialogueTotalFrames { get; private set; }
    private Rigidbody2D playerRb;
    private const float STILL_VELOCITY_THRESHOLD = 0.15f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (ttsSystem == null) ttsSystem = FindFirstObjectByType<ElevenLabsTTS>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerRb = player.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (dialogPanel != null && dialogPanel.activeInHierarchy)
        {
            if (isTyping && currentDialogue != null && index < currentDialogue.Length && !IsPlayerDialogue(currentDialogue[index].text))
            {
                dialogueTotalFrames++;
                if (playerRb != null && playerRb.linearVelocity.magnitude < STILL_VELOCITY_THRESHOLD)
                {
                    dialogueStillFrames++;
                }
                else if (playerRb == null)
                {
                    dialogueStillFrames++;
                }
            }

            // Input handling for continuing dialogue
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                if (currentDialogue != null && index < currentDialogue.Length && !IsPlayerDialogue(currentDialogue[index].text))
                {
                    if (isTyping && dialogText.maxVisibleCharacters < dialogText.textInfo.characterCount)
                    {
                        // Skip typing
                        StopAllCoroutines();
                        dialogText.maxVisibleCharacters = dialogText.textInfo.characterCount;
                        isTyping = false;

                        if (currentDialogue[index].isQuestion && currentDialogue[index].options != null && currentDialogue[index].options.Length > 0)
                        {
                            ShowOptions(currentDialogue[index].options);
                        }
                        else if (contButton != null)
                        {
                            contButton.SetActive(true);
                        }
                    }
                    else if (optionsPanel == null || !optionsPanel.activeInHierarchy)
                    {
                        if (!currentDialogue[index].isQuestion) // Prevent skipping past a question with E
                        {
                            NextLine();
                        }
                    }
                }
            }

            // Keyboard support for selecting options (1, 2, 3, etc.)
            if (optionsPanel != null && optionsPanel.activeInHierarchy && currentDialogue[index].options != null)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame) TrySelectOptionByIndex(0);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame) TrySelectOptionByIndex(1);
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame) TrySelectOptionByIndex(2);
                    else if (Keyboard.current.digit4Key.wasPressedThisFrame) TrySelectOptionByIndex(3);
                }
            }
        }
    }

    private void TrySelectOptionByIndex(int optionIndex)
    {
        if (currentDialogue != null && index < currentDialogue.Length && currentDialogue[index].options != null)
        {
            if (optionIndex < currentDialogue[index].options.Length)
            {
                SelectOption(optionIndex, currentDialogue[index].options[optionIndex]);
            }
        }
    }

    /// <summary>
    /// Starts a dialogue sequence with branching options.
    /// </summary>
    public void StartDialogue(DialogueLine[] dialogue, Action<int, ChoiceCategory> onOptionChosen = null, Action onComplete = null)
    {
        currentDialogue = dialogue;
        onOptionSelected = onOptionChosen;
        onDialogueComplete = onComplete;
        
        index = 0;
        dialogueStillFrames = 0f;
        dialogueTotalFrames = 0f;
        
        dialogPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Hide old option buttons
        if (optionsPanel != null)
        {
            foreach (Transform child in optionsPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (contButton != null)
        {
            Button btn = contButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(NextLine);
            }
        }

        StartCoroutine(Typing());
    }

    private void SelectOption(int choiceIndex, DialogueOption chosenOption)
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        // Report option selection immediately
        onOptionSelected?.Invoke(choiceIndex, chosenOption.category);

        // Handle Branching
        if (chosenOption.nextDialogueIndex >= 0 && chosenOption.nextDialogueIndex < currentDialogue.Length)
        {
            index = chosenOption.nextDialogueIndex;
            StartCoroutine(Typing());
        }
        else
        {
            // End dialogue
            CloseDialogue();
            onDialogueComplete?.Invoke();
        }
    }

    public void CloseDialogue()
    {
        if (dialogText != null) dialogText.text = "";
        index = 0;
        if (dialogText != null) dialogText.maxVisibleCharacters = 0;
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        if (readDialogueAloud && ttsSystem != null)
        {
            ttsSystem.StopSpeaking();
        }
        
        StopAllCoroutines();
    }

    IEnumerator Typing()
    {
        isTyping = true;
        if (contButton != null) contButton.SetActive(false);

        bool isPlayerLine = IsPlayerDialogue(currentDialogue[index].text);

        if (portraitImage != null) portraitImage.SetActive(!isPlayerLine);
        if (nameTitle != null) nameTitle.SetActive(!isPlayerLine);

        if (playerPortraitImage != null) playerPortraitImage.SetActive(isPlayerLine);
        if (playerNameTitle != null) playerNameTitle.SetActive(isPlayerLine);

        string displayAndSpokenText = currentDialogue[index].text;

        if (isPlayerLine && displayAndSpokenText.StartsWith("Musa:"))
        {
            displayAndSpokenText = displayAndSpokenText.Substring(5).Trim();
        }

        if (readDialogueAloud && ttsSystem != null && !string.IsNullOrWhiteSpace(displayAndSpokenText))
        {
            ttsSystem.Speak(displayAndSpokenText);
        }

        dialogText.text = displayAndSpokenText;
        dialogText.maxVisibleCharacters = 0;
        dialogText.ForceMeshUpdate();

        yield return null;

        int totalVisibleCharacters = dialogText.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            dialogText.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(wordSpeed);
        }

        isTyping = false;

        if (isPlayerLine)
        {
            yield return new WaitForSeconds(1.2f);
            NextLine();
        }
        else
        {
            if (currentDialogue[index].isQuestion && currentDialogue[index].options != null && currentDialogue[index].options.Length > 0)
            {
                ShowOptions(currentDialogue[index].options);
            }
            else if (contButton != null)
            {
                contButton.SetActive(true);
            }
        }
    }

    public void NextLine()
    {
        if (isTyping) return;
        if (contButton != null) contButton.SetActive(false);

        if (currentDialogue[index].isQuestion)
        {
            // Do not advance line normally if it's a question waiting for input
            return;
        }

        if (index < currentDialogue.Length - 1)
        {
            index++;
            StartCoroutine(Typing());
        }
        else
        {
            CloseDialogue();
            onDialogueComplete?.Invoke();
        }
    }

    private void ShowOptions(DialogueOption[] options)
    {
        Debug.Log($"[DialogueController] ShowOptions called with {options?.Length ?? 0} options.");
        
        if (optionsPanel == null)
        {
            Debug.LogError("[DialogueController] Cannot show options: OptionsPanel is NULL!");
            return;
        }
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[DialogueController] Cannot show options: choiceButtonPrefab is NULL! Please assign it in the Inspector.");
            return;
        }

        optionsPanel.SetActive(true);
        
        // Clear existing buttons
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        string optionsAloudText = "";

        if (options != null && options.Length > 0)
        {
            for (int i = 0; i < options.Length; i++)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, optionsPanel.transform);
                btnObj.SetActive(true);
                
                // Force scale to 1 in case the prefab imports weirdly
                btnObj.transform.localScale = Vector3.one;

                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = options[i].optionText;
                    optionsAloudText += $"Option {i + 1}: {options[i].optionText}. ";
                }
                else
                {
                    Debug.LogWarning($"[DialogueController] Button Prefab is missing a TMP_Text component in its children!");
                }
                
                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    int choiceIndex = i; 
                    DialogueOption chosenOption = options[i]; // Capture for lambda
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => SelectOption(choiceIndex, chosenOption));
                }
                else
                {
                    Debug.LogWarning($"[DialogueController] Button Prefab is missing a Button component!");
                }
            }
            Debug.Log($"[DialogueController] Successfully instantiated {options.Length} option buttons.");
        }

        // Read options aloud for accessibility and empathy immersion
        if (readDialogueAloud && ttsSystem != null && !string.IsNullOrWhiteSpace(optionsAloudText))
        {
            ttsSystem.Speak("Your choices are. " + optionsAloudText);
        }
    }

    private bool IsPlayerDialogue(string line)
    {
        if (string.IsNullOrEmpty(line)) return false;
        return line.TrimStart().StartsWith("Musa:");
    }
}
