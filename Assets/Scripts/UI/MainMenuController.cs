using System.Collections;
using Lilo.MonoBehaviours;
using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private SoundSettingsPanel soundSettings;
        [SerializeField] private AudioClip mainMenuBgm;
        [SerializeField, Min(0f)] private float mainMenuFadeInSeconds = 5f;

        private bool _transitioning;
        private AudioSource _musicSource;
        private int _lastLayoutWidth;
        private int _lastLayoutHeight;

        private void Awake()
        {
            SoundSettingsStore.Apply();
            StartMainMenuMusic();
            if (startButton != null) startButton.onClick.AddListener(StartRun);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void Start()
        {
            ApplyResponsiveMenuLayout();
        }

        private void Update()
        {
            if (Application.isMobilePlatform
                && (Screen.width != _lastLayoutWidth || Screen.height != _lastLayoutHeight))
                ApplyResponsiveMenuLayout();
        }

        private void ApplyResponsiveMenuLayout()
        {
            _lastLayoutWidth = Screen.width;
            _lastLayoutHeight = Screen.height;

            Canvas canvas = GetComponent<Canvas>();
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            Canvas.ForceUpdateCanvases();

            Transform menu = transform.Find("MenuPanel");
            if (menu == null)
                return;

            bool phone = Application.isMobilePlatform && Mathf.Min(Screen.width, Screen.height) < 1400;
            float scale = Mathf.Max(0.01f, canvas != null ? canvas.scaleFactor : 1f);
            float buttonWidth = (phone ? 280f : 250f) / scale;
            float buttonHeight = (phone ? 60f : 56f) / scale;
            float step = (phone ? 78f : 76f) / scale;
            float groupCenterY = (phone ? 0f : -55f) / scale;

            SetMenuButton(menu, "StartButton", buttonWidth, buttonHeight, groupCenterY + step);
            SetMenuButton(menu, "HowToPlayButton", buttonWidth, buttonHeight, groupCenterY);
            SetMenuButton(menu, "SettingsButton", buttonWidth, buttonHeight, groupCenterY - step);

            SetDecorativeText(menu, "Title", !phone, 44f / scale, 155f / scale, 56f / scale, scale);
            SetDecorativeText(menu, "Subtitle", !phone, 18f / scale, 105f / scale, 28f / scale, scale);
            SetDecorativeText(menu, "Credits", !phone, 12f / scale, 0f, 30f / scale, scale);

            int buttonFontSize = Mathf.RoundToInt((phone ? 18f : 17f) / scale);
            foreach (string buttonName in new[] { "StartButton", "HowToPlayButton", "SettingsButton" })
            {
                Transform button = menu.Find(buttonName);
                Text label = button != null ? button.GetComponentInChildren<Text>(true) : null;
                if (label != null)
                    label.fontSize = buttonFontSize;
            }
        }

        private static void SetMenuButton(Transform menu, string name, float width, float height, float y)
        {
            RectTransform rect = menu.Find(name) as RectTransform;
            if (rect == null)
                return;

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(0f, y);
        }

        private static void SetDecorativeText(Transform menu, string name, bool visible,
            float fontSize, float y, float height, float canvasScale)
        {
            Transform item = menu.Find(name);
            if (item == null)
                return;

            item.gameObject.SetActive(visible);
            if (!visible)
                return;

            RectTransform rect = item as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = rect.anchorMax = name == "Credits"
                    ? new Vector2(0.5f, 0.08f)
                    : new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(600f / canvasScale, height);
                rect.anchoredPosition = new Vector2(0f, y);
            }

            Text text = item.GetComponent<Text>();
            if (text != null)
                text.fontSize = Mathf.RoundToInt(fontSize);
        }

        private void OnEnable()
        {
            if (_musicSource == null || _musicSource.isPlaying) return;

            _musicSource.volume = 0f;
            _musicSource.Play();
            StartCoroutine(FadeInMainMenuMusic());
        }

        private void OnDisable()
        {
            if (_musicSource != null && _musicSource.isPlaying)
                _musicSource.Stop();
        }

        private void StartMainMenuMusic()
        {
            if (mainMenuBgm == null) return;

            if (FindFirstObjectByType<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.clip = mainMenuBgm;
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.spatialBlend = 0f;
            _musicSource.volume = 0f;
            _musicSource.Play();
            StartCoroutine(FadeInMainMenuMusic());
        }

        private IEnumerator FadeInMainMenuMusic()
        {
            if (mainMenuFadeInSeconds <= 0f)
            {
                _musicSource.volume = SoundSettingsStore.Music;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < mainMenuFadeInSeconds && _musicSource != null)
            {
                elapsed += Time.unscaledDeltaTime;
                _musicSource.volume = Mathf.Lerp(0f, SoundSettingsStore.Music, elapsed / mainMenuFadeInSeconds);
                yield return null;
            }

            if (_musicSource != null)
                _musicSource.volume = SoundSettingsStore.Music;
        }

        public void StartRun()
        {
            if (_transitioning) return;
            _transitioning = true;
            SetButtons(false);
            GameManager.Instance?.BeginNewRun();
            SceneManager.LoadScene("IntroCinematic");
        }

        public void OpenHowToPlay()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        public void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            soundSettings?.Refresh();
        }

        public void CloseOverlay()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void SetButtons(bool enabled)
        {
            if (startButton != null) startButton.interactable = enabled;
            if (howToPlayButton != null) howToPlayButton.interactable = enabled;
            if (settingsButton != null) settingsButton.interactable = enabled;
        }
    }
}
