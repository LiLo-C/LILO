using UnityEngine;

namespace Lilo.Systems.GameLoop
{
    /// <summary>Persistent sound effects preference for the settings toggle.</summary>
    public static class SoundSettingsStore
    {
        private const string SfxEnabledKey = "lilo.audio.sfxEnabled";

        public static bool SfxEnabled => PlayerPrefs.GetInt(SfxEnabledKey, 1) != 0;
        public static float Effects => SfxEnabled ? 1f : 0f;

        // The listener gates the full mix; keep authored source levels intact so
        // enabling sound again restores music and ambience already in progress.
        public static float Music => 1f;
        public static float Ambience => 1f;

        public static void SetSfxEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            // Listener volume is the single global gate for every game sound,
            // including music, ambience, one-shots, UI clicks, and live sources.
            AudioListener.volume = SfxEnabled ? 1f : 0f;
        }
    }
}
