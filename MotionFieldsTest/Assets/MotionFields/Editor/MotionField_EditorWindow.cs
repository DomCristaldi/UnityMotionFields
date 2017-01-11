﻿using UnityEngine;
using UnityEngine.Experimental.Director;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;

namespace AnimationMotionFields {

    public class MotionField_EditorWindow : EditorWindow {

        public enum WindowSetting {
            Markup = 0,
            Generation = 1,
            ClipExtraction = 2,
        }
        public WindowSetting curWindowSetting;
        protected bool lockSelection = false;


        public GameObject selectedGO;


        //MotionSkeleton selectedMotionSkeleton;
        SerializedObject serializedSelectedMotoinSkeleton;


        [SerializeField]
        public MotionFieldController selectedMotionFieldController;

        [SerializeField]
        public MotionFieldComponent selectedMotionFieldComponent;

        //public ModelImporterClipAnimation skinnedMesh;
        

        //[SerializeField]
        //public List<AnimationClip> animClips;

        //private ReorderableList reorderableAnimClips;

        [SerializeField]
        private int _frameResolution = 1;
        public int frameResolution {
            get { return _frameResolution; }
            set {
                if (value > 0) {
                    _frameResolution = value;
                }
            }
        }

        [SerializeField]
        private int _numKDTreeDimensions = 1;
        public int numKDTreeDimensions {
            get { return _numKDTreeDimensions; }
            set {
                if (value > 0) {
                    _numKDTreeDimensions = value;
                }
            }
        }

        [MenuItem("MotionFields/Motion Field Author")]
        static void Init() {
            MotionField_EditorWindow window = (MotionField_EditorWindow)EditorWindow.GetWindow(typeof(MotionField_EditorWindow));
            window.Show();
        }


        void OnEnable() {
            //animClips = new List<AnimationClip>();

            //reorderableAnimClips = new ReorderableList(new List<AnimationClip>(), typeof(AnimationClip), true, true, true, true);

        }

        void OnSelectionChange() {
            if (lockSelection) { return; }

            if (Selection.activeGameObject == null) {
                selectedGO = null;
            }
            else {
                selectedGO = Selection.activeGameObject;
            }
            //serializedSelectedMotoinSkeleton = new SerializedObject(Selection.activeGameObject);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI() {

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Markup", EditorStyles.toolbarButton)) {
                curWindowSetting = WindowSetting.Markup;
            }
            if (GUILayout.Button("Generation", EditorStyles.toolbarButton)) {
                curWindowSetting = WindowSetting.Generation;
            }
            if (GUILayout.Button("Clip Extraction", EditorStyles.toolbarButton)) {
                curWindowSetting = WindowSetting.ClipExtraction;
            }

            GUILayout.FlexibleSpace();

            lockSelection = GUILayout.Toggle(lockSelection, "Lock", EditorStyles.radioButton);

            EditorGUILayout.EndHorizontal();


