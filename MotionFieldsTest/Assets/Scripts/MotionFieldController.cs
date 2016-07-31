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
    ReferencePoint = 0,
    CenterOfMass = 1,
}

public enum RootMotionFrameHandling {
    DropFirstFrame = 0,
    LoopToFirstFrame = 1,
    SetFirstFrameToZero = 2,
}


[System.Serializable]
public class BoneTransform {
    public float posX, posY, posZ,
                 rotW, rotX, rotY, rotZ,
                 sclX, sclY, sclZ;

    //Initialize everyting to default values
    public BoneTransform() {
        this.posX = this.posY = this.posZ = this.rotX = this.rotY = this.rotZ = 0.0f;
        this.rotW = this.sclX = this.sclY = this.sclZ = 1.0f;
    }

    public BoneTransform(Vector3 position, Quaternion rotation, Vector3 scale) {
        this.posX = position.x;
        this.posY = position.y;
        this.posZ = position.z;

        this.rotW = rotation.w;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;

        this.sclX = scale.x;
        this.sclY = scale.y;
        this.sclZ = scale.z;
    }

    //Initialize everything to the same constant value (useful for setting velocity to 0)
    public BoneTransform(float constant) {
        this.posX = this.posY = this.posZ
      = this.rotW = this.rotX = this.rotY = this.rotZ
      = this.sclX = this.sclY = this.sclZ
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

            this.sclX = tf.localScale.x;
            this.sclY = tf.localScale.y;
            this.sclZ = tf.localScale.z;
        }
        else {//ASSUME WORLD
            //Debug.Log("world");

            this.posX = tf.position.x;
            this.posY = tf.position.y;
            this.posZ = tf.position.z;

            //HACN: projecting onto the plane of the transform's forward vector may not be the best idea here
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

            this.sclX = tf.lossyScale.x;
            this.sclY = tf.lossyScale.y;
            this.sclZ = tf.lossyScale.z;
        }
    }

    //Used for calculating velocity
    public BoneTransform(BoneTransform origin, BoneTransform destination) {
        this.posX = destination.posX - origin.posX;
        this.posY = destination.posY - origin.posY;
        this.posZ = destination.posZ - origin.posZ;

        this.rotW = destination.rotW - origin.rotW;
        this.rotX = destination.rotX - origin.rotX;
        this.rotY = destination.rotY - origin.rotY;
        this.rotZ = destination.rotZ - origin.rotZ;

        this.sclX = destination.sclX - origin.sclX;
        this.sclY = destination.sclY - origin.sclY;
        this.sclZ = destination.sclZ - origin.sclZ;
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

        this.sclX = copy.sclX;
        this.sclY = copy.sclY;
        this.sclZ = copy.sclZ;
    }

    public float[] flattenedTransform {
        get {
            return new float[] {posX, posY, posZ,
                                rotW, rotX, rotY, rotZ,
                                sclX, sclY, sclZ };
        }
    }

    public static BoneTransform BlendTransform(BoneTransform tf1, BoneTransform tf2, float alpha) {
        BoneTransform retBoneTf = new BoneTransform();

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

        return retBoneTf;
    }

}

[System.Serializable]
public class BonePose {
    public string boneLabel;

    public BoneTransform value;
    public BoneTransform positionNext;
    public BoneTransform positionNextNext;

    public BonePose(string boneLabel) {
        this.boneLabel = boneLabel;
    }

    public Vector3 positionValue {
        get { return new Vector3(value.posX, value.posY, value.posZ); }
    }
    public Quaternion rotationValue {
        get { return new Quaternion(value.rotX, value.rotY, value.rotZ, value.rotW); }
    }

    public float[] flattenedValue {
        get {
            return value.flattenedTransform;
        }
    }

