using UnityEngine;

namespace StaticLeakLab.Views
{
    /// <summary>Visual side of an enemy: texture, visibility and a small bobbing motion.</summary>
    [RequireComponent(typeof(Renderer))]
    public class EnemyView : MonoBehaviour
    {
        private Renderer m_Renderer;
        private Vector3 m_BasePosition;
        private float m_Phase;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            m_BasePosition = transform.position;
            m_Phase = transform.position.x;
        }

        public void SetTexture(Texture2D texture) => m_Renderer.material.mainTexture = texture;

        public void SetVisible(bool visible) => m_Renderer.enabled = visible;

        private void Update()
        {
            transform.position = m_BasePosition + Vector3.up * (Mathf.Sin(Time.time * 2f + m_Phase) * 0.25f);
            transform.Rotate(0f, 45f * Time.deltaTime, 0f);
        }
    }
}
