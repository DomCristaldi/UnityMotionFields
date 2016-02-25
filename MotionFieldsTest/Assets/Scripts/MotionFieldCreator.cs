using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AnimationMotionFields {


    public static class MotionFieldCreator {


        //public static void CreateMotionField(ref SO_MotionField motionField, )

        public static AnimationCurve[] FindAnimCurves(AnimationClip animClip) {

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
                embeddedAnimCurves.AddRange(FindAnimCurves(clip));
            }

            return embeddedAnimCurves.ToArray();
        }

        /// <summary>
        /// Creates Animation Poses from a Supplied Animation Clip
        /// </summary>
        /// <param name="animClip"> Animation Clip to generate Poses from</param>
        /// <param name="sampleStepSize"> Grab every n frame (1 is every frame, 3 is every third, 5 is every fifth) </param>
        /// <returns></returns>
        public static /*AnimationClip[]*/void CreateAnimationPoses(AnimationClip animClip, int sampleStepSize) {

            EditorCurveBinding[] embeddedCurveBindings = AnimationUtility.GetCurveBindings(animClip);//contains both curves and the path in the anim clip

            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFramePointer = 0.0f;

            Debug.LogFormat("Frame Step: 1 / Rate {0} = {1}", animClip.frameRate, frameStep);


            int frameCount = 0;
            /*
            for (int curFrame = 0; curFrame <= (animClip.length * animClip.frameRate) - frameStep; ++curFrame) {
                frameCount++;
            }

            Debug.LogFormat("Frame Count: {0}", frameCount);
            */
            
            //MOVE ACROSS ANIMATOIN CLIP FRAME BY FRAME
            while (currentFramePointer <= (animClip.length * animClip.frameRate) - frameStep) {

                //CREATE A POSE AS AN ANIMATION CLIP
                AnimationClip generatedAnimPose = new AnimationClip();





                //if (currentFramePointer + (frameStep * sampleStepSize) <= (animClip.length * animClip.frameRate) - frameStep) {
                /*
                if (currentFramePointer + (frameStep * sampleStepSize) > (animClip.length * animClip.frameRate) - frameStep) {
                    break;
                }
                else {*/
                    frameCount++;

                    currentFramePointer += frameStep * sampleStepSize;
                    /*
                }
                */
            }
            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            
            //Debug.Log(animClip.length * animClip.frameRate);

            List<AnimationClip> generatedPoses = new List<AnimationClip>();

            AnimationCurve[] extractedAnimCurves = FindAnimCurves(animClip);
            
            for (int i = 0; i < extractedAnimCurves.Length; ++i) {

                 
            }
            
        }


    }
}