using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Real-Time Debug View")]
    // We make these public so you can see them change in the Inspector while playing!
    public string lastSourceID;
    public int lastCountReceived;

    /// <summary>
    /// This is the "Inbox" method that Object1Interaction will call.
    /// </summary>
    public void ReceiveInteractionData(string sourceID, int count)
    {
        // 1. Update variables for Inspector verification
        lastSourceID = sourceID;
        lastCountReceived = count;

        // 2. Print to Console for Log verification
        Debug.Log($"<color=green>[InteractiveObject]</color> RECEIVED: Source='{sourceID}' | Count={count}");
    }
}