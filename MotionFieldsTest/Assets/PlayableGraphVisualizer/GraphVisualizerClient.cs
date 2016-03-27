using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Director;

// Bridge between runtime and editor code: the graph created in runtime code can call GraphVisualizerClient.Show(...)
// and the EditorWindow will register itself with the client to display any available graph.
public class GraphVisualizerClient
{
    public delegate void UpdateGraphDelegate(Playable p, string title);
    public UpdateGraphDelegate updateGraph;

    private static GraphVisualizerClient s_Instance;

    public static GraphVisualizerClient instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = new GraphVisualizerClient();
            return s_Instance;
        }
    }

    public static void Show(Playable p, string title)
    {
        if (instance.updateGraph != null)
            instance.updateGraph(p, title);
    }
}
