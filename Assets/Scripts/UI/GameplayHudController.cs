using Lilo.MonoBehaviours;
using Lilo.Config;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>Small shared HUD for the temporary three-office playable run.</summary>
    public sealed class GameplayHudController : MonoBehaviour
    {
        [SerializeField] private Text floorText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text batteryText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        private bool _paused;

        private void Awake()
        {
            pauseButton?.onClick.AddListener(Pause);
            resumeButton?.onClick.AddListener(Resume);
            mainMenuButton?.onClick.AddListener(ReturnToMainMenu);
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Update()
        {
            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (floorText != null)
            {
                int floorNumber = state.CurrentFloor == FloorId.Floor52 ? 52 :
                    state.CurrentFloor == FloorId.Floor51 ? 51 : 50;
                floorText.text = $"FLOOR {floorNumber}";
            }
            if (livesText != null) livesText.text = $"LIVES {state.Lives}";
            if (batteryText != null)
            {
                float duration = GameManager.Instance.Config != null ? GameManager.Instance.Config.batteryDuration : 1f;
                int percent = Mathf.RoundToInt(100f * Mathf.Clamp01(state.InstalledBatteryCharge / Mathf.Max(1f, duration)));
                batteryText.text = $"BATTERY {percent}%";
            }
        }

        public void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void ReturnToMainMenu()
        {
            Resume();
            Lilo.MonoBehaviours.Input.MobileControlsBootstrap.PrepareForSceneReload();
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (_paused) Time.timeScale = 1f;
        }
    }
}
