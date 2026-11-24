using System;
using StaticLeakLab.Models;
using StaticLeakLab.Systems;
using StaticLeakLab.Views;
using UnityEngine;

namespace StaticLeakLab.Presenters
{
    /// <summary>
    /// Entry point of the demo scene. Wires the hazards together exactly the way
    /// "typical" game code does, assuming every Play session starts from a clean slate.
    ///
    /// Each risky step is wrapped in try/catch so that one hazard blowing up does not hide
    /// the others: watch the Console for the exceptions from session 2 onward.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        // HAZARD #10 (bis) — a static counter on a MonoBehaviour, used here as a witness:
        // with Domain Reload enabled it is ALWAYS 1; with it disabled it counts sessions.
        private static int s_SessionCount;

        public static int SessionCount => s_SessionCount;

        [SerializeField] private ScoreView m_ScoreView;
        [SerializeField] private float m_Spacing = 1.8f;

        // Per-session snapshot (instance fields ARE reset: the scene is reloaded).
        public int ScoreAtStart { get; private set; }
        public int FirstIdThisSession { get; private set; }
        public int FirstRollThisSession { get; private set; }
        public int TexturesAtStart { get; private set; }
        public int LogsCountedAtStart { get; private set; }
        public int KillsThisSession { get; private set; }
        public int BootstrapErrors { get; private set; }

        private void Awake()
        {
            s_SessionCount++;

            ScoreAtStart = ScoreModel.TotalScore;
            FirstIdThisSession = IdGenerator.Peek;
            TexturesAtStart = TextureCache.Count;
            LogsCountedAtStart = LogCounter.LogsCounted;
            FirstRollThisSession = GameRandom.Range(0, 1000);

            Debug.Log($"[Bootstrap] Session #{s_SessionCount} — score at start {ScoreAtStart}, " +
                      $"first id {FirstIdThisSession}, first roll {FirstRollThisSession}");

            // HAZARD #6: throws from session 2 when Domain Reload is disabled.
            Try("ServiceLocator.Register", () => ServiceLocator.Register(this));

            // HAZARD #9: throws MissingReferenceException from session 2.
            Try("PersistentRoot.Root", () => transform.SetParent(PersistentRoot.Root, true));

            // HAZARD #1: method-group subscription to a static event, never removed.
            if (m_ScoreView != null)
                ScoreModel.ScoreChanged += m_ScoreView.OnScoreChanged;

            // HAZARD #4: lambda capturing "this" in a generic static bus, never removed.
            // In session N, N lambdas run for every kill => score gained N times faster.
            EventBus<EnemyKilled>.Subscribe(e =>
            {
                KillsThisSession++;
                ScoreModel.Add(e.Points);
            });

            EventBus<WaveStarted>.Subscribe(w => Debug.Log($"[Bootstrap] Wave {w.WaveIndex} started"));
        }

        private void Start()
        {
            SpawnEnemies();
            EventBus<WaveStarted>.Publish(new WaveStarted(1));
        }

        private void SpawnEnemies()
        {
            var count = SafeStatics.MaxEnemies;
            var startX = -(count - 1) * m_Spacing * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(startX + i * m_Spacing, 0.5f + GameRandom.Value(), 0f);
                go.AddComponent<EnemyView>();
                go.AddComponent<EnemyPresenter>();
            }
        }

        private void Try(string step, Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                BootstrapErrors++;
                Debug.LogError($"[Bootstrap] {step} failed: {exception.GetType().Name}: {exception.Message}");
            }
        }
    }
}
