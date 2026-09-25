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
            if (Screen.width != _lastLayoutWidth || Screen.height != _lastLayoutHeight)
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

            float shortSide = Mathf.Min(Screen.width, Screen.height);
            bool phone = shortSide < 1400f;
            float scale = Mathf.Max(0.01f, canvas != null ? canvas.scaleFactor : 1f);
            float buttonWidthPixels = Mathf.Clamp(shortSide * (phone ? 0.36f : 0.27f), 350f, 550f);
            float buttonHeightPixels = Mathf.Clamp(shortSide * (phone ? 0.10f : 0.065f), 96f, 140f);
            float buttonWidth = buttonWidthPixels * 1.5f / scale;
            float buttonHeight = buttonHeightPixels * 1.5f / scale;
            float step = (buttonHeightPixels * 1.5f + 30f) / scale;
            float groupCenterY = (phone ? 0f : -shortSide * 0.04f) / scale;

            SetMenuButton(menu, "StartButton", buttonWidth, buttonHeight, groupCenterY + step);
            SetMenuButton(menu, "HowToPlayButton", buttonWidth, buttonHeight, groupCenterY);
            SetMenuButton(menu, "SettingsButton", buttonWidth, buttonHeight, groupCenterY - step);

            SetDecorativeText(menu, "Title", !phone, 64f / scale, shortSide * 0.22f / scale, 90f / scale, scale);
            SetDecorativeText(menu, "Subtitle", !phone, 30f / scale, shortSide * 0.15f / scale, 48f / scale, scale);
            SetDecorativeText(menu, "Credits", !phone, 24f / scale, 0f, 64f / scale, scale);

            int buttonFontSize = Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.035f, 34f, 48f) * 1.5f / scale);
            foreach (string buttonName in new[] { "StartButton", "HowToPlayButton", "SettingsButton" })
            {
                Transform button = menu.Find(buttonName);
                Text label = button != null ? button.GetComponentInChildren<Text>(true) : null;
                if (label != null)
                {
                    label.fontSize = buttonFontSize;
                    RectTransform labelRect = label.rectTransform;
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.offsetMin = Vector2.zero;
                    labelRect.offsetMax = Vector2.zero;
                }
            }

            LayoutOverlays(shortSide, scale, buttonWidth, buttonHeight, buttonFontSize);
        }

        private void LayoutOverlays(float shortSide, float scale, float buttonWidth, float buttonHeight, int buttonFontSize)
        {
            Transform how = howToPlayPanel != null ? howToPlayPanel.transform : null;
            Transform settings = settingsPanel != null ? settingsPanel.transform : null;
            SetOverlayButton(how, "CloseHowToPlay", buttonWidth, buttonHeight, buttonFontSize);
            SetOverlayButton(settings, "CloseSettings", buttonWidth, buttonHeight, buttonFontSize);
            SetOverlayText(how, "HowToPlayText", Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.028f, 30f, 42f) * 1.5f / scale),
                Mathf.Min(Screen.width * 0.82f, 1500f) / scale, shortSide * 0.48f / scale);
            SetOverlayText(settings, "SettingsTitle", Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.04f, 42f, 64f) * 1.5f / scale),
                700f / scale, 90f / scale);

            Transform sfxToggle = settings != null ? settings.Find("SfxToggle") : null;
            RectTransform sfxRect = sfxToggle as RectTransform;
            if (sfxRect != null)
            {
                float rowHeight = Mathf.Clamp(shortSide * 0.09f, 96f, 140f) * 1.5f / scale;
                float boxSize = Mathf.Clamp(shortSide * 0.075f, 72f, 108f) * 1.5f / scale;
                sfxRect.sizeDelta = new Vector2(Mathf.Clamp(shortSide * 0.44f, 460f, 760f) * 1.5f / scale, rowHeight);
                RectTransform background = sfxToggle.Find("Background") as RectTransform;
                if (background != null)
                    background.sizeDelta = new Vector2(boxSize, boxSize);
                RectTransform labelRect = sfxToggle.Find("SfxToggleLabel") as RectTransform;
                if (labelRect != null)
                {
                    labelRect.offsetMin = new Vector2(boxSize + 24f / scale, 0f);
                    Text label = labelRect.GetComponent<Text>();
                    if (label != null)
                        label.fontSize = Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.03f, 32f, 46f) * 1.5f / scale);
                }
            }
        }

        private static void SetOverlayButton(Transform panel, string name, float width, float height, int fontSize)
        {
            RectTransform rect = panel != null ? panel.Find(name) as RectTransform : null;
            if (rect == null) return;
            rect.sizeDelta = new Vector2(width, height);
            Text label = rect.GetComponentInChildren<Text>(true);
            if (label == null) return;
            label.fontSize = fontSize;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        private static void SetOverlayText(Transform panel, string name, int fontSize, float width, float height)
        {
            Text label = panel != null ? panel.Find(name)?.GetComponent<Text>() : null;
            if (label == null) return;
            label.fontSize = fontSize;
            label.rectTransform.sizeDelta = new Vector2(width, height);
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

            if (FindAnyObjectByType<AudioListener>() == null)
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
            ApplyResponsiveMenuLayout();
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
