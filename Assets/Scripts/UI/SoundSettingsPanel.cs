using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    public sealed class SoundSettingsPanel : MonoBehaviour
    {
        private Toggle _sfxToggle;
        private Text _sfxLabel;

        private void Awake()
        {
            HideOldVolumeControls();
            CreateSfxToggle();
            Refresh();
        }

        public void Refresh()
        {
            if (_sfxToggle != null)
                _sfxToggle.SetIsOnWithoutNotify(SoundSettingsStore.SfxEnabled);
            UpdateSfxLabel(SoundSettingsStore.SfxEnabled);
        }

        private void HideOldVolumeControls()
        {
            foreach (string name in new[]
                     {
                         "MASTERLabel", "MASTERSlider", "MUSICLabel", "MUSICSlider",
                         "EFFECTSSlider", "AMBIENCESlider", "AMBIENCELabel"
                     })
            {
                Transform oldControl = transform.Find(name);
                if (oldControl != null)
                    oldControl.gameObject.SetActive(false);
            }
        }

        private void CreateSfxToggle()
        {
            if (transform.Find("SfxToggle") != null)
                return;

            var row = new GameObject("SfxToggle", typeof(RectTransform));
            row.transform.SetParent(transform, false);
            var rowRect = (RectTransform)row.transform;
            rowRect.anchorMin = rowRect.anchorMax = new Vector2(0.5f, 0.55f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.sizeDelta = new Vector2(460f, 84f);

            var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundObject.transform.SetParent(row.transform, false);
            var backgroundRect = (RectTransform)backgroundObject.transform;
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.pivot = new Vector2(0f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(64f, 64f);
            Image background = backgroundObject.GetComponent<Image>();
            background.color = new Color(0.08f, 0.12f, 0.15f, 1f);

            var checkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkObject.transform.SetParent(backgroundObject.transform, false);
            var checkRect = (RectTransform)checkObject.transform;
            checkRect.anchorMin = new Vector2(0.18f, 0.18f);
            checkRect.anchorMax = new Vector2(0.82f, 0.82f);
            checkRect.offsetMin = checkRect.offsetMax = Vector2.zero;
            Image checkmark = checkObject.GetComponent<Image>();
            checkmark.color = new Color(0.34f, 0.82f, 0.91f, 1f);

            Transform labelTransform = transform.Find("EFFECTSLabel");
            if (labelTransform == null)
            {
                var labelObject = new GameObject("SfxToggleLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                labelTransform = labelObject.transform;
            }
            labelTransform.SetParent(row.transform, false);
            labelTransform.name = "SfxToggleLabel";
            labelTransform.gameObject.SetActive(true);
            var labelRect = (RectTransform)labelTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(88f, 0f);
            labelRect.offsetMax = Vector2.zero;
            _sfxLabel = labelTransform.GetComponent<Text>();
            _sfxLabel.text = "SFX: ON";
            if (_sfxLabel.font == null)
                _sfxLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _sfxLabel.fontSize = 32;
            _sfxLabel.color = Color.white;
            _sfxLabel.alignment = TextAnchor.MiddleLeft;
            _sfxLabel.raycastTarget = false;

            _sfxToggle = row.AddComponent<Toggle>();
            _sfxToggle.targetGraphic = background;
            _sfxToggle.graphic = checkmark;
            _sfxToggle.transition = Selectable.Transition.ColorTint;
            _sfxToggle.onValueChanged.AddListener(SetSfxEnabled);
        }

        private void SetSfxEnabled(bool enabled)
        {
            SoundSettingsStore.SetSfxEnabled(enabled);
            UpdateSfxLabel(enabled);
        }

        private void UpdateSfxLabel(bool enabled)
        {
            if (_sfxLabel != null)
                _sfxLabel.text = enabled ? "SFX: ON" : "SFX: OFF";
        }
    }
}
