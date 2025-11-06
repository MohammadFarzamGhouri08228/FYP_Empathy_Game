using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

public class JumpButton : MonoBehaviour
{
	[Header("Button Settings")]
	[SerializeField] private PlayerMovementController playerMovementController;

	[Header("Visual Feedback")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
	[SerializeField] private float pressDuration = 0.1f;

	private Button button;
	private float pressTimer = 0f;

	void Reset()
	{
		#if UNITY_EDITOR
		// Clean up onClick when component is first added
		button = GetComponent<Button>();
		if (button != null)
		{
			int eventCount = button.onClick.GetPersistentEventCount();
			for (int i = eventCount - 1; i >= 0; i--)
			{
				UnityEventTools.RemovePersistentListener(button.onClick, i);
			}
			UnityEventTools.AddPersistentListener(button.onClick, ExecuteJump);
			EditorUtility.SetDirty(button);
		}
		#endif
	}

	void OnValidate()
	{
		#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			// Ensure Button component exists
			button = GetComponent<Button>();
			if (button == null)
			{
				button = gameObject.AddComponent<Button>();
				EditorUtility.SetDirty(gameObject);
			}

			// Wire up Button.onClick in edit mode so it's visible in Inspector
			if (button != null)
			{
				// Remove all existing listeners first to clean up empty entries
				int eventCount = button.onClick.GetPersistentEventCount();
				for (int i = eventCount - 1; i >= 0; i--)
				{
					UnityEventTools.RemovePersistentListener(button.onClick, i);
				}
				
				// Add only our listener
				UnityEventTools.AddPersistentListener(button.onClick, ExecuteJump);
				EditorUtility.SetDirty(button);
			}
		}
		#endif
	}

	void Start()
	{
		// Find PlayerMovementController if not assigned (look for it on the Player GameObject)
		if (playerMovementController == null)
		{
			// Try to find Player GameObject first
			GameObject playerObj = GameObject.Find("Player");
			if (playerObj != null)
			{
				playerMovementController = playerObj.GetComponent<PlayerMovementController>();
			}
			
			// If still not found, search all GameObjects
			if (playerMovementController == null)
			{
				playerMovementController = FindFirstObjectByType<PlayerMovementController>();
			}
			
			if (playerMovementController == null)
			{
				Debug.LogError($"JumpButton [{gameObject.name}]: PlayerMovementController not found! Please assign it in the Inspector.");
			}
			else
			{
				Debug.Log($"JumpButton [{gameObject.name}]: Found PlayerMovementController automatically on '{playerMovementController.gameObject.name}'");
			}
		}
		else
		{
			Debug.Log($"JumpButton [{gameObject.name}]: Using manually assigned PlayerMovementController on '{playerMovementController.gameObject.name}'");
		}

		// Get SpriteRenderer if not assigned
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		// Ensure there's a Collider2D for button detection
		Collider2D col = GetComponent<Collider2D>();
		if (col == null)
		{
			if (spriteRenderer != null && spriteRenderer.sprite != null)
			{
				BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
				boxCol.size = spriteRenderer.sprite.bounds.size;
				boxCol.offset = spriteRenderer.sprite.bounds.center;
				Debug.Log($"JumpButton [{gameObject.name}]: Added BoxCollider2D (size: {boxCol.size})");
			}
			else
			{
				Debug.LogWarning($"JumpButton [{gameObject.name}]: No sprite found. Adding default BoxCollider2D.");
				BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
				boxCol.size = Vector2.one;
			}
		}

		// Get or add Button component
		button = GetComponent<Button>();
		if (button == null)
		{
			// Add Button component for onClick functionality
			button = gameObject.AddComponent<Button>();
			Debug.Log($"JumpButton [{gameObject.name}]: Added Button component");
		}

		// Wire up onClick event with meaningful function name
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ExecuteJump);
		
		Debug.Log($"JumpButton [{gameObject.name}]: Button.onClick wired up to ExecuteJump()");
	}

	void Update()
	{
		// Reset visual feedback after press duration
		if (pressTimer > 0f)
		{
			pressTimer -= Time.deltaTime;
			if (pressTimer <= 0f && spriteRenderer != null)
			{
				spriteRenderer.color = normalColor;
			}
		}
	}

	// Meaningful function name for Button.onClick - Executes jump action
	public void ExecuteJump()
	{
		Debug.Log($"=== BUTTON CLICKED: JUMP BUTTON [{gameObject.name}] ===");
		Debug.Log($"  - Button Name: {gameObject.name}");
		Debug.Log($"  - PlayerMovementController Reference: {(playerMovementController != null ? $"ASSIGNED ({playerMovementController.gameObject.name})" : "NULL - ERROR!")}");
		
		if (playerMovementController != null)
		{
			// Call jump button down to initiate jump
			playerMovementController.OnJumpButtonDown();
			Debug.Log($"  ✓ Jump command sent (OnJumpButtonDown called)");
			
			// Call jump button up after a short delay to reset jump state
			// This ensures the jump happens in FixedUpdate before we reset it
			StartCoroutine(ResetJumpState());
		}
		else
		{
			Debug.LogError($"  ✗ ERROR: PlayerMovementController is NULL! Cannot jump.");
			Debug.LogError($"  → SOLUTION: Assign PlayerMovementController in Inspector to the Player GameObject.");
		}

		// Visual feedback
		if (spriteRenderer != null)
		{
			spriteRenderer.color = pressedColor;
			pressTimer = pressDuration;
		}
	}

	private IEnumerator ResetJumpState()
	{
		// Wait a frame to ensure FixedUpdate has processed the jump
		yield return new WaitForFixedUpdate();
		
		// Reset jump state
		if (playerMovementController != null)
		{
			playerMovementController.OnJumpButtonUp();
			Debug.Log($"  ✓ Jump state reset (OnJumpButtonUp called)");
		}
	}
}
