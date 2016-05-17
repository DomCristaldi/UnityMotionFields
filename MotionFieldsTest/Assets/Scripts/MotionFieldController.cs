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

[System.Serializable]
public class KeyframeData {
    public float value;
    public float velocity;
	public float velocityNext;

	public KeyframeData(float value = 0.0f, float velocity = 0.0f, float velocityNext = 0.0f) {
        this.value = value;
        this.velocity = velocity;
		this.velocityNext = velocityNext;
    }
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
}

[System.Serializable]
public class BonePose {
    public string boneLabel;

    public BoneTransform value;
    public BoneTransform velocity;
    public BoneTransform velocityNext;

    public BonePose(string boneLabel) {
        this.boneLabel = boneLabel;
    }
}

[System.Serializable]
public class MotionPose {

    public BonePose[] bonePoses;

    //public AnimationClip[] poses;
    public AnimationClip animClipRef;
    public float timestamp;

    //public float[] keyframeData;
    public KeyframeData[] keyframeData;

    public int frameSampleRate;//sampel rate that was used to create these poses

    //NEW
    public MotionPose(BonePose[] bonePoses, AnimationClip animClipRef, float timestamp) {
        this.bonePoses = bonePoses;

        this.animClipRef = animClipRef;
        this.timestamp = timestamp;
    }
    /*
    //OLD
    public MotionPose(AnimationClip animClipRef, float timestamp, KeyframeData[] keyframeData) {
        this.animClipRef = animClipRef;
        this.timestamp = timestamp;
        this.keyframeData = keyframeData;
    }
    //OLD
    public MotionPose(AnimationClip animClipRef, float timestamp, float[] keyframeValueData) {
        this.animClipRef = animClipRef;
        this.timestamp = timestamp;
        this.keyframeData = new KeyframeData[keyframeValueData.Length];//initialize array length

        for (int i = 0; i < keyframeData.Length; ++i) {
            //must create object so it exists in the array
            keyframeData[i] = new KeyframeData(value: keyframeValueData[i]);
        }
    }
    */
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

	public float moveOneTick(ref MotionPose currentPose, ref float[] currentTaskArray, int numActions = 1){
        //generate candidate states to move to by finding closest poses in kdtree
		float[] currentPoseArr = poseToPosVelArray (currentPose);

		List<MotionPose> neighbors = NearestNeighbor (currentPoseArr, numActions);
		float[][] neighborsArr = neighbors.Select (x => poseToPosVelArray (x)).ToArray ();

		float[] weights = GenerateWeights (currentPoseArr, neighborsArr);

		float[][] actionWeights = GenerateActions(weights, numActions);

		List<MotionPose> candidateActions = new List<MotionPose>();
		foreach (float[] action in actionWeights){
			candidateActions.Add(GeneratePose(neighbors, action));
		}

        //now 
		int chosenAction = -1;
		float bestReward = 0;
		float[] bestTaskArray = new float[TArrayInfo.TaskArray.Count()];
		for (int i=0; i < candidateActions.Count(); i++){
			float[] newTaskArray = GetTaskArray (candidateActions [i]);
			float reward = ComputeReward(candidateActions[i], currentTaskArray, newTaskArray, numActions);
			if (reward > bestReward){
				bestReward = reward;
				bestTaskArray = newTaskArray;
				chosenAction = i;
			}
		}

		Debug.Log ("best fitness is " + bestReward + " from Action " + chosenAction + "\n");
		currentPose = candidateActions [chosenAction];
		currentTaskArray = bestTaskArray;

        return bestReward;
	}

	public List<MotionPose> NearestNeighbor(float[] pose, int num_neighbors = 1){
		
		double[] dbl_pose = pose.Select (x => System.Convert.ToDouble (x)).ToArray ();
		object[] nn_data = kd.nearest (dbl_pose, num_neighbors);

		List<MotionPose> data = new List<MotionPose>();
		foreach(object obj in nn_data){
			data.Add((MotionPose) obj);
		}
		return data;
	}

	public float[][] GenerateActions(float[] weights, int numActions = 1){

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
		float[] weights = new float[neighbors.Length];

		//weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
		for(int i = 0; i < neighbors.Length; i++){
			weights [i] = 0;
			for(int j = 0; j < pose.Length; j++){
				weights [i] += Mathf.Pow (pose [j] - neighbors [i][j], 2);
				weights [i] += Mathf.Pow (pose [j] - neighbors [i][j], 2);
			}
			weights [i] = 1.0f / weights [i];
		}

		//now normalize weights so that they sum to 1
		float weightsSum = weights.Sum();
		string printW = "weights: ";
		for(int i = 0; i < weights.Length; i++){
			weights [i] = weights [i] / weightsSum;
			printW += weights[i] + "  ";
		}
		Debug.Log (printW);

		return weights;
	}

