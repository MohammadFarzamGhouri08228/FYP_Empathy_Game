/*
    Lvl2EntryPoint.cs  –  Lvl2Transition
    ──────────────────────────────────────
    Place ONE instance of this on any persistent GameObject in Level2
    (e.g. a "GameManager" or an empty "EntryPoint" object).

    On Start it checks whether the player arrived from Minigame1
    (detected via a PlayerPrefs flag set by MinigameSceneExit).
    If so, it teleports the player to the saved arrival position.

    ── Inspector Setup ──────────────────────────────────────────────
    • Player Tag   : Tag used to find the player (default "Player").
    • Clear Prefs  : If true, clears the arrival flag after use so that
                     normal Level2 restarts don't re-apply the teleport.
    ─────────────────────────────────────────────────────────────────
*/

using System.Collections;
using UnityEngine;

public class Lvl2EntryPoint : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Tag of the player GameObject to teleport.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Settings")]
    [Tooltip("Clear the PlayerPrefs arrival data after use so it doesn't persist across unrelated scene loads.")]
    [SerializeField] private bool clearPrefsAfterUse = true;

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Only teleport if we actually came from Minigame1
        if (PlayerPrefs.GetInt(MinigameSceneExit.KEY_FROM_MINIGAME, 0) != 1)
        {
            Debug.Log("[Lvl2EntryPoint] Not arriving from Minigame. Normal Level2 start.");
            return;
        }

        float x = PlayerPrefs.GetFloat(MinigameSceneExit.KEY_ARRIVAL_X, 0f);
        float y = PlayerPrefs.GetFloat(MinigameSceneExit.KEY_ARRIVAL_Y, 0f);
        float z = PlayerPrefs.GetFloat(MinigameSceneExit.KEY_ARRIVAL_Z, 0f);
        Vector3 arrivalPos = new Vector3(x, y, z);

        if (clearPrefsAfterUse)
        {
            PlayerPrefs.DeleteKey(MinigameSceneExit.KEY_ARRIVAL_X);
            PlayerPrefs.DeleteKey(MinigameSceneExit.KEY_ARRIVAL_Y);
            PlayerPrefs.DeleteKey(MinigameSceneExit.KEY_ARRIVAL_Z);
            PlayerPrefs.DeleteKey(MinigameSceneExit.KEY_FROM_MINIGAME);
            PlayerPrefs.Save();
        }

        // Wait 2 frames before teleporting. MinigameTeleport.RestorePlayerPosition-
        // IfNeededCoroutine waits 1 frame and may override us — running last wins.
        StartCoroutine(TeleportAfterFrames(arrivalPos, 2));
    }

    private IEnumerator TeleportAfterFrames(Vector3 position, int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;

        TeleportPlayer(position);
    }

    private void TeleportPlayer(Vector3 position)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            // Fallback: try movement script
            MonoBehaviour pc = (MonoBehaviour)FindFirstObjectByType<Lvl2movement>() ??
                               (MonoBehaviour)FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }

        if (player == null)
        {
            Debug.LogError("[Lvl2EntryPoint] Could not find the player! Make sure the player has the correct tag or a movement script.");
            return;
        }

        // Teleport
        player.transform.position = position;

        // Zero out velocity so they don't slide in
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Debug.Log($"[Lvl2EntryPoint] Teleported '{player.name}' to {position} (from Minigame1 exit).");
    }
}
