using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif



namespace AnimationMotionFields {

    [System.Serializable]
    public class MotionPose {

        public MotionPose(AnimationClip animClipRef, float timestamp, float[] keyframeData) {
            this.animClipRef = animClipRef;
            this.timestamp = timestamp;
            this.keyframeData = keyframeData;
        }

        //public AnimationClip[] poses;
        public AnimationClip animClipRef;
        public float timestamp;

        public float[] keyframeData;

        public int frameSampleRate;//sampel rate that was used to create these poses

    }

    [System.Serializable]
    public class AnimClipInfo {
        public bool useClip = true;
        public AnimationClip animClip;

        public MotionPose[] motionPoses;//all the poses generated for this animation clip

        public int frameSampleRate;//sampel rate that was used to create these poses

        public void GenerateMotionPoses(int samplingResolution, string[] totalAnimPaths) {
            motionPoses = MotionFieldCreator.GenerateMotionPoses(animClip,
                                                                 samplingResolution,
                                                                 totalAnimPaths);
        }

        public void PrintPathTest() {
            foreach (EditorCurveBinding ecb in AnimationUtility.GetCurveBindings(animClip)) {
                Debug.Log("path " + ecb.propertyName);
            }
        }
    }


    [CreateAssetMenu]
    public class SO_MotionField : ScriptableObject {

        public List<AnimClipInfo> animClipInfoList;

		public KDTreeDLL.KDTree kd;

        public void GenerateMotionField(int samplingResolution) {

            //Debug.LogFormat("Total things: {0}", MotionFieldCreator.GetUniquePaths(animClipInfoList.Select(x => x.animClip).ToArray()).Length);

			string[] uniquePaths = MotionFieldCreator.GetUniquePaths(animClipInfoList.Select(x => x.animClip).ToArray());

            foreach (AnimClipInfo clipInfo in animClipInfoList) {
                clipInfo.GenerateMotionPoses(samplingResolution, uniquePaths);
            }

			GenerateKDTree (uniquePaths.Length);
        }

		public void GenerateKDTree(int dim){
			
			kd = new KDTreeDLL.KDTree(dim); 

			foreach (AnimClipInfo clipinfo in animClipInfoList) {
			
				foreach (MotionPose pose in clipinfo.motionPoses) {
			
					NodeData data = new NodeData (pose.animClipRef.name, pose.timestamp);
					double[] pos = pose.keyframeData.Select(x => System.Convert.ToDouble(x)).ToArray();

					kd.insert (pos, data);
				}
			}
		}

		public NodeData[] NearestNeighbor(float[] float_pos, int num_neighbors = 1){
			double[] pos = float_pos.Select (x => System.Convert.ToDouble (x)).ToArray ();
			object[] nn_data = kd.nearest (pos, num_neighbors);
			NodeData[] data = (NodeData[])nn_data;

			return data;
		}
			
    }

	public class NodeData{
		public string clipId;
		public int timeSamp;

		public NodeData(string id, int time){
			this.clipId = id;
			this.timeSamp = time;
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