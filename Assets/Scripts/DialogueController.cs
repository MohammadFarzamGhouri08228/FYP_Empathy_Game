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
    public DialogueOption[] options;

    /// <summary>
    /// Returns true if this line has any options configured.
    /// </summary>
    public bool HasOptions => options != null && options.Length > 0;
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
    [Tooltip("The panel/container that holds the option buttons. Will be shown/hidden automatically.")]
    public GameObject optionsPanel;

    [Tooltip("Pre-placed option button GameObjects inside the optionsPanel. Set up 2-4 buttons in the Canvas and drag them here.")]
    public GameObject[] optionButtons;

    [Tooltip("The TMP_Text components on each option button. Must match optionButtons array order.")]
    public TMP_Text[] optionButtonTexts;

    [Header("Option Button Colors")]
    public Color optionNormalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color optionHighlightColor = new Color(0.35f, 0.55f, 0.85f, 1f);
    public Color optionPressedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
    public Color optionSelectedColor = new Color(0.35f, 0.55f, 0.85f, 1f);

    [Header("Settings")]
    public float wordSpeed = 0.03f;
    public ElevenLabsTTS ttsSystem;
    public bool readDialogueAloud = true;

    [Header("Audio")]
    private AudioSource audioSource;

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
        HideAllOptionButtons();

        if (ttsSystem == null) ttsSystem = FindFirstObjectByType<ElevenLabsTTS>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerRb = player.GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (dialogPanel == null || !dialogPanel.activeInHierarchy) return;

        // Empathy tracking: count frames while NPC is speaking
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

        // ---- E key: skip typing OR advance line ----
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentDialogue != null && index < currentDialogue.Length && !IsPlayerDialogue(currentDialogue[index].text))
            {
                if (isTyping)
                {
                    // Skip typing animation — show full text immediately
                    StopAllCoroutines();
                    dialogText.maxVisibleCharacters = dialogText.textInfo.characterCount;
                    isTyping = false;

                    // After skipping, show the appropriate UI
                    if (currentDialogue[index].HasOptions)
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
                    // Not typing and options aren't showing — advance to next line
                    if (!currentDialogue[index].HasOptions)
                    {
                        NextLine();
                    }
                }
            }
        }

        // ---- Number keys: select options (1, 2, 3, 4) ----
        if (optionsPanel != null && optionsPanel.activeInHierarchy)
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

    private void TrySelectOptionByIndex(int optionIndex)
    {
        if (currentDialogue != null && index < currentDialogue.Length && currentDialogue[index].HasOptions)
        {
            if (optionIndex < currentDialogue[index].options.Length)
            {
                SelectOption(optionIndex, currentDialogue[index].options[optionIndex]);
            }
        }
    }

    // ========================================================================
    // PUBLIC API
    // ========================================================================

    /// <summary>
    /// Starts a dialogue sequence. Called by Checkpoint / Checkpoint2 scripts.
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
        HideAllOptionButtons();

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

    public void CloseDialogue()
    {
        if (dialogText != null) dialogText.text = "";
        index = 0;
        if (dialogText != null) dialogText.maxVisibleCharacters = 0;
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        HideAllOptionButtons();
        
        if (readDialogueAloud && ttsSystem != null)
        {
            ttsSystem.StopSpeaking();
        }
        
        if (audioSource != null) audioSource.Stop();
        
        StopAllCoroutines();
    }

    // ========================================================================
    // DIALOGUE FLOW
    // ========================================================================

    IEnumerator Typing()
    {
        isTyping = true;
        if (contButton != null) contButton.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        HideAllOptionButtons();

        bool isPlayerLine = IsPlayerDialogue(currentDialogue[index].text);

        // Toggle NPC vs Player portrait/name
        if (portraitImage != null) portraitImage.SetActive(!isPlayerLine);
        if (nameTitle != null) nameTitle.SetActive(!isPlayerLine);
        if (playerPortraitImage != null) playerPortraitImage.SetActive(isPlayerLine);
        if (playerNameTitle != null) playerNameTitle.SetActive(isPlayerLine);

        string displayAndSpokenText = currentDialogue[index].text;

        // 1. Try to load and play pre-recorded audio
        AudioClip lineAudio = TryLoadAudioForLine(currentDialogue[index].text);
        if (lineAudio != null)
        {
            if (audioSource != null)
            {
                audioSource.clip = lineAudio;
                audioSource.Play();
            }
        }

        // 2. Clean up display text (Remove "No.X")
        int noIndex = displayAndSpokenText.LastIndexOf("No.");
        if (noIndex != -1)
        {
            displayAndSpokenText = displayAndSpokenText.Substring(0, noIndex).Trim();
        }

        if (isPlayerLine && displayAndSpokenText.StartsWith("Musa:"))
        {
            displayAndSpokenText = displayAndSpokenText.Substring(5).Trim();
        }

        // 3. Fallback to TTS if no pre-recorded audio was found
        if (lineAudio == null && readDialogueAloud && ttsSystem != null && !string.IsNullOrWhiteSpace(displayAndSpokenText))
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

        // ---- After typing completes ----
        if (isPlayerLine)
        {
            // Player line: show briefly then auto-advance
            yield return new WaitForSeconds(1.2f);
            NextLine();
        }
        else
        {
            // NPC line: show options if available, otherwise show continue button
            if (currentDialogue[index].HasOptions)
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

        // Don't advance if this line has options waiting for player input
        if (currentDialogue[index].HasOptions)
        {
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

    // ========================================================================
    // OPTIONS DISPLAY (BMo tutorial approach: pre-placed UI buttons)
    // ========================================================================

    private void SelectOption(int choiceIndex, DialogueOption chosenOption)
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        HideAllOptionButtons();
        
        // Report option selection to checkpoint
        onOptionSelected?.Invoke(choiceIndex, chosenOption.category);

        // Handle branching: jump to the specified index or end dialogue
        if (chosenOption.nextDialogueIndex >= 0 && chosenOption.nextDialogueIndex < currentDialogue.Length)
        {
            index = chosenOption.nextDialogueIndex;
            StartCoroutine(Typing());
        }
        else
        {
            // -1 or out of range = end dialogue
            CloseDialogue();
            onDialogueComplete?.Invoke();
        }
    }

    private void ShowOptions(DialogueOption[] options)
    {
        if (contButton != null) contButton.SetActive(false);

        // Validate references
        if (optionsPanel == null)
        {
            Debug.LogError("[DialogueController] optionsPanel is not assigned! Drag your Options Panel into the Inspector.");
            return;
        }
        if (optionButtons == null || optionButtons.Length == 0)
        {
            Debug.LogError("[DialogueController] optionButtons array is empty! Drag your option button GameObjects into the Inspector.");
            return;
        }

        // Show the options panel
        optionsPanel.SetActive(true);

        // Configure each button
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            if (i < options.Length)
            {
                // This button has a corresponding option — show it
                optionButtons[i].SetActive(true);

                // Set the text
                if (optionButtonTexts != null && i < optionButtonTexts.Length && optionButtonTexts[i] != null)
                {
                    optionButtonTexts[i].text = options[i].optionText;
                }

                // Wire the click handler
                Button btn = optionButtons[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int choiceIndex = i;
                    DialogueOption chosenOption = options[i];
                    btn.onClick.AddListener(() => SelectOption(choiceIndex, chosenOption));

                    // Apply highlight colors so the button visually responds to hover/press
                    ColorBlock colors = btn.colors;
                    colors.normalColor = optionNormalColor;
                    colors.highlightedColor = optionHighlightColor;
                    colors.pressedColor = optionPressedColor;
                    colors.selectedColor = optionSelectedColor;
                    colors.fadeDuration = 0.1f;
                    btn.colors = colors;
                }
            }
            else
            {
                // No option for this button — hide it
                optionButtons[i].SetActive(false);
            }
        }

        Debug.Log($"[DialogueController] Showing {options.Length} options.");

        // Read options aloud for accessibility
        if (readDialogueAloud && ttsSystem != null)
        {
            string optionsAloudText = "";
            for (int i = 0; i < options.Length; i++)
            {
                optionsAloudText += $"Option {i + 1}: {options[i].optionText}. ";
            }
            if (!string.IsNullOrWhiteSpace(optionsAloudText))
            {
                ttsSystem.Speak(optionsAloudText);
            }
        }
    }

    /// <summary>
    /// Hides all option buttons. Called at dialogue start, close, and before each new line.
    /// </summary>
    private void HideAllOptionButtons()
    {
        if (optionButtons == null) return;
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] != null)
                optionButtons[i].SetActive(false);
        }
    }

    // ========================================================================
    // HELPERS
    // ========================================================================

    private bool IsPlayerDialogue(string line)
    {
        if (string.IsNullOrEmpty(line)) return false;
        return line.TrimStart().StartsWith("Musa:");
    }

    private AudioClip TryLoadAudioForLine(string lineText)
    {
        if (string.IsNullOrEmpty(lineText)) return null;

        // Expected format: "CharacterName: .... No.X"
        int colonIndex = lineText.IndexOf(':');
        if (colonIndex == -1) return null;

        string charName = lineText.Substring(0, colonIndex).Trim();

        int noIndex = lineText.LastIndexOf("No.");
        if (noIndex == -1) return null;

        string numberStr = lineText.Substring(noIndex + 3).Trim();
        
        // Extract only digits to handle cases like "No.1." or "No. 12 "
        string cleanNum = "";
        foreach (char c in numberStr)
        {
            if (char.IsDigit(c)) cleanNum += c;
        }

        if (string.IsNullOrEmpty(cleanNum)) return null;

        string fileName = $"{charName}'sDialogue#{cleanNum}";
        
        // Load from Resources folder (we moved Level1Audios to Resources)
        AudioClip clip = Resources.Load<AudioClip>($"Level1Audios/{fileName}");
        
        if (clip == null)
        {
            Debug.LogWarning($"[DialogueAudio] Could not load audio clip at Resources/Level1Audios/{fileName}");
        }
        else
        {
            Debug.Log($"[DialogueAudio] Successfully loaded audio: {fileName}");
        }
        
        return clip;
    }
}
