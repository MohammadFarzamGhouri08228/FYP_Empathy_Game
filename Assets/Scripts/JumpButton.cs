using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class JumpButton : MonoBehaviour, IPointerDownHandler
{
	[Header("Button Settings")]
	[SerializeField] private PlayerController playerController;

	[Header("Visual Feedback")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Color normalColor = Color.white;
	[SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
	[SerializeField] private float pressDuration = 0.1f;

	private float pressTimer = 0f;

	void Start()
	{
		// Ensure EventSystem exists
		if (EventSystem.current == null)
		{
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			
			#if ENABLE_INPUT_SYSTEM
			es.AddComponent<InputSystemUIInputModule>();
			Debug.Log("JumpButton: Created EventSystem with InputSystemUIInputModule");
			#else
			es.AddComponent<StandaloneInputModule>();
			Debug.Log("JumpButton: Created EventSystem with StandaloneInputModule");
			#endif
		}

		// Find PlayerController if not assigned
		if (playerController == null)
		{
			playerController = FindFirstObjectByType<PlayerController>();
			if (playerController == null)
			{
				Debug.LogError($"JumpButton [{gameObject.name}]: PlayerController not found! Please assign it in the Inspector or ensure a PlayerController exists in the scene.");
			}
			else
			{
				Debug.Log($"JumpButton [{gameObject.name}]: Found PlayerController automatically");
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
				Debug.Log($"JumpButton [{gameObject.name}]: Added BoxCollider2D (size: {boxCol.size})");
			}
			else
			{
				Debug.LogWarning($"JumpButton [{gameObject.name}]: No sprite found. Adding default BoxCollider2D.");
				BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
				boxCol.size = Vector2.one;
			}
		}
		else
		{
			Debug.Log($"JumpButton [{gameObject.name}]: Found existing Collider2D ({col.GetType().Name})");
		}
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

	public void OnPointerDown(PointerEventData eventData)
	{
		Debug.Log($"=== BUTTON CLICKED: JUMP BUTTON [{gameObject.name}] ===");
		Debug.Log($"  - Button Name: {gameObject.name}");
		Debug.Log($"  - PlayerController Reference: {(playerController != null ? "ASSIGNED" : "NULL - ERROR!")}");
		Debug.Log($"  - EventSystem: {(EventSystem.current != null ? "EXISTS" : "MISSING")}");
		Debug.Log($"  - Collider2D: {(GetComponent<Collider2D>() != null ? "EXISTS" : "MISSING")}");
		
		if (playerController != null)
		{
			playerController.OnJumpButtonPressed();
			Debug.Log($"  ✓ Successfully sent jump command to PlayerController");
		}
		else
		{
			Debug.LogError($"  ✗ ERROR: PlayerController is NULL! Cannot jump.");
			Debug.LogError($"  → SOLUTION: Assign PlayerController in Inspector or ensure one exists in scene.");
		}

		// Visual feedback
		if (spriteRenderer != null)
		{
			spriteRenderer.color = pressedColor;
			pressTimer = pressDuration;
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
}

