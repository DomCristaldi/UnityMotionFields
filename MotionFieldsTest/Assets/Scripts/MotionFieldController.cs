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

        public KeyframeData(float value = 0.0f, float velocity = 0.0f, float outVelocity = 0.0f) {
            this.value = value;
            this.velocity = velocity;
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

			GenerateKDTree (uniquePaths.Length);
        }

		public void GenerateKDTree(int dim){
			
			kd = new KDTreeDLL.KDTree(dim); 

			Debug.Log ("tree made with " + dim + " dimensions");

			foreach (AnimClipInfo clipinfo in animClipInfoList) {
			
				foreach (MotionPose pose in clipinfo.motionPoses) {
			
					NodeData data = new NodeData (pose.animClipRef.name, pose.timestamp);
                    //extract all the values from the Motion Field Controller's Keyframe Datas and convert them to a list of doubles
					double[] pos = pose.keyframeData.Select(x => System.Convert.ToDouble(x.value)).ToArray();//hot damn LINQ

					string stuff = "Inserting id:" + data.clipId + " , time: " + data.timeStamp.ToString () + "  pos:(";
					foreach(double p in pos){ stuff += p.ToString() + ", ";  }
					Debug.Log (stuff + ")");

					try
					{
						kd.insert (pos, data);
					}
					catch (KDTreeDLL.KeyDuplicateException e)
					{
						Debug.Log("Duplicate pos! skip inserting pt.");
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
			
    }

	public class NodeData{
		public string clipId;
		public float timeStamp;

		public NodeData(string id, float time){
			this.clipId = id;
			this.timeStamp = time;
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