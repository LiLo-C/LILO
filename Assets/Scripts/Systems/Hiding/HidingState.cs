namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Canonical hiding state (GDD Ch. 4.3, spec 001 FR-001).
    /// Entering and Exiting are transient timed states.
    /// </summary>
    public enum HidingState
    {
        Visible,
        Entering,
        Hidden,
        Exiting
    }
}
