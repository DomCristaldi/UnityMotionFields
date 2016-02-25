using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AnimationMotionFields {


    public static class MotionFieldCreator {

        //public static void CreateMotionField(ref SO_MotionField motionField, )

        public static AnimationCurve[] FindAnimCurve(AnimationClip animClip) {

            if (animClip == null) { return new AnimationCurve[] { }; }//return an empty array if the supplied clip is null

            List<AnimationCurve> embeddedAnimCurves = new List<AnimationCurve>();

            EditorCurveBinding[] embeddedCurveBindings = AnimationUtility.GetCurveBindings(animClip);

            foreach (EditorCurveBinding binding in embeddedCurveBindings) {
                embeddedAnimCurves.Add(AnimationUtility.GetEditorCurve(animClip, binding));
            }

            return embeddedAnimCurves.ToArray();
        }

        public static AnimationCurve[] FindAnimCurves(AnimationClip[] animClips) {

            List<AnimationCurve> embeddedAnimCurves = new List<AnimationCurve>();

            foreach (AnimationClip clip in animClips) {
                //if (clip == null) { continue; }

                embeddedAnimCurves.AddRange(FindAnimCurve(clip));

                /*
                EditorCurveBinding[] embeddedCurveBindings = AnimationUtility.GetCurveBindings(clip);

                foreach (EditorCurveBinding binding in embeddedCurveBindings) {
                    embeddedAnimCurves.Add(AnimationUtility.GetEditorCurve(clip, binding));
                }
                */
            }

            return embeddedAnimCurves.ToArray();
        }

        /// <summary>
        /// Creates Animation Poses from a Supplied Animation Clip
        /// </summary>
        /// <param name="animClip"> Animation Clip to generate Poses from</param>
        /// <param name="sampleRate"> Frame Sample Rate (1 is every frame, 3 is skip every third, 5 is every fifth) </param>
        /// <returns></returns>
        public static /*AnimationClip[]*/void CreateAnimationPoses(AnimationClip animClip, int sampleRate) {
            Debug.Log(animClip.length * animClip.frameRate);
        }

    }
}