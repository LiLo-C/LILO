using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Lilo.Systems.GameLoop;

namespace Lilo.MonoBehaviours.UI
{
    /// <summary>
    /// Persistent UI sound player and runtime installer for responsive canvas and
    /// click feedback components, including controls created during play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiClickFeedback : MonoBehaviour
    {
        private const string ObjectName = "UiClickFeedback";
        private const int SampleRate = 22050;
        private const float RefreshInterval = 0.35f;

        public static UiClickFeedback Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float volume = 0.45f;

        private AudioSource _source;
        private AudioClip _clickClip;
        private float _refreshTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (Instance != null)
                return;

            var feedbackObject = new GameObject(ObjectName);
            feedbackObject.AddComponent<UiClickFeedback>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;

            SceneManager.sceneLoaded += OnSceneLoaded;
            InstallOnLoadedObjects();
        }

        private void Update()
        {
            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer > 0f)
                return;

            _refreshTimer = RefreshInterval;
            InstallOnLoadedObjects();
        }

        public void PlayClick()
        {
            if (_source == null)
                return;

            if (_clickClip == null)
                _clickClip = CreateHorrorClickClip();

            _source.PlayOneShot(_clickClip, volume * SoundSettingsStore.Effects);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InstallOnLoadedObjects();
        }

        private static void InstallOnLoadedObjects()
        {
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace)
                    continue;

                if (canvas.GetComponent<ResponsiveCanvasAdapter>() == null)
                    canvas.gameObject.AddComponent<ResponsiveCanvasAdapter>();
            }

            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button.GetComponent<UiButtonClickHook>() == null)
                    button.gameObject.AddComponent<UiButtonClickHook>();
            }

            foreach (Selectable selectable in FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (selectable is Button || selectable.GetComponent<UiButtonClickHook>() != null)
                    continue;

                selectable.gameObject.AddComponent<UiButtonClickHook>();
            }
        }

        private static AudioClip CreateHorrorClickClip()
        {
            const float duration = 0.14f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            var clip = AudioClip.Create("LILO_HorrorUiClick", sampleCount, 1, SampleRate, false);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                float normalized = t / duration;
                float envelope = Mathf.Exp(-29f * t) * Mathf.Clamp01((1f - normalized) * 5f);
                float descendingTone = Mathf.Sin(2f * Mathf.PI * (760f - 360f * normalized) * t);
                float dissonantTone = Mathf.Sin(2f * Mathf.PI * 510f * t);
                float lowKnock = Mathf.Sin(2f * Mathf.PI * 105f * t) * Mathf.Exp(-85f * t);
                samples[i] = (descendingTone * 0.18f + dissonantTone * 0.09f + lowKnock * 0.16f) * envelope;
            }

            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
                Instance = null;

            if (_clickClip != null)
                Destroy(_clickClip);
        }
    }
}
