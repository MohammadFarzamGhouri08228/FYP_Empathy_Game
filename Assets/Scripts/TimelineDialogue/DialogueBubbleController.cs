using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBubbleController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject bubbleContainer;
    public TextMeshProUGUI dialogueText;

    private void Start()
    {
        // Ensure bubble is hidden at start
        if (bubbleContainer != null)
        {
            bubbleContainer.SetActive(false);
        }
    }

    public void ShowDialogue(string text)
    {
        if (bubbleContainer != null) bubbleContainer.SetActive(true);
        if (dialogueText != null) dialogueText.text = text;
        
        // Add animation triggers here if you have an Animator on the bubbleContainer!
        // GetComponentInChildren<Animator>().SetTrigger("PopIn");
    }

    public void HideDialogue()
    {
        if (bubbleContainer != null) bubbleContainer.SetActive(false);
    }
}
