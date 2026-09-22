using System.Collections.Generic;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Interaction;
using Lilo.State;
using Lilo.Systems.GameLoop;
using Lilo.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SetupGameLoopScenes
{
    private const string SceneRoot = "Assets/Scenes/";
    private static readonly Color PanelColor = new Color(0.035f, 0.05f, 0.09f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.12f, 0.32f, 0.5f, 1f);

    [MenuItem("LILO/Setup Game Loop Scenes")]
    public static void Run()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EnsureGameplayScene("OfficeLevel1", Lilo.Config.FloorId.Floor51, "OfficeLevel2", Lilo.Config.FloorId.Floor50);
        EnsureGameplayScene("OfficeLevel2", Lilo.Config.FloorId.Floor50, "GoodEnding", Lilo.Config.FloorId.Floor50);
        CreateMainMenu();
        CreateEnding("GoodEnding", RunOutcome.GoodEnding);
        CreateEnding("BadEnding", RunOutcome.BadEnding);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GameLoopSetup] MainMenu, gameplay transitions, settings and endings are ready.");
    }

    private static void EnsureGameplayScene(string sceneName, FloorId floor, string nextScene, FloorId nextFloor)
    {
        string path = SceneRoot + sceneName + ".unity";
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        EnsureEventSystem();

        var controllerGo = GameObject.Find("GameLoopSceneController") ?? new GameObject("GameLoopSceneController");
        var controller = controllerGo.GetComponent<GameLoopSceneController>() ?? controllerGo.AddComponent<GameLoopSceneController>();
        var controllerSerialized = new SerializedObject(controller);
        controllerSerialized.FindProperty("role").enumValueIndex = (int)GameLoopSceneRole.Gameplay;
        controllerSerialized.FindProperty("floor").enumValueIndex = (int)floor;
        controllerSerialized.FindProperty("nextSceneName").stringValue = nextScene;
        controllerSerialized.FindProperty("nextFloor").enumValueIndex = (int)nextFloor;
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

        var doorGo = GameObject.Find("ExitDoorPlaceholder");
        if (doorGo == null)
        {
            doorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorGo.name = "ExitDoorPlaceholder";
            doorGo.transform.position = new Vector3(-9.4f, 1f, 0f);
            doorGo.transform.localScale = new Vector3(1.2f, 2f, 0.25f);
        }
        var door = doorGo.GetComponent<ExitDoorInteraction>() ?? doorGo.AddComponent<ExitDoorInteraction>();
        var doorSerialized = new SerializedObject(door);
        doorSerialized.FindProperty("interactRadius").floatValue = 2.2f;
        doorSerialized.FindProperty("nextSceneName").stringValue = nextScene;
        doorSerialized.FindProperty("advanceToNextFloor").boolValue = true;
        doorSerialized.FindProperty("nextFloor").enumValueIndex = (int)nextFloor;
        doorSerialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
    }

    private static void CreateMainMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas("MainMenuCanvas");
        var menu = canvas.gameObject.AddComponent<MainMenuController>();

        var managerGo = new GameObject("GameManager");
        var manager = managerGo.AddComponent<GameManager>();
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
        var managerSerialized = new SerializedObject(manager);
        managerSerialized.FindProperty("config").objectReferenceValue = config;
        managerSerialized.FindProperty("useConfiguredStartingFloor").boolValue = false;
        managerSerialized.ApplyModifiedPropertiesWithoutUndo();

        CreateEventSystem();
        var root = CreatePanel(canvas.transform, "MenuPanel", new Color(0.015f, 0.025f, 0.05f, 1f));
        var title = CreateText(root.transform, "Title", "LILO", 58, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(0, 90), new Vector2(600, 90));
        var subtitle = CreateText(root.transform, "Subtitle", "A quiet night in the office", 20, TextAnchor.MiddleCenter);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0, 50), new Vector2(600, 50));

        var start = CreateButton(root.transform, "StartButton", "START", new Vector2(0.5f, 0.54f));
        var howToPlay = CreateButton(root.transform, "HowToPlayButton", "HOW TO PLAY", new Vector2(0.5f, 0.43f));
        var settings = CreateButton(root.transform, "SettingsButton", "SETTINGS", new Vector2(0.5f, 0.32f));
        var credits = CreateText(root.transform, "Credits", "LILO\nCreated by LILO-C", 16, TextAnchor.MiddleCenter);
        SetRect(credits.rectTransform, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0, 60), new Vector2(600, 60));

        var howPanel = CreatePanel(canvas.transform, "HowToPlayPanel", PanelColor);
        CreateTextAt(howPanel.transform, "HowToPlayText", "HOW TO PLAY\n\nMove with the joystick.\nUse the lamp wisely.\nFind the exit before the monster catches you.\nYou have 3 lives for the whole run.", 22, new Vector2(0.5f, 0.58f), new Vector2(650, 300));
        var howClose = CreateButton(howPanel.transform, "CloseHowToPlay", "CLOSE", new Vector2(0.5f, 0.22f));

        var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel", PanelColor);
        CreateTextAt(settingsPanel.transform, "SettingsTitle", "SETTINGS", 30, new Vector2(0.5f, 0.82f), new Vector2(600, 60));
        var sound = settingsPanel.gameObject.AddComponent<SoundSettingsPanel>();
        var sliders = new Slider[4];
        string[] labels = { "MASTER", "MUSIC", "EFFECTS", "AMBIENCE" };
        for (int i = 0; i < sliders.Length; i++)
        {
            CreateTextAt(settingsPanel.transform, labels[i] + "Label", labels[i], 16, new Vector2(0.5f, 0.68f - i * 0.1f), new Vector2(220, 40), new Vector2(-170, 0));
            sliders[i] = CreateSlider(settingsPanel.transform, labels[i] + "Slider", new Vector2(0.5f, 0.68f - i * 0.1f));
        }
        var settingsClose = CreateButton(settingsPanel.transform, "CloseSettings", "CLOSE", new Vector2(0.5f, 0.16f));

        var menuSerialized = new SerializedObject(menu);
        menuSerialized.FindProperty("startButton").objectReferenceValue = start;
        menuSerialized.FindProperty("howToPlayButton").objectReferenceValue = howToPlay;
        menuSerialized.FindProperty("settingsButton").objectReferenceValue = settings;
        menuSerialized.FindProperty("howToPlayPanel").objectReferenceValue = howPanel.gameObject;
        menuSerialized.FindProperty("settingsPanel").objectReferenceValue = settingsPanel.gameObject;
        menuSerialized.FindProperty("soundSettings").objectReferenceValue = sound;
        menuSerialized.ApplyModifiedPropertiesWithoutUndo();
        var soundSerialized = new SerializedObject(sound);
        soundSerialized.FindProperty("master").objectReferenceValue = sliders[0];
        soundSerialized.FindProperty("music").objectReferenceValue = sliders[1];
        soundSerialized.FindProperty("effects").objectReferenceValue = sliders[2];
        soundSerialized.FindProperty("ambience").objectReferenceValue = sliders[3];
        soundSerialized.ApplyModifiedPropertiesWithoutUndo();
        UnityEventTools.AddPersistentListener(howClose.onClick, menu.CloseOverlay);
        UnityEventTools.AddPersistentListener(settingsClose.onClick, menu.CloseOverlay);
        EditorSceneManager.SaveScene(scene, SceneRoot + "MainMenu.unity");
    }

    private static void CreateEnding(string sceneName, RunOutcome outcome)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas(sceneName + "Canvas");
        CreateEventSystem();
        var background = CreatePanel(canvas.transform, sceneName + "Panel", new Color(0.015f, 0.025f, 0.05f, 1f));
        var ending = background.gameObject.AddComponent<EndingController>();
        var title = CreateTextAt(background.transform, "Title", "ENDING", 48, new Vector2(0.5f, 0.65f), new Vector2(700, 100));
        var message = CreateTextAt(background.transform, "Message", "", 22, new Vector2(0.5f, 0.48f), new Vector2(700, 120));
        var menuButton = CreateButton(background.transform, "MainMenuButton", "MAIN MENU", new Vector2(0.5f, 0.28f));
        var serialized = new SerializedObject(ending);
        serialized.FindProperty("outcome").enumValueIndex = (int)outcome;
        serialized.FindProperty("title").objectReferenceValue = title;
        serialized.FindProperty("message").objectReferenceValue = message;
        serialized.FindProperty("mainMenuButton").objectReferenceValue = menuButton;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene, SceneRoot + sceneName + ".unity");
    }

    private static void UpdateBuildSettings()
    {
        var paths = new List<string> { SceneRoot + "MainMenu.unity" };
        var enabledByPath = new Dictionary<string, bool>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            enabledByPath[scene.path] = scene.enabled;
            if (!paths.Contains(scene.path) && scene.path != SceneRoot + "MainMenu.unity") paths.Add(scene.path);
        }
        paths.Add(SceneRoot + "GoodEnding.unity");
        paths.Add(SceneRoot + "BadEnding.unity");
        var settings = new List<EditorBuildSettingsScene>();
        foreach (var path in paths)
            settings.Add(new EditorBuildSettingsScene(path, path == SceneRoot + "MainMenu.unity"
                || path == SceneRoot + "GoodEnding.unity"
                || path == SceneRoot + "BadEnding.unity"
                || !enabledByPath.TryGetValue(path, out var enabled) || enabled));
        EditorBuildSettings.scenes = settings.ToArray();
    }

    private static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        return canvas;
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null) CreateEventSystem();
    }

    private static Image CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<Image>();
    }

    private static Text CreateText(Transform parent, string name, string value, int size, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size;
        text.alignment = anchor; text.color = Color.white; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Text CreateTextAt(Transform parent, string name, string value, int size, Vector2 anchor, Vector2 dimensions, Vector2 offset = default)
    {
        var text = CreateText(parent, name, value, size, TextAnchor.MiddleCenter);
        SetRect(text.rectTransform, anchor, anchor, offset, dimensions);
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), anchor, anchor, Vector2.zero, new Vector2(300, 56));
        go.GetComponent<Image>().color = ButtonColor;
        var button = go.GetComponent<Button>();
        var text = CreateTextAt(go.transform, "Label", label, 18, new Vector2(0.5f, 0.5f), new Vector2(300, 56));
        text.raycastTarget = false;
        return button;
    }

    private static Slider CreateSlider(Transform parent, string name, Vector2 anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), anchor, anchor, new Vector2(70, 0), new Vector2(300, 30));
        var slider = go.GetComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;
        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(go.transform, false); SetRect(background.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); background.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(go.transform, false); SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); fill.GetComponent<Image>().color = new Color(0.25f, 0.75f, 1f, 1f);
        slider.fillRect = fill.GetComponent<RectTransform>(); slider.targetGraphic = fill.GetComponent<Image>();
        return slider;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.anchoredPosition = position; rect.sizeDelta = size;
    }
}
