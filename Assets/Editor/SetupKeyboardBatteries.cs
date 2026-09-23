using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Lilo.MonoBehaviours.Battery;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot setup: turns every "keyboard" prop in the active scene into a
    /// battery dispenser (KeyboardBatterySlot) and ensures a single
    /// KeyboardBatteryDirector exists. The glow material is built at runtime by
    /// the director, so setup only wires components. Run via menu
    /// LILO/Setup Keyboard Batteries. Safe to re-run (idempotent).
    /// </summary>
    public static class SetupKeyboardBatteries
    {
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

            EnsureActionButton();
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[BatteryKeyboards] Done. slots={slots} scene={scene.name} (glow built at runtime)");
        }

        /// <summary>
        /// Ensures the Take/Hide action button exists (OfficeLevel1 has no HUD
        /// button yet). Parents under HUDCanvas, else MobileControlsCanvas.
        /// Hiding stays optional — battery Take works without HidingController.
        /// </summary>
        private static void EnsureActionButton()
        {
            var canvasGo = GameObject.Find("HUDCanvas");
            if (canvasGo == null)
                canvasGo = GameObject.Find("MobileControlsCanvas");
            if (canvasGo == null)
            {
                Debug.LogWarning("[BatteryKeyboards] No canvas found for action button.");
                return;
            }

            if (canvasGo.transform.Find("ContextActionButton") != null) return;

            var buttonGo = new GameObject("ContextActionButton");
            buttonGo.transform.SetParent(canvasGo.transform, false);

            var rt = buttonGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-140f, 120f);
            rt.sizeDelta = new Vector2(160f, 60f);

            var cg = buttonGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            var img = buttonGo.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0f, 0f, 0f, 0.5f);

            var btn = buttonGo.AddComponent<UnityEngine.UI.Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.8f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            var text = textGo.AddComponent<UnityEngine.UI.Text>();
            text.text = "Take";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            var ctx = buttonGo.AddComponent<Lilo.MonoBehaviours.Hud.ContextActionButton>();
            var ctxSo = new SerializedObject(ctx);
            ctxSo.FindProperty("actionButton").objectReferenceValue = btn;
            ctxSo.FindProperty("label").objectReferenceValue = text;
            ctxSo.FindProperty("canvasGroup").objectReferenceValue = cg;
            ctxSo.ApplyModifiedPropertiesWithoutUndo();

            btn.onClick.AddListener(ctx.OnButtonPressed);
            EditorUtility.SetDirty(buttonGo);
            Debug.Log("[BatteryKeyboards] Created ContextActionButton.");
        }
    }
}
