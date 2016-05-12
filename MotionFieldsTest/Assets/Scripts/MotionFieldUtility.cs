using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {

    public enum VelocityCalculationMode {
        DropLastTwoFrames = 0,
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


        public static BonePose[] ExtractBonePoses(AnimationClip animClipRefrence, MotionFieldComponent modelRef, float timestamp) {
            Debug.LogError("IMPLEMENT ME!!!");
            return new BonePose[] { };
        }

        public static MotionPose[] DetermineBonePoseComponentVelocities(MotionPose[] motionPoses, VelocityCalculationMode calculationMode = VelocityCalculationMode.DropLastTwoFrames) {
            Debug.LogError("IMPLEMENT ME!!!");
            return new MotionPose[] { };
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

        public static string[] GetUniquePaths(MotionFieldController motionFieldConroller) {
            return (GetUniquePaths(motionFieldConroller.animClipInfoList.Select(x => x.animClip).ToArray()));
        }

//RETURN AN ARRAY OF ALL THE FLOATS AT A TIMESLICE OF THE SUPPLIED ANIMATION CLIP
        public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp, string[] totalUniquePaths) {
            //public static float[] ExtractKeyframe(AnimationClip animClipRefernce, float timestamp) {

            //Debug.LogFormat("timestamp: {0}", timestamp);

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



        public static MotionPose[] DetermineKeyframeComponentVelocities(MotionPose[] motionPoses, VelocityCalculationMode calculationMode = VelocityCalculationMode.DropLastTwoFrames) {
            switch (calculationMode) {
                case VelocityCalculationMode.DropLastTwoFrames:
                    return DetermineKeyframeComponentVelocities_DropLastTwoFrames(motionPoses);

                case VelocityCalculationMode.LoopToFirstFrame:
                    return DetermineKeyframeComponentVelocities_LoopToFirstFrame(motionPoses);

                case VelocityCalculationMode.UseVelocityFromSecondToLastFrame:
                    return DetermineKeyframeComponentVelocities_UseVelocityFromSecondToLastFrame(motionPoses);

                case VelocityCalculationMode.SetLastFrameToZero:
                    return DetermineKeyframeComponentVelocities_SetLastFrameToZero(motionPoses);

                default:
                    goto case VelocityCalculationMode.DropLastTwoFrames;
            }
        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_DropLastTwoFrames(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length - 2; ++i) {//loop through all values in the keyframe data, ignore the last two
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //Calculate the velocity by subtracting the current value from the next value
                    motionPoses[i].keyframeData[j].velocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
					motionPoses[i].keyframeData[j].velocityNext = motionPoses[i + 2].keyframeData[j].value - motionPoses[i + 1].keyframeData[j].value;
                }
            }
            
            List<MotionPose> mpList = motionPoses.ToList<MotionPose>();
            mpList.RemoveRange(mpList.Count - 2, 2);
            return mpList.ToArray();
            

        }

        private static MotionPose[] DetermineKeyframeComponentVelocities_LoopToFirstFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].keyframeData.Length; ++j) {//move across all the KeyframeData within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//do the velocity calculation using the first frame as the next frame for the math
                        motionPoses[i].keyframeData[j].velocity = motionPoses[0].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].velocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
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
                        motionPoses[i].keyframeData[j].velocity = motionPoses[i - 1].keyframeData[j].velocity;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].velocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
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
                        motionPoses[i].keyframeData[j].velocity = 0.0f;
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].keyframeData[j].velocity = motionPoses[i + 1].keyframeData[j].value - motionPoses[i].keyframeData[j].value;
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
        public static MotionPose[] GenerateMotionPoses(AnimationClip animClip, string[] totalUniquePaths, int sampleStepSize = 100, VelocityCalculationMode velCalculationMode = VelocityCalculationMode.DropLastTwoFrames) {

            //Debug.LogFormat("Total Unique Paths: {0}", totalUniquePaths.Length);

            List<MotionPose> motionPoses = new List<MotionPose>();

            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFrameTimePointer = 0.0f;

            //MOVE ACROSS ANIMATION CLIP FRAME BY FRAME
            while (currentFrameTimePointer <= ((animClip.length * animClip.frameRate) - frameStep) / animClip.frameRate) {

                float[] motionPoseKeyframes = ExtractKeyframe(animClip, currentFrameTimePointer, totalUniquePaths);

                //*******
                motionPoses.Add(new MotionPose(animClip, currentFrameTimePointer, motionPoseKeyframes));

                currentFrameTimePointer += frameStep * sampleStepSize;

                
                if (EditorUtility.DisplayCancelableProgressBar("Creating Motion Poses", "", currentFrameTimePointer / animClip.length)) {
                    EditorUtility.ClearProgressBar();
                    return motionPoses.ToArray();
                }

            }

            EditorUtility.ClearProgressBar();
            motionPoses = DetermineKeyframeComponentVelocities(motionPoses.ToArray<MotionPose>(), velCalculationMode).ToList<MotionPose>();

            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            //Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            return motionPoses.ToArray();

        }
        
        

        public static MotionPose[] GenerateMotionPoses(AnimationClip animClip, MotionFieldComponent modelRef, int sampleStepSize = 100, VelocityCalculationMode velCalculationMode = VelocityCalculationMode.DropLastTwoFrames) {
            List<MotionPose> motionPoses = new List<MotionPose>();

            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFrameTimePointer = 0.0f;

            //MOVE ACROSS ANIMATION CLIP FRAME BY FRAME
            while (currentFrameTimePointer <= ((animClip.length * animClip.frameRate) - frameStep) / animClip.frameRate) {

                //float[] motionPoseKeyframes = ExtractKeyframe(animClip, currentFrameTimePointer, totalUniquePaths);
                BonePose[] extractedBonePoses = ExtractBonePoses(animClip, modelRef, currentFrameTimePointer);
                //*******
                //motionPoses.Add(new MotionPose(animClip, currentFrameTimePointer, motionPoseKeyframes));
                motionPoses.Add(new MotionPose(extractedBonePoses, animClip, currentFrameTimePointer));

                currentFrameTimePointer += frameStep * sampleStepSize;

                /*
                if (EditorUtility.DisplayCancelableProgressBar("Creating Motion Poses", "", currentFrameTimePointer / animClip.length)) {
                    EditorUtility.ClearProgressBar();
                    return motionPoses.ToArray();
                }
                */
            }

            //EditorUtility.ClearProgressBar();
            //motionPoses = DetermineKeyframeComponentVelocities(motionPoses.ToArray<MotionPose>(), velCalculationMode).ToList<MotionPose>();

            motionPoses = DetermineBonePoseComponentVelocities(motionPoses.ToArray<MotionPose>(), velCalculationMode).ToList<MotionPose>();

            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            //Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            return motionPoses.ToArray();
        }

        public static List<AnimClipInfo> GenerateMotionField(List<AnimClipInfo> animClipInfos, MotionFieldComponent modelRef, int samplingRate, string[] totalUniquePaths) {


            foreach (AnimClipInfo clipInfo in animClipInfos) {
                if (!clipInfo.useClip) {//only generate motion poses for the selected animations
                    clipInfo.motionPoses = new MotionPose[] { };
                }
                else {
                    //clipInfo.GenerateMotionPoses(samplingRate, uniquePaths);
                    clipInfo.motionPoses = MotionFieldUtility.GenerateMotionPoses(clipInfo.animClip,
                                                                                  //totalUniquePaths,
                                                                                  modelRef,
                                                                                  samplingRate,
                                                                                  clipInfo.velocityCalculationMode);
                }
            }

            return animClipInfos;
        }

        public static void GenerateMotionField(ref MotionFieldController mfController, MotionFieldComponent modelRef, int samplingRate) {
            string[] uniquePaths = MotionFieldUtility.GetUniquePaths(mfController.animClipInfoList.Select(x => x.animClip).ToArray());

			mfController.animClipInfoList = MotionFieldUtility.GenerateMotionField(mfController.animClipInfoList, modelRef, samplingRate, uniquePaths);
			MotionFieldUtility.GenerateKDTree(ref mfController, uniquePaths, mfController.rootComponents);
        }


		public static void GenerateKDTree(ref KDTreeDLL.KDTree kdTree, List<AnimClipInfo> animClipInfoList, string[] uniquePaths, MotionFieldController.RootComponents rootComponents, int numDimensions) {

            kdTree = new KDTreeDLL.KDTree(numDimensions);

			int rootComponent_tx = ArrayUtility.IndexOf (uniquePaths, rootComponents.tx);
			int rootComponent_ty = ArrayUtility.IndexOf (uniquePaths, rootComponents.ty);
			int rootComponent_tz = ArrayUtility.IndexOf (uniquePaths, rootComponents.tz);

			int rootComponent_qx = ArrayUtility.IndexOf (uniquePaths, rootComponents.qx);
			int rootComponent_qy = ArrayUtility.IndexOf (uniquePaths, rootComponents.qy);
			int rootComponent_qz = ArrayUtility.IndexOf (uniquePaths, rootComponents.qz);
			int rootComponent_qw = ArrayUtility.IndexOf (uniquePaths, rootComponents.qw);

			Debug.Log("root components: " + rootComponent_tx + " " + rootComponent_ty + " " + rootComponent_tz + " " + rootComponent_qx + " " + rootComponent_qy + " " + rootComponent_qz + " " + rootComponent_qw);

			Debug.Log("unique path ex " + uniquePaths[0] + "     " + uniquePaths[1]);

            Debug.Log("tree made with " + numDimensions + " dimensions");

            foreach (AnimClipInfo clipinfo in animClipInfoList) {

                foreach (MotionPose pose in clipinfo.motionPoses) {

                    //extract all the values from the Motion Field Controller's Keyframe Datas and convert them to a list of doubles
                    double[] position = pose.keyframeData.Select(x => System.Convert.ToDouble(x.value)).ToArray();//hot damn LINQ
                    double[] velocity = pose.keyframeData.Select(x => System.Convert.ToDouble(x.velocity)).ToArray();//hot damn LINQ
                    double[] velocityNext = pose.keyframeData.Select(x => System.Convert.ToDouble(x.velocityNext)).ToArray();//hot damn LINQ

					NodeData data = new NodeData(pose.animClipRef.name, pose.timestamp, position, velocity, velocityNext, 
												rootComponent_tx, rootComponent_ty, rootComponent_tz, 
												rootComponent_qx, rootComponent_qy, rootComponent_qz, rootComponent_qw);

                    double[] position_velocity_pairings = new double[numDimensions];
                    //Debug.Log(position.Length);
                    for (int i = 0; i < position.Length; i++) {
                        position_velocity_pairings[i * 2] = position[i];
                        position_velocity_pairings[i * 2 + 1] = velocity[i];
                    }

                    string stuff = "Inserting id:" + data.clipId + " , time: " + data.timeStamp.ToString() + "  position_velocity_pairing:(";
                    foreach (double p in position_velocity_pairings) { stuff += p.ToString() + ", "; }
                    Debug.Log(stuff + ")");

                    try {
                        kdTree.insert(position_velocity_pairings, data);
                    }
                    catch (KDTreeDLL.KeyDuplicateException e) {
                        Debug.Log("Duplicate position_velocity_pairing! skip inserting pt.");
                    }
                }
            }

            Debug.Log("tree generated");
        }

		public static void GenerateKDTree(ref MotionFieldController mfController, string[] uniquePaths, MotionFieldController.RootComponents rootComponents ) {
			MotionFieldUtility.GenerateKDTree(ref mfController.kd, mfController.animClipInfoList, uniquePaths, rootComponents, uniquePaths.Length * 2);
        }
    }
}