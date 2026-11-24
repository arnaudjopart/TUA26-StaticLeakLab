using UnityEngine;

namespace StaticLeakLab.Views
{
    /// <summary>
    /// Shows the score in the top-right corner and "pulses" its transform when it changes.
    ///
    /// The presenter subscribes OnScoreChanged to a static event and never unsubscribes.
    /// After the Play session ends this component is destroyed, but the static event still
    /// holds a delegate to it. In the next session, OnScoreChanged runs on the destroyed
    /// instance and the first engine call (transform) throws MissingReferenceException.
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        private int m_Score;
        private float m_PulseUntil;
        private GUIStyle m_Style;

        public void OnScoreChanged(int score)
        {
            m_Score = score;
            m_PulseUntil = Time.time + 0.15f;
            transform.localScale = Vector3.one * 1.2f; // engine call: throws on a destroyed instance
        }

        private void Update()
        {
            if (Time.time > m_PulseUntil)
                transform.localScale = Vector3.one;
        }

        private void OnGUI()
        {
            if (m_Style == null)
                m_Style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };

            var scale = Mathf.Max(1f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            var pulsing = Time.time <= m_PulseUntil;
            m_Style.normal.textColor = pulsing ? Color.yellow : Color.white;
            GUI.Label(new Rect(Screen.width / scale - 312, 12, 300, 40), $"Score {m_Score}", m_Style);
        }
    }
}
