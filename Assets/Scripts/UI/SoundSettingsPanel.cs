using Lilo.Systems.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    public sealed class SoundSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider master;
        [SerializeField] private Slider music;
        [SerializeField] private Slider effects;
        [SerializeField] private Slider ambience;

        private void Awake()
        {
            if (master != null) master.onValueChanged.AddListener(SoundSettingsStore.SetMaster);
            if (music != null) music.onValueChanged.AddListener(SoundSettingsStore.SetMusic);
            if (effects != null) effects.onValueChanged.AddListener(SoundSettingsStore.SetEffects);
            if (ambience != null) ambience.onValueChanged.AddListener(SoundSettingsStore.SetAmbience);
            Refresh();
        }

        public void Refresh()
        {
            if (master != null) master.SetValueWithoutNotify(SoundSettingsStore.Master);
            if (music != null) music.SetValueWithoutNotify(SoundSettingsStore.Music);
            if (effects != null) effects.SetValueWithoutNotify(SoundSettingsStore.Effects);
            if (ambience != null) ambience.SetValueWithoutNotify(SoundSettingsStore.Ambience);
        }
    }
}
