using UnityEngine;

namespace Lilo.Systems.GameLoop
{
    /// <summary>Persistent sound effects preference for the settings toggle.</summary>
    public static class SoundSettingsStore
    {
        private const string SfxEnabledKey = "lilo.audio.sfxEnabled";

        public static bool SfxEnabled => PlayerPrefs.GetInt(SfxEnabledKey, 1) != 0;
        public static float Effects => SfxEnabled ? 1f : 0f;

        // Music and ambience remain at their authored levels; only SFX has a user setting.
        public static float Music => 1f;
        public static float Ambience => 1f;

        public static void SetSfxEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SfxEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Apply()
        {
            AudioListener.volume = 1f;
        }
    }
}
