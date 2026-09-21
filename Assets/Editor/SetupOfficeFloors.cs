using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Interaction;

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
            throw new System.InvalidOperationException("OfficeLevel2 is missing ExitDoorPlaceholder.");

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

        var interaction = exit.GetComponent<ExitDoorInteraction>();
        if (interaction != null)
        {
            var serialized = new SerializedObject(interaction);
            serialized.FindProperty("interactRadius").floatValue = 1.8f;
            serialized.FindProperty("nextSceneName").stringValue = active ? "OfficeLevel2" : string.Empty;
            serialized.FindProperty("advanceToNextFloor").boolValue = active;
            serialized.FindProperty("nextFloor").enumValueIndex = (int)FloorId.Floor50;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        exit.SetActive(active);
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
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = "OfficeExitArea" };
            AssetDatabase.CreateAsset(material, ExitMaterialPath);
        }

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
