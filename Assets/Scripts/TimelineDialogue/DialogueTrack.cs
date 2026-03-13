using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(DialogueClip))]
[TrackBindingType(typeof(DialogueBubbleController))]
public class DialogueTrack : TrackAsset
{
    // The Track Asset tells Timeline what type of component it can bind to
    // and what kind of clips are allowed here.
}
