using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lilo.MonoBehaviours.Battery;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot setup: turns every "keyboard" prop in the active scene into a
    /// battery dispenser (KeyboardBatterySlot with base/loaded materials) and
    /// ensures a single KeyboardBatteryDirector exists. Loaded keyboards glow via
    /// an emissive material. Run via menu LILO/Setup Keyboard Batteries.
    /// Safe to re-run (idempotent).
    /// </summary>
    public static class SetupKeyboardBatteries
    {
        private const string LoadedMatPath = "Assets/Resources/KeyboardBatteryLoaded.mat";

        [MenuItem("LILO/Setup Keyboard Batteries")]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Material loadedMat = EnsureLoadedMaterial();
            if (loadedMat == null) return;

            int slots = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!t.gameObject.name.Equals("keyboard", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Legacy cube cell from the first pass — the keyboard itself glows now.
                    var legacy = t.Find("BatteryVisual");
                    if (legacy != null)
                        Object.DestroyImmediate(legacy.gameObject);

                    var slot = t.gameObject.GetComponent<KeyboardBatterySlot>();
                    if (slot == null)
                        slot = t.gameObject.AddComponent<KeyboardBatterySlot>();

                    // Materials are self-resolved at runtime (base from the
                    // renderer, loaded glow from Resources); nothing to assign.
                    EditorUtility.SetDirty(t.gameObject);
                    slots++;
                }
            }

            if (slots == 0)
            {
                Debug.LogWarning("[BatteryKeyboards] No 'keyboard' props found in active scene.");
                return;
            }

            var director = Object.FindAnyObjectByType<KeyboardBatteryDirector>();
            if (director == null)
            {
                var go = new GameObject("KeyboardBatteryDirector");
                go.AddComponent<KeyboardBatteryDirector>();
                EditorUtility.SetDirty(go);
                Debug.Log("[BatteryKeyboards] Created KeyboardBatteryDirector.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[BatteryKeyboards] Done. slots={slots} scene={scene.name} glow={LoadedMatPath}");
        }

        private static Material EnsureLoadedMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(LoadedMatPath);
            if (existing != null) return existing;

            var urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogError("[BatteryKeyboards] URP/Lit shader not found.");
                return null;
            }

            var mat = new Material(urpShader) { name = "KeyboardBatteryLoaded" };
            mat.color = new Color(0.12f, 0.12f, 0.14f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.25f, 1f, 0.45f) * 1.6f);
            AssetDatabase.CreateAsset(mat, LoadedMatPath);
            Debug.Log($"[BatteryKeyboards] Created {LoadedMatPath}.");
            return mat;
        }
    }
}
