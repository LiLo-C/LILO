using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours
{
    /// <summary>Short context hints shown along the bottom edge during office gameplay.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayGuidanceHud : MonoBehaviour
    {
        private Text _text;
        private GameObject _panel;
        private Transform _player;
        private ExitDoorInteraction[] _doors;
        private int _lastWidth;
        private int _lastHeight;
        private float _lastScale = -1f;
        private string _lastHint;
        private float _hideAt;
        private string _respawnMessage;
        private float _respawnMessageUntil;
        private SfxController _sfx;
        private string _lastVoiceHint;
        private bool _needAwayPlayed;
        private bool _hasFoundDoor;
        private float _needAwayVoiceAt;

        private const string NeedAwayHint = "I need to find a way to get out.";
        private const string NeedAccessKeyHint = "I need to find the access key first.";
        private const string NeedBatteryHint = "I need to find some more battery.";

        private void Awake()
        {
            _needAwayVoiceAt = Time.unscaledTime + 60f;
            _player = GameObject.Find("PlayerCharacter")?.transform;
            _doors = FindObjectsByType<ExitDoorInteraction>();
            _respawnMessage = GameManager.Instance?.State?.ConsumeRespawnMessage();
            if (!string.IsNullOrWhiteSpace(_respawnMessage))
                _respawnMessageUntil = Time.unscaledTime + 4f;
            _sfx = GameObject.Find("SfxController")?.GetComponent<SfxController>();
            CreateLabel();
            ApplyLayout();
        }

        private void Update()
        {
            float scale = GetComponent<Canvas>()?.scaleFactor ?? 1f;
            if (Screen.width != _lastWidth || Screen.height != _lastHeight || !Mathf.Approximately(scale, _lastScale))
                ApplyLayout();
            if (_player == null)
                _player = GameObject.Find("PlayerCharacter")?.transform;
            if (_doors == null || _doors.Length == 0)
                _doors = FindObjectsByType<ExitDoorInteraction>();
            if (_text == null) return;

            string hint = NeedAwayHint;
            bool nearDoor = false;
            ExitDoorInteraction nearbyExit = null;
            if (_player != null)
            {
                Vector2 playerPosition = new Vector2(_player.position.x, _player.position.z);
                foreach (ExitDoorInteraction door in _doors)
                {
                    if (door == null || !door.isActiveAndEnabled) continue;
                    Vector2 doorPosition = new Vector2(door.transform.position.x, door.transform.position.z);
                    if (Vector2.Distance(playerPosition, doorPosition) <= door.InteractionRadius * 2.5f)
                    {
                        nearDoor = true;
                        nearbyExit = door;
                        break;
                    }
                }
            }

            if (nearDoor)
            {
                _hasFoundDoor = true;
                if (nearbyExit != null && nearbyExit.RequiresAccessKey)
                    hint = nearbyExit.HasRequiredAccessKey
                        ? "I have the access key. I can get out."
                        : "I need to find the access key first.";
                else
                    hint = "I found the exit. I can get out.";
            }
            else
            {
                var manager = GameManager.Instance;
                float duration = manager != null && manager.Config != null ? manager.Config.batteryDuration : 1f;
                float charge = manager != null && manager.State != null
                    ? manager.State.InstalledBatteryCharge / Mathf.Max(1f, duration) : 1f;
                if (charge <= 0.75f)
                    hint = NeedBatteryHint;
            }

            if (!string.IsNullOrWhiteSpace(_respawnMessage))
            {
                if (Time.unscaledTime < _respawnMessageUntil)
                    hint = _respawnMessage;
                else
                    _respawnMessage = null;
            }

            if (hint != _lastHint)
            {
                _lastHint = hint;
                _text.text = hint;
                _panel.SetActive(true);
                _hideAt = Time.unscaledTime + 5f;

            }
            else if (_panel.activeSelf && Time.unscaledTime >= _hideAt)
            {
                _panel.SetActive(false);
            }

            if (hint != _lastVoiceHint)
            {
                if (_sfx == null)
                    _sfx = Object.FindAnyObjectByType<SfxController>();

                bool isLifeSubtitle = hint == "WHAT WAS THAT? WHAT IS HAPPENING?!"
                    || hint == "I FELT IT ALL THROUGH MY SKIN OH GOD"
                    || hint == "NO NO NO I DON'T WANT TO FEEL IT AGAIN";
                bool voiceRequested = isLifeSubtitle
                    ? _sfx != null && _sfx.PlayLifeVoiceOverForSubtitle(hint)
                    : hint switch
                    {
                        NeedAwayHint when !_needAwayPlayed && !_hasFoundDoor && Time.unscaledTime >= _needAwayVoiceAt
                            => _sfx != null && _sfx.PlayNeedAwayVoiceOver(),
                        NeedAwayHint when !_needAwayPlayed => false,
                        NeedAwayHint => true,
                        NeedAccessKeyHint => _sfx != null && _sfx.PlayNeedAccessKeyVoiceOver(),
                        NeedBatteryHint => _sfx != null && _sfx.PlayBatteryRunsOutVoiceOver(),
                        _ => true,
                    };
                if (voiceRequested)
                {
                    if (hint == NeedAwayHint)
                    {
                        _needAwayPlayed = true;
                        if (!_panel.activeSelf)
                        {
                            _text.text = hint;
                            _panel.SetActive(true);
                            _hideAt = Time.unscaledTime + 5f;
                        }
                    }
                    _lastVoiceHint = hint;
                    if (isLifeSubtitle)
                        GameManager.Instance?.State?.ClearPendingLifeVoiceOver();
                }
            }
        }

        private void CreateLabel()
        {
            _panel = new GameObject("GameplayGuidance", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _panel.transform.SetParent(transform, false);
            RectTransform panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            _panel.GetComponent<Image>().raycastTarget = false;
            _panel.SetActive(false);

            GameObject label = new GameObject("Hint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(_panel.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 4f);
            labelRect.offsetMax = new Vector2(-18f, -4f);

            _text = label.GetComponent<Text>();
            _text.font = Lilo.UI.GameUIFont.Get();
            _text.alignment = TextAnchor.MiddleCenter;
            _text.color = new Color(0.95f, 0.94f, 0.89f, 1f);
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Truncate;
            _text.raycastTarget = false;
        }

        private void ApplyLayout()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            if (_text == null) return;
            float scale = Mathf.Max(0.01f, GetComponent<Canvas>()?.scaleFactor ?? 1f);
            _lastScale = scale;
            float shortSide = Mathf.Min(Screen.width, Screen.height);
            _text.fontSize = Mathf.RoundToInt(Mathf.Clamp(shortSide * 0.022f, 22f, 34f) / scale);
            RectTransform panelRect = _text.transform.parent as RectTransform;
            if (panelRect == null) return;
            panelRect.sizeDelta = new Vector2(Mathf.Min(Screen.width * 0.82f, 1180f),
                Mathf.Clamp(shortSide * 0.065f, 60f, 90f)) / scale;
            panelRect.anchoredPosition = new Vector2(0f, Mathf.Max(14f, shortSide * 0.018f) / scale);
        }
    }
}
