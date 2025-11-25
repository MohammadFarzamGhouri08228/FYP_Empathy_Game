using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class StartMenuUI : MonoBehaviour
{
	[Header("Buttons Config")]
	[SerializeField] private int buttonCount = 1;
	[SerializeField] private string buttonBaseName = "buttons";
	[SerializeField] private Sprite buttonSprite;
	[SerializeField] private Vector2 firstButtonPosition = new Vector2(0f, 0f);
	[SerializeField] private Vector2 verticalSpacing = new Vector2(0f, -80f);
	[SerializeField] private Vector2 buttonSize = new Vector2(200f, 60f);

	[Header("Text Config")]
	[SerializeField] private int fontSize = 24;
	[SerializeField] private Color textColor = Color.black;

	private Canvas canvas;

	void Awake()
	{
		EnsureCanvasAndEventSystem();
	}

	void Start()
	{
		// Ensure we're in the Start scene
		string sceneName = SceneManager.GetActiveScene().name;
		if (!sceneName.Contains("Start"))
		{
			Debug.LogWarning($"StartMenuUI: This script is designed for the Start scene, but current scene is '{sceneName}'. Buttons may not appear correctly.");
		}
		
		// Delay button creation to ensure canvas is fully initialized
		StartCoroutine(CreateButtonsDelayed());
	}
	
	private System.Collections.IEnumerator CreateButtonsDelayed()
	{
		// Wait for end of frame to ensure canvas is fully set up
		yield return new WaitForEndOfFrame();
		
		// Force canvas update before creating buttons
		Canvas.ForceUpdateCanvases();
		
		CreateButtons();
	}

	private void EnsureCanvasAndEventSystem()
	{
		canvas = FindFirstObjectByType<Canvas>();
		if (canvas == null)
		{
			GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			canvas = canvasObj.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
			scaler.matchWidthOrHeight = 0.5f; // Match both width and height
			scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			
			// Force canvas to update immediately
			Canvas.ForceUpdateCanvases();
			
			Debug.Log("StartMenuUI: Canvas created successfully");
		}
		else
		{
			// Ensure existing canvas has proper scaler settings
			CanvasScaler existingScaler = canvas.GetComponent<CanvasScaler>();
			if (existingScaler != null)
			{
				existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				existingScaler.matchWidthOrHeight = 0.5f;
				existingScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			}
			
			// Force canvas to update
			Canvas.ForceUpdateCanvases();
			
			Debug.Log("StartMenuUI: Using existing Canvas");
		}

		if (FindFirstObjectByType<EventSystem>() == null)
		{
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			
			#if ENABLE_INPUT_SYSTEM
			es.AddComponent<InputSystemUIInputModule>();
			Debug.Log("StartMenuUI: EventSystem created with InputSystemUIInputModule");
			#else
			es.AddComponent<StandaloneInputModule>();
			Debug.Log("StartMenuUI: EventSystem created with StandaloneInputModule");
			#endif
		}
	}

	private void CreateButtons()
	{
		if (canvas == null)
		{
			Debug.LogError("StartMenuUI: Canvas not found or created.");
			return;
		}

		Debug.Log($"StartMenuUI: Creating {buttonCount} button(s)");
		for (int i = 0; i < Mathf.Max(1, buttonCount); i++)
		{
			Vector2 pos = firstButtonPosition + new Vector2(verticalSpacing.x * i, verticalSpacing.y * i);
			string buttonName = buttonBaseName + (buttonCount > 1 ? $"_{i + 1}" : string.Empty);
			CreateButton(buttonName, pos);
			Debug.Log($"StartMenuUI: Created button '{buttonName}' at position {pos}");
		}
	}

	private void CreateButton(string name, Vector2 anchoredPosition)
	{
		GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
		buttonObj.transform.SetParent(canvas.transform, false);

		RectTransform rt = buttonObj.GetComponent<RectTransform>();
		rt.sizeDelta = buttonSize;
		rt.anchorMin = new Vector2(0.5f, 0.5f);
		rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = anchoredPosition;
		
		// Ensure the button is within valid screen bounds
		Canvas.ForceUpdateCanvases();
		
		// For ScreenSpaceOverlay canvas, validate anchored position directly
		// Get canvas scaler to account for scaling
		CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
		if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
		{
			// Calculate effective screen bounds considering canvas scaling
			float scaleFactor = canvas.scaleFactor;
			float maxX = (Screen.width / scaleFactor) * 0.4f;
			float maxY = (Screen.height / scaleFactor) * 0.4f;
			
			// Clamp position to be within reasonable bounds to prevent view frustum errors
			if (Mathf.Abs(anchoredPosition.x) > maxX || Mathf.Abs(anchoredPosition.y) > maxY)
			{
				Debug.LogWarning($"StartMenuUI: Button '{name}' position adjusted to prevent view frustum errors.");
				rt.anchoredPosition = new Vector2(
					Mathf.Clamp(anchoredPosition.x, -maxX, maxX),
					Mathf.Clamp(anchoredPosition.y, -maxY, maxY)
				);
			}
		}

		Image img = buttonObj.GetComponent<Image>();
		if (buttonSprite != null)
		{
			img.sprite = buttonSprite;
			img.type = Image.Type.Sliced;
		}
		else
		{
			// Set a default color so button is visible even without sprite
			img.color = new Color(0.2f, 0.6f, 1f, 1f); // Light blue color
		}

		Button btn = buttonObj.GetComponent<Button>();
		btn.targetGraphic = img;
		
		// Add colors to button for better visibility
		ColorBlock colors = btn.colors;
		colors.normalColor = img.color;
		colors.highlightedColor = new Color(img.color.r * 1.2f, img.color.g * 1.2f, img.color.b * 1.2f, img.color.a);
		colors.pressedColor = new Color(img.color.r * 0.8f, img.color.g * 0.8f, img.color.b * 0.8f, img.color.a);
		btn.colors = colors;

		GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
		textObj.transform.SetParent(buttonObj.transform, false);

		RectTransform trt = textObj.GetComponent<RectTransform>();
		trt.anchorMin = new Vector2(0, 0);
		trt.anchorMax = new Vector2(1, 1);
		trt.offsetMin = Vector2.zero;
		trt.offsetMax = Vector2.zero;

		Text txt = textObj.GetComponent<Text>();
		txt.text = name;
		txt.alignment = TextAnchor.MiddleCenter;
		txt.color = textColor;
		txt.fontSize = fontSize;
		txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		txt.supportRichText = false;
		
		// Ensure text is visible
		if (txt.font == null)
		{
			Debug.LogWarning("StartMenuUI: Could not load Arial font, text may not display");
		}
	}
}