            switch (curWindowSetting) {
                case WindowSetting.Markup:
                    DoMarkupGUI();
                    break;

                case WindowSetting.Generation:
                    DoGenerationGUI();
                    break;

                case WindowSetting.ClipExtraction:
                    DoClipExtractionGUI();
                    break;
               
                default:
                    goto case WindowSetting.Markup;
            }

        }

//RUNTIME SKELETON MARKUP
        private void DoMarkupGUI() {

            EditorGUILayout.BeginVertical();


            if (selectedGO == null) {
                EditorGUILayout.HelpBox("Please Select a GameObject to Mark Up", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            //EditorGUILayout.BeginHorizontal();
            //selectedMotionSkeleton = (MotionSkeleton) EditorGUILayout.ObjectField("Motion Skeleton: ", selectedMotionSkeleton, typeof(MotionSkeleton), false);
            //EditorGUILayout.EndHorizontal();

            //if (selectedMotionSkeleton == null) {
            //    EditorGUILayout.HelpBox("Please Select a Motion Skeleton to Modify", MessageType.Info);
            //    EditorGUILayout.EndVertical();
            //    return;
            //}

            //if (selectedMotionSkeleton.rootDeserializedBone == null) {
            //    if (GUILayout.Button("Assign Root Bone")) {
            //        selectedMotionSkeleton.rootDeserializedBone = new MotionSkeleton.MSBoneDeserialized() { boneLabel = selectedGO.transform.name };

            //        EditorUtility.SetDirty(selectedMotionSkeleton);
            //        //AssetDatabase.SaveAssets();
            //    }
            //}
            //else {
            //    DisplayMotionSkeletonHierarchy(selectedMotionSkeleton.rootDeserializedBone);
            //}
            

            /*
            if (selectedMotionSkeleton.rootBone == null) {
                if (GUILayout.Button("Assign Root Bone")) {
                    selectedMotionSkeleton.rootBone = new MotionSkeletonBone(selectedGO.transform);
                    //SerializedProperty rootBone = serializedSelectedMotoinSkeleton.FindProperty("rootBone");
                    //rootBone.objectReferenceValue = new MotionSkeletonBone(selectedGO.transform);
                }
            }
            else {
                EditorGUILayout.BeginVertical();

                DisplayMotionSkeletonHierarchy(selectedMotionSkeleton.rootBone);

                EditorGUILayout.EndVertical();
            }
            */


            EditorGUILayout.EndVertical();

        }

        //private void DisplayMotionSkeletonHierarchy(MotionSkeleton.MSBoneDeserialized rootBone) {
        //    EditorGUILayout.BeginVertical();

        //    ++EditorGUI.indentLevel;

        //    EditorGUILayout.LabelField(rootBone.boneLabel);

        //    if (rootBone.children.Count > 0) {
        //        foreach (MotionSkeleton.MSBoneDeserialized child in rootBone.children) {
        //            DisplayMotionSkeletonHierarchy(child);
        //        }   
        //    }

        //    --EditorGUI.indentLevel;

        //    EditorGUILayout.EndVertical();
        //}

        /*
        private void DisplayMotionSkeletonHierarchy(MotionSkeletonBone skeletonRoot) {

            EditorGUILayout.LabelField(skeletonRoot.boneTransformRef.name);

            EditorGUI.indentLevel++;

            Playable[] skeletonInputs = skeletonRoot.GetInputs();
            if (skeletonInputs.Length == 0) {
                foreach (Transform tf in skeletonRoot.boneTransformRef) {

//*********COME HERE, IMPLEMENT + AND - BUTTONS FOR ADDING AND SUBTRACTING BONES FROM MOTION SKELETON***************
                    
                    EditorGUILayout.LabelField(tf.name);
                }
            }
            else {
                foreach (Playable p in skeletonInputs) {
                    EditorGUILayout.BeginVertical();
                    DisplayMotionSkeletonHierarchy((MotionSkeletonBone) p);
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUI.indentLevel--;
        }
        */
        
//MOTION FIELD GENERATION TOOLS
        private void DoGenerationGUI() {

            EditorGUILayout.BeginVertical();
            //EditorGUILayout.BeginHorizontal();


            /*
            if (selectedMotionField != null) {
                if (GUILayout.Button("Load Motion Field Data")) {
                    LoadMotionField();
                }
            }
            */

            EditorGUILayout.BeginVertical();

            //OBJECT FIELD FOR THE MOTION FIELD
            selectedMotionFieldController = (MotionFieldController) EditorGUILayout.ObjectField("Motion Field: ", selectedMotionFieldController, typeof(MotionFieldController), false);

            selectedMotionFieldComponent = (MotionFieldComponent)EditorGUILayout.ObjectField("Model: ", selectedMotionFieldComponent, typeof(MotionFieldComponent), true);

            //EditorGUILayout.EndHorizontal();

            if (selectedMotionFieldController == null) {
                EditorGUILayout.HelpBox("Please assign a Motion Field Controller", MessageType.Info);
            }
            if (selectedMotionFieldComponent == null) {
                EditorGUILayout.HelpBox("Please assign a GameObject in the scene with a Motion Field Component attatched to it to Model", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            if (selectedMotionFieldController != null && selectedMotionFieldComponent != null) {

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

        //MOTION POSE GENERATION
                if (GUILayout.Button("Generate Poses")) {
                    BuildMotionField();
                }

            //FRAME RESOLUTION
                frameResolution = EditorGUILayout.IntField("Frame Resolution", frameResolution);


                GUILayout.EndHorizontal();


                if (GUILayout.Button("print path"))
                {
                    selectedMotionFieldController.animClipInfoList[0].PrintPathTest();
                }

                if (GUILayout.Button("extraction tests"))
                {
                    ExtractionTest();
                }

                //skinnedMesh = (ModelImporterClipAnimation) EditorGUILayout.ObjectField("skinMesh: ", skinnedMesh, typeof(ModelImporterClipAnimation), false);
            }

            EditorGUILayout.EndVertical();
        }

        private void BuildMotionField() {
            if (selectedMotionFieldController != null) {
                MotionFieldUtility.GenerateMotionField(ref selectedMotionFieldController, selectedMotionFieldComponent, frameResolution);
            }
            else {
                Debug.LogError("selectedMFController is null. No kdtree generation occurred.");
            }
        }

        //CLIP EXTRACTION
        private void DoClipExtractionGUI() {

        }

        private void ExtractionTest()
        {
            //data from frame 3 and 4 of WalkBackwardTurnRight_NtrlShort

            /*Vector3 Anim1HipPos = new Vector3(-0.3777913f, 0.9221899f, -0.5506681f);
            Quaternion Anim1HipRot = new Quaternion(0.9264047f, 0.03554143f, -0.371581f, -0.04938647f);*/
            Vector3 Anim1BodyPos = new Vector3(-0.3340412f, 1.165285f, -0.5276758f);
            Quaternion Anim1BodyRot = new Quaternion(0.9273599f, 0.01532163f, -0.3729019f, 0.02670267f);

            Vector3 Anim2HipPos = new Vector3(-0.401341f, 0.924288f, -0.5359539f);
            Quaternion Anim2HipRot = new Quaternion(0.9350371f, 0.03767011f, -0.3487912f, -0.0512968f);
            Vector3 Anim2BodyPos = new Vector3(-0.3571083f, 1.167358f, -0.5132191f);
            Quaternion Anim2BodyRot = new Quaternion(0.936076f, 0.01930104f, -0.3503604f, 0.02523528f);

            /*
            Vector3 Extract1HipPos = new Vector3();
            Quaternion Extract1HipRot = new Quaternion();
            Vector3 Extract1BodyPos = new Vector3();
            Quaternion Extract1BodyRot = new Quaternion();
            Vector3 Extract1RefPos = new Vector3();
            Quaternion Extract1RefRot = new Quaternion();

            Vector3 Extract2HipPos = new Vector3();
            Quaternion Extract2HipRot = new Quaternion();
            Vector3 Extract2BodyPos = new Vector3();
            Quaternion Extract2BodyRot = new Quaternion();
            Vector3 Extract2RefPos = new Vector3();
            Quaternion Extract2RefRot = new Quaternion();
            */

            /*Vector3 Our1HipPos = new Vector3(-0.04375014f, 0.9221899f, -0.02299225f);
            Quaternion Our1HipRot = new Quaternion(-0.007021381f, 0.00265461f, -0.006545626f, 0.9999504f);
            Vector3 Our1BodyPos = new Vector3(-0.3093421f, 1.161235f, -0.5405856f);
            Quaternion Our1BodyRot = new Quaternion(0.00974481f, -0.3973446f, 0.02646581f, 0.917236f);
            Vector3 Our1RefPos = new Vector3(-0.007449157f, 0.0000f, 0.02661834f);
            Quaternion Our1RefRot = new Quaternion(0.0000f, 0.02652925f, 0.0000f, 0.999648f);

            Vector3 Our2HipPos = new Vector3(-0.0442327f, 0.924288f, -0.02273476f);
            Quaternion Our2HipRot = new Quaternion(-0.007331711f, 0.002695813f, - 0.006178242f, 0.9999504f);
            Vector3 Our2BodyPos = new Vector3(-0.3571083f, 1.167358f, -0.5132191f);
            Quaternion Our2BodyRot = new Quaternion(0.01930104f, -0.3503604f, 0.02523528f, 0.936076f);
            Vector3 Our2RefPos = new Vector3(-0.00669281f, 0f, 0.02622292f);
            Quaternion Our2RefRot = new Quaternion(0f, 0.02417144f, 0f, 0.9997079f);*/



            //is the refPos the floored bodyPos??? weve been making this assumption, i believe...
            //should bodyRot also first be floored on xz plane? does flooring a quaternion even make sense?
            //hip position has to be rotated relative to the bodyorientation, i think?!?

            //these are all WRONG....
            Vector3 new2HipPos = Anim2HipPos - floored(Anim2BodyPos); //-0.401341 -0.2430701 -0.5359539
            Quaternion new2HipRot = Anim2HipRot * Quaternion.Inverse(Anim2BodyRot); //0.07807971 0.0008358167 -0.009559315 0.996901
            Vector3 new2RefPos = Anim2BodyPos - Anim1BodyPos; //-0.02306709 0.00207305 0.01445669
            Quaternion new2RefRot = Anim2BodyRot * Quaternion.Inverse(Anim1BodyRot); //0.003422844 -0.02402559 0.003611526 0.9996989

            printVec(new2HipPos, "hipPos");
            Debug.Log("HipRot: " + new2HipRot.debugString());
            printVec(new2RefPos, "RefPos");
            Debug.Log("RefPos: " + new2RefRot.debugString());
        }

        private Vector3 floored(Vector3 vec)
        {
            //project vector onto xz plane
            return Vector3.ProjectOnPlane(vec, Vector3.up);
        }

        private void printVec(Vector3 vec, string name = "")
        {
            Debug.Log(name + ":  " + vec.x + " " + vec.y + " " + vec.z);
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