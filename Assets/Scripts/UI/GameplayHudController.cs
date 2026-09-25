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
        private int _layoutWidth;
        private int _layoutHeight;

        private void Awake()
        {
            pauseButton?.onClick.AddListener(Pause);
            resumeButton?.onClick.AddListener(Resume);
            mainMenuButton?.onClick.AddListener(ReturnToMainMenu);
            if (livesText != null)
                livesText.gameObject.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (Screen.width != _layoutWidth || Screen.height != _layoutHeight)
                ApplyResponsiveLayout();

            var state = GameManager.Instance?.State;
            if (state == null) return;

            if (floorText != null)
            {
                int floorNumber = state.CurrentFloor == FloorId.Floor52 ? 52 :
                    state.CurrentFloor == FloorId.Floor51 ? 51 : 50;
                floorText.text = $"FLOOR {floorNumber}";
            }
            if (livesText != null)
                livesText.text = $"LIVES {state.Lives}";

            if (batteryText != null)
            {
                float duration = GameManager.Instance.Config != null ? GameManager.Instance.Config.batteryDuration : 1f;
                int percent = Mathf.RoundToInt(100f * Mathf.Clamp01(state.InstalledBatteryCharge / Mathf.Max(1f, duration)));
                batteryText.text = $"BATTERY {percent}%";
            }
        }

        private void ApplyResponsiveLayout()
        {
            _layoutWidth = Screen.width;
            _layoutHeight = Screen.height;
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) return;
            Canvas.ForceUpdateCanvases();
            float scale = Mathf.Max(0.01f, canvas.scaleFactor);
            float shortSide = Mathf.Min(Screen.width, Screen.height);
            float hudFont = Mathf.Clamp(shortSide * 0.032f, 32f, 48f) * 1.5f;
            foreach (Text label in new[] { floorText, batteryText })
            {
                if (label == null) continue;
                label.fontSize = Mathf.RoundToInt(hudFont / scale);
                label.rectTransform.sizeDelta = new Vector2(420f, 76f) * 1.5f / scale;
            }
            SizeButton(pauseButton, Mathf.Clamp(shortSide * 0.22f, 230f, 320f),
                Mathf.Clamp(shortSide * 0.085f, 90f, 124f), hudFont * 0.9f, scale);
            if (pausePanel != null)
            {
                Text title = pausePanel.transform.Find("PauseTitle")?.GetComponent<Text>();
                if (title != null)
                {
                    title.fontSize = Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.052f, 50f, 76f) * 1.5f / scale);
                    title.rectTransform.sizeDelta = new Vector2(700f, 120f) * 1.5f / scale;
                }
            }
            float menuWidth = Mathf.Clamp(shortSide * 0.36f, 350f, 550f);
            float menuHeight = Mathf.Clamp(shortSide * 0.1f, 96f, 140f);
            SizeButton(resumeButton, menuWidth, menuHeight, hudFont, scale);
            SizeButton(mainMenuButton, menuWidth, menuHeight, hudFont, scale);
        }

        private static void SizeButton(Button button, float width, float height, float fontPixels, float scale)
        {
            if (button == null) return;
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height) * 1.5f / scale;
            Text label = button.GetComponentInChildren<Text>(true);
            if (label == null) return;
            label.fontSize = Mathf.RoundToInt(fontPixels / scale);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
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
