using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Director;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GraphVisualizer : EditorWindow
{
    GraphRenderer m_Renderer;
    ReingoldTilford m_Layout;

    Dictionary<Playable, string> m_Graphs = new Dictionary<Playable, string>();
    Playable m_CurrentGraph = null;

    const float k_ToolbarHeight = 17f;

    [MenuItem("Window/Graph Visualizer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<GraphVisualizer>();
    }

    private GraphVisualizer()
    {
        GraphVisualizerClient.instance.updateGraph = OnUpdateGraph;
        EditorApplication.playmodeStateChanged = OnPlayModeChanged;
    }

    public void OnDestroy()
    {
        m_Graphs.Clear();
    }

    public void OnPlayModeChanged()
    {
        if (!EditorApplication.isPaused && !EditorApplication.isPlaying)
        {
            m_CurrentGraph = null;
            m_Graphs.Clear();
            Repaint();
        }
    }

    public void OnUpdateGraph(Playable p, string title)
    {
        if (!m_Graphs.ContainsKey(p))
        {
            m_Graphs.Add(p, title);
            m_CurrentGraph = p;
        }
        // Update titles if necessary
        else if (m_Graphs.ContainsKey(p) && m_Graphs[p] != title)
        {
            m_Graphs[p] = title;
        }

        if (m_CurrentGraph == p)
        {
            UpdateLayout(p);
        }
    }

    public void UpdateLayout(Playable graph)
    {
        if (m_Layout == null)
            m_Layout = new ReingoldTilford();

        m_Layout.Layout(graph);
        Repaint();
    }

    private void DoToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(position.width));
        GUIStyle onStyle = "ToolbarButton";
        const string graphTitle = "Graph: ";
        if (m_Graphs.Count == 1)
        {
            GUILayout.Label(graphTitle + m_Graphs[m_CurrentGraph]);
        }
        if (m_Graphs.Count > 1)
        {
            var options = m_Graphs.Values.ToArray();
            var playables = m_Graphs.Keys.ToArray();

            // Can buttons be used to select the multiple graphs, or do we need a drop down menu?
            float totalWidth = 0;
            foreach (var o in options)
            {
                totalWidth += onStyle.CalcSize(new GUIContent(o)).x;
            }

            if (totalWidth > position.width)
            {
                int initialIndex = Array.IndexOf(playables, m_CurrentGraph);
                int index = EditorGUILayout.Popup(graphTitle, initialIndex, options, GUILayout.Width(position.width));

                if (index !=  initialIndex)
                {
                    m_CurrentGraph = playables[index];
                    UpdateLayout(m_CurrentGraph);
                }
            }
            else
            {
                GUILayout.Label(graphTitle);
                for (int i = 0; i < m_Graphs.Count; i++)
                {
                    bool isCurrent = m_CurrentGraph == playables[i];
                    bool value = GUILayout.Toggle(isCurrent, options[i], onStyle);
                    if (value && value != isCurrent)
                    {
                        m_CurrentGraph = playables[i];
                        UpdateLayout(m_CurrentGraph);
                    }
                }
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        DoToolbar();

        if (m_CurrentGraph == null)
            return;

        if (m_Renderer == null)
            m_Renderer = new GraphRenderer();

        GUILayout.EndVertical();

        var graphRect = new Rect(0, k_ToolbarHeight, position.width, position.height - k_ToolbarHeight);
        if (m_Layout != null)
            m_Renderer.Draw(m_Layout, graphRect);
    }
}
