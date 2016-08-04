using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {


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

            if(modelRef == null) {
                Debug.LogError("modelref is null");
            }
            if (modelRef.cosmeticSkel == null) {
                Debug.LogError("modelref.cosmeticskel is null");
            }
            if (modelRef.cosmeticSkel.cosmeticBones == null) {
                Debug.LogError("modelRef.cosmeticSkel.cosmeticBones is null");
            }

            //we return this
            BonePose[] bonePoses = new BonePose[modelRef.cosmeticSkel.cosmeticBones.Count];

            //turn on animation sampling so we can sample the model
            if (!AnimationMode.InAnimationMode()) {
                AnimationMode.StartAnimationMode();
            }
            AnimationMode.BeginSampling();

            //set the model to the pose we want so we can sample the transforms
            AnimationMode.SampleAnimationClip(modelRef.gameObject, animClipRefrence, timestamp);

            //TODO: move some of this Pose Extraction code over to Cosmetic Skeleton

            //record all the transforms
            for (int i = 0; i < modelRef.cosmeticSkel.cosmeticBones.Count; ++i) {
                //create bone Pose
                bonePoses[i] = new BonePose(modelRef.cosmeticSkel.cosmeticBones[i].boneLabel);

                //Debug.Log(modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace);

                //assign Position to the bone pose
                
                bool isLocalSpace = true;//assume it is local space
                if (modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace == CosmeticSkeletonBone.MovementSpace.World) {
                    isLocalSpace = false;
                }
                bonePoses[i].value = new BoneTransform(modelRef.cosmeticSkel.cosmeticBones[i].boneTf, isLocalSpace);
                
                /*
                if (modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace == CosmeticSkeletonBone.MovementSpace.Local) {
                    bonePoses[i].value = new BoneTransform(modelRef.cosmeticSkel.cosmeticBones[i].boneTf, true);
                }
                else {
                    Animator modelRefAnimator = modelRef.GetComponent<Animator>();
                    bonePoses[i].value = new BoneTransform() {
                                                              posX = modelRefAnimator.deltaPosition.x,
                                                              posY = modelRefAnimator.deltaPosition.y,
                                                              posZ = modelRefAnimator.deltaPosition.z,

                                                              rotW = modelRefAnimator.deltaRotation.w,
                                                              rotX = modelRefAnimator.deltaRotation.x,
                                                              rotY = modelRefAnimator.deltaRotation.y,
                                                              rotZ = modelRefAnimator.deltaRotation.z,
                                                             };
                }
                */
            }

            //return model to it's non-animated pose
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

            return bonePoses;
        }


//BONE POSE VELOCITY EXTRACTION
        //TODO: make the Motion Pose array pass by ref
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

                //ROOT MOTION
                motionPoses[i].rootMotionInfo.positionNext = motionPoses[i + 1].rootMotionInfo.value;
                motionPoses[i].rootMotionInfo.positionNextNext = motionPoses[i + 2].rootMotionInfo.value;


                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //Calculate the velocity by subtracting the current value from the next value
                    //motionPoses[i].bonePoses[j].velocity = new BoneTransform(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                    motionPoses[i].bonePoses[j].positionNext = motionPoses[i + 1].bonePoses[j].value;

                    //motionPoses[i].bonePoses[j].velocityNext = new BoneTransform(motionPoses[i + 2].bonePoses[j].value, motionPoses[i + 1].bonePoses[j].value);
                    motionPoses[i].bonePoses[j].positionNextNext = motionPoses[i + 2].bonePoses[j].value;



                }
            }

            //TODO: handle the "too low resolution" problem more gracefully than this shit
            List<MotionPose> mpList = motionPoses.ToList<MotionPose>();
            bool resolutionTooLow = false;

            try {
                mpList.RemoveRange(mpList.Count - 2, 2);
            }
            catch {
                //return mpList;
                //TODO: this debug is stupid. Pass it some sort of info that's not reliant on index 0 of an array
                Debug.LogWarningFormat("resolution for {0} too low, returning no poses", motionPoses[0].animName);
                resolutionTooLow = true;
            }

            if (resolutionTooLow) {
                return new MotionPose[0];
            }

            return mpList.ToArray();


        }

        private static MotionPose[] DetermineBonePoseComponentVelocities_LoopToFirstFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data