	public MotionPose GeneratePose(List<MotionPose> neighbors, float[] action){
		//placeholder func. takes in current motionstate, neighbor states, and weights of neighbor states.
		//does weighted blending, returns blended state
		return neighbors[0];
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

    public float[] poseToPosVelArray(MotionPose pose)
    {
        //from MP, create float array with only position+velocity information
        //in order [p1,v1,p2,v2,p3,v3,ect...]
        float[] poseArray = new float[pose.bonePoses.Length * 20]; //20 because each bonePose has 10 pos vals and 10 vel vals
        for (int i = 0; i < pose.bonePoses.Length; i++)
        {
            poseArray[i * 20] = pose.bonePoses[i].position.posX;
            poseArray[i * 20 + 1] = pose.bonePoses[i].velocity.posX;
            poseArray[i * 20 + 2] = pose.bonePoses[i].position.posY;
            poseArray[i * 20 + 3] = pose.bonePoses[i].velocity.posY;
            poseArray[i * 20 + 4] = pose.bonePoses[i].position.posZ;
            poseArray[i * 20 + 5] = pose.bonePoses[i].velocity.posZ;
            poseArray[i * 20 + 6] = pose.bonePoses[i].position.rotX;
            poseArray[i * 20 + 7] = pose.bonePoses[i].velocity.rotX;
            poseArray[i * 20 + 8] = pose.bonePoses[i].position.rotY;
            poseArray[i * 20 + 9] = pose.bonePoses[i].velocity.rotY;
            poseArray[i * 20 + 10] = pose.bonePoses[i].position.rotZ;
            poseArray[i * 20 + 11] = pose.bonePoses[i].velocity.rotZ;
            poseArray[i * 20 + 12] = pose.bonePoses[i].position.rotW;
            poseArray[i * 20 + 13] = pose.bonePoses[i].velocity.rotW;
            poseArray[i * 20 + 14] = pose.bonePoses[i].position.sclX;
            poseArray[i * 20 + 15] = pose.bonePoses[i].velocity.sclX;
            poseArray[i * 20 + 16] = pose.bonePoses[i].position.sclY;
            poseArray[i * 20 + 17] = pose.bonePoses[i].velocity.sclY;
            poseArray[i * 20 + 18] = pose.bonePoses[i].position.sclZ;
            poseArray[i * 20 + 19] = pose.bonePoses[i].velocity.sclZ;
        }
        return poseArray;
    }

    public float[] GetTaskArray(MotionPose pose){
		int tasklength = TArrayInfo.TaskArray.Count ();
		float[] taskArr = new float[TArrayInfo.TaskArray.Count()];
		for(int i = 0; i < tasklength; i++){
			taskArr[i] = TArrayInfo.TaskArray[i].DetermineTaskValue(pose);
		}
		return taskArr;
	}

	public float ComputeReward(MotionPose pose, float[] currentTaskArray, float[] newTaskArr, int numActions = 1){
        //first calculate immediate reward
		float immediateReward = 0.0f;
		for(int i = 0; i < currentTaskArray.Length; i++){
			immediateReward += TArrayInfo.TaskArray[i].CheckReward (currentTaskArray[i], newTaskArr [i]);
		}

		//calculate continuousReward
		float continuousReward = RewardLookup(pose, newTaskArr, numActions);

		return immediateReward + scale*continuousReward;
	}

	public float RewardLookup(MotionPose pose, float[] Tasks, int numActions = 1){
		//get continuous reward from valuefunc lookup table.
		//reward is weighted blend of closest values in lookup table.
		//get closest poses from kdtree, and closest tasks from cartesian product
		//then get weighted rewards from lookup table for each pose+task combo

		//get closest poses.
		float[] poseArr = poseToPosVelArray(pose);
		List<MotionPose> neighbors = NearestNeighbor (poseArr, numActions);
		float[][] neighborsArr = neighbors.Select (x => poseToPosVelArray (x)).ToArray ();
		float[] neighbors_weights = GenerateWeights(poseArr, neighborsArr);

		//get closest tasks.
		List<List<float>> nearest_vals = new List<List<float>> ();
		for(int i=0; i < Tasks.Length; i++){
			//TODO: if Tasks[i] is the min or max, values added could e out of range. add a check
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
        precomputedRewards.Clear();
        foreach (ArrayList arrLst in lst)
        {
            MotionPose mp = arrLst[0] as MotionPose;
            float[] taskarr = arrLst[1] as float[];
            precomputedRewards.Add(new vfKey(mp.animClipRef.name, mp.timestamp, taskarr), System.Convert.ToSingle(arrLst[2]));
        }
    }
}

//NodeData to be removed, MotionPose will be the data field of the kd tree
public class NodeData{
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
}

public class vfKey{
	public string clipId;
	public float timeStamp;
	public float[] tasks;

	public vfKey(string id, float time, float[] tasks){
		this.clipId = id;
		this.timeStamp = time;
		this.tasks = tasks;
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
