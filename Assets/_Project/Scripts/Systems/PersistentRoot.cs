using UnityEngine;

namespace StaticLeakLab.Systems
{
    /// <summary>
    /// HAZARD #9 — Lazy-initialization flag guarding a scene object.
    ///
    /// The classic "if (!s_Initialized) { create; s_Initialized = true; }" pattern.
    /// When Play mode ends, the DontDestroyOnLoad GameObject is destroyed, but
    /// s_Initialized stays true and s_Root still points to the destroyed object.
    /// In session 2 the guard skips the creation, and the first access to Root.transform
    /// throws MissingReferenceException: "The object of type 'GameObject' has been
    /// destroyed but you are still trying to access it."
    /// </summary>
    public static class PersistentRoot
    {
        private static bool s_Initialized;
        private static GameObject s_Root;

        public static bool IsInitialized => s_Initialized;

        public static bool IsRootAlive => s_Root != null;

        public static Transform Root
        {
            get
            {
                if (!s_Initialized)
                {
                    s_Root = new GameObject("[PersistentRoot]");
                    Object.DontDestroyOnLoad(s_Root);
                    s_Initialized = true;
                }

                return s_Root.transform; // throws in session 2 when Domain Reload is off
            }
        }
    }
}
