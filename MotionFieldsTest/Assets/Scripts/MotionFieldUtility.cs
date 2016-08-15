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
                CosmeticSkeletonBone cosBone = modelRef.cosmeticSkel.cosmeticBones[i];

                bonePoses[i] = new BonePose(cosBone.boneLabel);

                //Debug.Log(modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace);

                //assign Position to the bone pose
                
                bool isLocalSpace = true;//assume it is local space
                if (modelRef.cosmeticSkel.cosmeticBones[i].boneMovementSpace == CosmeticSkeletonBone.MovementSpace.World) {
                    isLocalSpace = false;
                }
                bonePoses[i].value = new BoneTransform(cosBone.boneTf, isLocalSpace);
                
                //ENCODE BONE LENGTHS
                if (modelRef.cosmeticSkel.skeletonRoot == cosBone.boneTf) {
                    //HACK: predefine bone length of the root of the Skeleton to 0.5f meters
                    bonePoses[i].sqrtBoneLength = Mathf.Sqrt(0.5f);
                }
                else {
                    //bone length should be the magnitude of the local position
                    bonePoses[i].sqrtBoneLength = Mathf.Sqrt(cosBone.boneTf.localPosition.magnitude);
                }

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

            //motionPose.rootMotionInfo = new BonePose("RootMotion") {value = new BoneTransform(Vector3.zero, Quaternion.identity, Vector3.one) };
            //return;

        //ADJUST FOR HIP OFFSET
            switch (calculationMode) {
                case RootMotionCalculationMode.ReferencePoint:
                    //TODO: Implement - Reference Point Root Motion
                    break;

                case RootMotionCalculationMode.CenterOfMass:

                    //RECORD IMPORTANT POINTS FOR READABILITY
                    //get the Bone Pose associated with the transform assigned as the Skeleton Root Bone
                    Transform refPointTf = modelRef.cosmeticSkel.rootMotionReferencePoint;
                    Transform skelRootTf = modelRef.cosmeticSkel.skeletonRoot;
                    BonePose skelRootBone = motionPose.GetBonePose(modelRef.cosmeticSkel.GetBone(skelRootTf).boneLabel);

                    //Debug.Log("Bone Label: " + skeletonRootBonePose.boneLabel);

                    HumanPose hPose = GetHumanPose(modelRef, animClip, timestamp);




                    if (!AnimationMode.InAnimationMode()) { AnimationMode.StartAnimationMode(); }
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(modelRef.gameObject, animClip, timestamp);

                    Vector3 newLocalPos = skelRootTf.localPosition;
                    Quaternion newLocalRot = skelRootTf.localRotation;

                    Vector3 localBodyPos = skelRootTf.InverseTransformPoint(hPose.bodyPosition);
                    Vector3 localRefPos = skelRootTf.InverseTransformPoint(refPointTf.position);

                    //HACK: Duplicate of Floored Center of Mass??
                    Vector3 flooredLocalBodyPos = new Vector3(localBodyPos.x,
                                                              localRefPos.y,
                                                              localBodyPos.z);


                    Vector3 flooredCenterOfMass = new Vector3(hPose.bodyPosition.x,
                                                              refPointTf.position.y,
                                                              hPose.bodyPosition.z);

                    Vector3 adjustmentDirec = refPointTf.position - flooredCenterOfMass;

                    //selfScript.transform.position += adjustmentDirec;

                    //newLocalPos = skelRootTf.localPosition + skelRootTf.InverseTransformDirection(adjustmentDirec); //skelRootTf.localPosition;
                    newLocalPos = refPointTf.InverseTransformPoint(skelRootTf.position + adjustmentDirec);


                //ADJUST FOR ROTATION OFFSET

                    //floor out the two rotations to get only Yaw (XZ Plane) component
                    Quaternion bodyRot_Floored = Quaternion.LookRotation(Vector3.ProjectOnPlane(hPose.bodyRotation * Vector3.forward, Vector3.up).normalized, Vector3.up);
                    Quaternion refRot_Floored = Quaternion.LookRotation(Vector3.ProjectOnPlane(refPointTf.rotation * Vector3.forward, Vector3.up).normalized, Vector3.up);

                    //raw angle between two floored rotatoins (this is always positive)
                    float adjustmentAngle = Quaternion.Angle(bodyRot_Floored, refRot_Floored);

                    //calculate a plane that uses the floored reference point's rotation's right vector as the normal
                    Vector3 rightOfFlooredRefRot = Vector3.Cross(refRot_Floored * Vector3.forward, Vector3.up);
                    Plane testPlane = new Plane(rightOfFlooredRefRot, hPose.bodyPosition);

                    //use plane to determine direction of rotation (if we're oriented to the positive side, we need to rotate left, so we multiply by -1.0f)
                    if(!testPlane.GetSide(flooredCenterOfMass + (bodyRot_Floored * Vector3.forward))) { adjustmentAngle *= -1.0f; }

                    //HACK: This may break other calculatoins b/c it makes things dirty
                    skelRootTf.RotateAround(hPose.bodyPosition,
                                            Vector3.up,
                                            adjustmentAngle);

                    newLocalRot = skelRootTf.localRotation;

                    //Quaternion adjRot = flooredRefRot * Quaternion.Inverse(flooredBodyRot);

                    //selfScript.transform.rotation = adjRot; 


                    //selfScript.transform.rotation = flooredBodyRot; 


                    //Quaternion newHipsRot = selfScript.transform.rotation * Quaternion.Inverse(flooredBodyRot); 
                    //Quaternion newHipsRot = flooredBodyRot * Quaternion.Inverse(selfScript.transform.rotation); 

                    //selfScript.transform.rotation = newHipsRot; 

                    //Quaternion originalBodyRot 

                    //Plane testPlane = new Plane(hPose.bodyPosition, refPointTf.position);

                    //if(testPlane.GetSide(flooredBodyRot * Vector3.forward)) { adjustmentAngle *= -1.0f; }

                    ////HACK: This may break other calculatoins b/c it makes things dirty
                    //skelRootTf.RotateAround(hPose.bodyPosition,
                    //                        Vector3.up,
                    //                        adjustmentAngle);


                    //newLocalRot = skelRootTf.localRotation;

                    //AnimationMode.EndSampling();
                    //AnimationMode.StopAnimationMode();


                    skelRootBone.value = new BoneTransform(newLocalPos, newLocalRot, skelRootTf.localScale);

                    break;
            }
            

            //we're setting the root motion for the first frame to zero, just set it here and break out
            if (frameHandling == RootMotionFrameHandling.SetFirstFrameToZero && Mathf.Approximately(timestamp, 0.0f)) {
                motionPose.rootMotionInfo = new BonePose("RootMotion") { value = new BoneTransform(Vector3.zero, Quaternion.identity) };
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
            HumanPose prevHumanPose = GetHumanPose(modelRef, animClip, timestamp - frameStep);//this will store the previous frame's pose
            HumanPose curHumanPose = GetHumanPose(modelRef, animClip, timestamp);//this will store the current frame's pose

            //Debug.Log(modelRef.cosmeticSkel.rootMotionReferencePoint.rotation);

        //HACK: assume that we're using the XZ plane for root motion, flatten out the Y to the Reference Point's Y
            //Vector3 projectedCenterOfMass = prevHumanPose.bodyPosition;
            //projectedCenterOfMass.y = modelRef.cosmeticSkel.rootMotionReferencePoint.position.y;

            //HACK: get out the body position and rotation for debuggin purposes
            bool isAlmostOne = Mathf.Approximately(timestamp, 0.1f);
            if (timestamp == 0.06666667f || isAlmostOne) {
                Debug.Log("TStamp: " + timestamp);

                Vector3 pos = isAlmostOne ? curHumanPose.bodyPosition : prevHumanPose.bodyPosition;
                Quaternion rot = isAlmostOne ? curHumanPose.bodyRotation : prevHumanPose.bodyRotation;

                Debug.LogFormat("{0}:\nTimestamp {1}: Position: ({2}, {3}, {4})\tRotation: {5}", motionPose.animName,
                                                                                     motionPose.timestamp,
                                                                                     pos.x, pos.y, pos.z,
                                                                                     rot.debugString());
            }

            Quaternion adjustmentQuat = modelRef.cosmeticSkel.rootMotionReferencePoint.rotation * Quaternion.Inverse(prevHumanPose.bodyRotation);
            Vector3 positionMotion = Vector3.ProjectOnPlane(adjustmentQuat * (curHumanPose.bodyPosition - prevHumanPose.bodyPosition), Vector3.up);


            Quaternion curBodyRot_Flat = Quaternion.LookRotation(Vector3.ProjectOnPlane(curHumanPose.bodyRotation * Vector3.forward, Vector3.up));
            Quaternion prevBodyRot_Flat = Quaternion.LookRotation(Vector3.ProjectOnPlane(prevHumanPose.bodyRotation * Vector3.forward, Vector3.up));

            Quaternion rotationMotion = curBodyRot_Flat * Quaternion.Inverse(prevBodyRot_Flat);

            //Quaternion rotationMotion = curHumanPose.bodyRotation * Quaternion.Inverse(prevHumanPose.bodyRotation); //prevHumanPose.bodyRotation * Quaternion.Inverse(curHumanPose.bodyRotation);
            //project the rotation onto a plane by using it to modify a vector and then generating a quaternion out from that
            //rotationMotion = Quaternion.LookRotation(Vector3.ProjectOnPlane(rotationMotion * Vector3.forward, Vector3.up), Vector3.up);

            motionPose.rootMotionInfo = new BonePose("RootMotion") { value = new BoneTransform(positionMotion, rotationMotion) };

            //if (Mathf.Approximately(timestamp, frameStep)) {
                //GameObject.Instantiate(modelRef.cosmeticSkel.marker.gameObject, curHumanPose.bodyPosition, curHumanPose.bodyRotation);
            //}
        }

        public static HumanPose GetHumanPose(MotionFieldComponent modelRef, AnimationClip animClip, float timestamp)
        {
            //allocate space to store the human pose of the model during ANIMATION SAMPLING MODE
            HumanPose hPose = new HumanPose();

            //ACTIVATE ANIMATION SAMPLING MODE
            if (!AnimationMode.InAnimationMode()) { AnimationMode.StartAnimationMode(); }
            AnimationMode.BeginSampling();

            //sample pose and store in the Human Pose we allocated earlier
            HumanPoseHandler hPoseHandler = new HumanPoseHandler(modelRef.cosmeticSkel.avatar, modelRef.cosmeticSkel.skeletonRoot);
            AnimationMode.SampleAnimationClip(modelRef.gameObject, animClip, timestamp);
            hPoseHandler.GetHumanPose(ref hPose);

            AnimationMode.EndSampling();
            AnimationMode.StopAnimationMode();

            return hPose;
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

            //make KD Tree w/ number of dimension equal to total number of bone poses * (position * velocity) <- 14
            int KeyLength = 0;
            for (int i = 0; i < animClipInfoList.Count; i++)
            {
                if(animClipInfoList[i].useClip == true)
                {
                    if(animClipInfoList[i].motionPoses.Length != 0)
                    {
                        KeyLength = animClipInfoList[i].motionPoses[0].bonePoses.Length * 6; //HACK: 6 is the magic number of values in flattened bonePose. Its the directed rotation of value and positionNext.
                        break;
                    }
                }
            }
            if (KeyLength == 0)
            {
                Debug.LogError("Cannot Populate KDTree because no poses were generated! Try lowering the Frame Resolution and try again."); //this error happened once, then went away...
                return;
            }

            KeyLength += 6; //HACK: adding magic number of flattened rootMotionInfo. Its the position and directed rotation of PositionNext, unlike other bones.

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