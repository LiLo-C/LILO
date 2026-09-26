using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Interaction;
using Lilo.MonoBehaviours.Monster;
using Lilo.MonoBehaviours.Flashlight;
using UnityEngine.UI;

/// <summary>
/// Makes OfficeLevel1 a self-contained playable test slice. It is intentionally
/// idempotent and only authors the missing gameplay wiring/placeholder props.
/// </summary>
public static class SetupOfficeGameplay
{
    private const string ScenePath = "Assets/Scenes/OfficeLevel1.unity";

    [MenuItem("LILO/Setup Office Gameplay Slice")]
    public static void Run()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
        if (config == null)
            throw new System.InvalidOperationException("Assets/Config/GameConfig.asset is missing.");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorSettings.serializationMode = SerializationMode.ForceText;
        EnsureGameManager(config);
        EnsureReadabilityFillLight(config);
        EnsurePlayerLighting(config);
        EnsureCameraFollow();
        EnsureEnvironmentDimming();
        RemoveDebugOverlay();
        EnsureGameplayProps(config);
        EnsureNavMesh();
        RemoveJumpButton();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.ForceReserializeAssets(new[] { ScenePath });
        AssetDatabase.SaveAssets();
        Debug.Log("[OfficeGameplaySetup] OfficeLevel1 is wired for monster, exit, battery, light state, and catch loop.");
    }

    [MenuItem("LILO/Remove Office Jump Button")]
    public static void RemoveJumpButtonFromOfficeLevel1()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        RemoveJumpButton();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[OfficeGameplaySetup] Removed JumpButton from OfficeLevel1.");
    }

    [MenuItem("LILO/Repair Office Lighting")]
    public static void RepairOfficeLighting()
    {
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Config/GameConfig.asset");
        if (config == null)
            throw new System.InvalidOperationException("Assets/Config/GameConfig.asset is missing.");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureReadabilityFillLight(config);
        EnsurePlayerLighting(config);
        EnsureEnvironmentDimming();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[OfficeGameplaySetup] Restored OfficeLevel1 flashlight and readability fill lighting.");
    }

    [MenuItem("LILO/Repair Office Camera Follow")]
    public static void RepairOfficeCameraFollow()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnsureCameraFollow();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[OfficeGameplaySetup] Main Camera now follows PlayerCharacter.");
    }

    public static void Validate()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        string[] required =
        {
            "GameManager", "MonsterPlaceholder", "MonsterPatrolRoute", "MonsterSpawns",
            "MonsterSpawnObjectives", "ExitDoorPlaceholder", "BatteryPlaceholder", "NavMesh",
            "MobileControlsCanvas", "MobileInputBridge",
            "EnvironmentLightDimmer", "PlayerFlashlight"
        };
        foreach (string name in required)
        {
            if (GameObject.Find(name) == null)
                throw new System.InvalidOperationException($"OfficeLevel1 is missing '{name}'.");
        }
        if (GameObject.Find("JumpButton") != null)
            throw new System.InvalidOperationException("OfficeLevel1 should not contain a JumpButton.");
        Debug.Log("[OfficeGameplaySetup] Validation passed: gameplay slice objects are present.");
    }

    private static void RemoveJumpButton()
    {
        GameObject jumpButton = GameObject.Find("JumpButton");
        if (jumpButton != null)
            Object.DestroyImmediate(jumpButton);
    }

    private static void EnsureGameManager(GameConfig config)
    {
        var go = GameObject.Find("GameManager");
        if (go == null)
        {
            go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        var manager = go.GetComponent<GameManager>();
        var speedSettings = go.GetComponent<GameplaySpeedSettings>();
        if (speedSettings == null)
        {
            speedSettings = go.AddComponent<GameplaySpeedSettings>();
            var profile = config.GetMonsterProfile(FloorId.Floor51);
            speedSettings.playerWalkSpeed = config.walkSpeed;
            speedSettings.playerSprintSpeed = config.walkSpeed * config.sprintMultiplier;
            speedSettings.monsterPatrolSpeed = speedSettings.playerWalkSpeed * 0.5f;
            speedSettings.monsterChaseSpeed = speedSettings.playerSprintSpeed * 0.5f;
        }
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("config").objectReferenceValue = config;
        serialized.FindProperty("useConfiguredStartingFloor").boolValue = true;
        serialized.FindProperty("configuredStartingFloor").enumValueIndex = (int)FloorId.Floor51;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureGameplayProps(GameConfig config)
    {
        var managerGo = GameObject.Find("GameManager");
        var speedSettings = managerGo != null
            ? managerGo.GetComponent<GameplaySpeedSettings>()
            : null;
        var monsterGo = GameObject.Find("MonsterPlaceholder");
        var monster = monsterGo != null ? monsterGo.GetComponent<MonsterAIController>() : null;
        var sfxGo = GameObject.Find("SfxController");
        var sfx = sfxGo != null ? sfxGo.GetComponent<SfxController>() : null;
        var chaseBgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sfx/ambience/chase-refine.wav");
        var keyPickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sfx/ambience/key-pickup-sfx.mp3");
        if (sfx != null)
        {
            var sfxSerialized = new SerializedObject(sfx);
            if (chaseBgmClip != null)
                sfxSerialized.FindProperty("chaseBgmClip").objectReferenceValue = chaseBgmClip;
            if (keyPickupClip != null)
                sfxSerialized.FindProperty("keyPickupClip").objectReferenceValue = keyPickupClip;
            sfxSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        var door = EnsurePrimitive(
            "ExitDoorPlaceholder",
            PrimitiveType.Cube,
            new Vector3(-9.4f, 1f, 0f),
            new Vector3(0.35f, 2f, 2.5f),
            "Assets/Materials/MonsterArenaDoor.mat",
            new Color(0.2f, 0.45f, 0.8f));
        var doorObstacle = door.GetComponent<NavMeshObstacle>();
        if (doorObstacle == null)
            doorObstacle = door.AddComponent<NavMeshObstacle>();
        if (doorObstacle == null)
            throw new System.InvalidOperationException("Could not add NavMeshObstacle to ExitDoorPlaceholder.");
        doorObstacle.shape = NavMeshObstacleShape.Box;
        doorObstacle.carving = true;
        var exit = door.GetComponent<ExitDoorInteraction>();
        if (exit == null)
            exit = door.AddComponent<ExitDoorInteraction>();
        var exitSerialized = new SerializedObject(exit);
        exitSerialized.FindProperty("interactRadius").floatValue = 2f;
        exitSerialized.FindProperty("monster").objectReferenceValue = monster;
        exitSerialized.ApplyModifiedPropertiesWithoutUndo();

        var battery = EnsurePrimitive(
            "BatteryPlaceholder",
            PrimitiveType.Cylinder,
            new Vector3(-2f, 0.35f, -2f),
            new Vector3(0.45f, 0.45f, 0.45f),
            "Assets/Materials/MonsterArenaBattery.mat",
            new Color(0.95f, 0.75f, 0.12f));
        var batteryPickup = battery.GetComponent<BatteryPickup>();
        if (batteryPickup == null)
            batteryPickup = battery.AddComponent<BatteryPickup>();
        var batterySerialized = new SerializedObject(batteryPickup);
        batterySerialized.FindProperty("config").objectReferenceValue = config;
        batterySerialized.FindProperty("interactRadius").floatValue = 2f;
        batterySerialized.FindProperty("monster").objectReferenceValue = monster;
        batterySerialized.FindProperty("sfx").objectReferenceValue = sfx;
        batterySerialized.ApplyModifiedPropertiesWithoutUndo();

        IgnoreFromNavMeshBake(door);
        IgnoreFromNavMeshBake(battery);

        var objectives = GameObject.Find("MonsterSpawnObjectives");
        if (objectives == null) objectives = new GameObject("MonsterSpawnObjectives");
        var objective = objectives.transform.Find("ExitDoorObjective");
        if (objective == null)
        {
            objective = new GameObject("ExitDoorObjective").transform;
            objective.SetParent(objectives.transform, false);
        }
        objective.position = door.transform.position;

        if (monster != null)
        {
            var monsterSerialized = new SerializedObject(monster);
            monsterSerialized.FindProperty("config").objectReferenceValue = config;
            monsterSerialized.FindProperty("spawnObjectives").objectReferenceValue = objectives.transform;
            monsterSerialized.FindProperty("sfx").objectReferenceValue = sfx;
            monsterSerialized.FindProperty("speedSettings").objectReferenceValue = speedSettings;
            monsterSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        var bridgeObject = GameObject.Find("MobileInputBridge");
        if (bridgeObject != null)
        {
            var bridge = bridgeObject.GetComponent<Lilo.MonoBehaviours.Input.MobileStarterAssetsBridge>();
            if (bridge != null)
            {
                var bridgeSerialized = new SerializedObject(bridge);
                bridgeSerialized.FindProperty("speedSettings").objectReferenceValue = speedSettings;
                bridgeSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var batteryGlow = battery.transform.Find("BatteryGlow");
        if (batteryGlow == null)
        {
            batteryGlow = new GameObject("BatteryGlow").transform;
            batteryGlow.SetParent(battery.transform, false);
        }
        var glow = batteryGlow.GetComponent<Light>();
        if (glow == null)
            glow = batteryGlow.gameObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.range = 2f;
        glow.intensity = 1.5f;
        glow.color = new Color(1f, 0.75f, 0.15f);
    }

    private static void EnsureEnvironmentDimming()
    {
        var go = GameObject.Find("EnvironmentLightDimmer");
        if (go == null) go = new GameObject("EnvironmentLightDimmer");
        var dimmer = go.GetComponent<EnvironmentLightDimmer>();
        if (dimmer == null) dimmer = go.AddComponent<EnvironmentLightDimmer>();
        var serialized = new SerializedObject(dimmer);
        serialized.FindProperty("disableBakedLightmaps").boolValue = true;
        serialized.FindProperty("readabilityFillLight").objectReferenceValue =
            GameObject.Find("ReadabilityFillLight")?.GetComponent<Light>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureReadabilityFillLight(GameConfig config)
    {
        var go = GameObject.Find("ReadabilityFillLight");
        if (go == null)
            go = new GameObject("ReadabilityFillLight");

        var light = go.GetComponent<Light>();
        if (light == null)
            light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = Mathf.Max(0f, config.readabilityFillIntensity);
        light.shadows = LightShadows.None;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void EnsureCameraFollow()
    {
        var player = GameObject.Find("PlayerCharacter");
        if (player == null)
            throw new System.InvalidOperationException("OfficeLevel1 is missing PlayerCharacter.");

        var camera = UnityEngine.Camera.main;
        if (camera == null)
            camera = GameObject.Find("Main Camera")?.GetComponent<UnityEngine.Camera>();
        if (camera == null)
            throw new System.InvalidOperationException("OfficeLevel1 is missing its Main Camera.");

        var follow = camera.GetComponent<global::CameraFollow>();
        if (follow == null)
            follow = camera.gameObject.AddComponent<global::CameraFollow>();

        var serialized = new SerializedObject(follow);
        serialized.FindProperty("target").objectReferenceValue = player.transform;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveDebugOverlay()
    {
        var canvas = GameObject.Find("MobileControlsCanvas");
        if (canvas == null) return;
        foreach (string objectName in new[] { "DebugOverlayText", "LightDebugText", "DebugToggleButton" })
        {
            var debugObject = canvas.transform.Find(objectName);
            if (debugObject != null) Object.DestroyImmediate(debugObject.gameObject);
        }
    }

    private static void EnsurePlayerLighting(GameConfig config)
    {
        var player = GameObject.Find("PlayerCharacter");
        if (player == null)
        {
            Debug.LogWarning("[OfficeGameplaySetup] PlayerCharacter not found; flashlight wiring deferred.");
            return;
        }

        var flashlight = player.transform.Find("PlayerFlashlight");
        if (flashlight == null)
        {
            flashlight = new GameObject("PlayerFlashlight").transform;
            flashlight.SetParent(player.transform, false);
        }
        // Keep the omnidirectional source beside the held lamp prop instead of aiming it forward.
        flashlight.localPosition = new Vector3(-0.3f, 0.85f, 0.15f);
        flashlight.localRotation = Quaternion.identity;

        var light = flashlight.GetComponent<Light>();
        if (light == null)
            light = flashlight.gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = config.flashlightNormalRadius;
        light.intensity = 1.5f;
        light.shadows = config.shadowsEnabled ? LightShadows.Soft : LightShadows.None;
        light.color = new Color(1f, 0.92f, 0.78f);

        var rig = player.GetComponent<LightingRig>();
        if (rig == null)
            rig = player.AddComponent<LightingRig>();
        var serialized = new SerializedObject(rig);
        serialized.FindProperty("config").objectReferenceValue = config;
        serialized.FindProperty("flashlightLight").objectReferenceValue = light;
        serialized.FindProperty("readabilityFillLight").objectReferenceValue =
            GameObject.Find("ReadabilityFillLight")?.GetComponent<Light>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureNavMesh()
    {
        var navGo = GameObject.Find("NavMesh");
        if (navGo == null) navGo = new GameObject("NavMesh");
        var surface = navGo.GetComponent<NavMeshSurface>();
        if (surface == null)
            surface = navGo.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surface.agentTypeID = 0;
        surface.defaultArea = 0;
        surface.BuildNavMesh();
    }

    private static GameObject EnsurePrimitive(string name, PrimitiveType type, Vector3 position,
        Vector3 scale, string materialPath, Color fallbackColor)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = GameObject.CreatePrimitive(type);
            go.name = name;
        }
        go.transform.position = position;
        go.transform.localScale = scale;

        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(materialPath) };
            material.SetColor("_BaseColor", fallbackColor);
            if (material.HasProperty("_Color")) material.color = fallbackColor;
            AssetDatabase.CreateAsset(material, materialPath);
        }
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        if (name == "BatteryPlaceholder")
        {
            Color glowColor = new Color(1f, 0.55f, 0.05f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", glowColor);
            }
        }
        return go;
    }

    private static void IgnoreFromNavMeshBake(GameObject go)
    {
        var modifier = go.GetComponent<NavMeshModifier>();
        if (modifier == null)
            modifier = go.AddComponent<NavMeshModifier>();
        modifier.ignoreFromBuild = true;
    }
}
