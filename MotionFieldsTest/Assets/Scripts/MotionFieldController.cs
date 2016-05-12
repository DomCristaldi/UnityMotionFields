using UnityEngine;
using System.Collections.Generic;
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

    //Initialize with positional information
    public BoneTransform(Transform tf) {
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
}

[System.Serializable]
public class BonePose {
    public string boneLabel;

    public BoneTransform position;
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

	public float[] currentTaskArray;

	//TODO: have the precomputed value func dictionary here
	//wait...wouldn't this have to be a dictionary.
	//dictionaries are not serializable...
	//if i remember correctly, we found a way to manually make dicts serializable. 
	//create it as a list, then at runtime we transfer it to a dictionary lookup.

	public void moveOneTick(float[] currentPos, int numActions = 1){
		List<NodeData> neighbors = NearestNeighbor (currentPos, numActions);

		double[] weights = GenerateWeights (currentPos, neighbors);

		double[][] actionWeights = GenerateActions(weights, numActions);

		List<float[]> candidateActions = new List<float[]>(); //not actuallu MotionPose. datatyoe is type of dome new skeleton heirarchy.
		foreach (double[] action in actionWeights){
			candidateActions.Add(GeneratePose(currentPos, neighbors, action)); //GeneratePose does the weighted blending, will be written by Dom later.
		}

		int chosenAction = -1;
		float bestReward = 0;
		for (int i=0; i < candidateActions.Count(); i++){
			float reward = ComputeReward(candidateActions[i], numActions);
			if (reward > bestReward){
				bestReward = reward;
				chosenAction = i;
			}
		}

		//action for player to move through known! it is candidateActions[chosenAction]. 
		//TODO: apply it to the character, then using this new current state, find next state for him to move to.
		
		
		
	}

	public List<NodeData> NearestNeighbor(float[] float_pos, int num_neighbors = 1){

		double[] pos = float_pos.Select (x => System.Convert.ToDouble (x)).ToArray ();
		object[] nn_data = kd.nearest (pos, num_neighbors);

		List<NodeData> data = new List<NodeData>();
		foreach(object obj in nn_data){
			data.Add((NodeData) obj);
		}
		return data;
	}

	public double[][] GenerateActions(double[] weights, int numActions = 1){

		double[][] actions = new double[numActions] [];
		for(int i = 0; i < numActions; i++){
			//for each action array, set weight[i] to 1 and renormalize
			actions [i] = new double[weights.Length];
			actions [i] = (double[])weights.Clone ();
			actions [i] [i] = 1;
			double actionSum = actions[i].Sum();
			for(int j = 0; j < actions[i].Length; j++){
				actions[i][j] =actions[i][j] / actionSum;
			}
		}

		return actions;
	}

	public double[] GenerateWeights(float[] float_pos, List<NodeData> neighbors){

		double[] weights = new double[neighbors.Count];

		//weights[i] = 1/distance(neighbors[i] , floatpos) ^2 
		for(int i = 0; i < neighbors.Count; i++){
			weights [i] = 0;
			for(int j = 0; j < neighbors[i].position.Length; j++){
				weights [i] += (double)Mathf.Pow (float_pos [i * 2] - (float)neighbors [i].position [j], 2);
				weights [i] += (double)Mathf.Pow (float_pos [i * 2 + 1] - (float)neighbors [i].velocity [j], 2);
			}
			weights [i] = 1.0 / weights [i];
		}

		//now normalize weights so that they sum to 1
		double weightsSum = weights.Sum();
		string printW = "weights: ";
		for(int i = 0; i < weights.Length; i++){
			weights [i] = weights [i] / weightsSum;
			printW += weights[i] + "  ";
		}
		Debug.Log (printW);

		return weights;
	}

	public float[] GeneratePose(float[] currentPos, List<NodeData> neighbors, double[] action){
		//placeholder func. takes in current motionstate, neighbor states, and weights of neighbor states.
		//does weighted blending, returns blended state
		float[] ret = new float[1] {0.0f};
		return ret;
    }

	public static List<List<float>> CartesianProduct( List<List<float>> sequences){
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

	public float ComputeReward(float[] candidatePos, int numActions = 1){
		//Calculates the reward for a specific motionstate

		//frst, get the current task array for the motionstate
		//simultaneously calculate immediate reward
		int tasklength = TArrayInfo.TaskArray.Count();
		float immediateReward = 0.0f;
		float[] newtasks = new float[tasklength];
		for(int i = 0; i < tasklength; i++){
			newtasks[i] = TArrayInfo.TaskArray[i].DetermineTaskValue(candidatePos);
			immediateReward += TArrayInfo.TaskArray[i].CheckReward (currentTaskArray[i], newtasks [i]);
		}

		//calculate continuousReward
		float continuousReward = RewardLookup(candidatePos, newtasks, numActions);

		return immediateReward + continuousReward;
	}

	public float RewardLookup(float[] pose, float[] Tasks, int numActions = 1){
		//get continuous reward from valuefunc lookup table.
		//reward is weighted blend of closest values in lookup table.
		//get closest poses from kdtree, and closest tasks from cartesian product
		//then get weighted rewards from lookup table for each pose+task combo

		//get closest poses.
		List<NodeData> neighbors = NearestNeighbor (pose, numActions);
		double[] neighbors_weights = GenerateWeights(pose, neighbors);

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
		List<List<float>> taskMatrixCurrent = CartesianProduct(nearest_vals);
		List<float> taskMatrixCurrent_weights = new List<float> (); //TODO: make helper func to get these weights
																				//first normalize each element such that min-max is 0-1. so that each elem has equal weight.

		//get matrix of neighbors x tasks. The corresponding weight matrix should sum to 1.
		List<List<float>> taskNeighbors = new List<List<float>> ();
		List<float> taskNeighbors_weights = new List<float> ();
		for (int i = 0; i < neighbors.Count(); i++){
			List<float> pos = neighbors [i].position.Select(x => (float)x).ToList();
			for (int j = 0; j < taskMatrixCurrent.Count(); j++){
				taskNeighbors.Add (pos.Concat (taskMatrixCurrent [j]).ToList ());
				taskNeighbors_weights.Add ((float)neighbors_weights [i] * taskMatrixCurrent_weights [j]);
			}
		}

		//do lookups in precomputed table, get weighted sum
		float continuousReward = 0.0f;
		for(int i = 0; i < taskNeighbors.Count(); i++){
			//TODO: must first define precomputed table!
			//continuousReward += precomputedTable(taskNeighbors[i])*taskNeighbors_weights[i];
		}

		return continuousReward;
	}
}

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
