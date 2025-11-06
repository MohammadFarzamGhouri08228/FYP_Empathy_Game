using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

public class MovementButton : MonoBehaviour
{
	[Header("Button Settings")]
	[SerializeField] private float moveDirection = 1f; // 1 for Right, -1 for Left
	[SerializeField] private PlayerMovementController playerMovementController;

	[Header("Visual Feedback")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	private Button button;
	private bool isMoving = false;

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
			UnityEventTools.AddPersistentListener(button.onClick, ToggleMovement);
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
				UnityEventTools.AddPersistentListener(button.onClick, ToggleMovement);
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
				Debug.LogError($"MovementButton [{gameObject.name}]: PlayerMovementController not found! Please assign it in the Inspector.");
			}
			else
			{
				Debug.Log($"MovementButton [{gameObject.name}]: Found PlayerMovementController automatically on '{playerMovementController.gameObject.name}'");
			}
		}
		else
		{
			Debug.Log($"MovementButton [{gameObject.name}]: Using manually assigned PlayerMovementController on '{playerMovementController.gameObject.name}'");
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
				Debug.Log($"MovementButton [{gameObject.name}]: Added BoxCollider2D (size: {boxCol.size})");
			}
			else
			{
				Debug.LogWarning($"MovementButton [{gameObject.name}]: No sprite found. Adding default BoxCollider2D.");
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
			Debug.Log($"MovementButton [{gameObject.name}]: Added Button component");
		}

		// Wire up onClick event
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ToggleMovement);
		
		Debug.Log($"MovementButton [{gameObject.name}]: Button.onClick wired up to ToggleMovement()");
	}

	// Meaningful function name for Button.onClick - Toggles movement on/off
	public void ToggleMovement()
	{
		string direction = moveDirection > 0 ? "RIGHT" : "LEFT";
		Debug.Log($"=== BUTTON CLICKED: {direction} BUTTON [{gameObject.name}] ===");
		Debug.Log($"  - Button Name: {gameObject.name}");
		Debug.Log($"  - Move Direction: {moveDirection}");
		Debug.Log($"  - PlayerMovementController Reference: {(playerMovementController != null ? $"ASSIGNED ({playerMovementController.gameObject.name})" : "NULL - ERROR!")}");
		Debug.Log($"  - Current Movement State: {(isMoving ? "MOVING" : "STOPPED")}");
		
		if (playerMovementController != null)
		{
			// Toggle movement: if currently moving in this direction, stop. Otherwise, start moving.
			if (isMoving)
			{
				// Stop movement
				if (moveDirection > 0)
				{
					playerMovementController.OnRightButtonUp();
				}
				else
				{
					playerMovementController.OnLeftButtonUp();
				}
				isMoving = false;
				Debug.Log($"  ✓ STOPPED movement - Player will stop moving {direction}");
			}
			else
			{
				// Start movement
				if (moveDirection > 0)
				{
					playerMovementController.OnRightButtonDown();
				}
				else
				{
					playerMovementController.OnLeftButtonDown();
				}
				isMoving = true;
				Debug.Log($"  ✓ STARTED movement - Player will move {direction}");
			}
		}
		else
		{
			Debug.LogError($"  ✗ ERROR: PlayerMovementController is NULL! Cannot move player.");
			Debug.LogError($"  → SOLUTION: Assign PlayerMovementController in Inspector to the Player GameObject.");
		}

		// Visual feedback
		if (spriteRenderer != null)
		{
			spriteRenderer.color = isMoving ? pressedColor : normalColor;
		}
	}

	// Alternative function names for direct assignment (if you want separate start/stop buttons)
	public void StartMovement()
	{
		if (!isMoving && playerMovementController != null)
		{
			if (moveDirection > 0)
			{
				playerMovementController.OnRightButtonDown();
			}
			else
			{
				playerMovementController.OnLeftButtonDown();
			}
			isMoving = true;
			if (spriteRenderer != null)
			{
				spriteRenderer.color = pressedColor;
			}
		}
	}

	public void StopMovement()
	{
		if (isMoving && playerMovementController != null)
		{
			if (moveDirection > 0)
			{
				playerMovementController.OnRightButtonUp();
			}
			else
			{
				playerMovementController.OnLeftButtonUp();
			}
			isMoving = false;
			if (spriteRenderer != null)
			{
				spriteRenderer.color = normalColor;
			}
		}
	}

	void OnDisable()
	{
		// Stop movement when button is disabled
		if (isMoving && playerMovementController != null)
		{
			StopMovement();
		}
	}
}
