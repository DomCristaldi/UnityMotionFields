using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class MotionFieldCreator {

    public static AnimationCurve[] FindAnimCurves(AnimationClip[] animClips) {

        List<AnimationCurve> embeddedAnimCurves = new List<AnimationCurve>();

        foreach (AnimationClip clip in animClips) {
            EditorCurveBinding[] embeddedCurveBindings = AnimationUtility.GetCurveBindings(clip);

            

            foreach (EditorCurveBinding binding in embeddedCurveBindings) {
                embeddedAnimCurves.Add(AnimationUtility.GetEditorCurve(clip, binding));
            }

        }

        return embeddedAnimCurves.ToArray();

        //return (new SO_MotionField())


    }


}
