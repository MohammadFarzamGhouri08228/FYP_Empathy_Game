using UnityEngine;

public class MissingNPCReset : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time in seconds the NPC must be off-screen before the reset happens.")]
    public float allowedOffScreenTime = 0.5f;

    [Tooltip("If true, only triggers a reset if the NPC is off the LEFT or RIGHT sides of the screen. This prevents unfair resets when the NPC falls into a pit/fails a jump.")]
    public bool ignoreVerticalOffscreen = true;

    private Camera mainCamera;
    private Renderer npcRenderer;
    private float offScreenTimer = 0f;
    private float startupTimer = 0f;
    private enum State { Normal, WaitingForInput }
    private State currentState = State.Normal;
    private bool hasIgnoredThisOffscreen = false;

    void Start()
    {
        mainCamera = Camera.main;
        npcRenderer = GetComponent<Renderer>();

        if (npcRenderer == null)
        {
            Debug.LogWarning("[MissingNPCReset] No Renderer found! Script requires a SpriteRenderer or MeshRenderer on the identical object.");
        }
    }

    void Update()
    {
        if (mainCamera == null || npcRenderer == null) return;

        // Give the camera a few seconds to snap to the player when the scene starts
        if (startupTimer < 2.0f)
        {
            startupTimer += Time.deltaTime;
            return;
        }

        if (currentState == State.WaitingForInput)
        {
            // The game is frozen. Wait for the player's choice via unscaled input.
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Debug.Log("<color=green>[MissingNPCReset] Player chose to WAIT (Resetting...).</color>");
                Time.timeScale = 1f; // Unfreeze the game back to normal speed
                TriggerReset();
                currentState = State.Normal;
                offScreenTimer = -1f; // Temporary cooldown
                hasIgnoredThisOffscreen = false;
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("<color=red>[MissingNPCReset] Player chose to CONTINUE (Ignoring NPC).</color>");
                Time.timeScale = 1f; // Unfreeze the game back to normal speed
                currentState = State.Normal;
                hasIgnoredThisOffscreen = true; // Ignore this particular offscreen event until they catch up
            }
            // Stop executing the rest of Update while waiting
            return;
        }

        bool isOffScreen = CheckIfOffScreen();

        if (isOffScreen)
        {
            // Do not trigger the timer if we chose to ignore the NPC
            if (hasIgnoredThisOffscreen) return; 

            // Use Time.unscaledDeltaTime just in case timeScale was tinkered with elsewhere
            offScreenTimer += Time.unscaledDeltaTime;
            
            if (offScreenTimer >= allowedOffScreenTime)
            {
                Debug.Log("<color=yellow>[MissingNPCReset] NPC goes OFF SCREEN! Freezing the game.</color>");
                currentState = State.WaitingForInput;
                Time.timeScale = 0f; // Freeze the game
            }
        }
        else
        {
            // The NPC came back on screen, so reset the ignored flag
            hasIgnoredThisOffscreen = false;

            // Reset timer if they come back on screen (or after teleport)
            if (offScreenTimer < 0f)
            {
                 // Small cooldown logic to prevent instant double-resets
                 offScreenTimer += Time.unscaledDeltaTime;
                 if (offScreenTimer >= 0f) offScreenTimer = 0f;
            }
            else
            {
                offScreenTimer = 0f;
            }
        }
    }

    void OnGUI()
    {
        // Simple Debug UI so you can see when the game is paused and frozen waiting for your input
        if (currentState == State.WaitingForInput)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = Screen.height / 15; // scales with screen size
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;

            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.Label(rect, "You left the NPC behind!\n\nPress 'Y' to Reset to Checkpoint.\nPress 'N' to Move Forward.", style);
        }
    }

    private bool CheckIfOffScreen()
    {
        // 1. Calculate NPC position relative to the camera view
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        
        // Use the renderer's built in visibility check as a primary check.
        // This is usually more reliable than viewport point since it accounts for sprite size.
        bool isRendererVisible = npcRenderer.isVisible;
        
        // 2. Are they outside the 0.0 to 1.0 bounds?
        // We add a small buffer (-0.1 to 1.1) so being slightly on the edge doesn't trigger it
        bool offX = viewportPos.x < -0.1f || viewportPos.x > 1.1f;
        bool offY = viewportPos.y < -0.1f || viewportPos.y > 1.1f;

        // If the renderer is visible, we are definitely NOT off screen.
        if (isRendererVisible) 
        {
            return false;
        }

        if (ignoreVerticalOffscreen)
        {
            // Only care if the player horizontally left them behind. 
            // We also check viewportPos.z > 0 to make sure they aren't behind the camera entirely.
            return offX || viewportPos.z < 0f;
        }
        else
        {
            return offX || offY || viewportPos.z < 0f;
        }
    }

    private void TriggerReset()
    {
        Debug.Log("<color=orange>[MissingNPCReset] Player left the NPC behind! Resetting to checkpoint...</color>");

        if (CheckpointManager.Instance != null)
        {
            // 1. Reset Player (passing null uses the default player in CheckpointManager)
            CheckpointManager.Instance.RespawnAtCheckpoint(null);
            
            // 2. Reset NPC (passing this gameObject)
            CheckpointManager.Instance.RespawnAtCheckpoint(this.gameObject);
        }
        else
        {
            Debug.LogWarning("[MissingNPCReset] CheckpointManager not found in scene! Cannot respawn.");
        }
    }
}
