namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Evaluates whether a given HidingState is eligible for monster detection (spec 002 FR-001, GDD Ch. 4.3).
    /// Hidden grants full detection immunity; Visible, Entering, and Exiting remain eligible.
    /// </summary>
    public static class HidingDetectionEligibility
    {
        public static bool IsEligibleForDetection(HidingState hidingState)
        {
            return hidingState != HidingState.Hidden;
        }
    }
}
