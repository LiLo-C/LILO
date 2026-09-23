using System.Runtime.InteropServices;
using Lilo.State;

namespace Lilo.MonoBehaviours.Monster
{
    /// <summary>iOS haptic patterns for changes in the monster's awareness state.</summary>
    public static class MonsterHaptics
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void LiloMonsterHapticsAlert();
        [DllImport("__Internal")] private static extern void LiloMonsterHapticsChase();
        [DllImport("__Internal")] private static extern void LiloMonsterHapticsStop();
#endif

        public static void OnStateChanged(MonsterState state)
        {
#if UNITY_IOS && !UNITY_EDITOR
            LiloMonsterHapticsStop();
            if (state == MonsterState.Alert)
                LiloMonsterHapticsAlert();
            else if (state == MonsterState.Chase)
                LiloMonsterHapticsChase();
#endif
        }

        public static void Stop()
        {
#if UNITY_IOS && !UNITY_EDITOR
            LiloMonsterHapticsStop();
#endif
        }
    }
}
