using System.Collections.Generic;
using System.IO;
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
        CreateMainMenu();
        CreateEnding("GoodEnding", RunOutcome.GoodEnding);
        CreateEnding("BadEnding", RunOutcome.BadEnding);
        BuildThreeFloorGameFlow();
    }

    [MenuItem("LILO/Build Three Floor Game Flow")]
    public static void BuildThreeFloorGameFlow()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        CloneOfficeLevel("OfficeLevel2");
        CloneOfficeLevel("OfficeLevel3");
        EnsureGameplayScene("OfficeLevel1", FloorId.Floor52, "OfficeLevel2", FloorId.Floor51);
        EnsureGameplayScene("OfficeLevel2", FloorId.Floor51, "OfficeLevel3", FloorId.Floor50);
        EnsureGameplayScene("OfficeLevel3", FloorId.Floor50, "GoodEnding", FloorId.Floor50);
        SetOfficeOrthographicCameras();
        ApplyOfficeTraversalAndSpeedTuning();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidateThreeFloorGameFlow();
        Debug.Log("[GameLoopSetup] OfficeLevel1 → OfficeLevel2 → OfficeLevel3 → GoodEnding, with shared HUD and no jump.");
    }

    [MenuItem("LILO/Apply Gameplay HUD Prefab To Office Scenes")]
    public static void ApplyGameplayHudPrefabToOfficeScenes()
    {
        string[] scenes = { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" };
        foreach (string sceneName in scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(SceneRoot + sceneName + ".unity", OpenSceneMode.Single);
            EnsureGameplayHud();
            EditorSceneManager.SaveScene(scene);
        }
    }

    [MenuItem("LILO/Set Office Orthographic Cameras")]
    public static void SetOfficeOrthographicCameras()
    {
        string[] scenes = { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" };
        foreach (string sceneName in scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(SceneRoot + sceneName + ".unity", OpenSceneMode.Single);
            var player = GameObject.Find("PlayerCharacter");
            var camera = GameObject.Find("Main Camera")?.GetComponent<UnityEngine.Camera>();
            if (player == null || camera == null)
                throw new System.InvalidOperationException($"{sceneName} requires PlayerCharacter and Main Camera.");

            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.transform.position = player.transform.position + new Vector3(0f, 13f, -14f);
            camera.transform.rotation = Quaternion.Euler(43f, 0f, 0f);
            var follow = camera.GetComponent<global::CameraFollow>() ?? camera.gameObject.AddComponent<global::CameraFollow>();
            var followSerialized = new SerializedObject(follow);
            followSerialized.FindProperty("target").objectReferenceValue = player.transform;
            followSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(camera.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
            PrefabUtility.RecordPrefabInstancePropertyModifications(camera.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[CameraSetup] {sceneName}: orthographic size={camera.orthographicSize}, pitch={camera.transform.eulerAngles.x:0.#}°, camera position={camera.transform.position}.");
        }
    }

    [MenuItem("LILO/Apply Office Traversal And Speed Tuning")]
    public static void ApplyOfficeTraversalAndSpeedTuning()
    {
        string[] scenes = { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" };
        foreach (string sceneName in scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(SceneRoot + sceneName + ".unity", OpenSceneMode.Single);
            var player = GameObject.Find("PlayerCharacter");
            var controller = player != null ? player.GetComponent<CharacterController>() : null;
            var speeds = Object.FindAnyObjectByType<GameplaySpeedSettings>();
            if (controller == null || speeds == null)
                throw new System.InvalidOperationException($"{sceneName} is missing Player CharacterController or GameplaySpeedSettings.");

            controller.radius = 0.18f;
            EditorUtility.SetDirty(controller);
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            speeds.playerWalkSpeed = 1f;
            speeds.playerSprintSpeed = 3f;
            speeds.monsterPatrolSpeed = speeds.playerWalkSpeed * 0.5f;
            speeds.monsterChaseSpeed = speeds.playerSprintSpeed * 0.5f;
            EditorUtility.SetDirty(speeds);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[OfficeTuning] {sceneName}: player=1/3, monster base=0.5/1.5, runtime multiplier from GameConfig, collider radius={controller.radius}.");
        }

        var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
        if (config != null)
        {
            config.walkSpeed = 1f;
            config.sprintMultiplier = 3f;
            config.playerRadius = 0.25f;
            config.monsterTuningFloor51.patrolSpeed = 0.5f;
            config.monsterTuningFloor51.chaseSpeed = 1.5f;
            config.monsterTuningFloor50.patrolSpeed = 0.5f;
            config.monsterTuningFloor50.chaseSpeed = 1.5f;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("LILO/Validate Three Floor Game Flow")]
    public static void ValidateThreeFloorGameFlow()
    {
        string[] scenes = { "OfficeLevel1", "OfficeLevel2", "OfficeLevel3" };
        FloorId[] floors = { FloorId.Floor52, FloorId.Floor51, FloorId.Floor50 };
        string[] destinations = { "OfficeLevel2", "OfficeLevel3", "GoodEnding" };

        for (int index = 0; index < scenes.Length; index++)
        {
            Scene scene = EditorSceneManager.OpenScene(SceneRoot + scenes[index] + ".unity", OpenSceneMode.Single);
            var controller = GameObject.Find("GameLoopSceneController")?.GetComponent<GameLoopSceneController>();
            var hud = GameObject.Find("GameplayHudCanvas")?.GetComponent<GameplayHudController>();
            var door = GameObject.Find("ExitDoorPlaceholder")?.GetComponent<ExitDoorInteraction>();
            var speeds = Object.FindAnyObjectByType<GameplaySpeedSettings>();
            var capsule = GameObject.Find("PlayerCharacter")?.GetComponent<CharacterController>();
            var character = GameObject.Find("PlayerCharacter")?.GetComponent<StarterAssets.ThirdPersonController>();
            if (controller == null || hud == null || door == null || speeds == null || capsule == null || character == null)
                throw new System.InvalidOperationException($"{scene.name} is missing flow, HUD, door, speed settings, or player.");

            var flow = new SerializedObject(controller);
            var exit = new SerializedObject(door);
            int actualFloor = flow.FindProperty("floor").enumValueIndex;
            string actualNextScene = flow.FindProperty("nextSceneName").stringValue;
            string actualDoorScene = exit.FindProperty("nextSceneName").stringValue;
            if (actualFloor != (int)floors[index]
                || actualNextScene != destinations[index]
                || actualDoorScene != destinations[index]
                || speeds.playerWalkSpeed != 1f
                || speeds.playerSprintSpeed != 3f
                || speeds.monsterPatrolSpeed != speeds.playerWalkSpeed * 0.5f
                || speeds.monsterChaseSpeed != speeds.playerSprintSpeed * 0.5f
                || capsule.radius > 0.18f
                || character.JumpHeight > 0f)
                throw new System.InvalidOperationException($"{scene.name}: floor={actualFloor}, next={actualNextScene}, player={speeds.playerWalkSpeed}/{speeds.playerSprintSpeed}, monster={speeds.monsterPatrolSpeed}/{speeds.monsterChaseSpeed}, radius={capsule.radius}, jump={character.JumpHeight}.");

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == "JumpButton")
                        throw new System.InvalidOperationException($"{scene.name} still contains JumpButton.");

            Debug.Log($"[GameLoopSetup] Validated {scene.name}: {floors[index]} → {destinations[index]}, HUD present, player={speeds.playerWalkSpeed:0.##}/{speeds.playerSprintSpeed:0.##}, monster base={speeds.monsterPatrolSpeed:0.##}/{speeds.monsterChaseSpeed:0.##}, runtime multiplier from GameConfig, radius={capsule.radius:0.##}, jump disabled.");
        }
    }

    private static void CloneOfficeLevel(string sceneName)
    {
        string source = SceneRoot + "OfficeLevel1.unity";
        string destination = SceneRoot + sceneName + ".unity";
        Scene active = SceneManager.GetActiveScene();
        if (active.path == source && active.isDirty)
            EditorSceneManager.SaveScene(active);

        File.Copy(Path.Combine(Application.dataPath, "Scenes/OfficeLevel1.unity"),
            Path.Combine(Application.dataPath, "Scenes", sceneName + ".unity"), true);
        AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceUpdate);
    }

    private static void EnsureGameplayScene(string sceneName, FloorId floor, string nextScene, FloorId nextFloor)
    {
        string path = SceneRoot + sceneName + ".unity";
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        EnsureEventSystem();
        Lilo.Editor.SetupKeyboardBatteries.Run();

        var controllerGo = GameObject.Find("GameLoopSceneController") ?? new GameObject("GameLoopSceneController");
        var controller = controllerGo.GetComponent<GameLoopSceneController>() ?? controllerGo.AddComponent<GameLoopSceneController>();
        var controllerSerialized = new SerializedObject(controller);
        controllerSerialized.FindProperty("role").enumValueIndex = (int)GameLoopSceneRole.Gameplay;
        controllerSerialized.FindProperty("floor").enumValueIndex = (int)floor;
        controllerSerialized.FindProperty("nextSceneName").stringValue = nextScene;
        controllerSerialized.FindProperty("nextFloor").enumValueIndex = (int)nextFloor;
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

        var manager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        if (manager != null)
        {
            var managerSerialized = new SerializedObject(manager);
            managerSerialized.FindProperty("useConfiguredStartingFloor").boolValue = true;
            managerSerialized.FindProperty("configuredStartingFloor").enumValueIndex = (int)floor;
            managerSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        var speeds = Object.FindAnyObjectByType<GameplaySpeedSettings>();
        if (speeds != null)
        {
            speeds.monsterPatrolSpeed = speeds.playerWalkSpeed * 0.5f;
            speeds.monsterChaseSpeed = speeds.playerSprintSpeed * 0.5f;
            EditorUtility.SetDirty(speeds);
        }

        var character = GameObject.Find("PlayerCharacter")?.GetComponent<StarterAssets.ThirdPersonController>();
        if (character != null)
        {
            character.JumpHeight = 0f;
            EditorUtility.SetDirty(character);
            PrefabUtility.RecordPrefabInstancePropertyModifications(character);
        }

        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == "JumpButton")
                {
                    Object.DestroyImmediate(child.gameObject);
                    break;
                }

        EnsureGameplayHud();

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
        menuSerialized.FindProperty("mainMenuBgm").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sfx/main-menu.mp3");
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
        var paths = new List<string>
        {
            SceneRoot + "MainMenu.unity",
            SceneRoot + "OfficeLevel1.unity",
            SceneRoot + "OfficeLevel2.unity",
            SceneRoot + "OfficeLevel3.unity",
            SceneRoot + "GoodEnding.unity",
            SceneRoot + "BadEnding.unity",
        };
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!paths.Contains(scene.path)) paths.Add(scene.path);
        }
        var settings = new List<EditorBuildSettingsScene>();
        foreach (var path in paths)
            settings.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = settings.ToArray();
    }

    private static void EnsureGameplayHud()
    {
        var old = GameObject.Find("GameplayHudCanvas");
        if (old != null) Object.DestroyImmediate(old);

        // Tata letak HUD dan Pause Menu ada di prefab (LILO/UI/2. Build ... Prefabs).
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/GameplayHud.prefab");
        if (prefab == null)
            throw new System.InvalidOperationException("GameplayHud.prefab belum ada. Jalankan LILO/UI/2 dulu.");
        var hud = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        hud.name = "GameplayHudCanvas";
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
        if (Object.FindAnyObjectByType<EventSystem>() == null) CreateEventSystem();
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
