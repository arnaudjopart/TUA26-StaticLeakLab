using System.Collections.Generic;
using UnityEngine;

namespace StaticLeakLab.Views
{
    /// <summary>
    /// Passive IMGUI overlay listing the state of every static hazard.
    /// It knows nothing about the Models or Systems: the presenter pushes rows into it.
    /// </summary>
    public class LeakDashboardView : MonoBehaviour
    {
        private const float k_Width = 680f;

        private readonly List<DashboardRow> m_Rows = new List<DashboardRow>();
        private string m_Title = "Static Leak Lab";
        private GUIStyle m_LabelStyle;
        private GUIStyle m_ValueStyle;
        private GUIStyle m_AlarmStyle;
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_BoxStyle;

        public void SetTitle(string title) => m_Title = title;

        public void SetRows(IEnumerable<DashboardRow> rows)
        {
            m_Rows.Clear();
            m_Rows.AddRange(rows);
        }

        private void EnsureStyles()
        {
            if (m_LabelStyle != null) return;

            m_BoxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 10, 10) };
            m_LabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = false };
            m_ValueStyle = new GUIStyle(m_LabelStyle) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
            m_AlarmStyle = new GUIStyle(m_ValueStyle);
            m_AlarmStyle.normal.textColor = new Color(1f, 0.45f, 0.35f);
            m_HeaderStyle = new GUIStyle(m_LabelStyle) { fontSize = 15, fontStyle = FontStyle.Bold };
            m_HeaderStyle.normal.textColor = new Color(0.55f, 0.8f, 1f);
        }

        private void OnGUI()
        {
            EnsureStyles();

            // Keep the overlay readable on high-resolution Game views (designed for 720p).
            var scale = Mathf.Max(1f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUILayout.BeginArea(new Rect(12, 12, k_Width, Screen.height / scale - 24));
            GUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label(m_Title, m_HeaderStyle);

            foreach (var row in m_Rows)
            {
                if (row.IsHeader)
                {
                    GUILayout.Space(6);
                    GUILayout.Label(row.Label, m_HeaderStyle);
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(row.Label, m_LabelStyle);
                GUILayout.Label(row.Value, row.IsAlarm ? m_AlarmStyle : m_ValueStyle, GUILayout.Width(130));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
