using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lilo.Editor
{
    /// <summary>Applies reversible iOS import/build settings for the compact TestFlight build.</summary>
    public static class IOSBuildSizeOptimizer
    {
        private static readonly string[] ModelRoots =
        {
            "Assets/Character/Eddie",
            "Assets/Character/Ghost",
            "Assets/Starter Assets/Runtime/ThirdPersonController/Character/Models",
        };

        private static readonly string[] TextureRoots =
        {
            "Assets/Textures",
            "Assets/Character",
            "Assets/Starter Assets/Runtime/ThirdPersonController/Character/Textures",
        };

        [MenuItem("LILO/Optimize iOS Build Size")]
        public static void Apply()
        {
            RemoveDevelopmentScenesFromBuild();

            var changedImports = new List<string>();
            AssetDatabase.StartAssetEditing();
            try
            {
                ApplyMeshCompression(changedImports);
                ApplyTextureCompression(changedImports);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            foreach (string path in changedImports.Distinct())
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            AssetDatabase.SaveAssets();
            Debug.Log($"[IOSBuildSizeOptimizer] Applied settings and reimported {changedImports.Distinct().Count()} assets.");
        }

        private static void RemoveDevelopmentScenesFromBuild()
        {
            string[] excluded =
            {
                "Assets/Scenes/SampleScene.unity",
                "Assets/Scenes/DevTest_Movement.unity",
            };

            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(scene => !excluded.Contains(scene.path))
                .ToArray();
        }

        private static void ApplyMeshCompression(List<string> changedImports)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Model", ModelRoots))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                    continue;

                bool changed = false;
                if (importer.meshCompression != ModelImporterMeshCompression.High)
                {
                    importer.meshCompression = ModelImporterMeshCompression.High;
                    changed = true;
                }
                if (importer.importCameras || importer.importLights)
                {
                    importer.importCameras = false;
                    importer.importLights = false;
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(importer);
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    changedImports.Add(path);
                }
            }
        }

        private static void ApplyTextureCompression(List<string> changedImports)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", TextureRoots))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                TextureImporterPlatformSettings ios = importer.GetPlatformTextureSettings("iPhone");
                bool changed = false;
                if (!ios.overridden)
                {
                    ios.name = "iPhone";
                    ios.overridden = true;
                    changed = true;
                }

                int maximum = Mathf.Min(ios.maxTextureSize, 1024);
                if (ios.maxTextureSize != maximum)
                {
                    ios.maxTextureSize = maximum;
                    changed = true;
                }
                if (ios.format != TextureImporterFormat.ASTC_8x8)
                {
                    ios.format = TextureImporterFormat.ASTC_8x8;
                    changed = true;
                }
                if (ios.textureCompression != TextureImporterCompression.Compressed)
                {
                    ios.textureCompression = TextureImporterCompression.Compressed;
                    changed = true;
                }
                if (ios.crunchedCompression)
                {
                    ios.crunchedCompression = false;
                    changed = true;
                }
                if (ios.compressionQuality != 50)
                {
                    ios.compressionQuality = 50;
                    changed = true;
                }

                if (changed)
                {
                    importer.SetPlatformTextureSettings(ios);
                    EditorUtility.SetDirty(importer);
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    changedImports.Add(path);
                }
            }
        }
    }
}
