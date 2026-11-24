namespace StaticLeakLab.Models
{
    /// <summary>
    /// CONTROL GROUP — statics that are NOT a problem when Domain Reload is disabled.
    ///
    /// Use this file in class to discuss what the Auditor should (and should not) flag:
    ///   - const values are baked into the IL at compile time: nothing to reset.
    ///   - static readonly fields holding IMMUTABLE values (string, int, readonly struct)
    ///     are re-assigned to the same value anyway; their state cannot drift.
    ///   - pure static methods hold no state at all.
    /// Compare with GameRandom (static readonly but MUTABLE object) which IS a hazard.
    /// </summary>
    public static class SafeStatics
    {
        public const int MaxEnemies = 8;
        public const float RespawnDelay = 0.75f;

        public static readonly string GameTitle = "Static Leak Lab";

        public static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;
    }
}
