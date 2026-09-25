using System.Collections;
using Lilo.MonoBehaviours;
using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>
    /// Main menu logic only. Semua tata letak diatur lewat Inspector,
    /// bukan lewat kode. Versi lama menimpa posisi, ukuran, dan
    /// Canvas Scaler tiap frame lewat Update().
    /// </summary>
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
            SceneManager.LoadScene("Prologue");
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