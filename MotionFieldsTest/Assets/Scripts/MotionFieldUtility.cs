using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {

    public enum VelocityCalculationMode {
        DropLastFrame = 0,
        LoopToFirstFrame = 1,
        UseVelocityFromSecondToLastFrame = 2,
        SetLastFrameToZero = 3,
    }


    public static class MotionFieldUtility {

        /// <summary>
        /// Creates Animation Poses from a Supplied Animation Clip
        /// </summary>
        /// <param name="animClip"> Animation Clip to generate Poses from</param>
        /// <param name="sampleStepSize"> Grab every n frame (1 is every frame, 3 is every third, 5 is every fifth) </param>
        /// <returns></returns>


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

        public static string[] GetUniquePaths(MotionFieldController motionFieldConroller) {
            return (GetUniquePaths(motionFieldConroller.animClipInfoList.Select(x => x.animClip).ToArray()));
        }

//RETURN AN ARRAY OF ALL THE FLOATS AT A TIMESLICE OF THE SUPPLIED ANIMATION CLIP
        public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp, string[] totalUniquePaths) {
            //public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp) {

            Debug.LogFormat("timestamp: {0}", timestamp);

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



        public static MotionPose[] DetermineKeyframeComponentVelocities(MotionPose[] motionPoses, VelocityCalculationMode calculationMode = VelocityCalculationMode.DropLastFrame) {
            switch (calculationMode) {
                case VelocityCalculationMode.DropLastFrame:
                    return DetermineKeyframeComponentVelocities_DropLastFrame(motionPoses);

                case VelocityCalculationMode.LoopToFirstFrame:
                    return DetermineKeyframeComponentVelocities_LoopToFirstFrame(motionPoses);

                case VelocityCalculationMode.UseVelocityFromSecondToLastFrame:
                    return DetermineKeyframeComponentVelocities_UseVelocityFromSecondToLastFrame(motionPoses);

                case VelocityCalculationMode.SetLastFrameToZero:
                    return DetermineKeyframeComponentVelocities_SetLastFrameToZero(motionPoses);

                default:
                    goto case VelocityCalculationMode.DropLastFrame;
            }
        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_DropLastFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length - 1; ++i) {//loop through all values in the keyframe data, ignore the last one
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //Calculate the velocity by subtracting the current value from the next value
                    motionPoses[i].keyframeData[j].inVelocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                }
            }
            
            List<MotionPose> mpList = motionPoses.ToList<MotionPose>();
            mpList.RemoveAt(mpList.Count - 1);
            return mpList.ToArray();
            

        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_LoopToFirstFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//do the velocity calculation using the first frame as the next frame for the math
                        motionPoses[i].keyframeData[j].inVelocity = motionPoses[0].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].inVelocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                    }
                }
            }

            return motionPoses;
        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_UseVelocityFromSecondToLastFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//we're at the end of the array, use the value from the frame before it
                        motionPoses[i].keyframeData[j].inVelocity = motionPoses[i - 1].keyframeData[j].inVelocity;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].inVelocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                    }
                }
            }

            return motionPoses;
        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_SetLastFrameToZero(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {
                        motionPoses[i].keyframeData[j].inVelocity = 0.0f;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].inVelocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                    }
                }
            }

            return motionPoses;
        }
        


        /// <summary>
        /// Generates all Motion Poses for the supplied Animation Clip
        /// </summary>
        /// <param name="animClip">Clip to generate Motion Poses from</param>
        /// <param name="sampleStepSize">How many frames to skip when generating (Warning: smaller numbers is longer time, must be greater than 0)</param>
        /// <param name="totalUniquePaths">All the paths from all the Animation Clips in the Motion Field. Get using MotionFieldUtility().GetUniquePaths()</param>
        /// <returns></returns>
        public static MotionPose[] GenerateMotionPoses(AnimationClip animClip, string[] totalUniquePaths, int sampleStepSize = 100, VelocityCalculationMode velCalculationMode = VelocityCalculationMode.DropLastFrame) {

            //Debug.LogFormat("Total Unique Paths: {0}", totalUniquePaths.Length);

            List<MotionPose> motionPoses = new List<MotionPose>();

            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFrameTimePointer = 0.0f;

            //MOVE ACROSS ANIMATION CLIP FRAME BY FRAME
            while (currentFrameTimePointer <= ((animClip.length * animClip.frameRate) - frameStep) / animClip.frameRate) {

                float[] motionPoseKeyframes = ExtractKeyframe(animClip, currentFrameTimePointer, totalUniquePaths);
                                
                /*
                for (int i = 0; i < totalUniquePaths.Length; ++i) {
                    motionPoseKeyframes[i] = ExtractKeyframe(animClip, currentFramePointer, totalUniquePaths);
                }
                */

                //*******
                motionPoses.Add(new MotionPose(animClip, currentFrameTimePointer, motionPoseKeyframes));

                currentFrameTimePointer += frameStep * sampleStepSize;
            }

            motionPoses = DetermineKeyframeComponentVelocities(motionPoses.ToArray<MotionPose>(), velCalculationMode).ToList<MotionPose>();

            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            //Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            return motionPoses.ToArray();

        }

    }
}