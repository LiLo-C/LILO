namespace Lilo.State
{
    /// <summary>Monster AI states (GDD Ch. 6.1, spec 002 FR-005). Exactly one active at a time.</summary>
    public enum MonsterState
    {
        Patrol,
        Investigate,
        Alert,
        Chase,
        Search,
        Catch,
    }
}
