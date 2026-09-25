using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ApplyAmericanTypewriterAndFloorFlag
{
    private const string FontPath = "Assets/Resources/American Typewriter Regular.ttf";
    private const string FontAssetPath = "Assets/Fonts/American Typewriter SDF.asset";
    private const string FlagPath = "Assets/Resources/FloorFlag.png";

    [MenuItem("LILO/Apply American Typewriter Font and Floor Flag")]
    public static void Apply()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
            throw new System.InvalidOperationException($"Could not load font at {FontPath}.");

        ConfigureFlagImport();
        AssetDatabase.ImportAsset(FlagPath, ImportAssetOptions.ForceUpdate);

        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (tmpFont == null)
        {
            tmpFont = TMP_FontAsset.CreateFontAsset(font);
            tmpFont.name = "American Typewriter SDF";
            tmpFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            tmpFont.isMultiAtlasTexturesEnabled = true;
            AssetDatabase.CreateAsset(tmpFont, FontAssetPath);
            AssetDatabase.SaveAssets();
        }

        ApplyToPrefabs(font, tmpFont);
        ApplyToScenes(font, tmpFont);
        SetTmpDefaultFont(tmpFont);
        AssetDatabase.SaveAssets();
        Debug.Log("[LILO] American Typewriter applied to game text; FloorFlag is available in the gameplay HUD.");
    }

    private static void ConfigureFlagImport()
    {
        var importer = AssetImporter.GetAtPath(FlagPath) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static void ApplyToPrefabs(Font font, TMP_FontAsset tmpFont)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/TextMesh Pro/Examples & Extras/")) continue;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = ApplyToHierarchy(root, font, tmpFont);
            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ApplyToScenes(Font font, TMP_FontAsset tmpFont)
    {
        Scene originalActive = SceneManager.GetActiveScene();
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool wasLoaded = scene.IsValid() && scene.isLoaded;
            if (!wasLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            bool changed = false;
            foreach (GameObject root in scene.GetRootGameObjects())
                changed |= ApplyToHierarchy(root, font, tmpFont);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
        }
        if (originalActive.IsValid() && originalActive.isLoaded)
            SceneManager.SetActiveScene(originalActive);
    }

    private static bool ApplyToHierarchy(GameObject root, Font font, TMP_FontAsset tmpFont)
    {
        bool changed = false;
        foreach (Text text in root.GetComponentsInChildren<Text>(true))
        {
            if (text.font == font) continue;
            text.font = font;
            EditorUtility.SetDirty(text);
            changed = true;
        }
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font == tmpFont) continue;
            text.font = tmpFont;
            EditorUtility.SetDirty(text);
            changed = true;
        }
        return changed;
    }

    private static void SetTmpDefaultFont(TMP_FontAsset tmpFont)
    {
        TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        if (settings == null) return;
        SerializedObject serialized = new SerializedObject(settings);
        SerializedProperty property = serialized.FindProperty("m_defaultFontAsset");
        if (property == null || property.objectReferenceValue == tmpFont) return;
        property.objectReferenceValue = tmpFont;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
    }
}
