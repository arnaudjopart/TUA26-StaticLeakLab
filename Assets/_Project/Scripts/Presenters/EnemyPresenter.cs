using StaticLeakLab.Models;
using StaticLeakLab.Systems;
using StaticLeakLab.Views;
using UnityEngine;

namespace StaticLeakLab.Presenters
{
    /// <summary>
    /// Drives one enemy: it lives for a random time, "dies" (publishes EnemyKilled),
    /// then respawns after a short delay.
    ///
    /// HAZARD #10 — Static field declared on a MonoBehaviour.
    /// s_TotalSpawned reads like per-scene bookkeeping, but it belongs to the type,
    /// not to the scene: with Domain Reload disabled it keeps counting across sessions.
    /// </summary>
    [RequireComponent(typeof(EnemyView))]
    public class EnemyPresenter : MonoBehaviour
    {
        private static int s_TotalSpawned;

        public static int TotalSpawned => s_TotalSpawned;

        [SerializeField] private int m_Points = 10;

        private EnemyView m_View;
        private int m_Id;
        private float m_NextEventTime;
        private bool m_IsAlive;

        public int Id => m_Id;

        private void Awake()
        {
            m_View = GetComponent<EnemyView>();
            m_Id = IdGenerator.Next();
            name = $"Enemy_{m_Id}";
            s_TotalSpawned++;

            EntityRegistry.Register(transform);
            m_View.SetTexture(TextureCache.GetOrCreate(m_Id));
            Spawn();
        }

        private void Spawn()
        {
            m_IsAlive = true;
            m_View.SetVisible(true);
            m_NextEventTime = Time.time + 1f + GameRandom.Value() * 2f;
        }

        private void Die()
        {
            m_IsAlive = false;
            m_View.SetVisible(false);
            m_NextEventTime = Time.time + SafeStatics.RespawnDelay;

            Debug.Log($"[Enemy] {name} defeated (+{m_Points})");
            EventBus<EnemyKilled>.Publish(new EnemyKilled(m_Id, m_Points));
        }

        private void Update()
        {
            if (Time.time < m_NextEventTime) return;

            if (m_IsAlive) Die();
            else Spawn();
        }
    }
}
