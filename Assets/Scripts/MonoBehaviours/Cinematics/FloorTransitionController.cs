using System.Collections;
using System.Collections.Generic;
using Lilo.Config;
using Lilo.MonoBehaviours.Battery;
using Lilo.MonoBehaviours.Camera;
using Lilo.MonoBehaviours.Flashlight;
using Lilo.MonoBehaviours.Input;
using Lilo.MonoBehaviours.Interaction;
using Lilo.MonoBehaviours.Monster;
using Lilo.MonoBehaviours.Player;
using Lilo.UI;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Cinematics
{
    /// <summary>Fades between floors, then pulls back from Eddie before resuming gameplay.</summary>
    [DefaultExecutionOrder(10000)]
    public sealed class FloorTransitionController : MonoBehaviour
    {
        private static FloorTransitionController _instance;
        private readonly List<Behaviour> _suspended = new List<Behaviour>();
        private Canvas _canvas;
        private Image _black;
        private Text _floorLabel;
        private UnityEngine.Camera _camera;
        private float _normalSize;
        private float _normalFov;
        private float _previousTimeScale;
        private bool _finished;

        public static bool IsTransitioning => _instance != null;

        public static bool Begin(string sceneName, FloorId floor)
        {
            if (IsTransitioning || !Application.CanStreamedLevelBeLoaded(sceneName))
                return false;

            var root = new GameObject("Floor Transition", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var transition = root.AddComponent<FloorTransitionController>();
            transition.StartCoroutine(transition.Play(sceneName, floor));
            return true;
        }

        private void Awake()
        {
            _instance = this;
            _previousTimeScale = Time.timeScale;
            DontDestroyOnLoad(gameObject);
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 32700;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var blackObject = new GameObject("Fade", typeof(RectTransform), typeof(Image));
            blackObject.transform.SetParent(transform, false);
            _black = blackObject.GetComponent<Image>();
            _black.rectTransform.anchorMin = Vector2.zero;
            _black.rectTransform.anchorMax = Vector2.one;
            _black.rectTransform.offsetMin = Vector2.zero;
            _black.rectTransform.offsetMax = Vector2.zero;
            _black.color = Color.clear;
            _black.raycastTarget = true;

            var labelObject = new GameObject("Floor Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            _floorLabel = labelObject.GetComponent<Text>();
            _floorLabel.font = GameUIFont.Get();
            _floorLabel.fontSize = 38;
            _floorLabel.alignment = TextAnchor.MiddleCenter;
            _floorLabel.raycastTarget = false;
            _floorLabel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _floorLabel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _floorLabel.rectTransform.sizeDelta = new Vector2(800f, 100f);
            _floorLabel.color = Color.clear;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private IEnumerator Play(string sceneName, FloorId floor)
        {
            int number = floor == FloorId.Floor52 ? 52 : floor == FloorId.Floor51 ? 51 : 50;
            _floorLabel.text = $"FLOOR {number}";
            SuspendGameplay();
            CaptureCamera();

            float elapsed = 0f;
            const float fadeOutSeconds = 0.65f;
            while (elapsed < fadeOutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeOutSeconds));
                SetFade(t);
                SetCameraFraming(Mathf.Lerp(1f, 0.9f, t));
                yield return null;
            }
            SetFade(1f);
            RestoreCamera();

            var state = GameManager.Instance?.State;
            FloorId previousFloor = state != null ? state.CurrentFloor : floor;
            state?.AdvanceToFloor(floor);
            MobileControlsBootstrap.PrepareForSceneReload();
            AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loading == null)
            {
                state?.AdvanceToFloor(previousFloor);
                Debug.LogError($"[FloorTransition] Could not load '{sceneName}'.", this);
                Finish();
                yield break;
            }
            yield return loading;

            SuspendGameplay();
            GameObject.Find("PlayerCharacter")?.GetComponent<LightingRig>()?.InitializeForCinematic();
            CaptureCamera();
            SetCameraFraming(0.65f);
            yield return new WaitForSecondsRealtime(0.35f);

            elapsed = 0f;
            const float revealSeconds = 1.35f;
            while (elapsed < revealSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / revealSeconds));
                SetFade(1f - t);
                SetCameraFraming(Mathf.Lerp(0.65f, 1f, t));
                yield return null;
            }
            Finish();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SuspendGameplay();

        private void LateUpdate()
        {
            if (!_finished)
                SuspendGameplay();
        }

        private void SuspendGameplay()
        {
            Time.timeScale = 0f;
            foreach (MonoBehaviour component in FindObjectsByType<MonoBehaviour>())
            {
                if (component is CameraFollow || component is CameraFollowController
                    || component is ThirdPersonController || component is PlayerMovementController
                    || component is MobileStarterAssetsBridge || component is MonsterAIController
                    || component is KeyboardBatteryDirector || component is ExitDoorInteraction
                    || component is LightingRig || component is AccessKeyPickup || component is BatteryPickup
                    || component is GameplayGuidanceHud || component is GameplayHudController
                    || component is ChaseScreenEffects)
                    Suspend(component);
            }
            foreach (Canvas canvas in FindObjectsByType<Canvas>())
                if (canvas != _canvas) Suspend(canvas);
        }

        private void Suspend(Behaviour component)
        {
            if (component == null || !component.enabled) return;
            if (!_suspended.Contains(component)) _suspended.Add(component);
            component.enabled = false;
        }

        private void CaptureCamera()
        {
            _camera = UnityEngine.Camera.main;
            if (_camera == null) return;
            _normalSize = _camera.orthographicSize;
            _normalFov = _camera.fieldOfView;
        }

        private void SetCameraFraming(float scale)
        {
            if (_camera == null) return;
            if (_camera.orthographic) _camera.orthographicSize = _normalSize * scale;
            else _camera.fieldOfView = _normalFov * scale;
        }

        private void RestoreCamera()
        {
            if (_camera == null) return;
            _camera.orthographicSize = _normalSize;
            _camera.fieldOfView = _normalFov;
        }

        private void SetFade(float alpha)
        {
            _black.color = new Color(0f, 0f, 0f, alpha);
            _floorLabel.color = new Color(0.88f, 0.89f, 0.83f, alpha);
        }

        private void Finish()
        {
            _finished = true;
            RestoreCamera();
            RestoreGameplay();
            Destroy(gameObject);
        }

        private void RestoreGameplay()
        {
            foreach (Behaviour component in _suspended)
                if (component != null) component.enabled = true;
            _suspended.Clear();
            Time.timeScale = _previousTimeScale;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (!_finished)
            {
                RestoreCamera();
                RestoreGameplay();
            }
            if (_instance == this) _instance = null;
        }
    }
}
