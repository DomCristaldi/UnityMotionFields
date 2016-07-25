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

//namespace AnimationMotionFields {

/*[System.Serializable]
public class KeyframeData {
    public float value;
    public float velocity;
	public float velocityNext;

	public KeyframeData(float value = 0.0f, float velocity = 0.0f, float velocityNext = 0.0f) {
        this.value = value;
        this.velocity = velocity;
		this.velocityNext = velocityNext;
    }
}*/


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

    public float[] flattenedPosition {
        get { return new float[] { posX, posY, posZ }; }
    }

    public float[] flattenedRotation {
        get { return new float[] { rotW, rotX, rotY, rotZ }; }
    }

    public float[] flattenedScale {
        get { return new float[] { sclX, sclY, sclZ }; }
    }

    public float[] flattenedTransform {
        get {
            return flattenedPosition
                   .Concat<float>(flattenedRotation)
                   .Concat<float>(flattenedScale)
                   .ToArray<float>();
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


    public MotionPose(BonePose[] bonePoses) {
        this.bonePoses = bonePoses;
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
        if (posesToBlend.Count() != weights.Count()) { Debug.LogError("The number of poses does not match the number of weigts"); return; }
        
        //notify user that they have bad data and should take a look at it
        if (posesToBlend.Length != weights.Length) {
            Debug.LogError("Unequal number of Poses to Weights. Data may be unreliable. Please ensure the supplied arrays are mappedj properly");
        }

        //set initial bone pose array to that of the first pose to blend
        List<BonePose> newPoseBones = new List<BonePose>();
        for(int j = 0; j < posesToBlend[0].bonePoses.Length; ++j)
        {
            BonePose newBone = new BonePose(posesToBlend[0].bonePoses[j].boneLabel);
            newBone.value = new BoneTransform(posesToBlend[0].bonePoses[j].value);
            newBone.positionNext = new BoneTransform(posesToBlend[0].bonePoses[j].positionNext);
            newBone.positionNextNext = new BoneTransform(posesToBlend[0].bonePoses[j].positionNextNext);
            newPoseBones.Add(newBone);
        }
        bonePoses = newPoseBones.ToArray();

        //Break out early b/c there's only one Motion Pose to blend with
        if (posesToBlend.Length == 1) { return; }

        //represents the amount we've blended in so far
        float curBoneWeight = weights[0];

        for (int i = 1; i < posesToBlend.Length; ++i) {

            //create normalized weights for tiered blending
            float bpwNormalized = curBoneWeight / (curBoneWeight + weights[i]);
            //float wiNormalized = weights[i] / (curBoneWeight + weights[i]);

            for (int j = 0; j < posesToBlend[i].bonePoses.Length; ++j) {
                //do the blending
                bonePoses[j].value = BoneTransform.BlendTransform(bonePoses[j].value,
                                                                  posesToBlend[i].bonePoses[j].value, 
                                                                  bpwNormalized);

                bonePoses[j].positionNext = BoneTransform.BlendTransform(bonePoses[j].positionNext,
                                                                     posesToBlend[i].bonePoses[j].positionNext,
                                                                     bpwNormalized);

                bonePoses[j].positionNextNext = BoneTransform.BlendTransform(bonePoses[j].positionNextNext,
                                                                         posesToBlend[i].bonePoses[j].positionNextNext,
                                                                         bpwNormalized);
            }

            //add to the weight we iterated so far
            curBoneWeight += weights[i];
        }
    }

    public float[] flattenedMotionPose {
        get {
            float[] retArray = new float[] { };

            foreach (BonePose pose in bonePoses) {
                retArray = retArray.Concat<float>(pose.flattenedValue).ToArray().Concat<float>(pose.flattenedVelocity).ToArray();
            }

            //return retArray.ToArray<float>();
            return retArray;
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
    /*
    public void GenerateMotionPoses(int samplingResolution, string[] totalAnimPaths) {
        motionPoses = MotionFieldUtility.GenerateMotionPoses(animClip,
                                                             totalAnimPaths,
                                                             samplingResolution,
                                                             velocityCalculationMode);
    }
    */

    public void PrintPathTest() {
        foreach (EditorCurveBinding ecb in AnimationUtility.GetCurveBindings(animClip)) {
            Debug.Log("path " + ecb.propertyName);
        }
    }
}


[CreateAssetMenu]
public class MotionFieldController : ScriptableObject {

    //USER DEFINED ANIMATION CURVE PATHS FOR ROOT MOTION COMPONENT
	[System.Serializable]
	public class  RootComponents{
		public string tx;
		public string ty;
		public string tz;

		public string qx;
		public string qy;
		public string qz;
		public string qw;
	}

	public RootComponents rootComponents;

    public List<AnimClipInfo> animClipInfoList;

    public KDTreeDLL_f.KDTree kd;

	public TaskArrayInfo TArrayInfo;

	private Dictionary<vfKey, float> precomputedRewards;

	//using an ArrayList because Unity is dumb and doesn't have tuples.
	//each arralist should holds a MotionPose in [0] a float[] in [1], and a float in [2]
	//since ArrayList stores everything as object, must cast it when taking out data
	[HideInInspector]
	public List<ArrayList> precomputedRewards_Initializer;

    //how much to prefer the immediate reward vs future reward. 
    //reward = r(firstframe) + scale*r(secondframe) + scale^2*r(thirdframe) + ... ect
    //close to 0 has higher preference on early reward. closer to 1 has higher preference on later reward
    //closer to 1 also asymptotically increases time to generate precomputed rewards, so its recommended you dont set it too high. 
    public float scale = 0.5f; 

    public int numActions = 1;

	public float OneTick(MotionPose currentPose){

        float[] taskArr = GetTaskArray();
        Debug.Log("task Length: " + taskArr.Length.ToString());

        float reward = 0.0f;
        currentPose = MoveOneFrame(currentPose, taskArr, ref reward);

        //TODO: currentPose needs to be applied to the model! note: 95% sure code was written for this. is it just not called, or done elsewhere?

        return reward;
	}

    public MotionPose MoveOneFrame(MotionPose currentPose, float[] taskArr, ref float reward)
    {

        //float[] poseArr = currentPose.flattenedMotionPose;
        //Debug.Log("Move One Frame pose before GenCandActions: " + string.Join(" ", poseArr.Select(d => d.ToString()).ToArray()));

        List<MotionPose> candidateActions = GenerateCandidateActions(currentPose);

        //poseArr = currentPose.flattenedMotionPose;
        //Debug.Log("Move One Frame pose after GenCandActions: " + string.Join(" ", poseArr.Select(d => d.ToString()).ToArray()));

        int chosenAction = PickCandidate(currentPose, candidateActions, taskArr, ref reward);

        //Debug.Log("Candidate Chosen! best fitness is " + reward + " from Action " + chosenAction + "\n");

        return candidateActions[chosenAction];
    }

    public List<MotionPose> GenerateCandidateActions(MotionPose currentPose)
    {
        //generate candidate states to move to by finding closest poses in kdtree
        float[] currentPoseArr = currentPose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor(currentPoseArr);

        /*
        string StrNeighbors = "Neighbor Poses: ";
        for (int i = 0; i < neighbors.Count(); i++)
        {
            float[] poseArr = neighbors[i].flattenedMotionPose;
            StrNeighbors += "\n\n" + string.Join(" ", poseArr.Select(d => d.ToString()).ToArray());
        }
        Debug.Log(StrNeighbors);
        */

        float[][] neighborsArr = neighbors.Select(x => x.flattenedMotionPose).ToArray();

        float[] weights = GenerateWeights(currentPoseArr, neighborsArr);

        float[][] actionWeights = GenerateActionWeights(weights);

        List<MotionPose> candidateActions = new List<MotionPose>();
        foreach (float[] action in actionWeights)
        {
            candidateActions.Add(GeneratePose(currentPose, neighbors, action));
        }

        return candidateActions;
    }

    public int PickCandidate(MotionPose currentPose, List<MotionPose> candidateActions, float[] taskArr, ref float bestReward) {
        //choose the action with the highest reward
        int chosenAction = -1;

        for (int i = 0; i < candidateActions.Count(); i++) {
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

		List<MotionPose> data = new List<MotionPose>();
		foreach(object obj in nn_data){
			data.Add((MotionPose) obj);
		}
		return data.ToArray();
	}

    public float[][] GenerateActionWeights(float[] weights){
		float[][] actions = new float[numActions] [];
		for(int i = 0; i < numActions; i++){
			//for each action array, set weight[i] to 1 and renormalize
			actions [i] = new float[weights.Length];
			actions [i] = (float[])weights.Clone ();
			actions [i] [i] = 1;
			float actionSum = actions[i].Sum();
			for(int j = 0; j < actions[i].Length; j++){
				actions[i][j] = actions[i][j] / actionSum;
			}
		}
		return actions;
	}

	public float[] GenerateWeights(float[] pose, float[][] neighbors){
        //note: neighbors.Length == numActions
        float infCount = 0.0f;
		float[] weights = new float[neighbors.Length];

		//weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
		for(int i = 0; i < neighbors.Length; i++){
			weights [i] = 0.0f;
			for(int j = 0; j < pose.Length; j++){
				weights [i] += Mathf.Pow (pose [j] - neighbors [i][j], 2);
			}
			weights [i] = 1.0f / weights [i];
            if (float.IsInfinity(weights[i]))
            {
                infCount += 1;
            }
        }

        //now normalize weights so that they sum to 1
        if (infCount == 0.0f) {
            float weightsSum = weights.Sum();
            for (int i = 0; i < weights.Length; i++) {
                weights[i] = weights[i] / weightsSum;
            }
        }
        else { // at least one neighbor is identical to pose
            for (int i = 0; i < weights.Length; i++) {
                if (float.IsInfinity(weights[i])) {
                    weights[i] = 1.0f / infCount;
                }
                else {
                    weights[i] = 0.0f;
                }
            }
        }

        //Debug.Log("weights: " + string.Join(" ", weights.Select(w => w.ToString()).ToArray()));

        return weights;
	}

	public MotionPose GeneratePose(MotionPose currentPose, MotionPose[] neighbors, float[] action){
        //note: forgive me programming gods, for I have sinned by creating this ugly function.

        /*
        addition/subtraction logic:
        new_position = currentPose.position + blendedNeighbors.positionNext - blendedNeighbors.position
        new_positionNext = currentPose.position + blendedNeighbors.position + blendedNeighbors.positionNextNext - 2(blendedNeighbors.positionNext)
        new_positionNextNext = not needed 
        */

        MotionPose blendedNeighbors = new MotionPose(neighbors, action);

        List<BonePose> newPoseBones = new List<BonePose>();

        BonePose currBonePose;
        BonePose blendBonePose;
        int numBones = currentPose.bonePoses.Length;

        for (int i = 0; i < numBones; i++)
        {
            currBonePose = currentPose.bonePoses[i];
            blendBonePose = blendedNeighbors.bonePoses[i];

            //currentPoseArr = currentPose.flattenedMotionPose;
            //Debug.Log("   GenCand LOOP " + i.ToString() + " START pose: " + string.Join(" ", currentPoseArr.Select(d => d.ToString()).ToArray()));

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

            newPoseBones.Add(newBone);
        }

        MotionPose newPose = new MotionPose(newPoseBones.ToArray(), currentPose.animName, currentPose.timestamp);
        return newPose;
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

    /*public float[] poseToPosVelArray(MotionPose pose)
    {
        //from MP, create float array with only position+velocity information
        //in order [p1,v1,p2,v2,p3,v3,ect...]
        float[] poseArray = new float[pose.bonePoses.Length * 20]; //20 because each bonePose has 10 pos vals and 10 vel vals
        for (int i = 0; i < pose.bonePoses.Length; i++)
        {
            poseArray[i * 20] = pose.bonePoses[i].value.posX;
            poseArray[i * 20 + 1] = pose.bonePoses[i].velocity.posX;
            poseArray[i * 20 + 2] = pose.bonePoses[i].value.posY;
            poseArray[i * 20 + 3] = pose.bonePoses[i].velocity.posY;
            poseArray[i * 20 + 4] = pose.bonePoses[i].value.posZ;
            poseArray[i * 20 + 5] = pose.bonePoses[i].velocity.posZ;
            poseArray[i * 20 + 6] = pose.bonePoses[i].value.rotX;
            poseArray[i * 20 + 7] = pose.bonePoses[i].velocity.rotX;
            poseArray[i * 20 + 8] = pose.bonePoses[i].value.rotY;
            poseArray[i * 20 + 9] = pose.bonePoses[i].velocity.rotY;
            poseArray[i * 20 + 10] = pose.bonePoses[i].value.rotZ;
            poseArray[i * 20 + 11] = pose.bonePoses[i].velocity.rotZ;
            poseArray[i * 20 + 12] = pose.bonePoses[i].value.rotW;
            poseArray[i * 20 + 13] = pose.bonePoses[i].velocity.rotW;
            poseArray[i * 20 + 14] = pose.bonePoses[i].value.sclX;
            poseArray[i * 20 + 15] = pose.bonePoses[i].velocity.sclX;
            poseArray[i * 20 + 16] = pose.bonePoses[i].value.sclY;
            poseArray[i * 20 + 17] = pose.bonePoses[i].velocity.sclY;
            poseArray[i * 20 + 18] = pose.bonePoses[i].value.sclZ;
            poseArray[i * 20 + 19] = pose.bonePoses[i].velocity.sclZ;
        }
        return poseArray;
    }*/

    public float[] GetTaskArray(){
        //current value of task array determined by world params
		int tasklength = TArrayInfo.TaskArray.Count ();
		float[] taskArr = new float[tasklength];
		for(int i = 0; i < tasklength; i++){
			taskArr[i] = TArrayInfo.TaskArray[i].DetermineTaskValue();
		}
		return taskArr;
	}

	public float ComputeReward(MotionPose pose, MotionPose newPose, float[] taskArr){
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

	public float ContRewardLookup(MotionPose pose, float[] Tasks){
        //get continuous reward from valuefunc lookup table.
        //reward is weighted blend of closest values in lookup table.
        //get closest poses from kdtree, and closest tasks from cartesian product
        //then get weighted rewards from lookup table for each pose+task combo

        //get closest poses.

        float[] poseArr = pose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor (poseArr);
		float[][] neighborsArr = neighbors.Select (x => x.flattenedMotionPose).ToArray ();
		float[] neighbors_weights = GenerateWeights(poseArr, neighborsArr);

		//get closest tasks.
		List<List<float>> nearest_vals = new List<List<float>> ();
		for(int i=0; i < Tasks.Length; i++){
			List<float> nearest_val = new List<float> ();
			float interval = (TArrayInfo.TaskArray [i].max - TArrayInfo.TaskArray [i].min) / (TArrayInfo.TaskArray [i].numSamples - 1);
            //Debug.Log("interval for " + TArrayInfo.TaskArray[i].min.ToString() + " to " + TArrayInfo.TaskArray[i].max.ToString() + " for " + TArrayInfo.TaskArray[i].numSamples.ToString() + " samples is " + interval.ToString());
            float lower = Mathf.Floor((Tasks[i] - TArrayInfo.TaskArray[i].min) / interval) * interval + TArrayInfo.TaskArray[i].min;
            nearest_val.Add (lower);
            if(lower != TArrayInfo.TaskArray[i].max)
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
		for (int i = 0; i < neighbors.Count(); i++){
			for (int j = 0; j < nearestTasksArr.Length; j++){
				dictKeys.Add (new vfKey(neighbors[i].animName, neighbors[i].timestamp, nearestTasksArr[j]));
				dictKeys_weights.Add (neighbors_weights [i] * nearestTasks_weights [j]);
			}
		}

		//do lookups in precomputed table, get weighted sum
		float continuousReward = 0.0f;
		for(int i = 0; i < dictKeys.Count(); i++){
            //Debug.Log("lookup table vfkey:\nclipname: " + dictKeys[i].clipId + "\ntimestamp: " + dictKeys[i].timeStamp.ToString() + "\ntasks: " + string.Join(" ", dictKeys[i].tasks.Select(w => w.ToString()).ToArray()) + "\nhashcode: " + dictKeys[i].GetHashCode() + "\ncomponent hashcodes: " + dictKeys[i].clipId.GetHashCode() + "  " + dictKeys[i].timeStamp.GetHashCode() + "  " + dictKeys[i].tasks.GetHashCode());
            continuousReward += precomputedRewards[dictKeys[i]]*dictKeys_weights[i];
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
            //Debug.Log("VFKEY ADDED:\nclipname: " + mp.animName + "\ntimestamp: " + mp.timestamp.ToString() + "\ntasks: " + string.Join(" ", taskarr.Select(w => w.ToString()).ToArray()) + "\nhashcode: " + newkey.GetHashCode() + "\ncomponent hashcodes: " + newkey.clipId.GetHashCode() + "  " + newkey.timeStamp.GetHashCode() + "  " + newkey.tasks.GetHashCode());
            precomputedRewards.Add(newkey, System.Convert.ToSingle(arrLst[2]));
        }
    }
}

//NodeData to be removed, MotionPose will be the data field of the kd tree
/*public class NodeData{
	public string clipId;
	public float timeStamp;
	public double[] position;
	public double[] velocity;
	public double[] velocityNext;

	public int rootComponent_tx;
	public int rootComponent_ty;
	public int rootComponent_tz;

	public int rootComponent_qx;
	public int rootComponent_qy;
	public int rootComponent_qz;
	public int rootComponent_qw;


	public NodeData(string id, float time, double[] position, double[] velocity, double[] velocityNext, 
		int rootComponent_tx, int rootComponent_ty, int rootComponent_tz, int rootComponent_qx, int rootComponent_qy, int rootComponent_qz, int rootComponent_qw ){
		this.clipId = id;
		this.timeStamp = time;
		this.position = position;
		this.velocity = velocity;
		this.velocityNext = velocityNext;
		this.rootComponent_tx = rootComponent_tx;
		this.rootComponent_ty = rootComponent_ty;
		this.rootComponent_tz = rootComponent_tz;
		this.rootComponent_qx = rootComponent_qx;
		this.rootComponent_qy = rootComponent_qy;
		this.rootComponent_qz = rootComponent_qz;
		this.rootComponent_qw = rootComponent_qw;
	}

    public string PrintNode() {
        return string.Format("Clip ID: {0}, Timestamp: {1}", clipId, timeStamp);
    }
}*/

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
