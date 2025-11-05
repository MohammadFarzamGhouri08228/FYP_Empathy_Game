using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class MovementButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[Header("Button Settings")]
	[SerializeField] private float moveDirection = 1f; // 1 for Right, -1 for Left
	[SerializeField] private PlayerController playerController;

	[Header("Visual Feedback")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

	private bool isPressed = false;

	void Start()
	{
		// Ensure EventSystem exists
		if (EventSystem.current == null)
		{
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			
			#if ENABLE_INPUT_SYSTEM
			es.AddComponent<InputSystemUIInputModule>();
			Debug.Log("MovementButton: Created EventSystem with InputSystemUIInputModule");
			#else
			es.AddComponent<StandaloneInputModule>();
			Debug.Log("MovementButton: Created EventSystem with StandaloneInputModule");
			#endif
		}

		// Find PlayerController if not assigned
		if (playerController == null)
		{
			playerController = FindFirstObjectByType<PlayerController>();
			if (playerController == null)
			{
				Debug.LogError($"MovementButton [{gameObject.name}]: PlayerController not found! Please assign it in the Inspector or ensure a PlayerController exists in the scene.");
			}
			else
			{
				Debug.Log($"MovementButton [{gameObject.name}]: Found PlayerController automatically");
			}
		}

		// Get SpriteRenderer if not assigned
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		// Ensure there's a Collider2D for touch detection
		Collider2D col = GetComponent<Collider2D>();
		if (col == null)
		{
			// Add BoxCollider2D if sprite exists
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
		else
		{
			Debug.Log($"MovementButton [{gameObject.name}]: Found existing Collider2D ({col.GetType().Name})");
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isPressed = true;
		string direction = moveDirection > 0 ? "RIGHT" : "LEFT";
		Debug.Log($"=== BUTTON CLICKED: {direction} BUTTON [{gameObject.name}] ===");
		Debug.Log($"  - Button Name: {gameObject.name}");
		Debug.Log($"  - Move Direction: {moveDirection}");
		Debug.Log($"  - PlayerController Reference: {(playerController != null ? "ASSIGNED" : "NULL - ERROR!")}");
		Debug.Log($"  - EventSystem: {(EventSystem.current != null ? "EXISTS" : "MISSING")}");
		Debug.Log($"  - Collider2D: {(GetComponent<Collider2D>() != null ? "EXISTS" : "MISSING")}");
		
		if (playerController != null)
		{
			playerController.SetMoveInput(moveDirection);
			Debug.Log($"  ✓ Successfully sent movement command to PlayerController");
		}
		else
		{
			Debug.LogError($"  ✗ ERROR: PlayerController is NULL! Cannot move player.");
			Debug.LogError($"  → SOLUTION: Assign PlayerController in Inspector or ensure one exists in scene.");
		}

		// Visual feedback
		if (spriteRenderer != null)
		{
			spriteRenderer.color = pressedColor;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPressed = false;
		string direction = moveDirection > 0 ? "RIGHT" : "LEFT";
		Debug.Log($"=== BUTTON RELEASED: {direction} BUTTON [{gameObject.name}] ===");
		
		if (playerController != null)
		{
			playerController.SetMoveInput(0f); // Stop movement
			Debug.Log($"  ✓ Stopped movement command sent to PlayerController");
		}

		// Visual feedback
		if (spriteRenderer != null)
		{
			spriteRenderer.color = normalColor;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		// Stop movement if pointer exits button while pressed
		if (isPressed)
		{
			OnPointerUp(eventData);
		}
	}

	void OnDisable()
	{
		// Ensure movement stops when button is disabled
		if (isPressed && playerController != null)
		{
			playerController.SetMoveInput(0f);
			isPressed = false;
		}
	}

	// For legacy OnMouseDown support (works without EventSystem, but less reliable on mobile)
	void OnMouseDown()
	{
		if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
		{
			OnPointerDown(null);
		}
	}

	void OnMouseUp()
	{
		if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
		{
			OnPointerUp(null);
		}
	}
}

