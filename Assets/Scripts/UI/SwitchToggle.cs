using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>
    /// Visual untuk Toggle model saklar: ganti sprite track dan geser knob.
    /// Ukuran dan posisi diatur lewat Inspector; skrip ini hanya memindah knob
    /// ke kiri (mati) atau kanan (nyala) sejauh knobTravel.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class SwitchToggle : MonoBehaviour
    {
        [SerializeField] private Image track;
        [SerializeField] private Sprite trackOn;
        [SerializeField] private Sprite trackOff;
        [SerializeField] private RectTransform knob;
        [SerializeField, Min(0f)] private float knobTravel = 52f;
        [SerializeField, Min(0f)] private float slideSeconds = 0.12f;

        private Toggle _toggle;
        private Coroutine _slide;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnEnable()
        {
            // Pakai nilai sekarang tanpa animasi, misalnya setelah SetIsOnWithoutNotify.
            Snap(_toggle != null && _toggle.isOn);
        }

        private void OnDestroy()
        {
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>Samakan tampilan dengan isOn tanpa animasi.</summary>
        public void Snap(bool isOn)
        {
            if (_slide != null) StopCoroutine(_slide);
            _slide = null;
            SetTrack(isOn);
            SetKnobX(TargetX(isOn));
        }

        private void OnValueChanged(bool isOn)
        {
            SetTrack(isOn);
            if (knob == null) return;
            if (_slide != null) StopCoroutine(_slide);
            _slide = isActiveAndEnabled && slideSeconds > 0f
                ? StartCoroutine(Slide(TargetX(isOn)))
                : null;
            if (_slide == null) SetKnobX(TargetX(isOn));
        }

        private IEnumerator Slide(float targetX)
        {
            float startX = knob.anchoredPosition.x;
            float elapsed = 0f;
            // Unscaled supaya tetap jalan saat pause (timeScale 0).
            while (elapsed < slideSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                SetKnobX(Mathf.Lerp(startX, targetX, elapsed / slideSeconds));
                yield return null;
            }
            SetKnobX(targetX);
            _slide = null;
        }

        private float TargetX(bool isOn) => isOn ? knobTravel : -knobTravel;

        private void SetTrack(bool isOn)
        {
            if (track == null) return;
            Sprite sprite = isOn ? trackOn : trackOff;
            if (sprite != null) track.sprite = sprite;
        }

        private void SetKnobX(float x)
        {
            if (knob == null) return;
            knob.anchoredPosition = new Vector2(x, knob.anchoredPosition.y);
        }
    }
}
