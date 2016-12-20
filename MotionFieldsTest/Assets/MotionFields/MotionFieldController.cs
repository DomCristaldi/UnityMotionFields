﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using AnimationMotionFields;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif



public enum VelocityCalculationMode {
    DropLastTwoFrames = 0,
    LoopToFirstFrame = 1,
    UseVelocityFromSecondToLastFrame = 2,
    SetLastFrameToZero = 3,
}

public enum RootMotionCalculationMode {
    CenterOfMass = 0,
    ReferencePoint = 1,
}

public enum RootMotionFrameHandling {
    SetFirstFrameToZero = 0,
    DropFirstFrame = 1,
    LoopToFirstFrame = 2,
}


[System.Serializable]
public class BoneTransform {
    public float posX, posY, posZ,
                 rotW, rotX, rotY, rotZ;

    //Initialize everyting to default values
    public BoneTransform() {
        this.posX = this.posY = this.posZ = this.rotX = this.rotY = this.rotZ = 0.0f;
        this.rotW = 1.0f;
    }

    public BoneTransform(Vector3 position, Quaternion rotation) {
        this.posX = position.x;
        this.posY = position.y;
        this.posZ = position.z;

        this.rotW = rotation.w;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;
    }

    //Initialize everything to the same constant value (useful for setting velocity to 0)
    public BoneTransform(float constant) {
        this.posX = this.posY = this.posZ
      = this.rotW = this.rotX = this.rotY = this.rotZ
      = constant;
    }

    //Initialize with positional information (WARNING: Lossy Scale must be used for world scale)
    public BoneTransform(Transform tf, bool isLocal = true) {

        if (isLocal) {
            this.posX = tf.localPosition.x;
            this.posY = tf.localPosition.y;
            this.posZ = tf.localPosition.z;

            this.rotW = tf.localRotation.w;
            this.rotX = tf.localRotation.x;
            this.rotY = tf.localRotation.y;
            this.rotZ = tf.localRotation.z;
        }
        else {//ASSUME WORLD
            //Debug.Log("world");

            this.posX = tf.position.x;
            this.posY = tf.position.y;
            this.posZ = tf.position.z;

            //HACK: projecting onto the plane of the transform's forward vector may not be the best idea here
            Quaternion quat = new Quaternion(tf.rotation.x, tf.rotation.y, tf.rotation.z, tf.rotation.w);
            Vector3 forVec = quat * Vector3.forward;
            quat = Quaternion.LookRotation(Vector3.ProjectOnPlane(forVec, Vector3.up));

            this.rotW = quat.w;
            this.rotX = quat.x;
            this.rotY = quat.y;
            this.rotZ = quat.z;

            /*
            rotW = tf.rotation.w;
            rotX = tf.rotation.x;
            rotY = tf.rotation.y;
            rotZ = tf.rotation.z;
            */
        }
    }

    //Used for creating a copy
    public BoneTransform(BoneTransform copy) {
        this.posX = copy.posX;
        this.posY = copy.posY;
        this.posZ = copy.posZ;

        this.rotW = copy.rotW;
        this.rotX = copy.rotX;
        this.rotY = copy.rotY;
        this.rotZ = copy.rotZ;
    }

    /// <summary>
    ///  'Subtracts' the fields of the two bone transforms.
    ///  <para>Not implemented as an operator because it has special behavior </para>
    /// </summary>
    public static BoneTransform Subtract(BoneTransform b2, BoneTransform b1)
    {
        return new BoneTransform(b2.position - b1.position, b2.rotation * Quaternion.Inverse(b1.rotation));
    }

    /// <summary>
    ///  'Adds' the fields of the two bone transforms.
    ///  <para>Not implemented as an operator because it has special behavior (NOT COMMUTATIVE)</para>
    /// </summary>
    public static BoneTransform Add(BoneTransform b1, BoneTransform b2)
    {
        return new BoneTransform(b1.position + b2.position, b1.rotation * b2.rotation);
    }

    public float[] flattenedTransform(float sqrtBonelength) {
            Vector3 vec = rotation * Vector3.forward;
            return new float[] { vec.x * sqrtBonelength, vec.y * sqrtBonelength, vec.z * sqrtBonelength };
    }

    public Vector3 position
    {
        get{
            return new Vector3(posX, posY, posZ);
        }
    }

    public float[] positionArray()
    {
        return new float[] { posX, posY, posZ };
    }


    public Quaternion rotation
    {
        get{
            return new Quaternion(rotX, rotY, rotZ, rotW);
        }
    }

