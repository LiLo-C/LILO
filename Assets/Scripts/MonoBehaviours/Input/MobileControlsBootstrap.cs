using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Interaction;
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
            foreach (JoystickInputAdapter joystick in Object.FindObjectsByType<JoystickInputAdapter>(FindObjectsInactive.Include))
                joystick.CancelTouch();

            foreach (GraphicRaycaster raycaster in Object.FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include))
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
            GameObject player = GameObject.Find("PlayerCharacter");
            if (player == null)
                return;

            if (player.GetComponent<PlayerXRayOutline>() == null)
                player.AddComponent<PlayerXRayOutline>();

            AccessKeyBootstrap.Ensure();

            GameObject canvasObject = GameObject.Find(CanvasName);
            if (canvasObject == null)
                canvasObject = CreateCanvas();
            if (canvasObject.GetComponent<GameplayGuidanceHud>() == null)
                canvasObject.AddComponent<GameplayGuidanceHud>();

            if (!Application.isEditor && !Application.isMobilePlatform)
                return;
            if (player.GetComponent<StarterAssets.StarterAssetsInputs>() == null)
                return;

            GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
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

            Transform oldBatteryButton = canvasObject.transform.Find("BatterySwapButton");
            if (oldBatteryButton != null)
                Object.Destroy(oldBatteryButton.gameObject);
            Lilo.MonoBehaviours.Hiding.OfficeHidingBootstrap.Ensure(player, canvasObject.transform);
            EnsureEventSystem();
            EnsureBridge(canvasObject.transform, joystick);
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
            MobileStarterAssetsBridge bridge = Object.FindAnyObjectByType<MobileStarterAssetsBridge>();
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
