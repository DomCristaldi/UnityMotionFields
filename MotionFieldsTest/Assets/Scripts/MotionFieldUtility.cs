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
        /// Extract Bone Poses from the Animation Clip at the supplied timestamp (POSITION INFO ONLY, NO VELOCITIES)
        /// </summary>
        /// <param name="animClipRefrence">Animation Clip to generate Poses from</param>
        /// <param name="modelRef">Model to use as reference for extracting animation info</param>
        /// <param name="timestamp">Grab every n frame (1 is every frame, 3 is every third, 5 is every fifth)</param>
        /// <returns>BonePose[] representing the pose at the current timestamp</returns>
        public static BonePose[] ExtractBonePoses(AnimationClip animClipRefrence, MotionFieldComponent modelRef, float timestamp) {
            //Debug.LogError("IMPLEMENT ME!!!");

            //we return this
            BonePose[] bonePoses = new BonePose[modelRef.cosmeticSkel.cosmeticBones.Count];

            //turn on animation sampling so we can sample the model
            if (!AnimationMode.InAnimationMode()) {
                AnimationMode.StartAnimationMode();
            }
            AnimationMode.BeginSampling();

            //set the model to the pose we want so we can sample the transforms
            AnimationMode.SampleAnimationClip(modelRef.gameObject, animClipRefrence, timestamp);

            //record all the transforms
            for (int i = 0; i < modelRef.cosmeticSkel.cosmeticBones.Count; ++i) {
                //create bone Pose
                bonePoses[i] = new BonePose(modelRef.cosmeticSkel.cosmeticBones[i].boneLabel);

                //assign Position to the bone pose
                bool isLocalSpace = true;//assume it is local space
                if (modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace == CosmeticSkeletonBone.MovementSpace.World) {
                    isLocalSpace = false;
                }
                bonePoses[i].value = new BoneTransform(modelRef.cosmeticSkel.cosmeticBones[i].boneTf, isLocalSpace);
            }

            //return model to it's non-animated pose
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

            return bonePoses;
        }

        public static MotionPose[] DetermineBonePoseComponentVelocities(MotionPose[] motionPoses, VelocityCalculationMode calculationMode = VelocityCalculationMode.DropLastTwoFrames) {
            //Debug.LogError("IMPLEMENT ME!!!");
            //return new MotionPose[] { };

            switch (calculationMode) {
                case VelocityCalculationMode.DropLastTwoFrames:
                    return DetermineBonePoseComponentVelocities_DropLastTwoFrames(motionPoses);

                case VelocityCalculationMode.LoopToFirstFrame:
                    return DetermineBonePoseComponentVelocities_LoopToFirstFrame(motionPoses);

                case VelocityCalculationMode.UseVelocityFromSecondToLastFrame:
                    return DetermineBonePoseComponentVelocities_UseVelocityFromSecondToLastFrame(motionPoses);

                    /*
                case VelocityCalculationMode.SetLastFrameToZero:
                    return DetermineBonePoseComponentVelocities_SetLastFrameToZero(motionPoses);
                    */

                default:
                    goto case VelocityCalculationMode.DropLastTwoFrames;
            }

            //return motionPoses;
        }

        private static MotionPose[] DetermineBonePoseComponentVelocities_DropLastTwoFrames(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length - 2; ++i) {//loop through all values in the keyframe data, ignore the last two
                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //Calculate the velocity by subtracting the current value from the next value
                    motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                    motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[i + 2].bonePoses[j].value, motionPoses[i + 1].bonePoses[j].value);
                }
            }

            List<MotionPose> mpList = motionPoses.ToList<MotionPose>();
            mpList.RemoveRange(mpList.Count - 2, 2);
            return mpList.ToArray();


        }

        private static MotionPose[] DetermineBonePoseComponentVelocities_LoopToFirstFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//do the velocity calculation using the first frame as the next frame for the math
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[0].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                        motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[1].bonePoses[j].value, motionPoses[0].bonePoses[j].value);
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);

                        if (i == motionPoses.Length - 2) {
                            motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[0].bonePoses[j].value, motionPoses[i + 1].bonePoses[j].value);
                        }
                    }

                }
            }

            return motionPoses;
        }

        private static MotionPose[] DetermineBonePoseComponentVelocities_UseVelocityFromSecondToLastFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//we're at the end of the array, use the value from the frame before it
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i - 1].bonePoses[j].velocity);

                        //we can't calculate any further next velocities, use the last calculateable velocity
                        motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[i - 1].bonePoses[j].velocity);
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                        
                        //we can't calculate any further next velocities, use the last calculateable velocity
                        if (i == motionPoses.Length - 2) {
                            motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                        }
                    }
                }
            }

            return motionPoses;
        }
        /*
        private static MotionPose[] DetermineBonePoseComponentVelocities_SetLastFrameToZero(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data
                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(0.0f);
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i + 1].bonePoses[j].position, motionPoses[i].bonePoses[j].position);
                    }
                }
            }

            return motionPoses;
        }
        */


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

        public static List<AnimClipInfo> GenerateMotionField(List<AnimClipInfo> animClipInfos, MotionFieldComponent modelRef, int samplingRate) {


            foreach (AnimClipInfo clipInfo in animClipInfos) {
                if (!clipInfo.useClip) {//only generate motion poses for the selected animations
                    clipInfo.motionPoses = new MotionPose[] { };
                }
                else {
                    //clipInfo.GenerateMotionPoses(samplingRate, uniquePaths);
                    clipInfo.motionPoses = MotionFieldUtility.GenerateMotionPoses(clipInfo.animClip,
                                                                                  modelRef,
                                                                                  samplingRate,
                                                                                  clipInfo.velocityCalculationMode);
                }
            }

            return animClipInfos;
        }

        public static void GenerateMotionField(ref MotionFieldController mfController, MotionFieldComponent modelRef, int samplingRate) {
            string[] uniquePaths = MotionFieldUtility.GetUniquePaths(mfController.animClipInfoList.Select(x => x.animClip).ToArray());
            
            //recenter model b/c we might be recording world coordinates
            Vector3 originalModelPos = modelRef.transform.position;
            Quaternion originalModelRot = modelRef.transform.rotation;
            Vector3 originalModelScale = modelRef.transform.localScale;

            modelRef.transform.position = Vector3.zero;
            modelRef.transform.rotation = Quaternion.identity;
            modelRef.transform.localScale = Vector3.one;
            
            Animator modelRefAnimator = modelRef.gameObject.GetComponent<Animator>();
            bool addedAnimator = false;
            bool addedRuntimeController = false;

            if (modelRefAnimator == null) {
                modelRefAnimator = modelRef.gameObject.AddComponent<Animator>();
                addedAnimator = true;
            }
            if (modelRefAnimator.runtimeAnimatorController == null) {
                modelRefAnimator.runtimeAnimatorController = new RuntimeAnimatorController();
                addedRuntimeController = true;
            }

			mfController.animClipInfoList = MotionFieldUtility.GenerateMotionField(mfController.animClipInfoList, modelRef, samplingRate);

            if (addedRuntimeController) {
                MonoBehaviour.DestroyImmediate(modelRefAnimator.runtimeAnimatorController, true);
            }

            if (addedAnimator) {
                MonoBehaviour.DestroyImmediate(modelRefAnimator);
            }
            
            modelRef.transform.position = originalModelPos;
            modelRef.transform.rotation = originalModelRot;
            modelRef.transform.localScale = originalModelScale;


            //*****************UNCOMMENT TO GENERATE KD TREE*********************
            //MotionFieldUtility.GenerateKDTree(ref mfController, uniquePaths, mfController.rootComponents);
            MotionFieldUtility.GenerateKDTree(ref mfController, modelRef);
        }

        public static void GenerateKDTree(ref MotionFieldController mfController, MotionFieldComponent modelRef) {
            MotionFieldUtility.GenerateKDTree(ref mfController.kd, mfController.animClipInfoList, modelRef);
        }

        public static void GenerateKDTree(ref MotionFieldController mfController, string[] uniquePaths, MotionFieldController.RootComponents rootComponents) {
            MotionFieldUtility.GenerateKDTree(ref mfController.kd, mfController.animClipInfoList, uniquePaths, rootComponents, uniquePaths.Length * 2);
        }

        public static void GenerateKDTree(ref KDTreeDLL.KDTree kdTree, List<AnimClipInfo> animClipInfoList, MotionFieldComponent modelRef) {


            //make KD Tree w/ number of dimension equal to total number of bone poses * (position * velocity) <- 20
            kdTree = new KDTreeDLL.KDTree(animClipInfoList[0].motionPoses[0].bonePoses.Length * 20);

            foreach (AnimClipInfo clipInfo in animClipInfoList) {
                foreach (MotionPose mPose in clipInfo.motionPoses) {
                    foreach (BonePose bone in mPose.bonePoses) {

                        double[] boneValues = System.Array.ConvertAll<float, double>(bone.value.flattenedPosition, x => (double)x).Concat<double>(
                                              System.Array.ConvertAll<float, double>(bone.value.flattenedRotation, x => (double)x).Concat<double>(
                                              System.Array.ConvertAll<float, double>(bone.value.flattenedScale,    x => (double)x))).ToArray<double>();

                        double[] boneVelocities = System.Array.ConvertAll<float, double>(bone.velocity.flattenedPosition, x => (double)x).Concat<double>(
                                                  System.Array.ConvertAll<float, double>(bone.velocity.flattenedRotation, x => (double)x).Concat<double>(
                                                  System.Array.ConvertAll<float, double>(bone.velocity.flattenedScale,    x => (double)x))).ToArray<double>();
                        /*
                        double[] boneVelocityNexts = System.Array.ConvertAll<float, double>(bone.velocityNext.flattenedPosition, x => (double)x).Concat<double>(
                                                     System.Array.ConvertAll<float, double>(bone.velocityNext.flattenedRotation, x => (double)x).Concat<double>(
                                                     System.Array.ConvertAll<float, double>(bone.velocityNext.flattenedScale,    x => (double)x))).ToArray<double>();
                        */
                    }
                }
            }
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
                    /*
					NodeData data = new NodeData(pose.animClipRef.name, pose.timestamp, position, velocity, velocityNext, 
												rootComponent_tx, rootComponent_ty, rootComponent_tz, 
												rootComponent_qx, rootComponent_qy, rootComponent_qz, rootComponent_qw);
                    */

                    double[] position_velocity_pairings = new double[numDimensions];
                    //Debug.Log(position.Length);
                    for (int i = 0; i < position.Length; i++) {
                        position_velocity_pairings[i * 2] = position[i];
                        position_velocity_pairings[i * 2 + 1] = velocity[i];
                    }

                    string stuff = "Inserting id:" + pose.animClipRef.name + " , time: " + pose.timestamp + "  position_velocity_pairing:(";
                    foreach (double p in position_velocity_pairings) { stuff += p.ToString() + ", "; }
                    Debug.Log(stuff + ")");

                    try {
                        kdTree.insert(position_velocity_pairings, pose);
                    }
                    catch (KDTreeDLL.KeyDuplicateException e) {
                        Debug.Log("Duplicate position_velocity_pairing! skip inserting pt.");
                    }
                }
            }

            Debug.Log("tree generated");
        }

		
    }
}