//HACK: Come back and do velocities for root motion and bone poses in the same loop so you don't need duplicate if statments
                if (i == motionPoses.Length - 1) {
                    motionPoses[i].rootMotionInfo.positionNext = BoneTransform.Subtract(motionPoses[0].rootMotionInfo.value, motionPoses[i].rootMotionInfo.value);
                    motionPoses[i].rootMotionInfo.positionNextNext = BoneTransform.Subtract(motionPoses[1].rootMotionInfo.value, motionPoses[0].rootMotionInfo.value);
                }
                else {
                    motionPoses[i].rootMotionInfo.positionNext = BoneTransform.Subtract(motionPoses[i + 1].rootMotionInfo.value, motionPoses[i].rootMotionInfo.value);

                    if (i == motionPoses.Length - 2) {
                        motionPoses[i].rootMotionInfo.positionNextNext = BoneTransform.Subtract(motionPoses[0].rootMotionInfo.value, motionPoses[i + 1].rootMotionInfo.value);
                    }
                }
                


                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//do the velocity calculation using the first frame as the next frame for the math
                        motionPoses[i].bonePoses[j].positionNext = BoneTransform.Subtract(motionPoses[0].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                        motionPoses[i].bonePoses[j].positionNextNext = BoneTransform.Subtract(motionPoses[1].bonePoses[j].value, motionPoses[0].bonePoses[j].value);

                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].bonePoses[j].positionNext = BoneTransform.Subtract(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);

                        if (i == motionPoses.Length - 2) {
                            motionPoses[i].bonePoses[j].positionNextNext = BoneTransform.Subtract(motionPoses[0].bonePoses[j].value, motionPoses[i + 1].bonePoses[j].value);
                        }
                    }

                }
            }

            return motionPoses;
        }

        private static MotionPose[] DetermineBonePoseComponentVelocities_UseVelocityFromSecondToLastFrame(MotionPose[] motionPoses) {
            for (int i = 0; i < motionPoses.Length; ++i) {//loop through all values in the keyframe data

//HACK: COME BACK AND DO EVERYTHING IN ONE LOOP SO WE CAN AVOID DUPLICATE IF STATEMENTS
                //SPECIAL CASE
                if (i == motionPoses.Length - 1) {//we're at the end of the array, use the value from the frame before it
                    motionPoses[i].rootMotionInfo.positionNext = new BoneTransform(motionPoses[i - 1].rootMotionInfo.positionNext);

                    //we can't calculate any further next velocities, use the last calculateable velocity
                    motionPoses[i].rootMotionInfo.positionNextNext = new BoneTransform(motionPoses[i - 1].rootMotionInfo.positionNext);
                }
                //BUSINESS AS USUAL
                else {//Calculate the velocity by subtracting the current value from the next value
                    motionPoses[i].rootMotionInfo.positionNext = BoneTransform.Subtract(motionPoses[i + 1].rootMotionInfo.value, motionPoses[i].rootMotionInfo.value);

                    //we can't calculate any further next velocities, use the last calculateable velocity
                    if (i == motionPoses.Length - 2) {
                        motionPoses[i].rootMotionInfo.positionNextNext = BoneTransform.Subtract(motionPoses[i + 1].rootMotionInfo.value, motionPoses[i].rootMotionInfo.value);
                    }
                }

                for (int j = 0; j < motionPoses[i].bonePoses.Length; ++j) {//move across all the BonePoses within the current Motion Pose

                    //SPECIAL CASE
                    if (i == motionPoses.Length - 1) {//we're at the end of the array, use the value from the frame before it
                        motionPoses[i].bonePoses[j].positionNext = new BoneTransform(motionPoses[i - 1].bonePoses[j].positionNext);

                        //we can't calculate any further next velocities, use the last calculateable velocity
                        motionPoses[i].bonePoses[j].positionNextNext = new BoneTransform(motionPoses[i - 1].bonePoses[j].positionNext);
                    }
                    //BUSINESS AS USUAL
                    else {//Calculate the velocity by subtracting the current value from the next value
                        motionPoses[i].bonePoses[j].positionNext = BoneTransform.Subtract(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
                        
                        //we can't calculate any further next velocities, use the last calculateable velocity
                        if (i == motionPoses.Length - 2) {
                            motionPoses[i].bonePoses[j].positionNextNext = BoneTransform.Subtract(motionPoses[i + 1].bonePoses[j].value, motionPoses[i].bonePoses[j].value);
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


//ROOT MOTION EXTRACTION
        //TODO: restructure so looping is more intelligently implemented (maybe use deleagate functions? or more clever case checking? log position (center of mass / reference point) in hte umbrella function and pass that throught?)
        //TODO: add back in hip displacement
        public static void ExtractRootMotion(ref MotionPose motionPose, AnimationClip animClip, MotionFieldComponent modelRef, float timestamp, float frameStep, RootMotionCalculationMode calculationMode = RootMotionCalculationMode.CenterOfMass, RootMotionFrameHandling frameHandling = RootMotionFrameHandling.SetFirstFrameToZero) {
            

        //ADJUST FOR HIP OFFSET
            switch (calculationMode) {
                case RootMotionCalculationMode.ReferencePoint:
                    //TODO: Implement - Reference Point Root Motion
                    break;

                case RootMotionCalculationMode.CenterOfMass:

                //RECORD IMPORTANT POINTS FOR READABILITY
                    //get the Bone Pose associated with the transform assigned as the Skeleton Root Bone
                    BonePose skeletonRootBonePose = motionPose.GetBonePose(modelRef.cosmeticSkel.GetBone(modelRef.cosmeticSkel.skeletonRoot).boneLabel);
                    Transform rootMotionRefPoint = modelRef.cosmeticSkel.rootMotionReferencePoint;
                    Transform skelRoot = modelRef.cosmeticSkel.skeletonRoot;

                    //Debug.Log("Bone Label: " + skeletonRootBonePose.boneLabel);

                    //allocate space to store the human pose of the model during ANIMATION SAMPLING MODE
                    HumanPoseHandler hPoseHandler = new HumanPoseHandler(modelRef.cosmeticSkel.avatar, modelRef.cosmeticSkel.skeletonRoot);
                    HumanPose hPose = new HumanPose();

                    //ACTIVATE ANIMATION SAMPLING MODE
                    if (!AnimationMode.InAnimationMode()) { AnimationMode.StartAnimationMode(); }
                    AnimationMode.BeginSampling();

                    //sample pose and store in the Human Pose we allocated earlier
                    AnimationMode.SampleAnimationClip(modelRef.gameObject, animClip, timestamp);
                    hPoseHandler.GetHumanPose(ref hPose);

                    //calculate the distance between refererence point and the root, use that to adjust the hips location
                    Vector3 newPos = Vector3.ProjectOnPlane( (modelRef.cosmeticSkel.rootMotionReferencePoint.position - hPose.bodyPosition /*modelRef.cosmeticSkel.skeletonRoot.position*/)
                                     /*+ (hPose.bodyPosition - modelRef.cosmeticSkel.skeletonRoot.position)*/, Vector3.up)

                                     + skelRoot.position;

                    //transform the point to the reference point's local space, where the skeleton's root is originally located
                    newPos = modelRef.cosmeticSkel.rootMotionReferencePoint.InverseTransformPoint(newPos);

                    Quaternion newRot = (skelRoot.rotation * (rootMotionRefPoint.rotation * Quaternion.Inverse(skelRoot.rotation))) * (hPose.bodyRotation * Quaternion.Inverse(hPose.bodyRotation));

                    skeletonRootBonePose.value = new BoneTransform(newPos, newRot, skelRoot.localScale);

                    AnimationMode.EndSampling();
                    AnimationMode.StopAnimationMode();

                    break;
            }
            

            //we're setting the root motion for the first frame to zero, just set it here and break out
            if (frameHandling == RootMotionFrameHandling.SetFirstFrameToZero && Mathf.Approximately(timestamp, 0.0f)) {
                motionPose.rootMotionInfo = new BonePose("RootMotion") { value = new BoneTransform(Vector3.zero, Quaternion.identity, Vector3.zero) };
                return;
            }

            

        //CALCULATE MOTION OF MODEL
            switch (calculationMode) {
                case RootMotionCalculationMode.ReferencePoint:
                    ExtractRootMotion_CenterOfMass(ref motionPose, animClip, modelRef, timestamp, frameStep, frameHandling);
                    break;

                case RootMotionCalculationMode.CenterOfMass:
                    ExtractRootMotion_CenterOfMass(ref motionPose, animClip, modelRef, timestamp, frameStep, frameHandling);
                    break;

                default:
                    goto case RootMotionCalculationMode.ReferencePoint;
                    
            }

            //Debug.Log(skeletonRootBonePose.value.flattenedPosition[2]);

        }

        private static void ExtractRootMotion_ReferencePoint(ref MotionPose motionPose, AnimationClip animClip, MotionFieldComponent modelRef, float timestamp, float frameStep, RootMotionFrameHandling frameHandling = RootMotionFrameHandling.SetFirstFrameToZero) {
            //TODO: Implement - Reference Point Root Motion
        }

        public static void ExtractRootMotion_CenterOfMass(ref MotionPose motionPose, AnimationClip animClip, MotionFieldComponent modelRef, float timestamp, float frameStep, RootMotionFrameHandling frameHandling = RootMotionFrameHandling.SetFirstFrameToZero) {

            //set up a pose handler for this model
            HumanPoseHandler poseHandler = new HumanPoseHandler(modelRef.cosmeticSkel.avatar, modelRef.cosmeticSkel.skeletonRoot);
            HumanPose prevHumanPose = new HumanPose();//this will store the previous frame's pose
            HumanPose curHumanPose = new HumanPose();//this will store the current frame's pose
            //poseHandler.GetHumanPose(ref hPose);



        //HACK: assume that we're using the XZ plane for root motion, flatten out the Y to the Reference Point's Y
            Vector3 projectedCenterOfMass = prevHumanPose.bodyPosition;
            projectedCenterOfMass.y = modelRef.cosmeticSkel.rootMotionReferencePoint.position.y;


        //ACTIVATE ANIMATION SAMPLING MODE
            if (!AnimationMode.InAnimationMode()) {
                AnimationMode.StartAnimationMode();
            }
            AnimationMode.BeginSampling();

        //GRAB POSES
            //sample previous pose
            AnimationMode.SampleAnimationClip(modelRef.gameObject, animClip, timestamp - frameStep);
            poseHandler.GetHumanPose(ref prevHumanPose);//store the pose info here

            //sample current pose
            AnimationMode.SampleAnimationClip(modelRef.gameObject, animClip, timestamp);
            poseHandler.GetHumanPose(ref curHumanPose);//store the pose info here

            
            Quaternion adjustmentQuat = modelRef.cosmeticSkel.rootMotionReferencePoint.rotation * Quaternion.Inverse(prevHumanPose.bodyRotation);
            Vector3 positionMotion = Vector3.ProjectOnPlane(adjustmentQuat * (curHumanPose.bodyPosition - prevHumanPose.bodyPosition), Vector3.up);
            

            Quaternion rotationMotion = prevHumanPose.bodyRotation * Quaternion.Inverse(curHumanPose.bodyRotation);
            //project the rotation onto a plane by using it to modify a vector and then generating a quaternion out from that
            rotationMotion = Quaternion.LookRotation(Vector3.ProjectOnPlane(rotationMotion * Vector3.forward, Vector3.up), Vector3.up);

            motionPose.rootMotionInfo = new BonePose("RootMotion") { value = new BoneTransform(positionMotion, rotationMotion, Vector3.one) };

            //if (Mathf.Approximately(timestamp, frameStep)) {
                //GameObject.Instantiate(modelRef.cosmeticSkel.marker.gameObject, curHumanPose.bodyPosition, curHumanPose.bodyRotation);
            //}

        //DEACTIVATE ANIMATION SAMPLING MODE
            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

        }



        public static MotionPose[] GenerateMotionPoses(AnimationClip animClip, MotionFieldComponent modelRef, int sampleStepSize = 100, VelocityCalculationMode velCalculationMode = VelocityCalculationMode.DropLastTwoFrames) {
            //Animator modelAnimator = modelRef.GetComponent<Animator>();

            List<MotionPose> motionPoses = new List<MotionPose>();

            //TODO: make this a desired framerate sampline. Don't use animClip.framerate, but supply the framerate. ex, a supplied framerate of 60 fps would calc framerate as 1.0f / 60.0f
            float frameStep = 1.0f / animClip.frameRate;//time for one animation frame
            float currentFrameTimePointer = 0.0f;
            //Debug.LogFormat("{0} framestep: {1}", animClip.name, frameStep);


            //MOVE ACROSS ANIMATION CLIP FRAME BY FRAME
            while (currentFrameTimePointer <= ((animClip.length * animClip.frameRate) - frameStep) / animClip.frameRate) {

                //float[] motionPoseKeyframes = ExtractKeyframe(animClip, currentFrameTimePointer, totalUniquePaths);
                BonePose[] extractedBonePoses = ExtractBonePoses(animClip, modelRef, currentFrameTimePointer);

                MotionPose newPose = new MotionPose(extractedBonePoses, animClip.name, currentFrameTimePointer);

                ExtractRootMotion(ref newPose, animClip, modelRef, currentFrameTimePointer, frameStep);

                motionPoses.Add(newPose);

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

            //ExtractRootMotion(ref motionPoses, animClip, modelRef, sampleStepSize);

            //Debug.LogFormat("Frame Count: {0} | Aggregate: {1}", frameCount, currentFramePointer);
            //Debug.LogFormat("Aggregate: {0} | Clip Length: {1}", currentFramePointer / animClip.frameRate, animClip.length);

            return motionPoses.ToArray();
        }

        public static List<AnimClipInfo> GenerateMotionField(List<AnimClipInfo> animClipInfos, MotionFieldComponent modelRef, int samplingRate) {

            for (int i = 0; i < animClipInfos.Count; i++) {

                EditorUtility.DisplayProgressBar("Generating Poses", "generating motion fields... ", ((float)i / (float)animClipInfos.Count));

                if (!animClipInfos[i].useClip) {//only generate motion poses for the selected animations
                    animClipInfos[i].motionPoses = new MotionPose[] { };
                }
                else {
                    //clipInfo.GenerateMotionPoses(samplingRate, uniquePaths);
                    animClipInfos[i].motionPoses = MotionFieldUtility.GenerateMotionPoses(animClipInfos[i].animClip,
                                                                                  modelRef,
                                                                                  samplingRate,
                                                                                  animClipInfos[i].velocityCalculationMode);
                }
            }

            EditorUtility.ClearProgressBar();

            return animClipInfos;
        }

        public static void GenerateMotionField(ref MotionFieldController mfController, MotionFieldComponent modelRef, int samplingRate) {
            //string[] uniquePaths = MotionFieldUtility.GetUniquePaths(mfController.animClipInfoList.Select(x => x.animClip).ToArray());
            
            //recenter model b/c we might be recording world coordinates
            Vector3 originalModelPos = modelRef.transform.position;
            Quaternion originalModelRot = modelRef.transform.rotation;
            Vector3 originalModelScale = modelRef.transform.localScale;

            modelRef.transform.position = Vector3.zero;
            modelRef.transform.rotation = Quaternion.identity;
            modelRef.transform.localScale = Vector3.one;
            
            //settings to track which fields were added
            Animator modelRefAnimator = modelRef.gameObject.GetComponent<Animator>();
            bool addedAnimator = false;
            bool turnedOnRootMotion = false;
            bool addedRuntimeController = false;

            //add any necessary fields that may be missing
            if (modelRefAnimator == null) {
                modelRefAnimator = modelRef.gameObject.AddComponent<Animator>();
                addedAnimator = true;
            }
            if (!modelRefAnimator.applyRootMotion) {
                modelRefAnimator.applyRootMotion = true;
                turnedOnRootMotion = true;
            }
            if (modelRefAnimator.runtimeAnimatorController == null) {
                modelRefAnimator.runtimeAnimatorController = new RuntimeAnimatorController();
                addedRuntimeController = true;
            }

            mfController.animClipInfoList = MotionFieldUtility.GenerateMotionField(mfController.animClipInfoList, modelRef, samplingRate);

            //reset any fields that may have been added
            if (addedRuntimeController) {
                MonoBehaviour.DestroyImmediate(modelRefAnimator.runtimeAnimatorController, true);
            }
            if (turnedOnRootMotion) {
                modelRefAnimator.applyRootMotion = false;
            }
            if (addedAnimator) {
                MonoBehaviour.DestroyImmediate(modelRefAnimator);
            }
            
            modelRef.transform.position = originalModelPos;
            modelRef.transform.rotation = originalModelRot;
            modelRef.transform.localScale = originalModelScale;

            MotionFieldUtility.GenerateKDTree(ref mfController.kd, mfController.animClipInfoList);

        }

        /*public static void GenerateKDTree(ref MotionFieldController mfController, string[] uniquePaths, MotionFieldController.RootComponents rootComponents) {
            MotionFieldUtility.GenerateKDTree(ref mfController.kd, mfController.animClipInfoList, uniquePaths, rootComponents, uniquePaths.Length * 2);
        }*/

        public static void GenerateKDTree(ref KDTreeDLL_f.KDTree kdTree, List<AnimClipInfo> animClipInfoList) {

            //make KD Tree w/ number of dimension equal to total number of bone poses * (position * velocity) <- 20
            int KeyLength = 0;
            for (int i = 0; i < animClipInfoList.Count; i++)
            {
                if(animClipInfoList[i].useClip == true)
                {
                    if(animClipInfoList[i].motionPoses.Length != 0)
                    {
                        KeyLength = animClipInfoList[i].motionPoses[0].bonePoses.Length * 20; //HACK: 20 is the magic number of values in each bonePose that is aded to the kdtree.
                        break;
                    }
                }
            }
            if (KeyLength == 0)
            {
                Debug.LogError("Cannot Populate KDTree because no poses were generated! Try lowering the Frame Resolution and try again."); //this error happened once, then went away...
                return;
            }

            KeyLength += 20; // adding number of fields to store rootMotionInfo.

            Debug.Log("Length of kdtree key: " + KeyLength);
            kdTree = new KDTreeDLL_f.KDTree(KeyLength);

            int numPts = 0;
            for (int i = 0; i < animClipInfoList.Count; i++) {

                EditorUtility.DisplayProgressBar("Generating Poses", "generating ke tree... ", ((float)i / (float)animClipInfoList.Count));

                foreach (MotionPose pose in animClipInfoList[i].motionPoses) {
                    
                    float[] position_velocity_pairings = pose.flattenedMotionPose;

                    /*
                    string stuff = "Inserting id:" + pose.animName + " , time: " + pose.timestamp + "  position_velocity_pairing:(";
                    foreach (double p in position_velocity_pairings) { stuff += p.ToString() + ", "; }
                    Debug.Log(stuff + ")");
                    */

                    try {
                        kdTree.insert(position_velocity_pairings, pose);
                        numPts += 1;
                    }
                    catch (KDTreeDLL_f.KeyDuplicateException e) {
                        Debug.Log(e.ToString() + "\nDuplicates in the kdtree are redundant. Skip inserting pt.");
                    }
                }
            }
            Debug.Log(numPts.ToString() + " points added to KDTree");
            EditorUtility.ClearProgressBar();
        }

        /*public static void GenerateKDTree(ref KDTreeDLL.KDTree kdTree, List<AnimClipInfo> animClipInfoList, string[] uniquePaths, MotionFieldController.RootComponents rootComponents, int numDimensions) {

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
                    
                    double[] position_velocity_pairings = pose.flattenedMotionPose.Select(x => System.Convert.ToDouble(x)).ToArray();

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
        }*/

        //helper function to print out timings when debugging
        public static void printTime(System.Diagnostics.Stopwatch st, string name)
        {
            Debug.Log(name + ": " + string.Format("{0:00}:{1:000}",
            st.Elapsed.Seconds,
            st.Elapsed.Milliseconds));
        }
    }
}