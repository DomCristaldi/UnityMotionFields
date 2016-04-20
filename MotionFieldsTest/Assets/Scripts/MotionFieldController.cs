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
    public class MotionPose {

        public MotionPose(AnimationClip animClipRef, float timestamp, KeyframeData[] keyframeData) {
            this.animClipRef = animClipRef;
            this.timestamp = timestamp;
            this.keyframeData = keyframeData;
        }

        public MotionPose(AnimationClip animClipRef, float timestamp, float[] keyframeValueData) {
            this.animClipRef = animClipRef;
            this.timestamp = timestamp;
            this.keyframeData = new KeyframeData[keyframeValueData.Length];//initialize array length

            for (int i = 0; i < keyframeData.Length; ++i) {
                //must create object so it exists in the array
                keyframeData[i] = new KeyframeData(value:keyframeValueData[i]);
            }
        }

        //public AnimationClip[] poses;
        public AnimationClip animClipRef;
        public float timestamp;

        //public float[] keyframeData;
        public KeyframeData[] keyframeData;

        public int frameSampleRate;//sampel rate that was used to create these poses

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

		public ValueFunc vf;

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
			float reward = vf.ComputeReward(candidateActions[i]);
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
