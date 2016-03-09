using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif



namespace AnimationMotionFields {

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

        public void GenerateMotionPoses(int samplingResolution, string[] totalAnimPaths) {
            motionPoses = MotionFieldUtility.GenerateMotionPoses(animClip,
                                                                 totalAnimPaths,
                                                                 samplingResolution,
                                                                 velocityCalculationMode);
        }

        public void PrintPathTest() {
            foreach (EditorCurveBinding ecb in AnimationUtility.GetCurveBindings(animClip)) {
                Debug.Log("path " + ecb.propertyName);
            }
        }
    }


    [CreateAssetMenu]
    public class MotionFieldController : ScriptableObject {

        //USER DEFINED ANIMATION CURVE PATHS FOR ROOT MOTION COMPONENT
        public string rootComponent_tx;
        public string rootComponent_ty;
        public string rootComponent_tz;

        public string rootComponent_qx;
        public string rootComponent_qy;
        public string rootComponent_qz;
        public string rootComponent_qw;


        public List<AnimClipInfo> animClipInfoList;

		public KDTreeDLL.KDTree kd;

        public void GenerateMotionField(int samplingResolution) {

            //Debug.LogFormat("Total things: {0}", MotionFieldCreator.GetUniquePaths(animClipInfoList.Select(x => x.animClip).ToArray()).Length);

			string[] uniquePaths = MotionFieldUtility.GetUniquePaths(animClipInfoList.Select(x => x.animClip).ToArray());

            foreach (AnimClipInfo clipInfo in animClipInfoList) {
                if (!clipInfo.useClip) {//only generate motion poses for the selected animations
                    clipInfo.motionPoses = new MotionPose[] { };
                }
                else {
                    clipInfo.GenerateMotionPoses(samplingResolution, uniquePaths);
                }
            }

			GenerateKDTree (uniquePaths.Length * 2);
        }

		public void GenerateKDTree(int numDimensions){
			
			kd = new KDTreeDLL.KDTree(numDimensions); 

			Debug.Log ("tree made with " + numDimensions + " dimensions");

			foreach (AnimClipInfo clipinfo in animClipInfoList) {
			
				foreach (MotionPose pose in clipinfo.motionPoses) {
			
                    //extract all the values from the Motion Field Controller's Keyframe Datas and convert them to a list of doubles
					double[] position = pose.keyframeData.Select(x => System.Convert.ToDouble(x.value)).ToArray();//hot damn LINQ
					double[] velocity = pose.keyframeData.Select(x => System.Convert.ToDouble(x.velocity)).ToArray();//hot damn LINQ
					double[] velocityNext = pose.keyframeData.Select(x => System.Convert.ToDouble(x.velocityNext)).ToArray();//hot damn LINQ

					NodeData data = new NodeData (pose.animClipRef.name, pose.timestamp, position, velocity, velocityNext);

					double[] position_velocity_pairings = new double[numDimensions];
					for(int i = 0; i < position.Length; i++){
						position_velocity_pairings[i*2] = position[i];
						position_velocity_pairings[i*2+1] = velocity[i];
					}

					string stuff = "Inserting id:" + data.clipId + " , time: " + data.timeStamp.ToString () + "  position_velocity_pairing:(";
					foreach(double p in position_velocity_pairings){ stuff += p.ToString() + ", ";  }
					Debug.Log (stuff + ")");

					try
					{
						kd.insert (position_velocity_pairings, data);
					}
					catch (KDTreeDLL.KeyDuplicateException e)
					{
						Debug.Log("Duplicate position_velocity_pairing! skip inserting pt.");
					}
				}
			}

			Debug.Log ("tree generated");
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

		public double[][] GenerateActions(float[] float_pos, int numActions = 1){
			List<NodeData> neighbors = NearestNeighbor (float_pos, numActions);

			double[] weights = GenerateWeights (float_pos, neighbors);

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
			
    }

	public class NodeData{
		public string clipId;
		public float timeStamp;
		public double[] position;
		public double[] velocity;
		public double[] velocityNext;

		public NodeData(string id, float time, double[] position, double[] velocity, double[] velocityNext){
			this.clipId = id;
			this.timeStamp = time;
			this.position = position;
			this.velocity = velocity;
			this.velocityNext = velocityNext;
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

}