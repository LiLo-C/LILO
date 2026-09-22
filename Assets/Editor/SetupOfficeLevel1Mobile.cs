using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds the shared mobile controls to OfficeLevel1 using the tuned OfficeLevel2 setup.
/// Wires the mobile controls to the Starter Assets character used by OfficeLevel1.
/// </summary>
public static class SetupOfficeLevel1Mobile
{
    private const string OfficeScenePath = "Assets/Scenes/OfficeLevel1.unity";
    private const string SourceMobileScenePath = "Assets/Scenes/OfficeLevel2.unity";

    public static void Run()
    {
        Scene office = EditorSceneManager.OpenScene(OfficeScenePath, OpenSceneMode.Single);
        Scene source = EditorSceneManager.OpenScene(SourceMobileScenePath, OpenSceneMode.Additive);

        GameObject sourceCanvas = FindInScene(source, "MobileControlsCanvas");
        if (sourceCanvas == null)
            throw new System.InvalidOperationException("OfficeLevel2 is missing MobileControlsCanvas.");

        GameObject canvasObject = FindInScene(office, "MobileControlsCanvas");

        Transform canvasTransform;
        if (canvasObject == null)
        {
            canvasObject = Object.Instantiate(sourceCanvas);
            canvasObject.name = "MobileControlsCanvas";
            SceneManager.MoveGameObjectToScene(canvasObject, office);
            canvasTransform = canvasObject.transform;
        }
        else
        {
            canvasTransform = canvasObject.transform;
            CopyCanvasSettings(sourceCanvas, canvasObject);
            RemoveIfPresent(canvasTransform, "JoystickBackground");
            GameObject sourceJoystick = FindInScene(source, "JoystickBackground");
            GameObject joystick = Object.Instantiate(sourceJoystick);
            joystick.name = "JoystickBackground";
            SceneManager.MoveGameObjectToScene(joystick, office);
            joystick.transform.SetParent(canvasTransform, false);
        }

        if (FindInScene(office, "EventSystem") == null)
        {
            GameObject sourceEventSystem = FindInScene(source, "EventSystem");
            if (sourceEventSystem != null)
            {
                GameObject eventSystem = Object.Instantiate(sourceEventSystem);
                eventSystem.name = "EventSystem";
                SceneManager.MoveGameObjectToScene(eventSystem, office);
            }
            else
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                SceneManager.MoveGameObjectToScene(eventSystem, office);
            }
        }

        if (canvasTransform.Find("MobileInputBridge") == null)
        {
            var bridge = new GameObject("MobileInputBridge");
            bridge.transform.SetParent(canvasTransform, false);
            bridge.AddComponent<Lilo.MonoBehaviours.Input.MobileStarterAssetsBridge>();
        }

        EditorSceneManager.MarkSceneDirty(office);
        EditorSceneManager.SaveScene(office);
        EditorSceneManager.CloseScene(source, true);

        Debug.Log("OfficeLevel1 mobile controls configured from OfficeLevel2: canvas, joystick, EventSystem, and bridge copied.");
    }

    private static void CopyCanvasSettings(GameObject source, GameObject destination)
    {
        var sourceCanvas = source.GetComponent<Canvas>();
        var destinationCanvas = destination.GetComponent<Canvas>();
        if (sourceCanvas != null && destinationCanvas != null)
        {
            destinationCanvas.renderMode = sourceCanvas.renderMode;
            destinationCanvas.worldCamera = sourceCanvas.worldCamera;
            destinationCanvas.planeDistance = sourceCanvas.planeDistance;
            destinationCanvas.pixelPerfect = sourceCanvas.pixelPerfect;
            destinationCanvas.sortingOrder = sourceCanvas.sortingOrder;
        }

        var sourceScaler = source.GetComponent<CanvasScaler>();
        var destinationScaler = destination.GetComponent<CanvasScaler>();
        if (sourceScaler != null && destinationScaler != null)
        {
            destinationScaler.uiScaleMode = sourceScaler.uiScaleMode;
            destinationScaler.referenceResolution = sourceScaler.referenceResolution;
            destinationScaler.screenMatchMode = sourceScaler.screenMatchMode;
            destinationScaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
            destinationScaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;
        }
    }

    private static GameObject CreateCanvas()
    {
        var canvasObject = new GameObject(
            "MobileControlsCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvasObject;
    }

    private static void CreateJumpButton(Transform canvas)
    {
        var buttonObject = new GameObject("JumpButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-96f, -48f);
        rect.sizeDelta = new Vector2(84f, 84f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.42f);
        image.raycastTarget = true;

        var button = buttonObject.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.42f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.62f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.82f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        var label = labelObject.GetComponent<Text>();
        label.text = "JUMP";
        label.alignment = TextAnchor.MiddleCenter;
        label.fontSize = 16;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.raycastTarget = false;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == name && transform.gameObject.scene == scene)
                {
                    return transform.gameObject;
                }
            }
        }

        return null;
    }

    private static void RemoveIfPresent(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);
    }
}
