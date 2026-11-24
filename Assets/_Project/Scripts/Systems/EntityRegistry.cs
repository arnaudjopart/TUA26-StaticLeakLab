using System.Collections.Generic;
using UnityEngine;

namespace StaticLeakLab.Systems
{
    /// <summary>
    /// HAZARD #5 — Static collection holding references to UnityEngine.Objects.
    ///
    /// Entities register on spawn but nobody clears the list when Play mode ends.
    /// When the scene is unloaded, the native C++ objects are destroyed, but the
    /// managed C# wrappers are still referenced from this static list, so the GC can
    /// never collect them. They become "fake null" ghosts: (obj == null) is true,
    /// yet they are still in memory and still counted.
    ///
    /// The dashboard shows "Registered" (keeps growing every session) vs. "Alive"
    /// (what is actually in the scene).
    /// </summary>
    public static class EntityRegistry
    {
        private static readonly List<Transform> s_Entities = new List<Transform>();

        public static int RegisteredCount => s_Entities.Count;

        public static int AliveCount
        {
            get
            {
                var alive = 0;
                foreach (var t in s_Entities)
                    if (t != null) alive++;
                return alive;
            }
        }

        public static int GhostCount => RegisteredCount - AliveCount;

        public static void Register(Transform entity) => s_Entities.Add(entity);

        // Intentionally present but never called on scene teardown:
        // OnDestroy order during Play-mode exit is not something to rely on.
        public static void Unregister(Transform entity) => s_Entities.Remove(entity);

        public static IReadOnlyList<Transform> All => s_Entities;
    }
}
