using UnityEngine;

namespace StaticLeakLab.Models
{
    /// <summary>
    /// HAZARD #2 — Static constructor + monotonically increasing counter.
    ///
    /// A static constructor runs once per AppDomain. With Domain Reload enabled that is
    /// "once per Play session"; with Domain Reload DISABLED it is "once per Editor
    /// session (or script recompile)". Anything set up here is therefore NOT re-run
    /// when you press Play again.
    ///
    /// Symptom: entity ids keep growing across sessions (Enemy_0 ... Enemy_7 in session 1,
    /// Enemy_8 ... Enemy_15 in session 2, ...). Code that assumes ids start at 0
    /// (array indexing, save files, network ids) silently breaks.
    /// </summary>
    public static class IdGenerator
    {
        private static int s_NextId;
        private static readonly float s_CreatedAtRealtime;

        static IdGenerator()
        {
            s_NextId = 0;
            s_CreatedAtRealtime = Time.realtimeSinceStartup;
            Debug.Log($"[IdGenerator] static constructor executed at t={s_CreatedAtRealtime:F1}s");
        }

        public static int Next() => s_NextId++;

        public static int Peek => s_NextId;

        public static float CreatedAtRealtime => s_CreatedAtRealtime;
    }
}
