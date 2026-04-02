using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueBehaviour : PlayableBehaviour
{
    public string dialogueText;
    private bool hasPlayed = false;

    // This is called when the playhead enters a clip
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        hasPlayed = false;
    }

    // This runs every frame during the clip
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        DialogueBubbleController bubbleController = playerData as DialogueBubbleController;
        
        if (bubbleController != null && !hasPlayed && info.weight > 0f)
        {
            // Show dialogue
            bubbleController.ShowDialogue(dialogueText);
            hasPlayed = true;
        }
    }

    // This is called when the playhead exits the clip
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // When the clip finishes
        if (hasPlayed)
        {
            DialogueBubbleController bubbleController = info.output.GetUserData() as DialogueBubbleController;
            if (bubbleController != null)
            {
                bubbleController.HideDialogue();
            }
        }
    }
}
