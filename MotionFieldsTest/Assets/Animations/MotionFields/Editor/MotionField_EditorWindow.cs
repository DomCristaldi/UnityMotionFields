﻿using UnityEngine;
using UnityEngine.Experimental.Director;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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


        MotionSkeleton selectedMotionSkeleton;
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

        [SerializeField]
		public int numActions = 1;
    

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

            EditorGUILayout.BeginHorizontal();
            selectedMotionSkeleton = (MotionSkeleton) EditorGUILayout.ObjectField("Motion Skeleton: ", selectedMotionSkeleton, typeof(MotionSkeleton), false);
            EditorGUILayout.EndHorizontal();

            if (selectedMotionSkeleton == null) {
                EditorGUILayout.HelpBox("Please Select a Motion Skeleton to Modify", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            if (selectedMotionSkeleton.rootDeserializedBone == null) {
                if (GUILayout.Button("Assign Root Bone")) {
                    selectedMotionSkeleton.rootDeserializedBone = new MotionSkeleton.MSBoneDeserialized() { boneLabel = selectedGO.transform.name };

                    EditorUtility.SetDirty(selectedMotionSkeleton);
                    //AssetDatabase.SaveAssets();
                }
            }
            else {
                DisplayMotionSkeletonHierarchy(selectedMotionSkeleton.rootDeserializedBone);
            }
            

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

        private void DisplayMotionSkeletonHierarchy(MotionSkeleton.MSBoneDeserialized rootBone) {
            EditorGUILayout.BeginVertical();

            ++EditorGUI.indentLevel;

            EditorGUILayout.LabelField(rootBone.boneLabel);

            if (rootBone.children.Count > 0) {
                foreach (MotionSkeleton.MSBoneDeserialized child in rootBone.children) {
                    DisplayMotionSkeletonHierarchy(child);
                }   
            }

            --EditorGUI.indentLevel;

            EditorGUILayout.EndVertical();
        }

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

            if (selectedMotionFieldController == null || selectedMotionFieldComponent == null) {
                return;
            } 

            //if (selectedMotionFieldController != null) {
            else {
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
            }

            if (GUILayout.Button("print path")) {
                selectedMotionFieldController.animClipInfoList[0].PrintPathTest();
            }


            if (GUILayout.Button("test point 0")) {
                float[] queryPoint = new float[selectedMotionFieldController.animClipInfoList[0].motionPoses[0].flattenedMotionPose.Length];
                for (int i = 0; i < queryPoint.Length; ++i) {
                    queryPoint[i] = 0.0f;
                }

				foreach (MotionPose pose in selectedMotionFieldController.NearestNeighbor(queryPoint, numActions)) {
					Debug.Log("AnimName: " + pose.animClipRef.name + ", Timestamp: " + pose.timestamp + "\n");
                }

            }

			EditorGUILayout.BeginHorizontal ();

			numActions = EditorGUILayout.IntField (numActions);

			EditorGUILayout.EndHorizontal ();

            //skinnedMesh = (ModelImporterClipAnimation) EditorGUILayout.ObjectField("skinMesh: ", skinnedMesh, typeof(ModelImporterClipAnimation), false);
			if (GUILayout.Button("Generate Rewards Table")) {
                //create precomputed table of reward lookups at (every pose in kdtree)*(range of potential task values)

                //get list of task arrays to sample reward at
				int taskSize = selectedMotionFieldController.TArrayInfo.TaskArray.Count();
				List<List<float>> taskArr_samples = new List<List<float>> ();
				for(int i=0; i < taskSize; i++){
					List<float> tasksamples = new List<float> ();
					float min = selectedMotionFieldController.TArrayInfo.TaskArray [i].min;
					float max = selectedMotionFieldController.TArrayInfo.TaskArray [i].max;
					int numSamples = selectedMotionFieldController.TArrayInfo.TaskArray [i].numSamples;

					float interval = (max - min) / numSamples;
					for(int j = 0; j < numSamples; j++){
						tasksamples.Add (min + (interval * j));
					}
					taskArr_samples.Add (tasksamples);
				}
				taskArr_samples = selectedMotionFieldController.CartesianProduct(taskArr_samples);

                //create initial rewardTable as List<ArrayList>
                //each arralist has MotionPose in [0], float[] for Tarray in [1] and float for reward in [2]
                List<ArrayList> rewardTable = new List<ArrayList>(); 
				foreach(AnimClipInfo animclip in selectedMotionFieldController.animClipInfoList ){
					foreach(MotionPose pose in animclip.motionPoses){
						foreach(List<float> taskArr in taskArr_samples){
							ArrayList arr = new ArrayList();
							arr.Add (pose);
                            arr.Add(taskArr);
							arr.Add (0.0f);
                            rewardTable.Add (arr);
						}
					}
				}

                //now recursively update fitness values to get the future reward
                //to guarantee future reward is within r*(immediateReward) of future reward after infinite generations, 
                //with a scaling of s, number of gens to run is
                //ceil (log(-r log(S)) / log(S))
                float s = selectedMotionFieldController.scale;
                float r = 0.1f;
                int generations = System.Convert.ToInt32(Mathf.Ceil((Mathf.Log(-r * Mathf.Log(s)))/Mathf.Log(s)));

                for(int i = 0; i < generations; i++)
                {
                    selectedMotionFieldController.makeDictfromList(rewardTable);

                    foreach(ArrayList point in rewardTable)
                    {
                        MotionPose pose = point[0] as MotionPose;
                        float[] taskarr = point[1] as float[];
                        point[2] = selectedMotionFieldController.moveOneTick(ref pose, ref taskarr, numActions);
                    }
                }

                //finally, set to the initializer in selectedMotionFieldController.
                //at runtime, this is converted to a dictionary
                selectedMotionFieldController.precomputedRewards_Initializer = rewardTable;
            }

            EditorGUILayout.EndVertical();


        }

        private void BuildMotionField() {
            if (selectedMotionFieldController != null) {
                MotionFieldUtility.GenerateMotionField(ref selectedMotionFieldController, selectedMotionFieldComponent, frameResolution);
            }
        }

        //CLIP EXTRACTION
        private void DoClipExtractionGUI() {

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