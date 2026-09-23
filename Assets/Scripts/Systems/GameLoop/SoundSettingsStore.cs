using UnityEngine;

namespace Lilo.Systems.GameLoop
{
    /// <summary>Persistent, bounded audio preferences for the simple shell settings UI.</summary>
    public static class SoundSettingsStore
    {
        private const string MasterKey = "lilo.audio.master";
        private const string MusicKey = "lilo.audio.music";
        private const string EffectsKey = "lilo.audio.effects";
        private const string AmbienceKey = "lilo.audio.ambience";

        public static float Master => PlayerPrefs.GetFloat(MasterKey, 1f);
        public static float Music => PlayerPrefs.GetFloat(MusicKey, 1f);
        public static float Effects => PlayerPrefs.GetFloat(EffectsKey, 1f);
        public static float Ambience => PlayerPrefs.GetFloat(AmbienceKey, 1f);

        public static void SetMaster(float value) => Set(MasterKey, value);
        public static void SetMusic(float value) => Set(MusicKey, value);
        public static void SetEffects(float value) => Set(EffectsKey, value);
        public static void SetAmbience(float value) => Set(AmbienceKey, value);

        public static void Apply()
        {
            AudioListener.volume = Mathf.Clamp01(Master);
        }

        private static void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Apply();
        }
    }
}
