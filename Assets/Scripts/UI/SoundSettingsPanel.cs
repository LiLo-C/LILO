using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    /// <summary>
    /// Logika toggle sound effect. Toggle-nya dipasang lewat Inspector
    /// (prefab Assets/Prefabs/UI/SfxToggle), bukan dibuat dari kode.
    /// </summary>
    public sealed class SoundSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Toggle sfxToggle;

        private void Awake()
        {
            if (sfxToggle != null)
                sfxToggle.onValueChanged.AddListener(SoundSettingsStore.SetSfxEnabled);
            else
                Debug.LogWarning($"[SoundSettingsPanel] {name}: sfxToggle belum diisi di Inspector.", this);
            Refresh();
        }

        public void Refresh()
        {
            if (sfxToggle == null) return;
            bool enabled = SoundSettingsStore.SfxEnabled;
            sfxToggle.SetIsOnWithoutNotify(enabled);
            if (sfxToggle.TryGetComponent(out SwitchToggle visual))
                visual.Snap(enabled);
        }
    }
}
