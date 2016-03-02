using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {

    public class MotionField_EditorWindow : EditorWindow {

        [SerializeField]
        public MotionFieldController selectedMotionField;

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
            selectedMotionField = (MotionFieldController) EditorGUILayout.ObjectField("Motion Field: ", selectedMotionField, typeof(MotionFieldController), false);


            EditorGUILayout.EndHorizontal();

            if (selectedMotionField != null) {
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
                    selectedMotionField.GenerateMotionField(frameResolution);
                }

                frameResolution = EditorGUILayout.IntField("Frame Resolution ", frameResolution);
                if (frameResolution < 1) { frameResolution = 1; }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("print path")) {
                selectedMotionField.animClipInfoList[0].PrintPathTest();
            }

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