    public float[] flattenedVelocity {
        get {
            return positionNext.flattenedTransform;
        }
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

    public int frameSampleRate;//sampel rate that was used to create these poses


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

            newRootMotion.value = BoneTransform.BlendTransform(newRootMotion.value,
                                                    posesToBlend[i].rootMotionInfo.value,
                                                    bpwNormalized);
            newRootMotion.positionNext = BoneTransform.BlendTransform(newRootMotion.positionNext,
                                                           posesToBlend[i].rootMotionInfo.positionNext,
                                                           bpwNormalized);
            newRootMotion.positionNextNext = BoneTransform.BlendTransform(newRootMotion.positionNextNext,
                                                               posesToBlend[i].rootMotionInfo.positionNextNext,
                                                               bpwNormalized);

            for (int j = 0; j < posesToBlend[i].bonePoses.Length; ++j) {
                //do the blending
                newPoseBones[j].value = BoneTransform.BlendTransform(newPoseBones[j].value,
                                                                  posesToBlend[i].bonePoses[j].value, 
                                                                  bpwNormalized);

                newPoseBones[j].positionNext = BoneTransform.BlendTransform(newPoseBones[j].positionNext,
                                                                     posesToBlend[i].bonePoses[j].positionNext,
                                                                     bpwNormalized);

                newPoseBones[j].positionNextNext = BoneTransform.BlendTransform(newPoseBones[j].positionNextNext,
                                                                         posesToBlend[i].bonePoses[j].positionNextNext,
                                                                         bpwNormalized);
            }

            //add to the weight we iterated so far
            curBoneWeight += weights[i];
        }

