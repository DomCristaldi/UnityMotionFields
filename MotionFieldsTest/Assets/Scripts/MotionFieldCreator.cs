using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

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

        public static float[] EvaluateAnimCurvesAtTimestamp(AnimationCurve[] curves, float timestamp) {
            List<float> extractedFloats = new List<float>();


            //for (int i = 0; i < )

            return extractedFloats.ToArray();
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

            int operations = 0;

            //MOVE ACROSS ANIMATOIN CLIP FRAME BY FRAME
            while (currentFramePointer <= (animClip.length * animClip.frameRate) - frameStep) {

                //CREATE A POSE AS AN ANIMATION CLIP
                AnimationClip generatedAnimPose = new AnimationClip();


                
                //FOR EVERY CURVE INSIDE THE ANIMATION CLIP (navigate by Editor Curve Bindings
                for (int i = 0; i < embeddedCurveBindings.Length; ++i) {
                    /*
                    //create a 1 frame curve
                    AnimationCurve frameCurve = AnimationUtility.GetEditorCurve(animClip, embeddedCurveBindings[i]);
                    frameCurve = new AnimationCurve(new Keyframe(currentFramePointer, frameCurve.Evaluate(currentFramePointer)));

                    //assign the frame to the Animation Curve we are generating
                    AnimationUtility.SetEditorCurve(generatedAnimPose, embeddedCurveBindings[i], frameCurve);
                    */
                    ++operations;
                }
                


                //SaveAnimPose(generatedAnimPose, frameCount, animClip.name);

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

            Debug.LogFormat("Operations: {0}", operations);
            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            
            //Debug.Log(animClip.length * animClip.frameRate);
            /*
            List<AnimationClip> generatedPoses = new List<AnimationClip>();

            AnimationCurve[] extractedAnimCurves = FindAnimCurves(animClip);
            
            for (int i = 0; i < extractedAnimCurves.Length; ++i) {

                 
            }
            */
            
        }

        public static void SaveAnimPose(AnimationClip animClip, int frameNumber, string animName) {
            AssetDatabase.CreateAsset(animClip, string.Format("Assets/TestFolder/{0}_Frame{1}.anim", animName, frameNumber));
        }


//GET EVERY UNIQUE PATH FROM THE SUPPLIED ANIM CLIPS
        public static string[] GetUniquePaths(AnimationClip[] animClips) {
            //List<string> extractedUniquePaths = new List<string>();
            HashSet<string> extractedUniquePaths = new HashSet<string>();

            foreach (AnimationClip clip in animClips) {
                //extractedUniquePaths.Add( AnimationUtility.GetCurveBindings(clip).)

                int bindingcount = 0;
                foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)) {

                    extractedUniquePaths.Add(curveBinding.propertyName);
                    ++bindingcount;
                }
            }


            return extractedUniquePaths.ToArray<string>();
        }

        //RETURN AN ARRAY OF ALL THE FLOATS AT A TIMESLICE OF THE SUPPLIED ANIMATION CLIP
        public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp, string[] totalUniquePaths) {
        //public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp) {

            //convert the totalUniquePaths[] to a List for quick, convenient lookup
            List<string> totalUniquePaths_List = new List<string>(totalUniquePaths);

            //array we return. it will contain all the floats for the curve data a a time slice of the animation clip
            float[] motionPoseCoords = new float[totalUniquePaths.Length];

            //initialize all values of the pose coordinates to 0
            for (int i = 0; i < motionPoseCoords.Length; ++i) {
                motionPoseCoords[i] = 0.0f;
            }

            foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(animClipRefernce)) {
                //float[] motionPoseCoords = Enumerable.Range<float>(0.0f, totalUniquePaths.Length).Select(x => x = 0.0f)

                if (totalUniquePaths_List.Contains(curveBinding.propertyName)) {
                    //Debug.Log("timestamp: " + timestamp);

                    motionPoseCoords[totalUniquePaths_List.IndexOf(curveBinding.propertyName)] 
                        = AnimationUtility.GetEditorCurve(animClipRefernce, curveBinding).Evaluate(timestamp);
                }

            }

            return motionPoseCoords;
        }

        public static MotionPose[] GenerateMotionPoses(AnimationClip animClip, int sampleStepSize, string[] totalUniquePaths) {

            //Debug.LogFormat("Total Unique Paths: {0}", totalUniquePaths.Length);

            List<MotionPose> motionPoses = new List<MotionPose>();

            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFramePointer = 0.0f;

            //MOVE ACROSS ANIMATION CLIP FRAME BY FRAME
            while (currentFramePointer <= (animClip.length * animClip.frameRate) - frameStep) {

                float[] motionPoseKeyframes /*= new float[totalUniquePaths.Length];*/ = ExtractKeyframe(animClip, currentFramePointer, totalUniquePaths);
                /*
                for (int i = 0; i < totalUniquePaths.Length; ++i) {
                    motionPoseKeyframes[i] = ExtractKeyframe(animClip, currentFramePointer, totalUniquePaths);
                }
                */

                //*******
                motionPoses.Add(new MotionPose(animClip, currentFramePointer, motionPoseKeyframes));

                currentFramePointer += frameStep * sampleStepSize;
            }

            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            //Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            return motionPoses.ToArray();

        }

    }
}