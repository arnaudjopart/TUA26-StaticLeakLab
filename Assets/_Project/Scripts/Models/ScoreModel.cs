using System;
using Unity.Scripting.LifecycleManagement;

namespace StaticLeakLab.Models
{
    /// <summary>
    /// HAZARD #1 — Static value state + static auto-property + static event.
    ///
    /// With Domain Reload enabled, every Play session starts with a fresh AppDomain,
    /// so TotalScore == 0, HighScore == 0 and ScoreChanged == null.
    /// With Domain Reload DISABLED, all three survive from the previous Play session:
    ///   - TotalScore keeps accumulating (session 2 starts where session 1 ended).
    ///   - HighScore (an auto-property: the backing field is compiler-generated and
    ///     easy to miss in a code review) is never reset.
    ///   - ScoreChanged keeps every delegate ever subscribed, including delegates whose
    ///     target is a MonoBehaviour destroyed when the previous session ended.
    ///     Raising the event then throws MissingReferenceException and keeps the
    ///     destroyed objects' managed shells alive (memory leak).
    /// </summary>
    public static partial class ScoreModel
    {
        public static int TotalScore;

        public static int HighScore { get; private set; }

        public static event Action<int> ScoreChanged;

        public static int SubscriberCount =>
            ScoreChanged == null ? 0 : ScoreChanged.GetInvocationList().Length;

        public static void Add(int points)
        {
            TotalScore += points;
            if (TotalScore > HighScore)
                HighScore = TotalScore;

            ScoreChanged?.Invoke(TotalScore);
        }
    }
}
