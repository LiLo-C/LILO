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

        private bool _transitioning;

        private void Awake()
        {
            SoundSettingsStore.Apply();
            if (startButton != null) startButton.onClick.AddListener(StartRun);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void StartRun()
        {
            if (_transitioning) return;
            _transitioning = true;
            SetButtons(false);
            GameManager.Instance?.BeginNewRun();
            SceneManager.LoadScene("OfficeLevel1");
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
