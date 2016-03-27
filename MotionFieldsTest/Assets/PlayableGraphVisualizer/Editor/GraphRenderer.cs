using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;


public class GraphRenderer : ITreeRenderer
{
    private static Color k_EdgeColorMin = new Color(1.0f, 1.0f, 1.0f, 0.1f);
    private static Color k_EdgeColorMax = Color.white;
    private static Color k_LegendBackground = new Color(0, 0, 0, 0.1f);

    private const float k_BorderSize = 30;
    private const float k_LegendFixedOverheadWidth = 30;
    private const float k_DefaultMaximumNormalizedNodeSize = 0.8f;
    private const float k_DefaultMaximumNodeSizeInPixels = 100.0f;
    private const float k_DefaultAspectRatio = 1;

    private bool m_UseCustomDrawingMethods = false;
    private GUIStyle m_NodeLabelStyle;
    private GUIStyle m_LegendLabelStyle;
    private GUIStyle m_SubTitleStyle;

    private Dictionary<Type, NodeTypeLegend> m_LegendForType = new Dictionary<Type, NodeTypeLegend>();
    private Dictionary<Type, MethodInfo> m_CustomDrawingMethodForType = new Dictionary<Type, MethodInfo>();

    private static string[] k_PredefinedNodeStyles = new string[]
    {
        "flow node 1", // blue
        "flow node 4", // yellow
        "flow node 6", // red
        "flow node 3", // green
        "flow node 2"  // cyan
    };

    private const string k_DefaultNodeStyle = "flow node 0";

    private Texture2D m_ColorBar = null;

    private struct NodeTypeLegend
    {
        public GUIStyle style;
        public string label;
    }

    public GraphRenderer()
    {
        InitializeStyles();
        FindCustomDrawingFunctions();
    }

    public void Draw(ILayoutAlgorithm layout, Rect drawingArea)
    {
        NodeConstraints defaults;
        defaults.maximumNormalizedNodeSize = k_DefaultMaximumNormalizedNodeSize;
        defaults.maximumNodeSizeInPixels = k_DefaultMaximumNodeSizeInPixels;
        defaults.aspectRatio = k_DefaultAspectRatio;
        Draw(layout, drawingArea, defaults);
    }

    public void Draw(ILayoutAlgorithm layout, Rect totalDrawingArea, NodeConstraints nodeConstraints)
    {
        PrepareLegend(layout.vertices);

        var drawingArea = new Rect(totalDrawingArea);
        var legendArea = new Rect(totalDrawingArea);
        legendArea.width = EstimateLegendWidth() + k_BorderSize * 2;
        legendArea.x = drawingArea.xMax - legendArea.width;
        drawingArea.width -=  legendArea.width;

        DrawGraph(layout, drawingArea, nodeConstraints);
        DrawLegend(legendArea);
    }

    private void InitializeStyles()
    {
        m_NodeLabelStyle = new GUIStyle(GUI.skin.label);
        m_NodeLabelStyle.normal.textColor = Color.black;
        m_NodeLabelStyle.alignment = TextAnchor.MiddleCenter;

        m_LegendLabelStyle = new GUIStyle(GUI.skin.label);
        m_LegendLabelStyle.margin.top = 0;
        m_LegendLabelStyle.alignment = TextAnchor.UpperLeft;

        m_SubTitleStyle = EditorStyles.boldLabel;
    }

    private void PrepareLegend(Vertex[] vertices)
    {
        foreach (var v in vertices)
        {
            var nodeType = v.payload.GetType();

            if (m_LegendForType.ContainsKey(nodeType))
                continue;

            int nextStyleIndex = m_LegendForType.Count;

            NodeTypeLegend legend;
            char[] separators = {'.'};
            string[] typeElements = v.payload.GetType().ToString().Split(separators);
            legend.label = typeElements[typeElements.Length - 1];

            if (nextStyleIndex < k_PredefinedNodeStyles.Length)
                legend.style = new GUIStyle(k_PredefinedNodeStyles[nextStyleIndex]);
            else
                legend.style  = new GUIStyle(k_DefaultNodeStyle);

            m_LegendForType[nodeType] = legend;
        }
    }

