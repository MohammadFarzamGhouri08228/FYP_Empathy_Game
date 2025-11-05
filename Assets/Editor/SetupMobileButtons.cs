using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class SetupMobileButtons
{
	[MenuItem("Tools/Buttons/Setup Mobile Control Buttons")]
	public static void Setup()
	{
		// Find PlayerController
		PlayerController playerController = Object.FindFirstObjectByType<PlayerController>();
		if (playerController == null)
		{
			EditorUtility.DisplayDialog("Error", "PlayerController not found in the scene! Please add a PlayerController first.", "OK");
			return;
		}

		// Ensure EventSystem exists
		if (Object.FindFirstObjectByType<EventSystem>() == null)
		{
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			
			#if ENABLE_INPUT_SYSTEM
			es.AddComponent<InputSystemUIInputModule>();
			Debug.Log("Created EventSystem with InputSystemUIInputModule for mobile controls");
			#else
			es.AddComponent<StandaloneInputModule>();
			Debug.Log("Created EventSystem with StandaloneInputModule for mobile controls");
			#endif
		}

		// Find buttons
		Transform leftButton = FindButton("Left");
		Transform rightButton = FindButton("Right");
		Transform jumpButton = FindButton("Jump");

		bool setupComplete = true;

		// Setup Left Button
		if (leftButton != null)
		{
			SetupMovementButton(leftButton.gameObject, playerController, -1f);
			Debug.Log("Left button setup complete!");
		}
		else
		{
			Debug.LogWarning("Left button not found! Please create a GameObject named 'Left'");
			setupComplete = false;
		}

		// Setup Right Button
		if (rightButton != null)
		{
			SetupMovementButton(rightButton.gameObject, playerController, 1f);
			Debug.Log("Right button setup complete!");
		}
		else
		{
			Debug.LogWarning("Right button not found! Please create a GameObject named 'Right'");
			setupComplete = false;
		}

		// Setup Jump Button
		if (jumpButton != null)
		{
			SetupJumpButton(jumpButton.gameObject, playerController);
			Debug.Log("Jump button setup complete!");
		}
		else
		{
			Debug.LogWarning("Jump button not found! Please create a GameObject named 'Jump'");
			setupComplete = false;
		}

		if (setupComplete)
		{
			EditorUtility.DisplayDialog("Setup Complete", "All buttons have been configured!\n\nMake sure:\n1. Buttons have SpriteRenderer components\n2. Buttons have Collider2D components (auto-added if missing)\n3. PlayerController is assigned (auto-assigned)", "OK");
		}
		else
		{
			EditorUtility.DisplayDialog("Partial Setup", "Some buttons were not found. Please create missing buttons and run this tool again.", "OK");
		}
	}

	private static Transform FindButton(string buttonName)
	{
		// Try exact match first
		GameObject obj = GameObject.Find(buttonName);
		if (obj != null) return obj.transform;

		// Try finding in Buttons parent
		GameObject buttonsParent = GameObject.Find("Buttons");
		if (buttonsParent != null)
		{
			Transform found = buttonsParent.transform.Find(buttonName);
			if (found != null) return found;
		}

		// Try case-insensitive search
		GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		foreach (GameObject go in allObjects)
		{
			if (go.name.Equals(buttonName, System.StringComparison.OrdinalIgnoreCase))
			{
				return go.transform;
			}
		}

		return null;
	}

	private static void SetupMovementButton(GameObject buttonObj, PlayerController playerController, float direction)
	{
		// Remove existing MovementButton if any
		MovementButton existing = buttonObj.GetComponent<MovementButton>();
		if (existing != null)
		{
			Object.DestroyImmediate(existing);
		}

		// Add MovementButton component
		MovementButton movementButton = buttonObj.AddComponent<MovementButton>();
		
		// Set up serialized fields
		SerializedObject serialized = new SerializedObject(movementButton);
		serialized.FindProperty("moveDirection").floatValue = direction;
		serialized.FindProperty("playerController").objectReferenceValue = playerController;
		
		// Try to find SpriteRenderer
		SpriteRenderer sr = buttonObj.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
			serialized.FindProperty("spriteRenderer").objectReferenceValue = sr;
		}
		
		serialized.ApplyModifiedProperties();

		// Ensure Collider2D exists
		if (buttonObj.GetComponent<Collider2D>() == null)
		{
			BoxCollider2D col = buttonObj.AddComponent<BoxCollider2D>();
			if (sr != null && sr.sprite != null)
			{
				col.size = sr.sprite.bounds.size;
				col.offset = sr.sprite.bounds.center;
			}
		}
	}

	private static void SetupJumpButton(GameObject buttonObj, PlayerController playerController)
	{
		// Remove existing JumpButton if any
		JumpButton existing = buttonObj.GetComponent<JumpButton>();
		if (existing != null)
		{
			Object.DestroyImmediate(existing);
		}

		// Add JumpButton component
		JumpButton jumpButton = buttonObj.AddComponent<JumpButton>();
		
		// Set up serialized fields
		SerializedObject serialized = new SerializedObject(jumpButton);
		serialized.FindProperty("playerController").objectReferenceValue = playerController;
		
		// Try to find SpriteRenderer
		SpriteRenderer sr = buttonObj.GetComponent<SpriteRenderer>();
		if (sr != null)
		{
			serialized.FindProperty("spriteRenderer").objectReferenceValue = sr;
		}
		
		serialized.ApplyModifiedProperties();

		// Ensure Collider2D exists
		if (buttonObj.GetComponent<Collider2D>() == null)
		{
			BoxCollider2D col = buttonObj.AddComponent<BoxCollider2D>();
			if (sr != null && sr.sprite != null)
			{
				col.size = sr.sprite.bounds.size;
				col.offset = sr.sprite.bounds.center;
			}
		}
	}
}

