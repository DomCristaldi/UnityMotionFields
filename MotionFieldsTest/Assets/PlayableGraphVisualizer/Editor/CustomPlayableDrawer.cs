using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using UnityEngine.Experimental.Director;

// Example of how to implement a custom drawing method for a specific playable type
// as part of the Graph Visualizer tool.
[CustomPlayableDrawer(typeof(AnimationClipPlayable))]
public class AnimationClipPlayableDrawer
{
    public static void OnGUI(Rect position, Playable p)
    {
        const string msg = "custom\nfor clips";
        var nodeStyle = new GUIStyle("flow node 6");

        Vector2 sizeNeeded = nodeStyle.CalcSize(new GUIContent(msg));
        if (sizeNeeded.x < position.width && sizeNeeded.y < position.height)
            GUI.Label(position, msg, nodeStyle);
        else
            GUI.Label(position, "", nodeStyle);
    }
}

// Defines a new type of attribute for identifying custom node visualization nodes
// Every class marked as a playable drawer must implement the OnGUI(Rect, Playable) method
public class CustomPlayableDrawer : Attribute
{
    public Type m_Type;

    public CustomPlayableDrawer(Type type)
    {
        m_Type = type;
    }
}
