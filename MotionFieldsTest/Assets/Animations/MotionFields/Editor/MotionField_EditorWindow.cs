using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {

    public class MotionField_EditorWindow : EditorWindow {

        [SerializeField]
        public MotionFieldController selectedMotionFieldController;

        public ModelImporterClipAnimation skinnedMesh;
        

        //[SerializeField]
        //public List<AnimationClip> animClips;

        private ReorderableList reorderableAnimClips;

        [SerializeField]
        public int frameResolution = 1;
    

        [MenuItem("MotionFields/Motion Field Author")]
        static void Init() {
            MotionField_EditorWindow window = (MotionField_EditorWindow)EditorWindow.GetWindow(typeof(MotionField_EditorWindow));
            window.Show();
        }


        void OnEnable() {
            //animClips = new List<AnimationClip>();

            reorderableAnimClips = new ReorderableList(new List<AnimationClip>(), typeof(AnimationClip), true, true, true, true);

        }


        void OnGUI() {

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();


            /*
            if (selectedMotionField != null) {
                if (GUILayout.Button("Load Motion Field Data")) {
                    LoadMotionField();
                }
            }
            */
            //OBJECT FIELD FOR THE MOTION FIELD
            selectedMotionFieldController = (MotionFieldController) EditorGUILayout.ObjectField("Motion Field: ", selectedMotionFieldController, typeof(MotionFieldController), false);


            EditorGUILayout.EndHorizontal();

            if (selectedMotionFieldController != null) {
                //reorderableAnimClips.DoLayoutList();
                /*
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Analyze Curves")) {
                    //List<AnimationCurve> embeddedCurves = new List<AnimationCurve>();

                    AnimationCurve[] embeddedCurves = MotionFieldCreator.FindAnimCurves(selectedMotionField.animClipInfoList.Select(x => x.animClip).ToArray());

                    Debug.LogFormat("Total Curves: {0}", embeddedCurves.Length);
                }

                if (GUILayout.Button("Analyze Keyframes")) {

                    Debug.LogFormat("Total Keyframes: {0}", 
                                    selectedMotionField.animClipInfoList[0].animClip.length * selectedMotionField.animClipInfoList[0].animClip.frameRate);
                }
                GUILayout.EndHorizontal();
                */

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Generate Poses")) {
                    selectedMotionFieldController.GenerateMotionField(frameResolution);
                }

                frameResolution = EditorGUILayout.IntField("Frame Resolution ", frameResolution);
                if (frameResolution < 1) { frameResolution = 1; }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("print path")) {
                selectedMotionFieldController.animClipInfoList[0].PrintPathTest();
            }


            if (GUILayout.Button("test point 0")) {
                float[] queryPoint = new float[MotionFieldUtility.GetUniquePaths(selectedMotionFieldController).Length * 2];
                for (int i = 0; i < queryPoint.Length; ++i) {
                    queryPoint[i] = 0.0f;
                }

                foreach (NodeData node in selectedMotionFieldController.NearestNeighbor(queryPoint)) {
                    Debug.Log(node.PrintNode() + "\n");
                }

            }

            //skinnedMesh = (ModelImporterClipAnimation) EditorGUILayout.ObjectField("skinMesh: ", skinnedMesh, typeof(ModelImporterClipAnimation), false);


            EditorGUILayout.EndVertical();


        }

    /*
        private void LoadMotionField() {
            if (selectedMotionField != null) {
                var serObj = new UnityEditor.SerializedObject(selectedMotionField);

                //reorderableAnimClips = new ReorderableList(animClips, typeof(AnimationClip), true, true, true, true);
                reorderableAnimClips = new ReorderableList(serObj, serObj.FindProperty("animClips"), true, true, true, true);
            }
            else {
                Debug.LogWarning("Motion Field Author: No Assigned Motion Field Found");
            }
        
        }
        */

    }
}