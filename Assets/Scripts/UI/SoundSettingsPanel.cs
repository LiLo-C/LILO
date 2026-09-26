using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>Connects the SFX toggle to the global audio preference.</summary>
    public sealed class SoundSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Toggle sfxToggle;
        private SwitchToggle _switchToggle;

        private void Awake()
        {
            if (sfxToggle == null)
            {
                Transform child = transform.Find("SfxToggle");
                if (child != null) sfxToggle = child.GetComponent<Toggle>();
            }

            if (sfxToggle == null)
            {
                Debug.LogWarning($"[SoundSettingsPanel] {name}: SFX toggle is not assigned.", this);
                return;
            }

            sfxToggle.onValueChanged.AddListener(SetSfxEnabled);
            sfxToggle.TryGetComponent(out _switchToggle);
            sfxToggle.SetIsOnWithoutNotify(SoundSettingsStore.SfxEnabled);
            _switchToggle?.Snap(SoundSettingsStore.SfxEnabled);
        }

        public void Refresh()
        {
            if (sfxToggle == null) return;
            bool enabled = SoundSettingsStore.SfxEnabled;
            sfxToggle.SetIsOnWithoutNotify(enabled);
            _switchToggle?.Snap(enabled);
        }

        private static void SetSfxEnabled(bool enabled) => SoundSettingsStore.SetSfxEnabled(enabled);
    }
}