    public static BoneTransform BlendTransform(BoneTransform tf1, BoneTransform tf2, float alpha) {

        Vector3 newPos = Vector3.Lerp(tf1.position, tf2.position, alpha);
        Quaternion newRot = Quaternion.Slerp(tf1.rotation, tf2.rotation, alpha);

        return new BoneTransform(newPos, newRot);
    }

    public static BoneTransform BlendTransforms(BoneTransform[] trans, float[] weights)
    {
        Vector3 avgPos = Vector3.zero;
        Quaternion avgRot = new Quaternion(0, 0, 0, 0);
        Quaternion first = trans[0].rotation;

        for (int i = 0; i < trans.Length; ++i)
        {
            avgPos += (trans[i].position * weights[i]);

            //if the dot product is negaticve, negate quats[i] so that it exists on the same half-sphere. 
            //This is allowed because q = -q, and is nessesary because the error of aproximation increases the farther apart the quaternions are.
            if (Quaternion.Dot(trans[i].rotation, first) > 0.0f){
                avgRot.w += (trans[i].rotation.w * weights[i]);
                avgRot.x += (trans[i].rotation.x * weights[i]);
                avgRot.y += (trans[i].rotation.y * weights[i]);
                avgRot.z += (trans[i].rotation.z * weights[i]);
            }
            else{
                avgRot.w -= (trans[i].rotation.w * weights[i]);
                avgRot.x -= (trans[i].rotation.x * weights[i]);
                avgRot.y -= (trans[i].rotation.y * weights[i]);
                avgRot.z -= (trans[i].rotation.z * weights[i]);
            }
        }

        //note: is normalizing avgPos and avgScl nessesary? avgPos is not nessesarily a unit vector, but they should all be the same length, and avgPos may not be that length?

        //Normalize the result to a unit Quaternion
        avgRot = avgRot.Normalize();

        return new BoneTransform(avgPos, avgRot);
    }

}

[System.Serializable]
public class BonePose {
    public string boneLabel;
    public float sqrtBoneLength;

    public BoneTransform value;
    public BoneTransform positionNext;
    public BoneTransform positionNextNext;

    public BonePose(string boneLabel) {
        this.boneLabel = boneLabel;
    }

    public float[] flattenedValue()
    {
        return value.flattenedTransform(sqrtBoneLength);
    }

    public float[] flattenedPositionNext()
    {
        return positionNext.flattenedTransform(sqrtBoneLength);
    }
}

[System.Serializable]
public class MotionPose {

    public BonePose[] bonePoses;

    public BonePose rootMotionInfo;

    public string animName;
    public float timestamp;

    //public float[] keyframeData;
    //public KeyframeData[] keyframeData;

    public MotionPose(BonePose[] bonePoses, BonePose rootMotionInfo) {
        this.bonePoses = bonePoses;
        this.rootMotionInfo = rootMotionInfo;
    }


    //NEW
    public MotionPose(BonePose[] bonePoses, string animName, float timestamp) {
        this.bonePoses = bonePoses;
        this.animName = animName;
        this.timestamp = timestamp;
    }


    //CONSTRUCTOR FOR CREATING A MOTION POSE OUT OF BLENED POSES
    public MotionPose(MotionPose[] posesToBlend, float[] weights) {

        //Break out if there's no data to work with for either poses or weights
        if (posesToBlend.Length == 0) { Debug.LogError("Supplied Poses Array is of length 0"); return; }
        if (weights.Length == 0) { Debug.LogError("Supplied Weights Array is of length 0"); return; }
        if (posesToBlend.Length != weights.Length) { Debug.LogError("The number of poses does not match the number of weigts"); return; }

        BoneTransform[] BoneValues = new BoneTransform[posesToBlend.Length];
        BoneTransform[] BonePosNexts = new BoneTransform[posesToBlend.Length];
        BoneTransform[] BonePosNextNexts = new BoneTransform[posesToBlend.Length];
        int i, j;

        for (i = 0; i < posesToBlend.Length; ++i)
        {
            BoneValues[i] = posesToBlend[i].rootMotionInfo.value;
            BonePosNexts[i] = posesToBlend[i].rootMotionInfo.positionNext;
            BonePosNextNexts[i] = posesToBlend[i].rootMotionInfo.positionNextNext;
        }
        BonePose newRootMotion = new BonePose(posesToBlend[0].rootMotionInfo.boneLabel);
        newRootMotion.value = BoneTransform.BlendTransforms(BoneValues, weights);
        newRootMotion.positionNext = BoneTransform.BlendTransforms(BonePosNexts, weights);
        newRootMotion.positionNextNext = BoneTransform.BlendTransforms(BonePosNextNexts, weights);

        BonePose[] newPoseBones = new BonePose[posesToBlend[0].bonePoses.Length];
        for (i = 0; i < posesToBlend[0].bonePoses.Length; ++i)
        {
            for (j = 0; j < posesToBlend.Length; ++j)
            {
                BoneValues[j] = posesToBlend[j].bonePoses[i].value;
                BonePosNexts[j] = posesToBlend[j].bonePoses[i].positionNext;
                BonePosNextNexts[j] = posesToBlend[j].bonePoses[i].positionNextNext;
            }
            newPoseBones[i] = new BonePose(posesToBlend[0].bonePoses[i].boneLabel);
            newPoseBones[i].value = BoneTransform.BlendTransforms(BoneValues, weights);
            newPoseBones[i].positionNext = BoneTransform.BlendTransforms(BonePosNexts, weights);
            newPoseBones[i].positionNextNext = BoneTransform.BlendTransforms(BonePosNextNexts, weights);
        }

        this.bonePoses = newPoseBones;
        this.rootMotionInfo = newRootMotion;
    }

