using Unity.Scripting.LifecycleManagement;

namespace StaticLeakLab.Models
{
    /// <summary>
    /// HAZARD #3 — static readonly reference to a MUTABLE object.
    ///
    /// "readonly" only protects the reference, not the object's internal state.
    /// The seeded Random is meant to make every Play session deterministic
    /// (same spawn positions, same colors). With Domain Reload DISABLED the generator
    /// is never re-created, so session 2 continues the sequence of session 1:
    /// the "deterministic" level is different every time you press Play.
    ///
    /// Watch the "First roll" value on the dashboard: it is identical every session
    /// with Domain Reload on, and changes every session with Domain Reload off.
    /// </summary>
    public static class GameRandom
    {
        public const int Seed = 12345;
        
        private static System.Random s_Rng = new System.Random(Seed);

        public static int Range(int minInclusive, int maxExclusive) => s_Rng.Next(minInclusive, maxExclusive);

        public static float Value() => (float)s_Rng.NextDouble();
    }
}
