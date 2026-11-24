using System.Collections.Generic;
using StaticLeakLab.Models;
using StaticLeakLab.Systems;
using StaticLeakLab.Views;
using UnityEngine;

namespace StaticLeakLab.Presenters
{
    /// <summary>
    /// Reads every static hazard and pushes a human-readable report to the dashboard view.
    /// Values highlighted in red are the ones that differ from what a fresh domain
    /// (Domain Reload enabled) would show.
    /// </summary>
    public class LeakDashboardPresenter : MonoBehaviour
    {
        [SerializeField] private GameBootstrap m_Bootstrap;
        [SerializeField] private LeakDashboardView m_View;
        [SerializeField] private float m_RefreshInterval = 0.25f;

        private readonly List<DashboardRow> m_Rows = new List<DashboardRow>();
        private float m_NextRefresh;

        private void Update()
        {
            if (m_View == null || m_Bootstrap == null || Time.unscaledTime < m_NextRefresh) return;
            m_NextRefresh = Time.unscaledTime + m_RefreshInterval;

            m_View.SetTitle($"{SafeStatics.GameTitle} — Domain Reload: {DomainReloadLabel()}");
            m_View.SetRows(BuildRows());
        }

        private List<DashboardRow> BuildRows()
        {
            var b = m_Bootstrap;
            var sessions = GameBootstrap.SessionCount;
            var expectedEnemies = SafeStatics.MaxEnemies;
            var logsThisSession = LogCounter.LogsCounted - b.LogsCountedAtStart;

            m_Rows.Clear();

            m_Rows.Add(DashboardRow.Header("Session"));
            m_Rows.Add(new DashboardRow("#10 Static session counter (fresh = 1)", sessions.ToString(), sessions != 1));
            m_Rows.Add(new DashboardRow("Bootstrap errors this session", b.BootstrapErrors.ToString(), b.BootstrapErrors > 0));

            m_Rows.Add(DashboardRow.Header("Values"));
            m_Rows.Add(new DashboardRow("#1 Score at session start (fresh = 0)", b.ScoreAtStart.ToString(), b.ScoreAtStart != 0));
            m_Rows.Add(new DashboardRow("#1 High score (auto-property)", ScoreModel.HighScore.ToString()));
            m_Rows.Add(new DashboardRow("#2 First entity id (fresh = 0)", b.FirstIdThisSession.ToString(), b.FirstIdThisSession != 0));
            m_Rows.Add(new DashboardRow("#3 First roll, seed " + GameRandom.Seed + " (fresh = constant)", b.FirstRollThisSession.ToString()));
            m_Rows.Add(new DashboardRow("#10 Enemies spawned (fresh = " + expectedEnemies + ")", EnemyPresenter.TotalSpawned.ToString(), EnemyPresenter.TotalSpawned != expectedEnemies));

            m_Rows.Add(DashboardRow.Header("Subscribers"));
            m_Rows.Add(new DashboardRow("#1 ScoreModel.ScoreChanged (fresh = 1)", ScoreModel.SubscriberCount.ToString(), ScoreModel.SubscriberCount > 1));
            m_Rows.Add(new DashboardRow("#4 EventBus<EnemyKilled> (fresh = 1)", EventBus<EnemyKilled>.HandlerCount.ToString(), EventBus<EnemyKilled>.HandlerCount > 1));
            m_Rows.Add(new DashboardRow("#4 EventBus<WaveStarted> (fresh = 1)", EventBus<WaveStarted>.HandlerCount.ToString(), EventBus<WaveStarted>.HandlerCount > 1));
            m_Rows.Add(new DashboardRow("#8 logMessageReceived handlers (fresh = 1)", LogCounter.HandlerRegistrations.ToString(), LogCounter.HandlerRegistrations > 1));
            // Only this session's bootstrap lambda increments b.KillsThisSession, so it is the true kill count.
            m_Rows.Add(new DashboardRow("#8 Kill logs counted / emitted", $"{logsThisSession} / {b.KillsThisSession}",
                logsThisSession > b.KillsThisSession));

            m_Rows.Add(DashboardRow.Header("References & memory"));
            m_Rows.Add(new DashboardRow("#5 EntityRegistry registered / alive", $"{EntityRegistry.RegisteredCount} / {EntityRegistry.AliveCount}", EntityRegistry.GhostCount > 0));
            m_Rows.Add(new DashboardRow("#6 ServiceLocator entries", ServiceLocator.Count.ToString()));
            m_Rows.Add(new DashboardRow("#7 Cached textures (fresh = " + expectedEnemies + ")", TextureCache.Count.ToString(), TextureCache.Count > expectedEnemies));
            m_Rows.Add(new DashboardRow("#7 Texture memory held by statics", $"{TextureCache.ApproximateBytes / 1024f / 1024f:F2} MB", TextureCache.Count > expectedEnemies));
            m_Rows.Add(new DashboardRow("#9 PersistentRoot initialized / alive", $"{PersistentRoot.IsInitialized} / {PersistentRoot.IsRootAlive}", PersistentRoot.IsInitialized && !PersistentRoot.IsRootAlive));

            return m_Rows;
        }

        private static string DomainReloadLabel()
        {
#if UNITY_EDITOR
            var options = UnityEditor.EditorSettings.enterPlayModeOptions;
            var disabled = UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                           (options & UnityEditor.EnterPlayModeOptions.DisableDomainReload) != 0;
            return disabled ? "OFF" : "ON";
#else
            return "n/a (player)";
#endif
        }
    }
}