    public float[] flattenedMotionPose {
        //the initial 'position' of the rootmotion is not factored in, just the velocity from value(position 1) to positionnext(position 2), which is currently stored as the values for the root in positionnext
        get {
            var retArray = rootMotionInfo.positionNext.positionArray().Concat<float>(rootMotionInfo.flattenedPositionNext());
            int length = bonePoses.Length;
            for (int i = 0; i < length; ++i)
            {
                retArray = retArray.Concat<float>(bonePoses[i].flattenedValue()
                                   .Concat<float>(bonePoses[i].flattenedPositionNext()));
            }
            return retArray.ToArray();
        }
    }

    //RETRIEVE THE BONE POSE WITH THE SPECIFIED LABEL
    public BonePose GetBonePose(string label) { //TODO: update to Dictionary lookukp
        foreach (BonePose pose in bonePoses) {
            if (pose.boneLabel == label) {

                return pose;

            }
        }

        //no bone pose with that label was found, return Null
        return null;
    }

}

[System.Serializable]
public class AnimClipInfo {
    public bool useClip = true;
    public VelocityCalculationMode velocityCalculationMode;
    public RootMotionCalculationMode rootMotionCalculationMode;
    public RootMotionFrameHandling rootMotionFrameHandling;
    public bool looping = false;
    public AnimationClip animClip;

    public MotionPose[] motionPoses;//all the poses generated for this animation clip

    public int frameResolution;//sample rate that was used to create these poses
    public float frameStep; //time in seconds between every sampled frame of the clip

    public void PrintPathTest() {
        foreach (EditorCurveBinding ecb in AnimationUtility.GetCurveBindings(animClip)) {
            Debug.Log("path " + ecb.propertyName);
        }
    }
}


[CreateAssetMenu]
public class MotionFieldController : ScriptableObject {

    public List<AnimClipInfo> animClipInfoList;

    public KDTreeDLL_f.KDTree kd;

	public TaskArrayInfo TArrayInfo;

    public int numActions = 1;

    private float startingReward = float.MaxValue;
    //DEBUG
    public string currentTaskOutput;

    /*
    //legacy from motion fields
    [Range(0.0f, 1.0f)]
    public float driftCorrection = 0.1f;
    */

	public candidatePose[] OneTick(MotionPose currentPose){

        float[] taskArr = GetTaskArray();
        //Debug.Log("task Length: " + taskArr.Length.ToString());

        candidatePose[] newPoses = MoveOneFrame(currentPose, taskArr);

        //Debug.Log("root motion of chosen pose:\n posX: " + newPose.rootMotionInfo.value.posX + "  posY: " + newPose.rootMotionInfo.value.posY + "  posZ: " + newPose.rootMotionInfo.value.posZ);

        return newPoses;
	}

    public candidatePose[] MoveOneFrame(MotionPose currentPose, float[] taskArr)
    {
        //Debug.Log("Move One Frame pose before GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        candidatePose[] candidateActions = GenerateCandidateActions(currentPose);

        //Debug.Log("Move One Frame pose after GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        RankCandidates(currentPose, ref candidateActions, taskArr);

        return candidateActions;
    }

