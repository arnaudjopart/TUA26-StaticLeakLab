using UnityEngine;

namespace StaticLeakLab.Systems
{
    /// <summary>
    /// HAZARD #8 — Static handler subscribed to an ENGINE event on every Play session.
    ///
    /// [RuntimeInitializeOnLoadMethod] still runs on every Play session, even with
    /// Domain Reload disabled. Application.logMessageReceived is an engine-side event that
    /// outlives the session, so each press of Play adds ONE MORE copy of OnLog:
    ///   session 1 -> each log counted once
    ///   session 2 -> each log counted twice
    ///   session N -> each log counted N times
    /// The "Logs counted" vs "Logs emitted" lines on the dashboard drift apart.
    /// </summary>
    public static class LogCounter
    {
        public static int LogsCounted;
        public static int HandlerRegistrations;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Application.logMessageReceived += OnLog;
            HandlerRegistrations++;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (condition.StartsWith("[Enemy]"))
                LogsCounted++;
        }
    }
}
