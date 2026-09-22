using Lilo.Systems.Monster;

namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Pure detection gate for hiding immunity (spec 002 FR-001, FR-002, GDD Ch. 4.3, 7.2).
    /// If the player is in the canonical Hidden state, detection is suppressed to false.
    /// In all other states (Visible, Entering, Exiting), the raw detection signal is passed through unchanged.
    /// </summary>
    public static class HidingDetectionGate
    {
        public static MonsterDetectionSignal Apply(MonsterDetectionSignal rawSignal, HidingState hidingState)
        {
            if (!HidingDetectionEligibility.IsEligibleForDetection(hidingState))
            {
                return new MonsterDetectionSignal(false, rawSignal.SourcePosition);
            }
            return rawSignal;
        }
    }
}
