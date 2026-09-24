using Lilo.MonoBehaviours.Battery;
using Lilo.MonoBehaviours.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Hiding
{
    /// <summary>Wires under-desk hiding to the office keyboard cubicles on scene load.</summary>
    public static class OfficeHidingBootstrap
    {
        public static void Ensure(GameObject player, Transform controlsCanvas)
        {
            if (player.GetComponent<HidingController>() == null)
                player.AddComponent<HidingController>();
            if (player.GetComponent<ScratchMarkTrail>() == null)
                player.AddComponent<ScratchMarkTrail>();

            int spots = 0;
            foreach (KeyboardBatterySlot slot in Object.FindObjectsByType<KeyboardBatterySlot>(
                         FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                Transform cubicle = slot.transform.parent;
                if (cubicle == null || !cubicle.name.StartsWith("Cubicle-", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                HidingSpot spot = cubicle.GetComponent<HidingSpot>();
                if (spot == null)
                    spot = cubicle.gameObject.AddComponent<HidingSpot>();

                Renderer desk = FindDesk(cubicle);
                Vector3 center = desk != null ? desk.bounds.center : slot.transform.position;
                float floorY = player.transform.position.y;

                Transform hide = EnsureAnchor(cubicle, "HideAnchor");
                hide.position = new Vector3(center.x, floorY, center.z);
                Transform exit = EnsureAnchor(cubicle, "ExitAnchor");
                exit.position = new Vector3(center.x, floorY,
                    desk != null ? desk.bounds.min.z - 0.7f : center.z - 1.2f);
                spot.Configure(slot, hide, exit, 1.8f);
                spots++;
            }

            if (spots > 0)
                EnsureButton(controlsCanvas);
        }

        private static Renderer FindDesk(Transform cubicle)
        {
            Renderer best = null;
            float largestArea = 0f;
            foreach (Transform child in cubicle)
            {
                if (!child.name.Equals("desk", System.StringComparison.OrdinalIgnoreCase)
                    && !child.name.StartsWith("cubicle-", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer == null)
                    continue;
                float area = renderer.bounds.size.x * renderer.bounds.size.z;
                if (area <= largestArea)
                    continue;
                largestArea = area;
                best = renderer;
            }
            return best;
        }

        private static Transform EnsureAnchor(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing;
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            return anchor;
        }

        private static void EnsureButton(Transform canvas)
        {
            if (canvas.Find("ContextActionButton") != null)
                return;

            var root = new GameObject("ContextActionButton", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup));
            root.transform.SetParent(canvas, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.23f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-36f, 0f);
            rect.sizeDelta = new Vector2(150f, 64f);

            Image image = root.GetComponent<Image>();
            image.color = new Color(0.12f, 0.25f, 0.31f, 0.96f);
            root.GetComponent<Button>().targetGraphic = image;

            var labelObject = new GameObject("Label", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(root.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            Text label = labelObject.GetComponent<Text>();
            label.text = "HIDE";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 17;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            root.AddComponent<ContextActionButton>();
        }
    }
}
