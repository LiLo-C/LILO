using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-off setup tasks run via `Unity -batchmode -quit -executeMethod`.
/// Not part of the shipped game — lives under Assets/Editor so it's editor-only.
/// See specs/001-core-prototype and the constitution's Technology Stack section.
/// </summary>
public static class LiloBatchSetup
{
    public static void RunSetup()
    {
        SetIOSDeploymentTarget();
        CreatePlaceholderPrimitives();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LiloBatchSetup.RunSetup: done.");
    }

    public static void SetIOSDeploymentTarget()
    {
        const string target = "26.0";
        PlayerSettings.iOS.targetOSVersionString = target;
        Debug.Log($"LiloBatchSetup: PlayerSettings.iOS.targetOSVersionString set to {target}");
    }

    public static void CreatePlaceholderPrimitives()
    {
        const string materialsDir = "Assets/Materials";
        const string prefabsDir = "Assets/Prefabs";
        EnsureFolder(materialsDir);
        EnsureFolder(prefabsDir);

        CreatePrimitivePrefab(
            PrimitiveType.Cube,
            "PlaceholderBox",
            new Vector3(1f, 1f, 1f),
            new Color(0.25f, 0.25f, 0.25f),
            materialsDir,
            prefabsDir);

        CreatePrimitivePrefab(
            PrimitiveType.Sphere,
            "PlaceholderBall",
            new Vector3(1f, 1f, 1f),
            new Color(0.3f, 0.3f, 0.3f),
            materialsDir,
            prefabsDir);
    }

    private static void CreatePrimitivePrefab(
        PrimitiveType type,
        string name,
        Vector3 localScale,
        Color color,
        string materialsDir,
        string prefabsDir)
    {
        string prefabPath = $"{prefabsDir}/{name}.prefab";

        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = localScale;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = $"{name}Material" };
        material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        string materialPath = $"{materialsDir}/{name}Material.mat";
        AssetDatabase.CreateAsset(material, materialPath);

        go.GetComponent<Renderer>().sharedMaterial = material;

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log($"LiloBatchSetup: created {prefabPath} ({materialPath})");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
