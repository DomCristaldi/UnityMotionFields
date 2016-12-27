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
        //const string msg = "custom\nfor clips";

        string msg = "invalid name";

        AnimationClipPlayable castedP;
        if (p.IsValid()) {
            castedP = p.CastTo<AnimationClipPlayable>();
            if (castedP.IsValid()) {
                msg = castedP.clip.name + "\n" + castedP.time;
                Debug.Log(castedP.clip.name);
            }
            else {
                Debug.Log("BAD DATA");
            }
        }
        else { Debug.Log("core playable is bad"); }

        //var nodeStyle = new GUIStyle("flow node 6");
        /*
        if (p.IsValid()) {

            GUI.Window(EditorGUIUtility.GetControlID(FocusType.Passive),
                       position,
                       DoNodeWindow,
                       p.time.ToString(),
                       nodeStyle);
        }

        else {
            */
        GUI.Label(position, msg);//, nodeStyle);
        //}

        /*
        Vector2 sizeNeeded = nodeStyle.CalcSize(new GUIContent(msg));
        if (sizeNeeded.x < position.width && sizeNeeded.y < position.height)
            GUI.Label(position, msg, nodeStyle);
        else
            GUI.Label(position, "", nodeStyle);
        */
    }

    private static void DoNodeWindow(int windowID)
    {
        //GUI.Label(GUI.window)

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



//[CustomPlayableDrawer(typeof(AnimationMotionFields.BlendSwitcherPlayable))]
//public class BlendSwitcherPlayable_Drawer
//{
//    public static void OnGUI(Rect position, Playable p)
//    {

//        if (!p.IsValid()) {
//            DisplayBrokenNodeInfo();
//            return;
//        }

//        AnimationMotionFields.BlendSwitcherPlayable blendSwitcher;
//        blendSwitcher = p.CastTo<AnimationMotionFields.BlendSwitcherPlayable>();


//        string msg = "Invalid Playable";

//        GUIStyle nodeStyle = new GUIStyle("flow node 1");

//        Vector2 sizeNeeded = nodeStyle.CalcSize(new GUIContent(msg));
//        if (sizeNeeded.x < position.width
//         && sizeNeeded.y < position.height) {
//            GUI.Label(position, msg, nodeStyle);
//        }
//        else {
//            GUI.Label(position, "", nodeStyle);
//        }
//    }

//    private static void DisplayNodeInfo()
//    {

//    }

//    private static void DisplayBrokenNodeInfo()
//    {

//    }
//}
