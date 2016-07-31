using UnityEngine;
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
                int length = selectedMotionFieldController.animClipInfoList[0].motionPoses[0].flattenedMotionPose.Length;
                float[] queryPoint = new float[length];

                for(int i = 0; i < length; ++i){
                    queryPoint[i] = 0;
                }
                MotionPose[] poses = selectedMotionFieldController.NearestNeighbor(queryPoint);

                foreach(MotionPose pose in poses)
                {
                    Debug.Log("AnimName: " + pose.animName + ", Timestamp: " + pose.timestamp + "\n");
                }
            }

            //skinnedMesh = (ModelImporterClipAnimation) EditorGUILayout.ObjectField("skinMesh: ", skinnedMesh, typeof(ModelImporterClipAnimation), false);
            if (GUILayout.Button("Generate Rewards Table")) {
                if(selectedMotionFieldController.kd == null)
                {
                    Debug.LogError("KDTree is not initialized! Generate Poses first.");
                }
                else
                {
                    GenerateRewardsTable();
                }
            }

            EditorGUILayout.EndVertical();


        }

        private void GenerateRewardsTable()
        {
            if(selectedMotionFieldController.TArrayInfo == null){
                Debug.LogError("You must assign a TaskArray to the MotionField");
                return;
            }
            if(selectedMotionFieldController.numActions <= 0)
            {
                Debug.LogError("The numActions variable in the MotionField must be a positive int");
                return;
            }
            //create precomputed table of reward lookups at (every pose in kdtree)*(range of potential task values)

            //get list of task arrays to sample reward at
            int taskSize = selectedMotionFieldController.TArrayInfo.TaskArray.Count();
            List<List<float>> taskArr_samples = new List<List<float>>();
            for (int i = 0; i < taskSize; i++)
            {
                //dont change math on how tasks are sampled unless you know what your doing. must make equivalent changes when accessing dict in MFController
                List<float> tasksamples = new List<float>();
                float min = selectedMotionFieldController.TArrayInfo.TaskArray[i].min;
                float max = selectedMotionFieldController.TArrayInfo.TaskArray[i].max;
                int numSamples = selectedMotionFieldController.TArrayInfo.TaskArray[i].numSamples;

                //min + ((max-min)*i)/(numSamples-1);
                float interval = (max - min) / (numSamples - 1);
                for (int j = 0; j < numSamples; ++j)
                {
                    float sample = j * interval + min;
                    tasksamples.Add(sample);
                }
                taskArr_samples.Add(tasksamples);
            }
            taskArr_samples = selectedMotionFieldController.CartesianProduct(taskArr_samples);
            Debug.Log("num of task samples: " + taskArr_samples.Count + "\ntask length: " + taskArr_samples[0].Count + "\n task 0 val: " + taskArr_samples[0][0].ToString());

            //create initial rewardTable as List<ArrayList>
            //each arraylist has MotionPose in [0], float[] of tasks in [1] and float for reward in [2]
            List<ArrayList> rewardTable = new List<ArrayList>();
            foreach (AnimClipInfo animclip in selectedMotionFieldController.animClipInfoList)
            {
                foreach (MotionPose pose in animclip.motionPoses)
                {
                    foreach (List<float> taskArr in taskArr_samples)
                    {
                        ArrayList arr = new ArrayList();
                        arr.Add(pose);
                        arr.Add(taskArr.ToArray());
                        arr.Add(0.0f);
                        rewardTable.Add(arr);
                    }
                }
            }

            //now recursively update fitness values to get the future reward
            //to guarantee calculated future reward is within p*(immediateReward) of 'the future reward after infinite generations', 
            //with a scaling of s (the nth gen has scaled reward of reward*s^(n-1), so with 0<s<1, this will approach a limit) number of gens to run is
            //ceil (ln(-p ln(S)) / ln(S))

            float s = selectedMotionFieldController.scale;
            float p = 0.1f;
            int generations = System.Convert.ToInt32(Mathf.Ceil((Mathf.Log(-p * Mathf.Log(s))) / Mathf.Log(s)));

            Debug.Log("numActions: " + selectedMotionFieldController.numActions.ToString());
            Debug.Log("running for " + generations.ToString() + " generations");

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

            float numcycles = generations * rewardTable.Count;
            float averagetime = 0.0f;
            int maxtime = 0;
            int mintime = 1000;

            for (int i = 0; i < generations; i++)
            {
                selectedMotionFieldController.makeDictfromList(rewardTable);

                int genstart = i * rewardTable.Count;
                for (int j = 0; j < rewardTable.Count; j++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Generating Rewards", "generation " + (i + 1).ToString() + " of " + generations.ToString() + "... ", ((genstart + j) / numcycles)))
                    {
                        Debug.Log("Gen Rewards Canceled");
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    //TODO: note that the precomputedRewards table is only updated between generations.
                    //therefore, the order points are run to find there rewards does not matter, making this section easy to parallelize.
                    //could be very beneficial, as generating the rewards table is likely to be rather slow.

                    //also, if in need of more performance, could perhaps only calucate reward for every 'x' points, and other nearby points are extrapolated.
                    //dont know how negatively this would effect accuracy, but if negligible could provide large speed boost.
                    MotionPose pose = (MotionPose)rewardTable[j][0];
                    float[] taskarr = (float[])rewardTable[j][1];
                    float reward = float.MinValue;

                    stopWatch.Start();

                    selectedMotionFieldController.MoveOneFrame(pose, taskarr, ref reward);

                    stopWatch.Stop();
                    System.TimeSpan ts = stopWatch.Elapsed;
                    averagetime += ts.Milliseconds;
                    if(maxtime < ts.Milliseconds){
                        maxtime = ts.Milliseconds;
                    }
                    if(mintime > ts.Milliseconds){
                        mintime = ts.Milliseconds;
                    }
                    stopWatch.Reset();

                    rewardTable[j][2] = reward;
                }
            }

            EditorUtility.ClearProgressBar();

            averagetime = averagetime / numcycles;
            Debug.Log("Move One Frame Timings:     avg: " + averagetime.ToString() + "     max: " + maxtime.ToString() + "     min: " + mintime.ToString());

            //finally, set to the initializer in selectedMotionFieldController.
            //at runtime, this is converted to a dictionary
            selectedMotionFieldController.precomputedRewards_Initializer = rewardTable;
        }

        private void BuildMotionField() {
            if (selectedMotionFieldController != null) {
                MotionFieldUtility.GenerateMotionField(ref selectedMotionFieldController, selectedMotionFieldComponent, frameResolution);
            }
            else
            {
                Debug.LogError("selectedMFController is null. No kdtree generation occurred.");
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