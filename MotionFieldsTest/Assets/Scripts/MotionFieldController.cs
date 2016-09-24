using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
        /*BoneTransform retBoneTf = new BoneTransform();

        retBoneTf.posX = Mathf.Lerp(tf1.posX, tf2.posX, alpha);
        retBoneTf.posY = Mathf.Lerp(tf1.posY, tf2.posY, alpha);
        retBoneTf.posZ = Mathf.Lerp(tf1.posZ, tf2.posZ, alpha);

        Quaternion slerpedRot = Quaternion.Slerp(new Quaternion(tf1.rotX, tf1.rotY, tf1.rotZ, tf1.rotW),
                                                 new Quaternion(tf2.rotX, tf2.rotY, tf2.rotZ, tf2.rotW),
                                                 alpha);
        retBoneTf.rotW = slerpedRot.w;
        retBoneTf.rotX = slerpedRot.x;
        retBoneTf.rotY = slerpedRot.y;
        retBoneTf.rotZ = slerpedRot.z;

        retBoneTf.sclX = Mathf.Lerp(tf1.sclX, tf2.sclX, alpha);
        retBoneTf.sclY = Mathf.Lerp(tf1.sclY, tf2.sclY, alpha);
        retBoneTf.sclZ = Mathf.Lerp(tf1.sclZ, tf2.sclZ, alpha);

        return retBoneTf;*/
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

        /*
        //set initial bone pose array and rootMotionBone to that of the first pose to blend
        BonePose newRootMotion =new BonePose(posesToBlend[0].rootMotionInfo.boneLabel);
        newRootMotion.value = new BoneTransform(posesToBlend[0].rootMotionInfo.value);
        newRootMotion.positionNext = new BoneTransform(posesToBlend[0].rootMotionInfo.positionNext);
        newRootMotion.positionNextNext = new BoneTransform(posesToBlend[0].rootMotionInfo.positionNextNext);

        BonePose[] newPoseBones = new BonePose[posesToBlend[0].bonePoses.Length];
        for(int j = 0; j < posesToBlend[0].bonePoses.Length; ++j)
        {
            BonePose newBone = new BonePose(posesToBlend[0].bonePoses[j].boneLabel);
            newBone.value = new BoneTransform(posesToBlend[0].bonePoses[j].value);
            newBone.positionNext = new BoneTransform(posesToBlend[0].bonePoses[j].positionNext);
            newBone.positionNextNext = new BoneTransform(posesToBlend[0].bonePoses[j].positionNextNext);
            newPoseBones[j] = newBone;
        }
        //represents the amount we've blended in so far
        float curBoneWeight = weights[0];

        for (int i = 1; i < posesToBlend.Length; ++i) {
            //create normalized weights for tiered blending
            float bpwNormalized = curBoneWeight / (curBoneWeight + weights[i]);
            //float wiNormalized = weights[i] / (curBoneWeight + weights[i]);

            newRootMotion.value = BoneTransform.BlendTransform(newRootMotion.value, posesToBlend[i].rootMotionInfo.value, bpwNormalized);
            newRootMotion.positionNext = BoneTransform.BlendTransform(newRootMotion.positionNext, posesToBlend[i].rootMotionInfo.positionNext, bpwNormalized);
            newRootMotion.positionNextNext = BoneTransform.BlendTransform(newRootMotion.positionNextNext, posesToBlend[i].rootMotionInfo.positionNextNext, bpwNormalized);

            for (int j = 0; j < posesToBlend[i].bonePoses.Length; ++j) {
                //do the blending
                newPoseBones[j].value = BoneTransform.BlendTransform(newPoseBones[j].value, posesToBlend[i].bonePoses[j].value, bpwNormalized);
                newPoseBones[j].positionNext = BoneTransform.BlendTransform(newPoseBones[j].positionNext, posesToBlend[i].bonePoses[j].positionNext, bpwNormalized);
                newPoseBones[j].positionNextNext = BoneTransform.BlendTransform(newPoseBones[j].positionNextNext, posesToBlend[i].bonePoses[j].positionNextNext, bpwNormalized);
            }
            //add to the weight we iterated so far
            curBoneWeight += weights[i];
        }*/

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

	private Dictionary<vfKey, float> precomputedRewards;

	//using an ArrayList because Unity is dumb and doesn't have tuples.
	//each arralist should holds a MotionPose in [0] a float[] in [1], and a float in [2]
	//since ArrayList stores everything as object, must cast it when taking out data
	[HideInInspector]
	public List<precomputedRewards_Initializer_Element> precomputedRewards_Initializer;

    //how much to prefer the immediate reward vs future reward. 
    //reward = r(firstframe) + scale*r(secondframe) + scale^2*r(thirdframe) + ... ect
    //close to 0 has higher preference on early reward. closer to 1 has higher preference on later reward
    //closer to 1 also asymptotically increases time to generate precomputed rewards, so its recommended you dont set it too high. 
    [Range(0.0f,0.999f)]
    public float scale = 0.5f; 

    public int numActions = 1;

    private float startingReward = float.MaxValue;
    //DEBUG
    public string currentTaskOutput;

    /*
    //legacy from motion fields
    [Range(0.0f, 1.0f)]
    public float driftCorrection = 0.1f;
    */

	public MotionPose OneTick(MotionPose currentPose){

        float[] taskArr = GetTaskArray();
        //Debug.Log("task Length: " + taskArr.Length.ToString());

        //float reward = float.MinValue;

        float reward = 0.0f;
        MotionPose newPose = MoveOneFrame(currentPose, taskArr, ref reward);

        //Debug.Log("root motion of chosen pose:\n posX: " + newPose.rootMotionInfo.value.posX + "  posY: " + newPose.rootMotionInfo.value.posY + "  posZ: " + newPose.rootMotionInfo.value.posZ);

        return newPose;
	}

    public MotionPose MoveOneFrame(MotionPose currentPose, float[] taskArr, ref float reward)
    {
        //Debug.Log("Move One Frame pose before GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        MotionPose[] candidateActions = GenerateCandidateActions(currentPose);

        //Debug.Log("Move One Frame pose after GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        int chosenAction = PickCandidate(currentPose, candidateActions, taskArr, ref reward);

        //Debug.Log("Candidate Chosen! best fitness is " + reward.ToString() + " from Action " + chosenAction.ToString());

        return candidateActions[chosenAction];
         
    }

    private MotionPose[] GenerateCandidateActions(MotionPose currentPose)
    {
        //generate candidate states to move to by finding closest poses in kdtree
        float[] currentPoseArr = currentPose.flattenedMotionPose;

        return NearestNeighbor(currentPoseArr);

        /*
        //legacy from motion fields
        MotionPose[] neighbors = NearestNeighbor(currentPoseArr);
        float[] weights = GenerateWeights(currentPose, neighbors);
        float[][] actionWeights = GenerateActionWeights(weights);
        MotionPose[] candidateActions = new MotionPose[actionWeights.Length];
        for(int i = 0; i < actionWeights.Length; ++i){
            candidateActions[i] = GeneratePose(currentPose, neighbors, 0,  actionWeights[i]); //0 is the index in neighbors of the neighbor which is closest to the current pose. Because of how the kdtree works, the closest neighbor pose will ALWAYS be at index 0, hence the magic number. sorry.
            candidateActions[i].animName = neighbors[i].animName;
            candidateActions[i].timestamp = neighbors[i].timestamp;
        }
        return candidateActions;
        */
    }

    private int PickCandidate(MotionPose currentPose, MotionPose[] candidateActions, float[] taskArr, ref float bestReward) {
        bestReward = startingReward;
        //choose the action with the highest reward
        int chosenAction = -1;

        for (int i = 0; i < candidateActions.Length; ++i) {
            float reward = ComputeReward(currentPose, candidateActions[i], taskArr);
            //Debug.Log("Reward for action " + i.ToString() + " is " + reward.ToString());
            if (reward < bestReward) {
                bestReward = reward;
                chosenAction = i;
            }
        }
        return chosenAction;
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

    /*
    //legacy from motion fields
    private float[][] GenerateActionWeights(float[] weights){
        int i, j;
        float[][] actions = new float[numActions][];
        float actionSum = 0.0f;

        for (i = 0; i < numActions; i++)
        {
            //for each action array, set weight[i] to 1 and renormalize
            actions[i] = new float[numActions];
            actionSum = 1.0f + (1.0f - weights[i]); //note sum of weights[] is 1.0f

            for (j = 0; j < numActions; j++)
            {
                if (i == j){
                    actions[i][j] = 1.0f / actionSum;
                }
                else {
                    actions[i][j] = weights[j] / actionSum;
                }
            }
        }
        return actions;
    }
    */


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

    /*
    //legacy from motion fields
    private float[] GenerateWeights(MotionPose pose, MotionPose[] neighbors)
    {
        //note: neighbors.Length == numActions
        BonePose[] Bones = pose.bonePoses;
        BonePose[] neighborBones = new BonePose[Bones.Length];
        float[] weights = new float[neighbors.Length];
        float weightsSum = 0.0f;
        int i, j;

        //weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
        for (i = 0; i < neighbors.Length; ++i)
        {
            neighborBones = neighbors[i].bonePoses;
            weights[i] = 0.0f;
            for (j = 0; j < pose.bonePoses.Length; ++j)
            {
                weights[i] += BoneDist(Bones[j].value, neighborBones[j].value, Bones[j].sqrtBoneLength);
                weights[i] += BoneDist(Bones[j].positionNext, neighborBones[j].positionNext, Bones[j].sqrtBoneLength);
            }
            weights[i] += RootBoneDist(pose.rootMotionInfo.positionNext, neighbors[i].rootMotionInfo.positionNext, pose.rootMotionInfo.sqrtBoneLength); //only velocity of root is considered, not the position.

            weights[i] = 1.0f / Mathf.Sqrt(weights[i]);
            weightsSum += weights[i];

            if (float.IsInfinity(weights[i])){
                for (j = 0; j < weights.Length; j++)
                {
                    if (j == i){
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
        for (i = 0; i < weights.Length; i++){
            weights[i] = weights[i] / weightsSum;
        }

        return weights;
    }
    */
    /*
    //legacy from motion fields
    private float BoneDist(BoneTransform b1, BoneTransform b2, float sqrtBonelength)
    {
        float sqDist = 0.0f;

        float[] b1Arr = b1.flattenedTransform(sqrtBonelength);
        float[] b2Arr = b2.flattenedTransform(sqrtBonelength);
        for(int i = 0; i < b1Arr.Length; ++i)
        {
            sqDist += ((b1Arr[i] - b2Arr[i]) * (b1Arr[i] - b2Arr[i]));
        }

        return sqDist;
    }
    */
    /*
    //legacy from motion fields
    private float RootBoneDist(BoneTransform b1, BoneTransform b2, float sqrtBonelength)
    {
        //rootBone is calculated differently as the difference in position is also factored in along eith difference in the rotation.
        float sqDist = 0.0f;

        float[] b1Arr = b1.flattenedTransform(sqrtBonelength);
        float[] b2Arr = b2.flattenedTransform(sqrtBonelength);
        for (int i = 0; i < b1Arr.Length; ++i)
        {
            sqDist += ((b1Arr[i] - b2Arr[i]) * (b1Arr[i] - b2Arr[i]));
        }

        sqDist += ((b1.posX - b2.posX) * (b1.posX - b2.posX)) * sqrtBonelength * sqrtBonelength;
        sqDist += ((b1.posY - b2.posY) * (b1.posY - b2.posY)) * sqrtBonelength * sqrtBonelength;
        sqDist += ((b1.posZ - b2.posZ) * (b1.posZ - b2.posZ)) * sqrtBonelength * sqrtBonelength;

        return sqDist;
    }
    */
    /*
    //legacy from motion fields
    private MotionPose GeneratePose(MotionPose currentPose, MotionPose[] neighbors, int closestNeighborIndex, float[] action){
        if(action[0] == 0.0f || action[0] == -0.0f)
        {
            Debug.LogError("This shouldnt happen and is bad.");
        }
        MotionPose blendedNeighbors = new MotionPose(neighbors, action);

        int numBones = currentPose.bonePoses.Length;

        BonePose newRootBone = GenerateRootBone(currentPose.rootMotionInfo, blendedNeighbors.rootMotionInfo, neighbors[closestNeighborIndex].rootMotionInfo);

        BonePose[] newPoseBones = new BonePose[numBones];
        for (int i = 0; i < numBones; i++)
        {
            newPoseBones[i] = GenerateBone(currentPose.bonePoses[i], blendedNeighbors.bonePoses[i], neighbors[closestNeighborIndex].bonePoses[i]);
        }

        MotionPose newPose = new MotionPose(newPoseBones, newRootBone);
        return newPose;
    }
    */
    /*
    //legacy from motion fields
    public BonePose GenerateBone(BonePose currBone, BonePose blendBone, BonePose closestBone)
    {
        //TODO: add drift correction with closestbone

        //note about quaternion math: v = x2 * x1^-1,     x2 = x1 * v,     x1 * v != v * x1  (quaternion math is not commutative)
        //b * (a * b^-1) == a, i believe
        
        //OLD broken logic:
        //new_position = currentPose.position + blendedNeighbors.positionNext - blendedNeighbors.position
        //new_positionNext = currentPose.position + blendedNeighbors.position + blendedNeighbors.positionNextNext - 2(blendedNeighbors.positionNext)
        
        //NEW logic: 
        //new_position = currentPose.position + (blendedNeighbors.positionNext - blendedNeighbors.position)
        //new_positionNext = new_position + (blendedNeighbors.positionNextNext - blendedNeighbors.positionNext)  
        
        BoneTransform V1 = BoneTransform.Subtract(blendBone.positionNext, blendBone.value);
        BoneTransform V2 = BoneTransform.Subtract(closestBone.positionNext, currBone.value);
        BoneTransform V = BoneTransform.BlendTransform(V1, V2, driftCorrection);
        

        BoneTransform Y1 = BoneTransform.Subtract(blendBone.positionNextNext, blendBone.positionNext);
        BoneTransform Y2 = BoneTransform.Subtract(closestBone.positionNextNext, closestBone.positionNext);
        BoneTransform Y = BoneTransform.BlendTransform(Y1, Y2, driftCorrection);

        BonePose newBone = new BonePose(currBone.boneLabel);

        newBone.value = BoneTransform.Add(currBone.value, V);
        newBone.positionNext = BoneTransform.Add(newBone.value, Y);

        return newBone;
    }
    */
    /*
    //legacy from motion fields
    public BonePose GenerateRootBone(BonePose currBone, BonePose blendBone, BonePose closestBone)
    {
        //TODO: add drift correction with closestbone

        //note about quaternion math: v = x2 * x1^-1,     x2 = x1 * v,     x1 * v != v * x1  (quaternion math is not commutative)
        //b * (a * b^-1) == a, i believe
        //root bone must be handled differently because it is stored as a displacement, not a position!
        //new_position = blendBone.positionNext  //note this works if currBone.value is either a velocity from previous frame or a value of 0, which are the current two modes for root calculation. If, somehow, currBone.value contains a non-zero position for root, change this to currBone.Value + blendBone.PositionNext
        //new_positionNext = blendBone.positionNextNext  
        
        BonePose newBone = new BonePose(currBone.boneLabel);

        //newBone.value = new BoneTransform(blendBone.positionNext);
        //newBone.positionNext = new BoneTransform(blendBone.positionNextNext);
        newBone.value = BoneTransform.BlendTransform(blendBone.positionNext, closestBone.positionNext, driftCorrection);
        newBone.positionNext = BoneTransform.BlendTransform(blendBone.positionNextNext, closestBone.positionNextNext, driftCorrection);

        return newBone;
    }
    */

        //TODO: Move out to a Math Utility static class
    public List<List<float>> CartesianProduct( List<List<float>> sequences){
		// base case: 
		List<List<float>> product = new List<List<float>>(); 
		product.Add (new List<float> ());
		foreach(List<float> sequence in sequences) 
		{ 
			// don't close over the loop variable (fixed in C# 5 BTW)
			List<float> s = sequence; 
			List<List<float>> newProduct = new List<List<float>> ();
			foreach (List<float> p in product){
				foreach (float item in s){
					newProduct.Add(p.Concat (new List<float> (new float[] { item })).ToList());
				}
			}
			product = newProduct;
		} 
		return product; 
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

	private float ComputeReward(MotionPose pose, MotionPose newPose, float[] taskArr){
        //first calculate immediate reward
		float immediateReward = 0.0f;

        for(int i = 0; i < taskArr.Length; i++){
			float taskReward = TArrayInfo.TaskArray[i].CheckReward (pose, newPose, taskArr[i]);

            Debug.LogFormat("Task: {0} - Value: {1}", TArrayInfo.TaskArray[i].name,
                                                      taskReward);

            immediateReward += taskReward;
        }

        //calculate continuousReward
        float continuousReward = ContRewardLookup(newPose, taskArr);

        //Debug.Log("Continuous Reward is " + continuousReward.ToString());

		return immediateReward + scale*continuousReward;
	}

	private float ContRewardLookup(MotionPose pose, float[] Tasks){
        //get continuous reward from valuefunc lookup table.
        //reward is weighted blend of closest values in lookup table.
        //get closest poses from kdtree, and closest tasks from cartesian product
        //then get weighted rewards from lookup table for each pose+task combo
        int i, j;

        /*
        //legacy from motion fields
        //get closest poses (now with pose matching, pose is already an existing pose, rather than a blend of poses, so getting nearest existing poses is no longer nessesary.)
        float[] poseArr = pose.flattenedMotionPose;
        MotionPose[] neighbors = NearestNeighbor (poseArr);
        float[] neighbors_weights = GenerateWeights(pose, neighbors);
        */

		//get closest tasks.
		List<List<float>> nearest_vals = new List<List<float>> ();
        float min, max, numSamples, interval;
		for(i=0; i < Tasks.Length; i++){

            List<float> nearest_val = new List<float> ();

            //dont change math on how tasks are sampled unless you know what your doing. must make equivalent changes when creating dict in MFEditorWindow
            min = TArrayInfo.TaskArray[i].min;
            max = TArrayInfo.TaskArray[i].max;
            numSamples = TArrayInfo.TaskArray[i].numSamples;
            interval = (max - min) / (numSamples - 1);

            //Debug.Log("interval for " + min.ToString() + " to " + max.ToString() + " for " + numSamples.ToString() + " samples is " + interval.ToString());
            float lower = Mathf.FloorToInt((Tasks[i] - min) / interval) * interval + min;
            nearest_val.Add (lower);
            if(lower != max && lower != Tasks[i])
            {
                nearest_val.Add(lower + interval);
            }
			nearest_vals.Add (nearest_val);
		}
        
        //turn the above/below vals for each task into 2^Tasks.Length() task arrays, each of which exists in precalculated dataset
        List<List<float>> nearestTasks = CartesianProduct(nearest_vals);
		float[][] nearestTasksArr = nearestTasks.Select(a => a.ToArray()).ToArray();

        /*
        string StrSampleTasks = "nearest sampled tasks to " + string.Join(" ", Tasks.Select(t => t.ToString()).ToArray()) + ":";
        for (int rr = 0; rr < nearestTasksArr.Count(); rr++)
        {
            StrSampleTasks += "\n" + string.Join(" ", nearestTasksArr[rr].Select(t => t.ToString()).ToArray());
        }
        Debug.Log(StrSampleTasks);
        */
        
        float[] dictKeys_weights = GenerateWeights(Tasks, nearestTasksArr);

		//get matrix of neighbors x tasks. The corresponding weight matrix should sum to 1.
		vfKey[] dictKeys = new vfKey[nearestTasksArr.Length];
        for (j = 0; j < nearestTasksArr.Length; j++) {
            dictKeys[j] = new vfKey(pose.animName, pose.timestamp, nearestTasksArr[j]);
        }

        /*
        legacy from motionfields
        float[] nearestTasks_weights = GenerateWeights(Tasks, nearestTasksArr);
        List<vfKey> dictKeys = new List<vfKey> ();
		List<float> dictKeys_weights = new List<float> ();
		for (i = 0; i < neighbors.Length; i++){
			for (j = 0; j < nearestTasksArr.Length; j++){
				dictKeys.Add (new vfKey(neighbors[i].animName, neighbors[i].timestamp, nearestTasksArr[j]));
				dictKeys_weights.Add (neighbors_weights [i] * nearestTasks_weights [j]);
			}
		}
        */

        //do lookups in precomputed table, get weighted sum
        float continuousReward = 0.0f;
		for(i = 0; i < dictKeys.Length; i++){
            //Debug.Log("lookup table vfkey:\nclipname: " + dictKeys[i].clipId + "\ntimestamp: " + dictKeys[i].timeStamp.ToString() + "\ntasks: " + string.Join(" ", dictKeys[i].tasks.Select(w => w.ToString()).ToArray()) + "\nhashcode: " + dictKeys[i].GetHashCode() + "\ncomponent hashcodes: " + dictKeys[i].clipId.GetHashCode() + "  " + dictKeys[i].timeStamp.GetHashCode() + "  (" + string.Join(" ", dictKeys[i].tasks.Select(w => w.GetHashCode().ToString()).ToArray()) + ")");
            try
            {
                continuousReward += precomputedRewards[dictKeys[i]] * dictKeys_weights[i];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Failed to find key in dict with params \nclipname: " + dictKeys[i].clipId + "\ntimestamp: " + dictKeys[i].timeStamp.ToString() + "\ntasks: " + string.Join(" ", dictKeys[i].tasks.Select(w => ((double)w).ToString()).ToArray()) + "\nhashcode: " + dictKeys[i].GetHashCode() + "\ncomponent hashcodes: " + dictKeys[i].clipId.GetHashCode() + "  " + dictKeys[i].timeStamp.GetHashCode() + " (" + string.Join(" ", dictKeys[i].tasks.Select(w => w.GetHashCode().ToString()).ToArray()) + ")");
            }
            
		}

        //Debug.Log("Continuous Reward Lookup complete, cont reward is " + continuousReward.ToString());

		return continuousReward;
	}

    public void makeDictfromList(List<ArrayList> lst)
    {
        precomputedRewards = new Dictionary<vfKey, float>();
        foreach (ArrayList arrLst in lst)
        {
            MotionPose mp = (MotionPose)arrLst[0];
            float[] taskarr = (float[])arrLst[1];
            vfKey newkey = new vfKey(mp.animName, mp.timestamp, taskarr);
            //Debug.Log("VFKEY ADDED:\nclipname: " + mp.animName + "\ntimestamp: " + mp.timestamp.ToString() + "\ntasks: " + string.Join(" ", taskarr.Select(w => ((double)w).ToString()).ToArray()) + "\nhashcode: " + newkey.GetHashCode() + "\ncomponent hashcodes: " + newkey.clipId.GetHashCode() + "  " + newkey.timeStamp.GetHashCode() + " (" + string.Join(" ", newkey.tasks.Select(w => w.GetHashCode().ToString()).ToArray()) + ")");
            precomputedRewards.Add(newkey, System.Convert.ToSingle(arrLst[2]));
        }
    }

    public void DeserializeDict()
    {
        precomputedRewards = new Dictionary<vfKey, float>();
        foreach (precomputedRewards_Initializer_Element elem in precomputedRewards_Initializer)
        {
            vfKey newKey = new vfKey(elem.animName, elem.timestamp, elem.taskArr);
            precomputedRewards.Add(newKey, elem.reward);
        }
    }
}

[System.Serializable]
public class precomputedRewards_Initializer_Element
{
    public string animName;
    public float timestamp;
    public float[] taskArr;
    public float reward;
}

public struct vfKey{
	private readonly string _clipId;
	private readonly float _timeStamp;
	private readonly float[] _tasks;

    public string clipId
    {
        get { return _clipId; }
    }
    public float timeStamp
    {
        get { return _timeStamp; }
    }
    public float[] tasks
    {
        get { return _tasks; }
    }

	public vfKey(string id, float time, float[] tasks){
		this._clipId = id;
		this._timeStamp = time;
		this._tasks = tasks;
	}

    public static bool operator ==(vfKey vfKey1, vfKey vfKey2)
    {
        return vfKey1.Equals(vfKey2);
    }

    public static bool operator !=(vfKey vfKey1, vfKey vfKey2)
    {
        return !vfKey1.Equals(vfKey2);
    }

    public override bool Equals(object obj)
    {
        return (obj is vfKey)
            && this._clipId.Equals(((vfKey)obj).clipId)
            && this._timeStamp.Equals(((vfKey)obj).timeStamp)
            && this._tasks.SequenceEqual(((vfKey)obj).tasks);
    }

    public override int GetHashCode()
    {
        //hashcode implentation from 'Effective Java' by Josh Bloch
        unchecked
        {
            var hash = 17;
 
            hash = (31 * hash) + this._clipId.GetHashCode();
            hash = (31 * hash) + this._timeStamp.GetHashCode();
            for(int i = 0; i < _tasks.Length; i++)
            {
                hash = (31 * hash) + this._tasks[i].GetHashCode();
            }
            
            return hash;
        }
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
