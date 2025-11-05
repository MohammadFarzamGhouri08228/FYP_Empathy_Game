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
			
			Debug.Log("StartMenuUI: Canvas created successfully");
		}
		else
		{
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


