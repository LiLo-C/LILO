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
        private Image _batteryPanel;
        private Text _batteryPercentText;
        private bool _paused;
        private int _layoutWidth;
        private int _layoutHeight;

        private void Awake()
        {
            DisableDebugUi();
            SetupFloorFlag();
            SetupBatteryPanel();
            if (GetComponent<ChaseScreenEffects>() == null)
                gameObject.AddComponent<ChaseScreenEffects>();
            pauseButton?.onClick.AddListener(Pause);
            resumeButton?.onClick.AddListener(Resume);
            mainMenuButton?.onClick.AddListener(ReturnToMainMenu);
            if (livesText != null)
                livesText.gameObject.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private static void DisableDebugUi()
        {
            foreach (DebugOverlay overlay in FindObjectsByType<DebugOverlay>())
                if (overlay != null) overlay.gameObject.SetActive(false);

            GameObject debugToggle = GameObject.Find("DebugToggleButton");
            if (debugToggle != null) debugToggle.SetActive(false);
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
                if (_batteryPercentText != null)
                    _batteryPercentText.text = $"{percent}%";
                else
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
                float flagWidthPixels = Mathf.Clamp(shortSide * 0.14f, 72f, 112f);
                float flagHeightPixels = flagWidthPixels * (98f / 88f);
                float flagWidth = flagWidthPixels / scale;
                float flagHeight = flagHeightPixels / scale;
                RectTransform flagRect = _floorFlag.rectTransform;
                flagRect.anchoredPosition = new Vector2(30f / scale, -10f / scale);
                flagRect.sizeDelta = new Vector2(flagWidth, flagHeight);

                RectTransform numberRect = floorText.rectTransform;
                numberRect.anchorMin = new Vector2(0f, 1f);
                numberRect.anchorMax = new Vector2(0f, 1f);
                numberRect.pivot = new Vector2(0f, 1f);
                numberRect.anchoredPosition = new Vector2(flagWidthPixels * 0.07f / scale,
                    -flagHeightPixels * 0.42f / scale);
                numberRect.sizeDelta = new Vector2(flagWidthPixels * 0.76f / scale,
                    flagHeightPixels * 0.34f / scale);
                floorText.fontSize = Mathf.RoundToInt(flagWidthPixels * 0.34f / scale);
            }
            if (_batteryPanel != null)
            {
                float batteryWidthPixels = Mathf.Clamp(shortSide * 0.36f, 180f, 320f);
                float batteryHeightPixels = Mathf.Clamp(shortSide * 0.09f, 48f, 70f);
                float flagWidthPixels = Mathf.Clamp(shortSide * 0.14f, 72f, 112f);
                RectTransform panelRect = _batteryPanel.rectTransform;
                panelRect.anchoredPosition = new Vector2((30f + flagWidthPixels - 1f) / scale, -7f / scale);
                panelRect.sizeDelta = new Vector2(batteryWidthPixels, batteryHeightPixels) / scale;

                if (batteryText != null)
                {
                    RectTransform labelRect = batteryText.rectTransform;
                    labelRect.anchorMin = new Vector2(0f, 1f);
                    labelRect.anchorMax = new Vector2(0f, 1f);
                    labelRect.pivot = new Vector2(0f, 1f);
                    labelRect.anchoredPosition = new Vector2(11f / scale, 0f);
                    labelRect.sizeDelta = new Vector2(batteryWidthPixels * 0.58f, batteryHeightPixels) / scale;
                    batteryText.fontSize = Mathf.RoundToInt(25f / scale);
                }
                if (_batteryPercentText != null)
                {
                    RectTransform percentRect = _batteryPercentText.rectTransform;
                    percentRect.anchorMin = new Vector2(1f, 1f);
                    percentRect.anchorMax = new Vector2(1f, 1f);
                    percentRect.pivot = new Vector2(1f, 1f);
                    percentRect.anchoredPosition = new Vector2(-22f / scale, 0f);
                    percentRect.sizeDelta = new Vector2(batteryWidthPixels * 0.35f, batteryHeightPixels) / scale;
                    _batteryPercentText.fontSize = Mathf.RoundToInt(25f / scale);
                }
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

        private void SetupBatteryPanel()
        {
            if (batteryText == null) return;
            Canvas canvas = batteryText.canvas;
            if (canvas == null) return;

            var panel = new GameObject("BatteryHudPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            _batteryPanel = panel.GetComponent<Image>();
            _batteryPanel.enabled = false;
            _batteryPanel.raycastTarget = false;

            batteryText.transform.SetParent(panel.transform, false);
            batteryText.text = "Battery";
            batteryText.alignment = TextAnchor.MiddleLeft;
            batteryText.color = new Color(0.96f, 0.96f, 0.93f, 1f);
            batteryText.raycastTarget = false;

            var percent = new GameObject("BatteryPercentText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            percent.transform.SetParent(panel.transform, false);
            _batteryPercentText = percent.GetComponent<Text>();
            _batteryPercentText.font = batteryText.font;
            _batteryPercentText.fontStyle = batteryText.fontStyle;
            _batteryPercentText.alignment = TextAnchor.MiddleRight;
            _batteryPercentText.color = batteryText.color;
            _batteryPercentText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _batteryPercentText.verticalOverflow = VerticalWrapMode.Overflow;
            _batteryPercentText.raycastTarget = false;
            _batteryPanel.transform.SetAsFirstSibling();
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