        this.bonePoses = newPoseBones;
        this.rootMotionInfo = newRootMotion;
    }

    public float[] flattenedMotionPose {
        get {
            var retArray = rootMotionInfo.flattenedValue.Concat<float>(bonePoses[0].flattenedVelocity);
            int length = bonePoses.Length;
            for (int i = 0; i < length; ++i)
            {
                retArray = retArray.Concat<float>(bonePoses[i].flattenedValue.Concat<float>(bonePoses[i].flattenedVelocity));
            }

            //return retArray.ToArray<float>();
            return retArray.ToArray();
        }
    }

    //RETRIEVE THE BONE POSE WITH THE SPECIFIED LABEL
    public BonePose GetBonePose(string label) {
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
    public RootMotionCalculationMode rootMotionMode;
    public bool looping = false;
    public AnimationClip animClip;

    public MotionPose[] motionPoses;//all the poses generated for this animation clip

    public int frameSampleRate;//sample rate that was used to create these poses

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
    [Range(0.0f,1.0f)]
    public float scale = 0.5f; 

    public int numActions = 1;

	public MotionPose OneTick(MotionPose currentPose){

        float[] taskArr = GetTaskArray();
        //Debug.Log("task Length: " + taskArr.Length.ToString());

        float reward = float.MinValue;
        MotionPose newPose = MoveOneFrame(currentPose, taskArr, ref reward);

        return newPose;
	}

    public MotionPose MoveOneFrame(MotionPose currentPose, float[] taskArr, ref float reward)
    {
        //Debug.Log("Move One Frame pose before GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        MotionPose[] candidateActions = GenerateCandidateActions(currentPose);

        //Debug.Log("Move One Frame pose after GenCandActions: " + string.Join(" ", currentPose.flattenedMotionPose.Select(d => d.ToString()).ToArray()));
        int chosenAction = PickCandidate(currentPose, candidateActions, taskArr, ref reward);

        Debug.Log("Candidate Chosen! best fitness is " + reward + " from Action " + chosenAction + "\n");
        return candidateActions[chosenAction];
         
    }

    private MotionPose[] GenerateCandidateActions(MotionPose currentPose)
    {
        //generate candidate states to move to by finding closest poses in kdtree
        float[] currentPoseArr = currentPose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor(currentPoseArr);

        /*string StrNeighbors = "Neighbor Poses: ";
        for (int i = 0; i < neighbors.Count(); i++){
            StrNeighbors += "\n\n" + string.Join(" ", neighbors[i].flattenedMotionPose.Select(d => d.ToString()).ToArray());
        }
        Debug.Log(StrNeighbors);*/

        float[] weights = GenerateWeights(currentPose, neighbors);

        float[][] actionWeights = GenerateActionWeights(weights);

        MotionPose[] candidateActions = new MotionPose[actionWeights.Length];
        for(int i = 0; i < actionWeights.Length; ++i)
        {
            candidateActions[i] = GeneratePose(currentPose, neighbors, actionWeights[i]);
        }

        return candidateActions;
    }

    private int PickCandidate(MotionPose currentPose, MotionPose[] candidateActions, float[] taskArr, ref float bestReward) {
        //choose the action with the highest reward
        int chosenAction = -1;

        for (int i = 0; i < candidateActions.Length; ++i) {
            float reward = ComputeReward(currentPose, candidateActions[i], taskArr);
            //Debug.Log("Reward for action " + i.ToString() + " is " + reward.ToString());
            if (reward > bestReward) {
                bestReward = reward;
                chosenAction = i;
            }
        }
        return chosenAction;
    }

    public MotionPose[] NearestNeighbor(float[] pose){
        object[] nn_data = kd.nearest (pose, numActions);
        
        MotionPose[] data = new MotionPose[nn_data.Length];
        for(int i = 0; i < nn_data.Length; ++i)
        {
            data[i] = (MotionPose)nn_data[i];
        }
        return data;
    }

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


	private float[] GenerateWeights(float[] pose, float[][] neighbors){

		float[] weights = new float[neighbors.Length];
        float diff;
        float weightsSum = 0;
        int i, j;

		//weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
		for(i = 0; i < neighbors.Length; i++){
			weights [i] = 0.0f;
			for(j = 0; j < pose.Length; j++){
                diff = pose[j] - neighbors[i][j];
				weights [i] += diff*diff;
			}
			weights [i] = 1.0f / weights [i];
            weightsSum += weights[i];
            if (float.IsInfinity(weights[i]))
            {
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
        for (i = 0; i < weights.Length; i++) {
            weights[i] = weights[i] / weightsSum;
        }

        //Debug.Log("weights: " + string.Join(" ", weights.Select(w => w.ToString()).ToArray()));

        return weights;
	}

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
                weights[i] += sqDist(Bones[j].value, neighborBones[j].value);
                weights[i] += sqDist(Bones[j].positionNext, neighborBones[j].positionNext);
            }
            weights[i] += sqDist(pose.rootMotionInfo.value, neighbors[i].rootMotionInfo.value);
            weights[i] += sqDist(pose.rootMotionInfo.positionNext, neighbors[i].rootMotionInfo.positionNext);

            weights[i] = 1.0f / weights[i];
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
        for (i = 0; i < weights.Length; i++)
        {
            weights[i] = weights[i] / weightsSum;
        }

        return weights;
    }

    private float sqDist(BoneTransform b1, BoneTransform b2)
    {
        float sqDist = 0.0f;
        sqDist += ((b1.posX - b2.posX) * (b1.posX - b2.posX));
        sqDist += ((b1.posY - b2.posY) * (b1.posY - b2.posY));
        sqDist += ((b1.posZ - b2.posZ) * (b1.posZ - b2.posZ));
        sqDist += ((b1.rotX - b2.rotX) * (b1.rotX - b2.rotX));
        sqDist += ((b1.rotY - b2.rotY) * (b1.rotY - b2.rotY));
        sqDist += ((b1.rotZ - b2.rotZ) * (b1.rotZ - b2.rotZ));
        sqDist += ((b1.rotW - b2.rotW) * (b1.rotW - b2.rotW));
        sqDist += ((b1.sclX - b2.sclX) * (b1.sclX - b2.sclX));
        sqDist += ((b1.sclY - b2.sclY) * (b1.sclY - b2.sclY));
        sqDist += ((b1.sclZ - b2.sclZ) * (b1.sclZ - b2.sclZ));
        return sqDist;
    }

    private MotionPose GeneratePose(MotionPose currentPose, MotionPose[] neighbors, float[] action){

        MotionPose blendedNeighbors = new MotionPose(neighbors, action);

        int numBones = currentPose.bonePoses.Length;

        BonePose newRootBone = GenerateBone(currentPose.rootMotionInfo, blendedNeighbors.rootMotionInfo);

        BonePose[] newPoseBones = new BonePose[numBones];
        for (int i = 0; i < numBones; i++)
        {
            newPoseBones[i] = GenerateBone(currentPose.bonePoses[i], blendedNeighbors.bonePoses[i]);
        }

        MotionPose newPose = new MotionPose(newPoseBones, newRootBone);
        return newPose;
    }

    public BonePose GenerateBone(BonePose currBonePose, BonePose blendBonePose)
    {
        /*
        addition/subtraction logic:
        new_position = currentPose.position + blendedNeighbors.positionNext - blendedNeighbors.position
        new_positionNext = currentPose.position + blendedNeighbors.position + blendedNeighbors.positionNextNext - 2(blendedNeighbors.positionNext)
        new_positionNextNext = not needed 
        */

        Quaternion Q_currPosition = new Quaternion(currBonePose.value.rotX, currBonePose.value.rotY, currBonePose.value.rotZ, currBonePose.value.rotW);
        Quaternion Q_blendPosition = new Quaternion(blendBonePose.value.rotX, blendBonePose.value.rotY, blendBonePose.value.rotZ, blendBonePose.value.rotW);
        Quaternion Q_blendPositionNext = new Quaternion(blendBonePose.positionNext.rotX, blendBonePose.positionNext.rotY, blendBonePose.positionNext.rotZ, blendBonePose.positionNext.rotW);
        Quaternion Q_blendPositionNextNext = new Quaternion(blendBonePose.positionNextNext.rotX, blendBonePose.positionNextNext.rotY, blendBonePose.positionNextNext.rotZ, blendBonePose.positionNextNext.rotW);

        Quaternion Q_newPostion = (Q_currPosition * Q_blendPositionNext) * Quaternion.Inverse(Q_blendPosition);
        Quaternion Q_newPostionNext = (((Q_currPosition * Q_blendPosition) * Q_blendPositionNextNext) * Quaternion.Inverse(Q_blendPositionNext)) * Quaternion.Inverse(Q_blendPositionNext);

        BonePose newBone = new BonePose(currBonePose.boneLabel);
        newBone.value = new BoneTransform()
        {
            posX = currBonePose.value.posX + blendBonePose.positionNext.posX - blendBonePose.value.posX,
            posY = currBonePose.value.posY + blendBonePose.positionNext.posY - blendBonePose.value.posY,
            posZ = currBonePose.value.posZ + blendBonePose.positionNext.posZ - blendBonePose.value.posZ,

            rotX = Q_newPostion.x,
            rotY = Q_newPostion.y,
            rotZ = Q_newPostion.z,
            rotW = Q_newPostion.w,

            sclX = currBonePose.value.sclX + blendBonePose.positionNext.sclX - blendBonePose.value.sclX,
            sclY = currBonePose.value.sclY + blendBonePose.positionNext.sclY - blendBonePose.value.sclY,
            sclZ = currBonePose.value.sclZ + blendBonePose.positionNext.sclZ - blendBonePose.value.sclZ,
        };

        newBone.positionNext = new BoneTransform()
        {
            posX = currBonePose.value.posX + blendBonePose.value.posX + blendBonePose.positionNextNext.posX - 2 * blendBonePose.positionNext.posX,
            posY = currBonePose.value.posY + blendBonePose.value.posY + blendBonePose.positionNextNext.posY - 2 * blendBonePose.positionNext.posY,
            posZ = currBonePose.value.posZ + blendBonePose.value.posZ + blendBonePose.positionNextNext.posZ - 2 * blendBonePose.positionNext.posZ,

            rotX = Q_newPostionNext.x,
            rotY = Q_newPostionNext.y,
            rotZ = Q_newPostionNext.z,
            rotW = Q_newPostionNext.w,

            sclX = currBonePose.value.sclX + blendBonePose.value.sclX + blendBonePose.positionNextNext.sclX - 2 * blendBonePose.positionNext.sclX,
            sclY = currBonePose.value.sclY + blendBonePose.value.sclY + blendBonePose.positionNextNext.sclY - 2 * blendBonePose.positionNext.sclY,
            sclZ = currBonePose.value.sclZ + blendBonePose.value.sclZ + blendBonePose.positionNextNext.sclZ - 2 * blendBonePose.positionNext.sclZ
        };

        return newBone;
    }


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
			immediateReward += TArrayInfo.TaskArray[i].CheckReward (pose, newPose, taskArr[i]);
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

        //get closest poses.

        float[] poseArr = pose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor (poseArr);
		float[] neighbors_weights = GenerateWeights(pose, neighbors);

		//get closest tasks.
		List<List<float>> nearest_vals = new List<List<float>> ();
        float min, max, numSamples, interval;
		for(int i=0; i < Tasks.Length; i++){

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
        
        float[] nearestTasks_weights = GenerateWeights(Tasks, nearestTasksArr);

		//get matrix of neighbors x tasks. The corresponding weight matrix should sum to 1.
		List<vfKey> dictKeys = new List<vfKey> ();
		List<float> dictKeys_weights = new List<float> ();
		for (int i = 0; i < neighbors.Length; i++){
			for (int j = 0; j < nearestTasksArr.Length; j++){
				dictKeys.Add (new vfKey(neighbors[i].animName, neighbors[i].timestamp, nearestTasksArr[j]));
				dictKeys_weights.Add (neighbors_weights [i] * nearestTasks_weights [j]);
			}
		}

		//do lookups in precomputed table, get weighted sum
		float continuousReward = 0.0f;
		for(int i = 0; i < dictKeys.Count; i++){
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