    private candidatePose[] GenerateCandidateActions(MotionPose currentPose)
    {
        //generate candidate states to move to by finding closest poses in kdtree
        float[] currentPoseArr = currentPose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor(currentPoseArr);

        candidatePose[] candidates = new candidatePose[neighbors.Length];
        for (int i = 0; i < candidates.Length; ++i)
        {
            candidates[i] = new candidatePose(neighbors[i]);
        }

        return candidates;
    }

    private candidatePose[] RankCandidates(MotionPose currentPose, ref candidatePose[] candidateActions, float[] taskArr) {

        for (int i = 0; i < candidateActions.Length; ++i) {
            ComputeReward(currentPose, ref candidateActions[i], taskArr); //sets the reward for that candidate
        }

        Array.Sort(candidateActions);
        return candidateActions;
    }

    private MotionPose[] NearestNeighbor(float[] pose){
        object[] nn_data = kd.nearest (pose, numActions);
        
        MotionPose[] data = new MotionPose[nn_data.Length];
        for(int i = 0; i < nn_data.Length; ++i)
        {
            data[i] = (MotionPose)nn_data[i];
        }
        return data;
    }

	private float[] GenerateWeights(float[] ideal, float[][] neighbors){

		float[] weights = new float[neighbors.Length];
        float diff;
        float weightsSum = 0;
        int i, j;

		//weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
		for(i = 0; i < neighbors.Length; i++){
			weights [i] = 0.0f;
			for(j = 0; j < ideal.Length; j++){
                diff = ideal[j] - neighbors[i][j];
				weights [i] += diff*diff;
			}
			weights [i] = 1.0f / weights [i];
            weightsSum += weights[i];

            if (float.IsInfinity(weights[i])) { //special case where a neighbor is identical to ideal.
                for (j = 0; j < weights.Length; j++) {
                    if (j == i) {
                        weights[j] = 1.0f;
                    }
                    else {
                        weights[j] = 0.0f;
                    }
                }
                return weights;
            }
        }

        //now normalize weights so that they sum to 1
        for (i = 0; i < weights.Length; i++) {
            weights[i] = weights[i] / weightsSum;
        }
        //Debug.Log("weights: " + string.Join(" ", weights.Select(w => w.ToString()).ToArray()));
        return weights;
	}

    private float[] GetTaskArray(){
        //current value of task array determined by world params
		int tasklength = TArrayInfo.TaskArray.Count;
		float[] taskArr = new float[tasklength];
		for(int i = 0; i < tasklength; i++){
			taskArr[i] = TArrayInfo.TaskArray[i].DetermineTaskValue();
		}
		return taskArr;
	}

	private void ComputeReward(MotionPose pose, ref candidatePose newPose, float[] taskArr){
        //first calculate immediate reward
		float immediateReward = 0.0f;

        for(int i = 0; i < taskArr.Length; i++){
			float taskReward = TArrayInfo.TaskArray[i].CheckReward (pose, newPose.pose, taskArr[i]);

            //Debug.LogFormat("Task: {0} - Value: {1}", TArrayInfo.TaskArray[i].name, taskReward);

            immediateReward += taskReward;
        }
        newPose.reward = immediateReward;
    }
}

public class candidatePose : IComparable<candidatePose>
{
    private MotionPose _pose;
    private float _reward;

    public candidatePose(MotionPose pose)
    {
        this._pose = pose;
    }

    public candidatePose(MotionPose pose, float reward)
    {
        this._pose = pose;
        this._reward = reward;
    }

    public MotionPose pose
    {
        get { return _pose; }
    }
    public float reward
    {
        get { return _reward; }
        set { _reward = value; }
    }

    public int CompareTo(candidatePose P)
    {
        if (P == null || this.reward > P.reward) return 1;

        else if(this.reward == P.reward) return 0;

        else return -1;
    }
}


/*
#if UNITY_EDITOR
[CustomEditor(typeof(SO_MotionField))]
public class SO_MotionField_Editor : Editor {

    private ReorderableList reorderAnimList;


    void OnEnable() {
        reorderAnimList = new ReorderableList(serializedObject, serializedObject.FindProperty("animClipInfoList"), true, true, true, true);

        reorderAnimList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = reorderAnimList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("useClip"),
                                    GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("animClip"),
                                    GUIContent.none);



        };


        reorderAnimList.elementHeightCallback = (index) => {
            //return 20;

            var element = reorderAnimList.serializedProperty.GetArrayElementAtIndex(index);
            if (element.FindPropertyRelative("animClip") == null) {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUIUtility.singleLineHeight * 3.0f;
        };

    }


    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();

        serializedObject.Update();
        reorderAnimList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();



    }

}
#endif
*/

//}
