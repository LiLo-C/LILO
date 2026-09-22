namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Explicit result shape for enter/exit hiding attempts (spec 001 US1).
    /// </summary>
    public readonly struct HidingTransitionResult
    {
        public readonly bool Success;
        public readonly string FailureReason;

        private HidingTransitionResult(bool success, string failureReason)
        {
            Success = success;
            FailureReason = failureReason;
        }

        public static HidingTransitionResult Ok() => new HidingTransitionResult(true, string.Empty);
        public static HidingTransitionResult Fail(string reason) => new HidingTransitionResult(false, reason);
    }
}