    private void FindCustomDrawingFunctions()
    {
        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes(typeof(CustomPlayableDrawer), true);
                if (attributes.Length == 1)
                {
                    CustomPlayableDrawer drawer = attributes[0] as CustomPlayableDrawer;
                    m_CustomDrawingMethodForType[drawer.m_Type] = type.GetMethod("OnGUI");
                }
            }
        }
    }

    private float EstimateLegendWidth()
    {
        float legendWidth = 0;
        foreach (var legend in m_LegendForType.Values)
        {
            legendWidth = Mathf.Max(legendWidth, GUI.skin.label.CalcSize(new GUIContent(legend.label)).x);
        }
        legendWidth += k_LegendFixedOverheadWidth;
        return legendWidth;
    }

    private void DrawLegend(Rect legendArea)
    {
        EditorGUI.DrawRect(legendArea, k_LegendBackground);

        // Add a border around legend area
        legendArea.x += k_BorderSize;
        legendArea.width -= k_BorderSize * 2;
        legendArea.y += k_BorderSize;
        legendArea.height -= k_BorderSize * 2;

        GUILayout.BeginArea(legendArea);
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.Label("Legend", m_SubTitleStyle);
        foreach (var pair in m_LegendForType)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.Height(20));
            Rect legendIconRect = GUILayoutUtility.GetRect(1, 1, GUILayout.Width(15), GUILayout.Height(20));

            if (m_UseCustomDrawingMethods && m_CustomDrawingMethodForType.ContainsKey(pair.Key))
            {
                m_CustomDrawingMethodForType[pair.Key].Invoke(null, new object[] {legendIconRect, null});
            }
            else
            {
                GUI.Label(legendIconRect, "", pair.Value.style);
            }
            GUILayout.Label(pair.Value.label, m_LegendLabelStyle);

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        GUILayout.Label("Edge weight", m_SubTitleStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label("0");
        GUILayout.FlexibleSpace();
        GUILayout.Label("1");
        GUILayout.EndHorizontal();

        DrawEdgeWeightColorBar(legendArea.width);

        GUILayout.Space(20);

        m_UseCustomDrawingMethods = GUILayout.Toggle(m_UseCustomDrawingMethods, "Use custom drawing");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawEdgeWeightColorBar(float width)
    {
        const int k_NbLevels = 64;

        if (m_ColorBar == null)
        {
            m_ColorBar = new Texture2D(k_NbLevels, 1);
            m_ColorBar.wrapMode = TextureWrapMode.Clamp;

            var cols = m_ColorBar.GetPixels();
            for (int x = 0; x < k_NbLevels; x++)
            {
                Color c = Color.Lerp(k_EdgeColorMin, k_EdgeColorMax, (float)x / (float)k_NbLevels);
                cols[x] = c;
            }

            m_ColorBar.SetPixels(cols);
            m_ColorBar.Apply(false);
        }

        const int k_ColorbarHeight = 20;
        GUI.DrawTexture(GUILayoutUtility.GetRect(width, k_ColorbarHeight), m_ColorBar);
    }

    private void DrawGraph(ILayoutAlgorithm layout, Rect drawingArea, NodeConstraints nodeConstraints)
    {
        // add border, except on right-hand side where the legend will provide necessary padding
        drawingArea = new Rect(drawingArea.x + k_BorderSize,
                drawingArea.y + k_BorderSize,
                drawingArea.width - k_BorderSize,
                drawingArea.height - k_BorderSize * 2);

        var b = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var c in layout.vertices)
        {
            b.Encapsulate(new Vector3(c.position.x, c.position.y, 0.0f));
        }

        // Increase b by maximum node size (since b is measured between node centers)
        b.Expand(new Vector3(nodeConstraints.maximumNormalizedNodeSize, nodeConstraints.maximumNormalizedNodeSize, 0));

        var scale = new Vector2(drawingArea.width / b.size.x, drawingArea.height / b.size.y);
        var offset = new Vector2(-b.min.x, -b.min.y);

        Vector2 nodeSize = ComputeNodeSize(scale, nodeConstraints);

        GUI.BeginGroup(drawingArea);

        foreach (var e in layout.edges)
        {
            Vector2 v0 = ScaleVertex(layout.vertices[e.source].position, offset, scale);
            Vector2 v1 = ScaleVertex(layout.vertices[e.destination].position, offset, scale);
            DrawEdge(v0, v1, layout.vertices[e.source].propagatedWeight);
        }

        int index = 0;
        foreach (var v in layout.vertices)
        {
            DrawNode(v, ScaleVertex(v.position, offset, scale) - nodeSize / 2, nodeSize, index.ToString());
            index++;
        }

        GUI.EndGroup();
    }

    // Apply node constraints to node size
    private Vector2 ComputeNodeSize(Vector2 scale, NodeConstraints nodeConstraints)
    {
        var nodeSize = new Vector2(nodeConstraints.maximumNormalizedNodeSize * scale.x,
                nodeConstraints.maximumNormalizedNodeSize * scale.y);

        // Adjust aspect ratio after scaling
        float currentAspectRatio = nodeSize.x / nodeSize.y;

        if (currentAspectRatio > nodeConstraints.aspectRatio)
        {
            // Shrink x dimension
            nodeSize.x = nodeSize.y * nodeConstraints.aspectRatio;
        }
        else
        {
            // Shrink y dimension
            nodeSize.y = nodeSize.x / nodeConstraints.aspectRatio;
        }

        // If node size is still too big, scale down
        if (nodeSize.x > nodeConstraints.maximumNodeSizeInPixels)
        {
            nodeSize *= nodeConstraints.maximumNodeSizeInPixels / nodeSize.x;
        }

        if (nodeSize.y > nodeConstraints.maximumNodeSizeInPixels)
        {
            nodeSize *= nodeConstraints.maximumNodeSizeInPixels / nodeSize.y;
        }
        return nodeSize;
    }

    // Convert vertex position from normalized layout to render rect
    private Vector2 ScaleVertex(Vector2 v, Vector2 offset, Vector2 scaleFactor)
    {
        return new Vector2((v.x + offset.x) * scaleFactor.x, (v.y + offset.y) * scaleFactor.y);
    }

    private void DrawNode(Vertex v, Vector2 nodeCenter, Vector2 nodeSize, string name)
    {
        var nodeType = v.payload.GetType();
        var nodeRect = new Rect(nodeCenter.x, nodeCenter.y, nodeSize.x, nodeSize.y);

        if (m_UseCustomDrawingMethods && m_CustomDrawingMethodForType.ContainsKey(nodeType))
        {
            m_CustomDrawingMethodForType[nodeType].Invoke(null, new object[] {nodeRect, v.payload});
        }
        else
        {
            GUI.Label(nodeRect, "", m_LegendForType[nodeType].style);
        }
    }

    // Compute the tangents for the graph edges. Assumes that graph is drawn from left to right
    private void GetTangents(Vector2 start, Vector2 end, out Vector3[] points, out Vector3[] tangents)
    {
        points = new Vector3[] {start, end};
        tangents = new Vector3[2];

        // Heuristics to define the length of the tangents and tweak the look of the bezier curves.
        const float k_MinTangent = 30;
        const float k_Weight = 0.5f;
        float cleverness = Mathf.Clamp01(((start - end).magnitude - 10) / 50);
        tangents[0] = start + new Vector2((end.x - start.x) * k_Weight + k_MinTangent, 0) * cleverness;
        tangents[1] = end + new Vector2((end.x - start.x) * -k_Weight - k_MinTangent,  0) * cleverness;
    }

    private void DrawEdge(Vector2 a, Vector2 b, float weight)
    {
        Vector3[] points, tangents;
        GetTangents(a, b, out points, out tangents);
        Handles.DrawBezier(points[0], points[1], tangents[0], tangents[1],
            Color.Lerp(k_EdgeColorMin, k_EdgeColorMax, weight), null, 5f);
    }
}
