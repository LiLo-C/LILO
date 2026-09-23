using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lilo.MonoBehaviours.Battery;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot setup: turns every "keyboard" prop in the active scene into a
    /// battery dispenser (KeyboardBatterySlot + BatteryVisual child) and ensures a
    /// single KeyboardBatteryDirector exists. Run via menu LILO/Setup Keyboard
    /// Batteries. Safe to re-run (idempotent).
    /// </summary>
    public static class SetupKeyboardBatteries
    {
        private const string BatteryMatPath = "Assets/Materials/MonsterArenaBattery.mat";

        [MenuItem("LILO/Setup Keyboard Batteries")]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            int slots = 0;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!t.gameObject.name.Equals("keyboard", System.StringComparison.OrdinalIgnoreCase))
                        continue;

                    var slot = t.gameObject.GetComponent<KeyboardBatterySlot>();
                    if (slot == null)
                    {
                        slot = t.gameObject.AddComponent<KeyboardBatterySlot>();
                        EditorUtility.SetDirty(t.gameObject);
                    }
                    EnsureBatteryVisual(t.gameObject);
                    slots++;
                }
            }

            if (slots == 0)
            {
                Debug.LogWarning("[BatteryKeyboards] No 'keyboard' props found in active scene.");
                return;
            }

            var director = Object.FindFirstObjectByType<KeyboardBatteryDirector>();
            if (director == null)
            {
                var go = new GameObject("KeyboardBatteryDirector");
                go.AddComponent<KeyboardBatteryDirector>();
                EditorUtility.SetDirty(go);
                Debug.Log("[BatteryKeyboards] Created KeyboardBatteryDirector.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[BatteryKeyboards] Done. slots={slots} scene={scene.name}");
        }

        private static void EnsureBatteryVisual(GameObject keyboard)
        {
            var existing = keyboard.transform.Find("BatteryVisual");
            if (existing != null) return;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "BatteryVisual";
            visual.transform.SetParent(keyboard.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            visual.transform.localScale = new Vector3(0.28f, 0.45f, 0.16f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var mat = AssetDatabase.LoadAssetAtPath<Material>(BatteryMatPath);
            if (mat != null)
                visual.GetComponent<Renderer>().sharedMaterial = mat;

            EditorUtility.SetDirty(keyboard);
        }
    }
}
