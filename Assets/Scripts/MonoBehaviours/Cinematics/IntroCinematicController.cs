using System.Collections;
using System.Collections.Generic;
using Lilo.MonoBehaviours.Battery;
using Lilo.MonoBehaviours.Camera;
using Lilo.MonoBehaviours.Flashlight;
using Lilo.MonoBehaviours.Input;
using Lilo.MonoBehaviours.Interaction;
using Lilo.MonoBehaviours.Monster;
using Lilo.MonoBehaviours.Player;
using Lilo.Systems.GameLoop;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.Cinematics
{
    /// <summary>
    /// Plays an in-engine opening shot on the actual first floor, then hands that
    /// loaded scene back to gameplay without a camera or environment cut.
    /// </summary>
    public sealed class IntroCinematicController : MonoBehaviour
    {
        [SerializeField] private string firstFloorScene = "OfficeLevel1";
        [SerializeField, Min(0.1f)] private float closeOrthographicSize = 1.25f;
        [SerializeField, Min(1f)] private float closeCameraDistance = 4f;
        [SerializeField, Min(0f)] private float darkHoldSeconds = 0.4f;
        [SerializeField, Min(0.1f)] private float pullBackSeconds = 1.8f;
        [SerializeField] private AudioClip lightFlickerClip;

        private readonly List<Behaviour> _suspendedBehaviours = new List<Behaviour>();
        private readonly List<GameObject> _hiddenCanvasObjects = new List<GameObject>();
        private Scene _floorScene;
        private Transform _player;
        private UnityEngine.Camera _camera;
        private Light _lamp;
        private LightingRig _lightingRig;
        private Vector3 _normalCameraPosition;
        private Quaternion _normalCameraRotation;
        private float _normalOrthographicSize;
        private float _normalFieldOfView;
        private float _normalLampIntensity;
        private float _introLampIntensity;
        private bool _normalLampEnabled;
        private bool _cameraCaptured;
        private bool _finished;
        private UnityEngine.Camera _loadingCamera;
        private AudioListener _loadingListener;
        private AudioSource _lampSfxSource;
        private bool _lampFlickerSfxPlayed;

        private void Awake()
        {
            _loadingCamera = gameObject.AddComponent<UnityEngine.Camera>();
            _loadingCamera.clearFlags = CameraClearFlags.SolidColor;
            _loadingCamera.backgroundColor = Color.black;
            _loadingCamera.cullingMask = 0;
            _loadingCamera.depth = -100f;
            _loadingListener = gameObject.AddComponent<AudioListener>();
            _lampSfxSource = gameObject.AddComponent<AudioSource>();
            _lampSfxSource.playOnAwake = false;
            _lampSfxSource.spatialBlend = 0f;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void LateUpdate()
        {
            if (!_finished && _floorScene.IsValid() && _floorScene.isLoaded)
                SuspendInterface();
        }

        private IEnumerator Start()
        {
            if (!Application.CanStreamedLevelBeLoaded(firstFloorScene))
            {
                Debug.LogError($"[IntroCinematic] Scene '{firstFloorScene}' is missing from Build Settings.", this);
                SceneManager.LoadScene("MainMenu");
                yield break;
            }

            AsyncOperation loading = SceneManager.LoadSceneAsync(firstFloorScene, LoadSceneMode.Additive);
            if (loading == null)
            {
                Debug.LogError($"[IntroCinematic] Could not load '{firstFloorScene}'.", this);
                SceneManager.LoadScene("MainMenu");
                yield break;
            }

            yield return loading;
            _floorScene = SceneManager.GetSceneByName(firstFloorScene);
            if (!_floorScene.IsValid() || !_floorScene.isLoaded)
            {
                Debug.LogError("[IntroCinematic] First floor did not finish loading.", this);
                SceneManager.LoadScene("MainMenu");
                yield break;
            }

            SceneManager.SetActiveScene(_floorScene);
            MoveRuntimeObjectsToFloor();
            SuspendGameplay();
            HideCanvasObjects();

            GameObject playerObject = GameObject.Find("PlayerCharacter");
            _player = playerObject != null ? playerObject.transform : null;
            _camera = UnityEngine.Camera.main;
            if (_camera == null)
                _camera = FindFirstObjectByType<UnityEngine.Camera>();
            _lightingRig = playerObject != null ? playerObject.GetComponent<LightingRig>() : null;
            if (_lightingRig != null)
                _lightingRig.InitializeForCinematic();
            _lamp = _lightingRig != null ? _lightingRig.PlayerFlashlight : null;

            if (_player == null || _camera == null || _lamp == null)
            {
                Debug.LogError("[IntroCinematic] Eddie, gameplay camera, or held lamp is missing; opening gameplay directly.", this);
                FinishIntro();
                yield break;
            }

            _normalCameraPosition = _camera.transform.position;
            _normalCameraRotation = _camera.transform.rotation;
            _normalOrthographicSize = _camera.orthographicSize;
            _normalFieldOfView = _camera.fieldOfView;
            _cameraCaptured = true;
            _normalLampIntensity = _lamp.intensity;
            _introLampIntensity = Mathf.Max(1f, _normalLampIntensity);
            _normalLampEnabled = _lamp.enabled;

            Vector3 focus = FindEddieFocus(_player);
            Vector3 closePosition = focus - _normalCameraRotation * Vector3.forward * closeCameraDistance;
            _camera.transform.position = closePosition;
            if (_camera.orthographic)
                _camera.orthographicSize = Mathf.Min(_normalOrthographicSize, closeOrthographicSize);
            else
                _camera.fieldOfView = Mathf.Min(_normalFieldOfView, 35f);

            _lamp.enabled = true;
            _lamp.intensity = 0f;
            yield return new WaitForSecondsRealtime(darkHoldSeconds);

            // The three audible flickers land at roughly 0.12s, 0.82s, and 1.62s.
            PlayLampFlickerSfx();
            yield return new WaitForSecondsRealtime(0.12f);
            yield return SetLampFor(0.65f, 0.28f);
            yield return SetLampFor(0f, 0.42f);
            yield return SetLampFor(0.3f, 0.32f);
            yield return SetLampFor(0f, 0.48f);
            yield return SetLampFor(0.85f, 0.32f);
            yield return SetLampFor(0f, 0.3f);
            yield return SetLampFor(1f, 1.2f);

            float elapsed = 0f;
            float startingSize = _camera.orthographicSize;
            float startingFieldOfView = _camera.fieldOfView;
            while (elapsed < pullBackSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / pullBackSeconds));
                _camera.transform.position = Vector3.Lerp(closePosition, _normalCameraPosition, t);
                if (_camera.orthographic)
                    _camera.orthographicSize = Mathf.Lerp(startingSize, _normalOrthographicSize, t);
                else
                    _camera.fieldOfView = Mathf.Lerp(startingFieldOfView, _normalFieldOfView, t);
                yield return null;
            }

            FinishIntro();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Additive || scene.name != firstFloorScene)
                return;

            _floorScene = scene;
            if (_loadingCamera != null)
                _loadingCamera.enabled = false;
            if (_loadingListener != null)
                _loadingListener.enabled = false;
            Time.timeScale = 0f;
            SuspendGameplay();
        }

        private void SuspendGameplay()
        {
            SuspendInterface();

            Suspend(FindFirstObjectByType<CameraFollow>());
            Suspend(FindFirstObjectByType<CameraFollowController>());
            Suspend(FindFirstObjectByType<ThirdPersonController>());
            Suspend(FindFirstObjectByType<PlayerMovementController>());
            Suspend(FindFirstObjectByType<MobileStarterAssetsBridge>());
            Suspend(FindFirstObjectByType<MonsterAIController>());
            Suspend(FindFirstObjectByType<KeyboardBatteryDirector>());
            Suspend(FindFirstObjectByType<ExitDoorInteraction>());
            Suspend(FindFirstObjectByType<LightingRig>());

            Time.timeScale = 0f;
        }

        private void SuspendInterface()
        {
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Suspend(canvas);

            foreach (GraphicRaycaster raycaster in FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Suspend(raycaster);

            HideCanvasObjects();
        }

        private void HideCanvasObjects()
        {
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                GameObject canvasObject = canvas.gameObject;
                if (!canvasObject.activeSelf || _hiddenCanvasObjects.Contains(canvasObject))
                    continue;

                _hiddenCanvasObjects.Add(canvasObject);
                canvasObject.SetActive(false);
            }
        }

        private void Suspend(Behaviour component)
        {
            if (component == null || !component.enabled || _suspendedBehaviours.Contains(component))
                return;

            _suspendedBehaviours.Add(component);
            component.enabled = false;
        }

        private IEnumerator SetLampFor(float intensityFraction, float seconds)
        {
            _lamp.intensity = _introLampIntensity * intensityFraction;
            yield return new WaitForSecondsRealtime(seconds);
        }

        private void PlayLampFlickerSfx()
        {
            if (_lampFlickerSfxPlayed || lightFlickerClip == null || _lampSfxSource == null)
                return;

            _lampSfxSource.PlayOneShot(lightFlickerClip, SoundSettingsStore.Effects);
            _lampFlickerSfxPlayed = true;
        }

        private void FinishIntro()
        {
            if (_finished)
                return;

            _finished = true;
            if (_camera != null && _cameraCaptured)
            {
                _camera.transform.position = _normalCameraPosition;
                _camera.transform.rotation = _normalCameraRotation;
                _camera.orthographicSize = _normalOrthographicSize;
                _camera.fieldOfView = _normalFieldOfView;
            }
            if (_lamp != null)
            {
                _lamp.enabled = _normalLampEnabled;
                _lamp.intensity = _normalLampIntensity;
            }

            foreach (Behaviour component in _suspendedBehaviours)
            {
                if (component != null)
                    component.enabled = true;
            }
            _suspendedBehaviours.Clear();

            foreach (GameObject canvasObject in _hiddenCanvasObjects)
            {
                if (canvasObject != null)
                    canvasObject.SetActive(true);
            }
            _hiddenCanvasObjects.Clear();

            Time.timeScale = 1f;

            if (_floorScene.IsValid() && _floorScene.isLoaded)
            {
                SceneManager.SetActiveScene(_floorScene);
                SceneManager.UnloadSceneAsync(gameObject.scene);
            }
        }

        private void MoveRuntimeObjectsToFloor()
        {
            Scene introScene = gameObject.scene;
            foreach (GameObject root in introScene.GetRootGameObjects())
            {
                if (root == gameObject)
                    continue;

                SceneManager.MoveGameObjectToScene(root, _floorScene);
            }
        }

        private static Vector3 FindEddieFocus(Transform player)
        {
            Animator animator = player.GetComponentInChildren<Animator>(true);
            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null)
                    return Vector3.Lerp(player.position + Vector3.up * 0.9f, head.position, 0.7f);
            }

            return player.position + Vector3.up * 1.4f;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (!_finished)
                Time.timeScale = 1f;
        }
    }
}
