using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace StaticLeakLab.Models
{
    /// <summary>
    /// HAZARD #4 — Static fields inside a GENERIC type.
    ///
    /// Every closed generic type (EventBus&lt;EnemyKilled&gt;, EventBus&lt;WaveStarted&gt;, ...)
    /// gets its OWN copy of s_Handlers. Those copies only exist once the type is used,
    /// which makes them hard to find by reading code and easy to forget when
    /// writing reset logic by hand.
    ///
    /// Subscribers that never unsubscribe (typical lambdas capturing "this") accumulate
    /// across Play sessions when Domain Reload is disabled.
    /// </summary>
    public static class EventBus<T>
    {
        private static readonly List<Action<T>> s_Handlers = new List<Action<T>>();

        public static int HandlerCount => s_Handlers.Count;

        public static void Subscribe(Action<T> handler) => s_Handlers.Add(handler);

        public static void Unsubscribe(Action<T> handler) => s_Handlers.Remove(handler);

        public static void Publish(T message)
        {
            // Iterate over a copy: a handler may throw or unsubscribe.
            // One failing handler must not prevent the others from running.
            foreach (var handler in s_Handlers.ToArray())
            {
                try
                {
                    handler(message);
                }
                catch (Exception exception)
                {
                    UnityEngine.Debug.LogException(exception);
                }
            }
        }
    }

    public readonly struct EnemyKilled
    {
        public readonly int EnemyId;
        public readonly int Points;

        public EnemyKilled(int enemyId, int points)
        {
            EnemyId = enemyId;
            Points = points;
        }
    }

    public readonly struct WaveStarted
    {
        public readonly int WaveIndex;

        public WaveStarted(int waveIndex) => WaveIndex = waveIndex;
    }
}
