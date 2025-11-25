using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class CreateStartMenuUI
{
	[MenuItem("Tools/Start Menu/Create Start Menu UI")]
	public static void Create()
	{
		// Ensure Canvas exists
		Canvas canvas = Object.FindFirstObjectByType<Canvas>();
		if (canvas == null)
		{
			GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			canvas = canvasObj.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
		}

		// Ensure EventSystem exists
		if (Object.FindFirstObjectByType<EventSystem>() == null)
		{
			GameObject es = new GameObject("EventSystem");
			es.AddComponent<EventSystem>();
			
			#if ENABLE_INPUT_SYSTEM
			es.AddComponent<InputSystemUIInputModule>();
			Debug.Log("Created EventSystem with InputSystemUIInputModule");
			#else
			es.AddComponent<StandaloneInputModule>();
			Debug.Log("Created EventSystem with StandaloneInputModule");
			#endif
		}

		// Create holder
		GameObject holder = new GameObject("StartMenuUI", typeof(RectTransform));
		holder.transform.SetParent(canvas.transform, false);

		// Add script component
		var startMenu = holder.AddComponent<StartMenuUI>();

		// Set some sensible defaults; sprite left null for user to assign
		SerializedObject so = new SerializedObject(startMenu);
		so.FindProperty("buttonCount").intValue = 1;
		so.FindProperty("buttonBaseName").stringValue = "buttons";
		so.ApplyModifiedPropertiesWithoutUndo();

		// Focus selection so user can assign sprite immediately
		Selection.activeGameObject = holder;
		EditorGUIUtility.PingObject(holder);

		Debug.Log("StartMenuUI object created. Select it to assign the Button Sprite in the Inspector.");
	}
}


