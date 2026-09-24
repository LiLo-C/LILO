using Lilo.Config;
using Lilo.MonoBehaviours;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Input
{
    /// <summary>
    /// Guarantees that touch controls exist when a playable scene is loaded.
    /// This covers device builds where an editor-only scene setup step was not run.
    /// </summary>
    public static class MobileControlsBootstrap
    {
        private const string CanvasName = "MobileControlsCanvas";
        private const string JoystickName = "JoystickBackground";

        /// <summary>
        /// Stops the current scene's touch pipeline before unloading it. iOS can
        /// deliver a final touch event after the scene objects have been destroyed,
        /// which otherwise produces "Touch was already deallocated" on respawn.
        /// </summary>
        public static void PrepareForSceneReload()
        {
            foreach (JoystickInputAdapter joystick in Object.FindObjectsByType<JoystickInputAdapter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                joystick.CancelTouch();

            foreach (GraphicRaycaster raycaster in Object.FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (raycaster.transform.root.name == CanvasName)
                    raycaster.enabled = false;
            }

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                var inputModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                if (inputModule != null)
                    inputModule.enabled = false;
                eventSystem.enabled = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallOnStartup()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureControls();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureControls();
        }

        private static void EnsureControls()
        {
            if (!Application.isEditor && !Application.isMobilePlatform)
                return;

            GameObject player = GameObject.Find("PlayerCharacter");
            if (player == null || player.GetComponent<StarterAssets.StarterAssetsInputs>() == null)
                return;

            GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            GameObject canvasObject = GameObject.Find(CanvasName);
            if (canvasObject == null)
                canvasObject = CreateCanvas();

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.enabled = true;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            Canvas.ForceUpdateCanvases();

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
                canvasObject.AddComponent<GraphicRaycaster>();

            JoystickInputAdapter joystick = canvasObject.GetComponentInChildren<JoystickInputAdapter>(true);
            if (joystick == null)
                joystick = CreateJoystick(canvasObject.transform, config);
            else
                joystick.Initialize(config, joystick.transform as RectTransform, joystick.transform.Find("JoystickHandle") as RectTransform);

            EnsureBatterySwapButton(canvasObject.transform);
            Lilo.MonoBehaviours.Hiding.OfficeHidingBootstrap.Ensure(player, canvasObject.transform);
            EnsureEventSystem();
            EnsureBridge(canvasObject.transform, joystick);
        }

        private static void EnsureBatterySwapButton(Transform canvas)
        {
            if (canvas.Find("BatterySwapButton") != null)
                return;

            GameObject buttonObject = new GameObject(
                "BatterySwapButton",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(CanvasGroup));
            buttonObject.transform.SetParent(canvas, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0.35f);
            rect.anchorMax = new Vector2(1f, 0.35f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-36f, 0f);
            rect.sizeDelta = new Vector2(150f, 64f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.32f, 0.5f, 0.96f);
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.text = "CHANGE BATTERY";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Canvas rootCanvas = canvas.GetComponentInParent<Canvas>();
            float canvasScale = Mathf.Max(0.01f, rootCanvas != null ? rootCanvas.scaleFactor : 1f);
            bool tablet = Mathf.Min(Screen.width, Screen.height) >= 1400;
            float tabletScale = tablet ? 0.82f : 1f;
            rect.sizeDelta = new Vector2(132f, 52f) * tabletScale / canvasScale;
            rect.anchoredPosition = new Vector2(-22f / canvasScale, 0f);
            label.fontSize = Mathf.RoundToInt(14f * tabletScale / canvasScale);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            buttonObject.AddComponent<Lilo.MonoBehaviours.UI.BatterySwapButton>();
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasObject = new GameObject(
                CanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject;
        }

        private static JoystickInputAdapter CreateJoystick(Transform canvas, GameConfig config)
        {
            GameObject root = new GameObject(JoystickName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(canvas, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.localScale = Vector3.one;

            GameObject handle = new GameObject("JoystickHandle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            handle.transform.SetParent(root.transform, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;

            Image backgroundImage = root.GetComponent<Image>();
            backgroundImage.color = new Color(1f, 1f, 1f, 0.2f);
            backgroundImage.raycastTarget = true;
            Image handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.35f);
            handleImage.raycastTarget = false;

            JoystickInputAdapter joystick = root.AddComponent<JoystickInputAdapter>();
            joystick.Initialize(config, rootRect, handleRect);
            return joystick;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = eventObject.GetComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static void EnsureBridge(Transform canvas, JoystickInputAdapter joystick)
        {
            MobileStarterAssetsBridge bridge = Object.FindFirstObjectByType<MobileStarterAssetsBridge>();
            if (bridge == null)
            {
                GameObject bridgeObject = new GameObject("MobileInputBridge");
                bridgeObject.transform.SetParent(canvas, false);
                bridge = bridgeObject.AddComponent<MobileStarterAssetsBridge>();
            }

            if (bridge.GetComponent<MobileControlsBootstrapMarker>() == null)
                bridge.gameObject.AddComponent<MobileControlsBootstrapMarker>();
        }

        private sealed class MobileControlsBootstrapMarker : MonoBehaviour { }
    }
}
