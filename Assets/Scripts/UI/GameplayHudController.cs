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

        private Image _floorFlag;
        private bool _paused;
        private int _layoutWidth;
        private int _layoutHeight;

        private void Awake()
        {
            SetupFloorFlag();
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
                floorText.text = _floorFlag != null ? floorNumber.ToString() : $"FLOOR {floorNumber}";
            }
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
            if (_floorFlag != null)
            {
                float flagWidth = Mathf.Clamp(shortSide * 0.16f, 110f, 160f) / scale;
                float flagHeight = flagWidth * (98f / 88f);
                RectTransform flagRect = _floorFlag.rectTransform;
                flagRect.anchoredPosition = new Vector2(20f / scale, -20f / scale);
                flagRect.sizeDelta = new Vector2(flagWidth, flagHeight);

                RectTransform numberRect = floorText.rectTransform;
                numberRect.anchorMin = new Vector2(0f, 1f);
                numberRect.anchorMax = new Vector2(0f, 1f);
                numberRect.pivot = new Vector2(0f, 1f);
                numberRect.anchoredPosition = new Vector2(flagWidth * 0.92f, -flagHeight * 0.25f);
                numberRect.sizeDelta = new Vector2(flagWidth * 0.65f, flagHeight * 0.55f);
                floorText.fontSize = Mathf.RoundToInt(flagWidth * 0.34f);
            }
            if (batteryText != null)
            {
                batteryText.fontSize = Mathf.RoundToInt(hudFont / scale);
                batteryText.rectTransform.sizeDelta = new Vector2(420f, 76f) * 1.5f / scale;
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

        private void SetupFloorFlag()
        {
            if (floorText == null) return;
            Sprite sprite = Resources.Load<Sprite>("FloorFlag");
            Canvas canvas = floorText.canvas;
            if (sprite == null || canvas == null) return;

            var badge = new GameObject("FloorFlagBadge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(canvas.transform, false);
            RectTransform badgeRect = (RectTransform)badge.transform;
            badgeRect.anchorMin = new Vector2(0f, 1f);
            badgeRect.anchorMax = new Vector2(0f, 1f);
            badgeRect.pivot = new Vector2(0f, 1f);

            _floorFlag = badge.GetComponent<Image>();
            _floorFlag.sprite = sprite;
            _floorFlag.preserveAspect = true;
            _floorFlag.raycastTarget = false;

            RectTransform numberRect = floorText.rectTransform;
            numberRect.SetParent(badgeRect, false);
            numberRect.anchorMin = new Vector2(0f, 1f);
            numberRect.anchorMax = new Vector2(0f, 1f);
            numberRect.pivot = new Vector2(0f, 1f);
            floorText.alignment = TextAnchor.MiddleCenter;
            floorText.color = new Color(0.09f, 0.11f, 0.12f, 1f);
            floorText.raycastTarget = false;
            floorText.transform.SetAsLastSibling();
            _floorFlag.transform.SetAsFirstSibling();
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
