using Lilo.MonoBehaviours;
using Lilo.Config;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>
    /// HUD gameplay dan Pause Menu. Logika saja; tata letak ada di prefab
    /// Assets/Prefabs/UI/GameplayHud dan diatur lewat Inspector.
    /// </summary>
    public sealed class GameplayHudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text floorText;
        [SerializeField] private TMP_Text batteryText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private SoundSettingsPanel soundSettings;
        [Tooltip("Scene pertama gameplay saat Restart memulai run baru.")]
        [SerializeField] private string restartSceneName = "OfficeLevel1";

        private bool _paused;

        private void Awake()
        {
            pauseButton?.onClick.AddListener(Pause);
            resumeButton?.onClick.AddListener(Resume);
            restartButton?.onClick.AddListener(RestartRun);
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
                floorText.SetText("{0}", floorNumber);
            }
            if (batteryText != null)
            {
                float duration = GameManager.Instance.Config != null ? GameManager.Instance.Config.batteryDuration : 1f;
                int percent = Mathf.RoundToInt(100f * Mathf.Clamp01(state.InstalledBatteryCharge / Mathf.Max(1f, duration)));
                batteryText.SetText("BATTERY {0}%", percent);
            }
        }

        public void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            if (pauseButton != null) pauseButton.gameObject.SetActive(false);
            soundSettings?.Refresh();
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        }

        public void RestartRun()
        {
            Resume();
            GameManager.Instance?.BeginNewRun();
            Lilo.MonoBehaviours.Input.MobileControlsBootstrap.PrepareForSceneReload();
            SceneManager.LoadScene(restartSceneName);
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
