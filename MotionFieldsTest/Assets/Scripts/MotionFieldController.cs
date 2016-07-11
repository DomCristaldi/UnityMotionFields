using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using AnimationMotionFields;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif





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

    //Initialize everything to the same constant value (useful for setting velocity to 0)
    public BoneTransform(float constant) {
        posX = posY = posZ
            = rotW = rotX = rotY = rotZ
            = sclX = sclY = sclZ
            = constant;
    }

    //Initialize with positional information (WARNING: Lossy Scale must be used for world scale)
    public BoneTransform(Transform tf, bool isLocal = true) {

        if (isLocal) {
            posX = tf.localPosition.x;
            posY = tf.localPosition.y;
            posZ = tf.localPosition.z;

            rotW = tf.localRotation.w;
            rotX = tf.localRotation.x;
            rotY = tf.localRotation.y;
            rotZ = tf.localRotation.z;

            sclX = tf.localScale.x;
            sclY = tf.localScale.y;
            sclZ = tf.localScale.z;
        }
        else {//ASSUME WORLD
            posX = tf.position.x;
            posY = tf.position.y;
            posZ = tf.position.z;

            rotW = tf.rotation.w;
            rotX = tf.rotation.x;
            rotY = tf.rotation.y;
            rotZ = tf.rotation.z;

            sclX = tf.lossyScale.x;
            sclY = tf.lossyScale.y;
            sclZ = tf.lossyScale.z;
        }
    }

    //Used for calculating velocity
    public BoneTransform(BoneTransform origin, BoneTransform destination) {
        posX = destination.posX - origin.posX;
        posY = destination.posY - origin.posY;
        posZ = destination.posZ - origin.posZ;

        rotW = destination.rotW - origin.rotW;
        rotX = destination.rotX - origin.rotX;
        rotY = destination.rotY - origin.rotY;
        rotZ = destination.rotZ - origin.rotZ;

        sclX = destination.sclX - origin.sclX;
        sclY = destination.sclY - origin.sclY;
        sclZ = destination.sclZ - origin.sclZ;
    }

    //Used for creating a copy
    public BoneTransform(BoneTransform copy) {
        posX = copy.posX;
        posY = copy.posY;
        posZ = copy.posZ;

        rotW = copy.rotW;
        rotX = copy.rotX;
        rotY = copy.rotY;
        rotZ = copy.rotZ;

        sclX = copy.sclX;
        sclY = copy.sclY;
        sclZ = copy.sclZ;
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

    //public AnimationClip[] poses;
    public AnimationClip animClipRef;
    public float timestamp;

    //public float[] keyframeData;
    //public KeyframeData[] keyframeData;

    public int frameSampleRate;//sampel rate that was used to create these poses

    //NEW
    public MotionPose(BonePose[] bonePoses, AnimationClip animClipRef, float timestamp) {
        this.bonePoses = bonePoses;

        this.animClipRef = animClipRef;
        this.timestamp = timestamp;
    }


    //CONSTRUCTOR FOR CREATING A MOTION POSE OUT OF BLENED POSES
    public MotionPose(MotionPose[] posesToBlend, float[] weights) {
        //Break out if there's no data to work with for either poses or weights
        if (posesToBlend.Length == 0) { Debug.LogError("Supplied Poses Array is of length 0"); return; }
        if (weights.Length == 0) { Debug.LogError("Supplied Weights Array is of length 0"); return; }

        //notify user that they have bad data and should take a look at it
        if (posesToBlend.Length != weights.Length) {
            Debug.LogError("Unequal number of Poses to Weights. Data may be unreliable. Please ensure the supplied arrays are mappedj properly");
        }

        bonePoses = posesToBlend[0].bonePoses.Clone() as BonePose[];

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

}

[System.Serializable]
public class AnimClipInfo {
    public bool useClip = true;
    public VelocityCalculationMode velocityCalculationMode;
    public AnimationClip animClip;

    public MotionPose[] motionPoses;//all the poses generated for this animation clip

    public int frameSampleRate;//sampel rate that was used to create these poses
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

	public KDTreeDLL.KDTree kd;

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

	public float OneTick(MotionPose currentPose, int numActions = 1){

        float[] taskArr = GetTaskArray();

        float reward = 0.0f;
        currentPose = MoveOneFrame(currentPose, taskArr, numActions, ref reward);

        //TODO: currentPose needs to be applied to the model! note: 95% sure code was written for this. is it just not called, or done elsewhere?

        return reward;
	}

    public MotionPose MoveOneFrame(MotionPose currentPose, float[] taskArr, int numActions, ref float reward)
    {
        List<MotionPose> candidateActions = GenerateCandidateActions(currentPose, numActions);

        int chosenAction = PickCandidate(currentPose, candidateActions, taskArr, numActions, ref reward);

        Debug.Log("best fitness is " + reward + " from Action " + chosenAction + "\n");

        return candidateActions[chosenAction];
    }

    public List<MotionPose> GenerateCandidateActions(MotionPose currentPose, int numActions)
    {
        //generate candidate states to move to by finding closest poses in kdtree
        float[] currentPoseArr = currentPose.flattenedMotionPose;

        MotionPose[] neighbors = NearestNeighbor(currentPoseArr, numActions);
        float[][] neighborsArr = neighbors.Select(x => x.flattenedMotionPose).ToArray();

        float[] weights = GenerateWeights(currentPoseArr, neighborsArr);

        float[][] actionWeights = GenerateActionWeights(weights, numActions);

        List<MotionPose> candidateActions = new List<MotionPose>();
        foreach (float[] action in actionWeights)
        {
            candidateActions.Add(GeneratePose(currentPose, neighbors, action));
        }
        return candidateActions;
    }

    public int PickCandidate(MotionPose currentPose, List<MotionPose> candidateActions, float[] taskArr, int numActions, ref float bestReward) {
        //choose the action with the highest reward
        int chosenAction = -1;
        for (int i = 0; i < candidateActions.Count(); i++) {
            float reward = ComputeReward(currentPose, candidateActions[i], taskArr, numActions);
            if (reward > bestReward) {
                bestReward = reward;
                chosenAction = i;
            }
        }
        return chosenAction;
    }

    public MotionPose[] NearestNeighbor(float[] pose, int num_neighbors){
		
		double[] dbl_pose = pose.Select (x => System.Convert.ToDouble (x)).ToArray ();
		object[] nn_data = kd.nearest (dbl_pose, num_neighbors);

		List<MotionPose> data = new List<MotionPose>();
		foreach(object obj in nn_data){
			data.Add((MotionPose) obj);
		}
		return data.ToArray();
	}

	public float[][] GenerateActionWeights(float[] weights, int numActions = 1){

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

        Debug.Log("weights: " + string.Join(" ", weights.Select(w => w.ToString()).ToArray()));

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

        MotionPose newPose = currentPose;
        BonePose currBonePose;
        BonePose blendBonePose;
        int numBones = currentPose.bonePoses.Length;
        for(int i = 0; i < numBones; i++)
        {
            currBonePose = currentPose.bonePoses[i];
            blendBonePose = blendedNeighbors.bonePoses[i];
            newPose.bonePoses[i].value.posX = currBonePose.value.posX + blendBonePose.positionNext.posX - blendBonePose.value.posX;
            newPose.bonePoses[i].value.posY = currBonePose.value.posY + blendBonePose.positionNext.posY - blendBonePose.value.posY;
            newPose.bonePoses[i].value.posZ = currBonePose.value.posZ + blendBonePose.positionNext.posZ - blendBonePose.value.posZ;

            newPose.bonePoses[i].positionNext.posX = currBonePose.value.posX + blendBonePose.value.posX + blendBonePose.positionNextNext.posX - 2 * blendBonePose.positionNext.posX;
            newPose.bonePoses[i].positionNext.posY = currBonePose.value.posY + blendBonePose.value.posY + blendBonePose.positionNextNext.posY - 2 * blendBonePose.positionNext.posY;
            newPose.bonePoses[i].positionNext.posZ = currBonePose.value.posZ + blendBonePose.value.posZ + blendBonePose.positionNextNext.posZ - 2 * blendBonePose.positionNext.posZ;

            Quaternion Q_currPosition = new Quaternion(currBonePose.value.rotX, currBonePose.value.rotY, currBonePose.value.rotZ, currBonePose.value.rotW);
            Quaternion Q_blendPosition = new Quaternion(blendBonePose.value.rotX, blendBonePose.value.rotY, blendBonePose.value.rotZ, blendBonePose.value.rotW);
            Quaternion Q_blendPositionNext = new Quaternion(blendBonePose.positionNext.rotX, blendBonePose.positionNext.rotY, blendBonePose.positionNext.rotZ, blendBonePose.positionNext.rotW);
            Quaternion Q_blendPositionNextNext = new Quaternion(blendBonePose.positionNextNext.rotX, blendBonePose.positionNextNext.rotY, blendBonePose.positionNextNext.rotZ, blendBonePose.positionNextNext.rotW);

            Quaternion Q_newPostion = (Q_currPosition * Q_blendPositionNext) * Quaternion.Inverse(Q_blendPosition);
            Quaternion Q_newPostionNext = (((Q_currPosition * Q_blendPosition) * Q_blendPositionNextNext) * Quaternion.Inverse(Q_blendPositionNext)) * Quaternion.Inverse(Q_blendPositionNext);

            newPose.bonePoses[i].value.rotX = Q_newPostion.x;
            newPose.bonePoses[i].value.rotY = Q_newPostion.y;
            newPose.bonePoses[i].value.rotZ = Q_newPostion.z;
            newPose.bonePoses[i].value.rotW = Q_newPostion.w;

            newPose.bonePoses[i].positionNext.rotX = Q_newPostionNext.x;
            newPose.bonePoses[i].positionNext.rotY = Q_newPostionNext.y;
            newPose.bonePoses[i].positionNext.rotZ = Q_newPostionNext.z;
            newPose.bonePoses[i].positionNext.rotW = Q_newPostionNext.w;

            newPose.bonePoses[i].value.sclX = currBonePose.value.sclX + blendBonePose.positionNext.sclX - blendBonePose.value.sclX;
            newPose.bonePoses[i].value.sclY = currBonePose.value.sclY + blendBonePose.positionNext.sclY - blendBonePose.value.sclY;
            newPose.bonePoses[i].value.sclZ = currBonePose.value.sclZ + blendBonePose.positionNext.sclZ - blendBonePose.value.sclZ;

            newPose.bonePoses[i].positionNext.sclX = currBonePose.value.sclX + blendBonePose.value.sclX + blendBonePose.positionNextNext.sclX - 2 * blendBonePose.positionNext.sclX;
            newPose.bonePoses[i].positionNext.sclY = currBonePose.value.sclY + blendBonePose.value.sclY + blendBonePose.positionNextNext.sclY - 2 * blendBonePose.positionNext.sclY;
            newPose.bonePoses[i].positionNext.sclZ = currBonePose.value.sclZ + blendBonePose.value.sclZ + blendBonePose.positionNextNext.sclZ - 2 * blendBonePose.positionNext.sclZ;
        }

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

	public float ComputeReward(MotionPose pose, MotionPose newPose, float[] taskArr, int numActions = 1){
        //first calculate immediate reward
		float immediateReward = 0.0f;
		for(int i = 0; i < taskArr.Length; i++){
			immediateReward += TArrayInfo.TaskArray[i].CheckReward (pose, newPose, taskArr[i]);
		}

		//calculate continuousReward
		float continuousReward = RewardLookup(newPose, taskArr, numActions);

		return immediateReward + scale*continuousReward;
	}

	public float RewardLookup(MotionPose pose, float[] Tasks, int numActions = 1){
		//get continuous reward from valuefunc lookup table.
		//reward is weighted blend of closest values in lookup table.
		//get closest poses from kdtree, and closest tasks from cartesian product
		//then get weighted rewards from lookup table for each pose+task combo

		//get closest poses.
		float[] poseArr = pose.flattenedMotionPose;
		MotionPose[] neighbors = NearestNeighbor (poseArr, numActions);
		float[][] neighborsArr = neighbors.Select (x => x.flattenedMotionPose).ToArray ();
		float[] neighbors_weights = GenerateWeights(poseArr, neighborsArr);

		//get closest tasks.
		List<List<float>> nearest_vals = new List<List<float>> ();
		for(int i=0; i < Tasks.Length; i++){
			//TODO: if Tasks[i] is the min or max, i think values added could e out of range. add a check
			List<float> nearest_val = new List<float> ();
			float interval = (TArrayInfo.TaskArray [i].max - TArrayInfo.TaskArray [i].min) / TArrayInfo.TaskArray [i].numSamples;
			nearest_val.Add (Mathf.Floor ((Tasks [i] - TArrayInfo.TaskArray [i].min) / interval) * interval + TArrayInfo.TaskArray [i].min);
			nearest_val.Add (Mathf.Floor((Tasks [i] - TArrayInfo.TaskArray [i].min) / interval) * (interval+1) + TArrayInfo.TaskArray [i].min);
			nearest_vals.Add (nearest_val);
		}
		//turn the above/below vals for each task into 2^Tasks.Length() task arrays, each of which exists in precalculated dataset
		List<List<float>> nearestTasks = CartesianProduct(nearest_vals);
		float[][] nearestTasksArr = nearestTasks.Select(a => a.ToArray()).ToArray();
		float[] nearestTasks_weights = GenerateWeights(Tasks, nearestTasksArr);

		//get matrix of neighbors x tasks. The corresponding weight matrix should sum to 1.
		List<vfKey> dictKeys = new List<vfKey> ();
		List<float> dictKeys_weights = new List<float> ();
		for (int i = 0; i < neighbors.Count(); i++){
			for (int j = 0; j < nearestTasksArr.Length; j++){
				dictKeys.Add (new vfKey(neighbors[i].animClipRef.name, neighbors[i].timestamp, nearestTasksArr[j]));
				dictKeys_weights.Add (neighbors_weights [i] * nearestTasks_weights [j]);
			}
		}

		//do lookups in precomputed table, get weighted sum
		float continuousReward = 0.0f;
		for(int i = 0; i < dictKeys.Count(); i++){
			continuousReward += precomputedRewards[dictKeys[i]]*dictKeys_weights[i];
		}

		return continuousReward;
	}

    public void makeDictfromList(List<ArrayList> lst)
    {
        precomputedRewards = new Dictionary<vfKey, float>();
        foreach (ArrayList arrLst in lst)
        {
            MotionPose mp = arrLst[0] as MotionPose;
            float[] taskarr = arrLst[1] as float[];
            precomputedRewards.Add(new vfKey(mp.animClipRef.name, mp.timestamp, taskarr), System.Convert.ToSingle(arrLst[2]));
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
