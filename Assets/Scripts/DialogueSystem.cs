using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; // Required for TextMeshPro

public class DialogueSystem : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI textComponent;
    
    [Header("Settings")]
    public float typingSpeed = 0.05f; // How fast the text types

    [Header("Dialogue Content")]
    [TextArea(3, 10)] // Makes the box in Inspector bigger for typing
    public string[] lines; // Array of sentences
    
    private int index; // Tracks which sentence we are on

    void Start()
    {
        // Ensure text is empty at start
        textComponent.text = string.Empty;
        
        // Start the dialogue
        StartDialogue();
    }

    void Update()
    {
        // Check for mouse click or screen tap using new Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // If the text has finished typing, go to next line
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                // Optional: If player clicks while typing, finish instantly
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    // This is the Typewriter Effect logic
    IEnumerator TypeLine()
    {
        // Loop through each character in the specific sentence
        foreach (char letter in lines[index].ToCharArray())
        {
            textComponent.text += letter; // Add one letter
            yield return new WaitForSeconds(typingSpeed); // Wait
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            // Dialogue finished - close the box
            gameObject.SetActive(false);
        }
    }
}