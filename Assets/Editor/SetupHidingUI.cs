using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.Editor
{
    public static class SetupHidingUI
    {
        [MenuItem("LILO/Setup Hiding UI")]
        public static void Run()
        {
            var playerGo = GameObject.Find("PlayerCharacter");
            if (playerGo == null)
            {
                Debug.LogError("[HidingUI] PlayerCharacter not found.");
                return;
            }

            var hc = playerGo.GetComponent<Lilo.MonoBehaviours.Hiding.HidingController>();
            if (hc == null)
            {
                hc = playerGo.AddComponent<Lilo.MonoBehaviours.Hiding.HidingController>();
                Debug.Log("[HidingUI] Added HidingController to PlayerCharacter.");
            }

            var desk = GameObject.Find("Desk");
            if (desk == null)
            {
                Debug.LogError("[HidingUI] Desk not found.");
                return;
            }

            var spot = desk.GetComponent<Lilo.MonoBehaviours.Hiding.HidingSpot>();
            if (spot == null)
            {
                spot = desk.AddComponent<Lilo.MonoBehaviours.Hiding.HidingSpot>();
                Debug.Log("[HidingUI] Added HidingSpot to Desk.");
            }

            var hideAnchor = desk.transform.Find("HideAnchor");
            if (hideAnchor == null)
            {
                var go = new GameObject("HideAnchor");
                go.transform.SetParent(desk.transform, false);
                go.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                hideAnchor = go.transform;
            }

            var exitAnchor = desk.transform.Find("ExitAnchor");
            if (exitAnchor == null)
            {
                var go = new GameObject("ExitAnchor");
                go.transform.SetParent(desk.transform, false);
                go.transform.localPosition = new Vector3(1.5f, 0f, 0f);
                exitAnchor = go.transform;
            }

            var so = new SerializedObject(spot);
            so.FindProperty("hideAnchor").objectReferenceValue = hideAnchor;
            so.FindProperty("exitAnchor").objectReferenceValue = exitAnchor;
            so.FindProperty("interactionRadius").floatValue = 2.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var canvasGo = GameObject.Find("HUDCanvas");
            if (canvasGo == null)
            {
                Debug.LogError("[HidingUI] HUDCanvas not found.");
                return;
            }

            if (canvasGo.transform.Find("ContextActionButton") == null)
            {
                var buttonGo = new GameObject("ContextActionButton");
                buttonGo.transform.SetParent(canvasGo.transform, false);

                var rt = buttonGo.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.anchoredPosition = new Vector2(220f, 40f);
                rt.sizeDelta = new Vector2(120f, 50f);

                var cg = buttonGo.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;

                var img = buttonGo.AddComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.5f);

                var btn = buttonGo.AddComponent<Button>();
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
                var text = textGo.AddComponent<Text>();
                text.text = "Hide";
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 20;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;

                var ctx = buttonGo.AddComponent<Lilo.MonoBehaviours.Hud.ContextActionButton>();
                var ctxSo = new SerializedObject(ctx);
                ctxSo.FindProperty("hidingController").objectReferenceValue = hc;
                ctxSo.FindProperty("actionButton").objectReferenceValue = btn;
                ctxSo.FindProperty("label").objectReferenceValue = text;
                ctxSo.FindProperty("canvasGroup").objectReferenceValue = cg;
                ctxSo.ApplyModifiedPropertiesWithoutUndo();

                btn.onClick.AddListener(ctx.OnButtonPressed);
                Debug.Log("[HidingUI] Created ContextActionButton on HUDCanvas.");
            }

            EditorSceneManager.MarkSceneDirty(playerGo.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[HidingUI] Done. Hiding system fully wired.");
        }
    }
}
