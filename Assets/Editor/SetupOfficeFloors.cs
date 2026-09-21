using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Interaction;
using Lilo.MonoBehaviours.Monster;

/// <summary>
/// Authors the first two office floor scenes from the current OfficeLevel1 layout.
/// Floor 1 gets the playable exit area; Floor 2 keeps the layout but has no exit yet.
/// </summary>
public static class SetupOfficeFloors
{
    private const string Floor1Path = "Assets/Scenes/OfficeLevel1.unity";
    private const string Floor2Path = "Assets/Scenes/OfficeLevel2.unity";
    private const string ExitMaterialPath = "Assets/Materials/OfficeExitArea.mat";

    [MenuItem("LILO/Setup Office Floors 1 & 2")]
    public static void Run()
    {
        SetupOfficeGameplay.Run();
        ConfigureExitAreaMaterial();
        ConfigureFloor1();
        DuplicateFloor1ToFloor2();
        ConfigureFloor2();
        ConfigureBuildScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[OfficeFloorsSetup] OfficeLevel1 exit and OfficeLevel2 layout are ready.");
    }

    public static void Validate()
    {
        Scene scene = EditorSceneManager.OpenScene(Floor1Path, OpenSceneMode.Single);
        GameObject exit = GameObject.Find("ExitDoorPlaceholder");
        if (exit == null)
            throw new System.InvalidOperationException("OfficeLevel1 is missing ExitDoorPlaceholder.");

        var interaction = exit.GetComponent<ExitDoorInteraction>();
        var serialized = interaction != null ? new SerializedObject(interaction) : null;
        float radius = serialized?.FindProperty("interactRadius")?.floatValue ?? 0f;
        string nextScene = serialized?.FindProperty("nextSceneName")?.stringValue ?? "<missing>";
        bool advances = serialized?.FindProperty("advanceToNextFloor")?.boolValue ?? false;
        bool sceneEnabled = false;
        foreach (var buildScene in EditorBuildSettings.scenes)
            sceneEnabled |= buildScene.enabled && buildScene.path.EndsWith("OfficeLevel2.unity");

        GameObject player = GameObject.Find("PlayerCharacter");
        bool exitOnNavMesh = NavMesh.SamplePosition(exit.transform.position, out NavMeshHit exitHit, 2f, NavMesh.AllAreas);
        bool playerToExitReachable = false;
        if (player != null && exitOnNavMesh && NavMesh.SamplePosition(player.transform.position, out NavMeshHit playerHit, 2f, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            playerToExitReachable = NavMesh.CalculatePath(playerHit.position, exitHit.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete;
        }

        Debug.Log($"[OfficeFloorsSetup] Exit active={exit.activeSelf} position={exit.transform.position} radius={radius:0.00} "
            + $"nextScene={nextScene} advanceFloor={advances} OfficeLevel2InBuild={sceneEnabled} "
            + $"exitOnNavMesh={exitOnNavMesh} playerToExitReachable={playerToExitReachable}");
    }

    private static void ConfigureFloor1()
    {
        Scene scene = EditorSceneManager.OpenScene(Floor1Path, OpenSceneMode.Single);
        GameObject exit = GameObject.Find("ExitDoorPlaceholder");
        if (exit == null)
            throw new System.InvalidOperationException("OfficeLevel1 is missing ExitDoorPlaceholder.");

        ConfigureExitObject(exit, new Vector3(-9.4f, 0.05f, 0f), true);
        ConfigureManager(scene, FloorId.Floor51);
        EditorSceneManager.SaveScene(scene);
    }

    private static void DuplicateFloor1ToFloor2()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Floor2Path) == null)
        {
            if (!AssetDatabase.CopyAsset(Floor1Path, Floor2Path))
                throw new System.InvalidOperationException("Could not duplicate OfficeLevel1 as OfficeLevel2.");
            AssetDatabase.ImportAsset(Floor2Path, ImportAssetOptions.ForceUpdate);
        }
    }

    private static void ConfigureFloor2()
    {
        Scene scene = EditorSceneManager.OpenScene(Floor2Path, OpenSceneMode.Single);
        GameObject exit = GameObject.Find("ExitDoorPlaceholder");
        if (exit == null)
        {
            exit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exit.name = "ExitDoorPlaceholder";
            var obstacle = exit.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;
            var interaction = exit.AddComponent<ExitDoorInteraction>();
            var monster = GameObject.Find("MonsterPlaceholder")?.GetComponent<MonsterAIController>();
            var serialized = new SerializedObject(interaction);
            serialized.FindProperty("monster").objectReferenceValue = monster;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        ConfigureExitObject(exit, new Vector3(-9.4f, 0.05f, 0f), false);
        ConfigureManager(scene, FloorId.Floor50);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigureExitObject(GameObject exit, Vector3 position, bool active)
    {
        exit.SetActive(true);
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(3f, 0.08f, 3f);

        var renderer = exit.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ExitMaterialPath);

        var collider = exit.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = true;

        var obstacle = exit.GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle != null)
            obstacle.enabled = false;

        ConfigureExitHighlight(exit, active);

        var interaction = exit.GetComponent<ExitDoorInteraction>();
        if (interaction != null)
        {
            var serialized = new SerializedObject(interaction);
            serialized.FindProperty("interactRadius").floatValue = 2.5f;
            serialized.FindProperty("nextSceneName").stringValue = active ? "OfficeLevel2" : string.Empty;
            serialized.FindProperty("advanceToNextFloor").boolValue = active;
            serialized.FindProperty("nextFloor").enumValueIndex = (int)FloorId.Floor50;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        exit.SetActive(active);
    }

    private static void ConfigureExitHighlight(GameObject exit, bool active)
    {
        Transform highlightRoot = exit.transform.Find("ExitGreenHighlight");
        if (highlightRoot == null)
        {
            highlightRoot = new GameObject("ExitGreenHighlight").transform;
            highlightRoot.SetParent(exit.transform, false);

            CreateHighlightBar(highlightRoot, "North", new Vector3(0f, -0.5f, 0.517f), new Vector3(1.067f, 0.5f, 0.04f));
            CreateHighlightBar(highlightRoot, "South", new Vector3(0f, -0.5f, -0.517f), new Vector3(1.067f, 0.5f, 0.04f));
            CreateHighlightBar(highlightRoot, "East", new Vector3(0.517f, -0.5f, 0f), new Vector3(0.04f, 0.5f, 1.067f));
            CreateHighlightBar(highlightRoot, "West", new Vector3(-0.517f, -0.5f, 0f), new Vector3(0.04f, 0.5f, 1.067f));
        }

        highlightRoot.gameObject.SetActive(active);

        Transform glowTransform = exit.transform.Find("ExitGreenGlow");
        if (glowTransform == null)
        {
            glowTransform = new GameObject("ExitGreenGlow").transform;
            glowTransform.SetParent(exit.transform, false);
            glowTransform.localPosition = new Vector3(0f, 5f, 0f);
            var light = glowTransform.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.05f, 1f, 0.18f);
            light.intensity = 8f;
            light.range = 5f;
            light.shadows = LightShadows.None;
        }

        glowTransform.gameObject.SetActive(active);
    }

    private static void CreateHighlightBar(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = name;
        bar.transform.SetParent(parent, false);
        bar.transform.localPosition = localPosition;
        bar.transform.localScale = localScale;

        var collider = bar.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);

        var renderer = bar.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ExitMaterialPath);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void ConfigureManager(Scene scene, FloorId startingFloor)
    {
        GameObject managerGo = FindRootObject(scene, "GameManager");
        if (managerGo == null)
            throw new System.InvalidOperationException($"{scene.name} is missing GameManager.");

        var serialized = new SerializedObject(managerGo.GetComponent<GameManager>());
        serialized.FindProperty("useConfiguredStartingFloor").boolValue = true;
        serialized.FindProperty("configuredStartingFloor").enumValueIndex = (int)startingFloor;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindRootObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }

    private static void ConfigureExitAreaMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(ExitMaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            material = new Material(shader) { name = "OfficeExitArea" };
            AssetDatabase.CreateAsset(material, ExitMaterialPath);
        }

        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlitShader != null)
            material.shader = unlitShader;

        Color green = new Color(0.05f, 1f, 0.18f, 1f);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", green);
        if (material.HasProperty("_Color"))
            material.color = green;
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", green * 2f);
        }

        EditorUtility.SetDirty(material);
    }

    private static void ConfigureBuildScenes()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string guid = AssetDatabase.AssetPathToGUID(Floor2Path);
        bool found = false;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == Floor2Path)
            {
                scenes[i] = new EditorBuildSettingsScene(Floor2Path, true);
                found = true;
                break;
            }
        }

        if (!found)
            scenes.Insert(1, new EditorBuildSettingsScene(Floor2Path, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
