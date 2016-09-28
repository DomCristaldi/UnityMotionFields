using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace AnimationMotionFields {

    [System.Serializable]
    public class CosmeticSkeletonBone {
        public enum MovementSpace {
            Local = 0,
            World = 1,
        }

        public MovementSpace boneMovementSpace;

        public string boneLabel;
        public Transform boneTf;

    }

    //TODO: SERIALIZE TO DICTIONARY AT RUNTIME
    [System.Serializable]
    public class CosmeticSkeleton {

        //public GameObject marker;

        public Transform skeletonRoot;
        public Transform rootMotionReferencePoint;

        public Avatar avatar;

        public List<CosmeticSkeletonBone> cosmeticBones;

        //TODO: Write GetCurrentPose() to get the current Motion Pose of the skeleton

        public void ApplyPose(MotionPose pose) {
            foreach (BonePose poseBone in pose.bonePoses) {
                foreach (CosmeticSkeletonBone cosBone in cosmeticBones) {
                    if (cosBone.boneLabel == poseBone.boneLabel) {
                        cosBone.boneTf.localPosition = new Vector3(poseBone.value.posX,
                                                                   poseBone.value.posY,
                                                                   poseBone.value.posZ);
                        cosBone.boneTf.localRotation = new Quaternion(poseBone.value.rotX,
                                                                      poseBone.value.rotY,
                                                                      poseBone.value.rotZ,
                                                                      poseBone.value.rotW);
                        //cosBone.boneTf.l

                        continue;
                    }
                }
            }
        }

        //TODO: Try to make dictionary lookup
        public CosmeticSkeletonBone GetBone(string boneLabel)
        {
            foreach (CosmeticSkeletonBone bone in cosmeticBones) {
                if (bone.boneLabel == boneLabel) {
                    return bone;
                }
            }

            return null;
        }

        //TODO: Try to make dictionary lookup
        public CosmeticSkeletonBone GetBone(Transform boneTf)
        {
            foreach (CosmeticSkeletonBone bone in cosmeticBones) {
                if (bone.boneTf == boneTf) {
                    return bone;
                }
            }

            return null;
        }
        /*
        public MotionPose GetCurrentPose()
        {
            BonePose[] currentBonePositions = cosmeticBones.Select(cb => cb.boneTf)
                                                           .Select(tf => )
        }
        */
    }

#if UNITY_EDITOR
    /*
    [CustomPropertyDrawer(typeof(CosmeticSkeletonBone))]
    public class CosmeticSkeletonBone_PropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            //base.OnGUI(position, property, label);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.PropertyField(position, property, new GUIContent("bone"), true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            //return base.GetPropertyHeight(property, label);

            return 200.0f;
        }

    }
    */
    
    [CustomPropertyDrawer(typeof(CosmeticSkeleton))]
    public class CosmeticSkeleton_PropertyDrawer : PropertyDrawer {

        private ReorderableList _reorderList;
        private float _elementPadding = 0.5f;

        private ReorderableList GetReorderList(SerializedProperty property) {
            if (_reorderList == null) {

                //SerializedProperty serList = property.FindPropertyRelative("cosmeticBones");

                _reorderList = new ReorderableList(property.serializedObject, property, true, true, true, true);

    //DRAW ELEMENT CALLBACK
                _reorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.width -= 40;
                    rect.x += 20;

                    //if (isFocused) { Debug.Log(property.GetArrayElementAtIndex(index).FindPropertyRelative("boneLabel").stringValue); }


                    EditorGUI.PropertyField(rect, 
                                            property.GetArrayElementAtIndex(index), 
                                            new GUIContent(property.GetArrayElementAtIndex(index).FindPropertyRelative("boneLabel").stringValue), 
                                            true);
                };


    //ELEMENT HEIGHT CALLBACK
                _reorderList.elementHeightCallback = (int index) => {
                    return EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)) + _elementPadding;
                };

    //DRAW HEADER CALLBACK
                _reorderList.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "Bone Lookup Table", EditorStyles.boldLabel);
                };

            }
            return _reorderList;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            if (_reorderList == null) { _reorderList = GetReorderList(property.FindPropertyRelative("cosmeticBones")); }

            float propHeight = 0.0f;

            propHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("skeletonRoot"))
                        + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rootMotionReferencePoint"))
                        + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("avatar"))
                        + _reorderList.GetHeight();
                        //+ EditorGUI.GetPropertyHeight(_reorderList.serializedProperty);

            return propHeight;
            /*
            propHeight += _reorderList.GetHeight();
            */

            //return EditorGUI.GetPropertyHeight(_reorderList.serializedProperty);

            //return EditorGUI.GetPropertyHeight(property);

            //return propHeight;
            //return GetReorderList(property.FindPropertyRelative("cosmeticBones")).GetHeight();

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            float yVal = position.y;

            //SerializedProperty markerProp = property.FindPropertyRelative("marker");

            SerializedProperty skelRootProp = property.FindPropertyRelative("skeletonRoot");
            SerializedProperty rootMotionRefPointProp = property.FindPropertyRelative("rootMotionReferencePoint");
            SerializedProperty avatarProp = property.FindPropertyRelative("avatar");

            EditorGUI.BeginProperty(position, label, property);


            if (_reorderList == null) { _reorderList = GetReorderList(property.FindPropertyRelative("cosmeticBones")); }

            property.serializedObject.Update();
            /*
            EditorGUI.PropertyField(new Rect(position.x, yVal,
                                             position.width, EditorGUI.GetPropertyHeight(markerProp)),
                                    markerProp);
            yVal += EditorGUI.GetPropertyHeight(markerProp);
            */

            //DRAW SKELETON ROOT PROPERTY FIELD
            EditorGUI.PropertyField(new Rect(position.x, yVal,
                                             position.width, EditorGUI.GetPropertyHeight(skelRootProp)),
                                    skelRootProp);
            yVal += EditorGUI.GetPropertyHeight(skelRootProp);


            //DRAW ROOT MOTION REFERENCE POINT PROPERTY FIELD
            EditorGUI.PropertyField(new Rect(position.x, yVal,
                                             position.width, EditorGUI.GetPropertyHeight(rootMotionRefPointProp)),
                                    rootMotionRefPointProp);
            yVal += EditorGUI.GetPropertyHeight(rootMotionRefPointProp);


            //DRAW AVATAR PROPERTY FIELD
            EditorGUI.PropertyField(new Rect(position.x, yVal,
                                             position.width, EditorGUI.GetPropertyHeight(avatarProp)),
                                    avatarProp);
            yVal += EditorGUI.GetPropertyHeight(avatarProp);


            //_reorderList.DoList(position);
            _reorderList.DoList(new Rect(position.x, yVal,
                                         position.width, EditorGUI.GetPropertyHeight(_reorderList.serializedProperty)));

            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
    
#endif


    //[ExecuteInEditMode]
    [RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        Animator animControl;

        BlendSwitcherPlayable blendSwitcher;

        public int initialPoseClipIndex;
        public float initialPoseClipTimestamp;

        public CosmeticSkeleton cosmeticSkel;
        public MotionFieldController controller;


        //public MotionSkeleton skeleton;

        //private CosmeticSkeleton curPose;
        //private CosmeticSkeleton nextPose;

        //public GameObject startPos;
        //public GameObject endPos;

        //float lerpVal = 0.0f;
        //public float lerpDelta = 0.2f;
        //public MotionSkeleton skeleton;

        //private MotionPose currentPos;

        private float[] currentTaskArray;
        //public MotionSkeletonBonePlayable testRoot;
        //MotionSkeletonBonePlayable testPlayable;

        //public float t;

        public bool useRootMotion = true;

        private MotionPose curMotionPose;


        void Awake() {

            animControl = GetComponent<Animator>();

            //HACK: in release build, it should be impossible to call functions from MotionFieldUtility
            MotionFieldUtility.GenerateKDTree(ref controller.kd, controller.animClipInfoList);
            controller.DeserializeDict();

            //create the custom playalbe for driving the animations
            blendSwitcher = Playable.Create<BlendSwitcherPlayable>();
            blendSwitcher.InitBlendSwitcher(controller.animClipInfoList[1].animClip,
                                            0.0f);

            animControl.Play(blendSwitcher);
        }
		
        // Use this for initialization
        void Start () {
            if (controller != null) {
                for(int i = 0; i < controller.animClipInfoList.Count; ++i)
                {
                    if(controller.animClipInfoList[i].useClip == true)
                    {
                        curMotionPose = controller.animClipInfoList[i].motionPoses[0];
                        break;
                    }
                }
            }

            StartCoroutine(RunMixers_TEST(0.25f));
        }

        // Update is called once per frame
        void Update () {

            if (blendSwitcher.mixer.IsValid()) {
                GraphVisualizerClient.Show(blendSwitcher.mixer, "Blend Switcher");
            }

        }

        void OnGUI()
        {
            Rect windowRect = new Rect(5, 5, 1000, 500);

            windowRect = GUILayout.Window(0, windowRect, DisplayDebugInfo, "Debug Window");

            //DisplayDebugInfo();

        }

        public void ApplyMotionPoseToSkeleton(MotionPose pose) {
            if (controller == null) {
                Debug.LogErrorFormat("Motion Field Controller for {0} has not been assigned. Cannot apply Motion Pose", gameObject.name);
                return;
            }

            if (cosmeticSkel == null) {
                Debug.LogErrorFormat("Skeleton for {0} has not been assigned. Cannot apply Motion Pose", gameObject.name);
                return;
            }



            /*
            cosmeticSkel.ApplyPose(pose);

            if (useRootMotion) {
                ApplyRootMotion(pose.rootMotionInfo.value.position, pose.rootMotionInfo.value.rotation);
            }
            */
        }

        //Apply the root motoin to the character
        public void ApplyRootMotion(Vector3 translation, Quaternion rotation) {

            //apply position first, because it was extracted from the movement of the previous frame and that previous frame's rotation
            transform.position += transform.rotation * translation;

            //now it's safe to apply the rotation
            transform.rotation *= rotation;
        }

        //TEMP: Function to help test if the motion poses are correct, called from an Editor Script
        public IEnumerator PlayOutMotionPoseAnim(int animIndex) {

            if (controller == null) { yield break; }

            foreach (MotionPose pose in controller.animClipInfoList[animIndex].motionPoses) {

                ApplyMotionPoseToSkeleton(pose);

                yield return null;
            }


            yield break;
        }

        public IEnumerator RunMixers_TEST(float waitTime)
        {

            MotionPose newPose;
            candidatePose[] candidates;

            int clipIndex = 0;

            float timeSinceLastBlend = 0.0f - Time.deltaTime;
            int framesSinceLastBlend = 0;
            float timeStampOfLastBlend = curMotionPose.timestamp;
            int indexOfLastBlend = 0;
            int index = 0;

            AnimClipInfo currentAnimInfo = controller.animClipInfoList
                                        .Where(clipInfo => clipInfo.animClip.name == curMotionPose.animName)
                                        .First();

            //get index of curMotionPose
            for (indexOfLastBlend = 0; indexOfLastBlend < currentAnimInfo.motionPoses.Length; indexOfLastBlend++) {
                if (currentAnimInfo.motionPoses[indexOfLastBlend].timestamp == curMotionPose.timestamp) {
                    break;
                }
            }

            //yield return new WaitForSeconds(waitTime);
            yield return null;

            while (true) {

                //yield return null;

                //advance curMotionPose as time pases
                timeSinceLastBlend += Time.deltaTime;
                framesSinceLastBlend = (int) (timeSinceLastBlend / currentAnimInfo.frameStep);
                index = (int)Mathf.Min(indexOfLastBlend + framesSinceLastBlend, currentAnimInfo.motionPoses.Length - 1); //gets min to prevent index being higher than array length
                curMotionPose = currentAnimInfo.motionPoses[index];

                Debug.LogFormat("Time Since Last Blend: {0}", timeSinceLastBlend);
                Debug.LogFormat("Current Anim Frame Step: {0}", currentAnimInfo.frameStep);
                Debug.LogFormat("Frames Since Last Blend: {0}", framesSinceLastBlend);
                Debug.LogFormat("Current Pose Timestamp: {0}", curMotionPose.timestamp);


                candidates = controller.OneTick(curMotionPose);

                newPose = candidates[0].pose;

                AnimClipInfo selectedAnimInfo = controller.animClipInfoList
                                                        .Where(clipInfo => clipInfo.animClip.name == newPose.animName)
                                                        .First();

                //CHECK IF WE SHOUDL BLEND OUT TO ANOTHER ANIMATION OR A DIFFERENT TIME ON THE SAME TRACK
                //bool newPoseIsTooSimilar = selectedAnimInfo.animClip.name == blendSwitcher.targetClipName
                //                           && Mathf.Abs(newPose.timestamp - blendSwitcher.targetClipTime) < 0.2f;

                bool newPoseIsTooSimilar = selectedAnimInfo.animClip.name == curMotionPose.animName
                                           && Mathf.Abs(newPose.timestamp - curMotionPose.timestamp) < 0.2f;


                /*
                float targetNodeTimestamp = blendSwitcher.targetClipTime;
                if((selectedAnim.name != blendSwitcher.targetClipName)  //we're jumping to a different animation
                    |                                                     // OR  
                    Mathf.Abs(targetNodeTimestamp - newPose.timestamp) > 2.0f //we're on the same anim, but the time difference is large enough
                ){                                                                //HACK: 0.2f magic number, it represents transition diffrence threshold
                    */

                /*
                Debug.LogFormat("Current Pose Timestamp: {0}\nPose is too similar: {1}",
                                curMotionPose.timestamp,
                                newPoseIsTooSimilar);
                */

                string AbrahamLincoln = "";
                for (int i = 0; i < candidates.Length; ++i)
                {
                    AbrahamLincoln += " candidate #" + (i + 1) + ": " + candidates[i].pose.animName + " at time " + candidates[i].pose.timestamp + " with reward " + candidates[i].reward + "\n";
                }
                Debug.Log(AbrahamLincoln);

                if (!newPoseIsTooSimilar) {

                    Debug.LogWarning("-------Blend To New Pose-------");

                    blendSwitcher.BlendToAnim(selectedAnimInfo.animClip, newPose.timestamp);
                    curMotionPose = newPose;

                    timeSinceLastBlend = 0.0f;
                    framesSinceLastBlend = 0;
                    timeStampOfLastBlend = curMotionPose.timestamp;
                    currentAnimInfo = selectedAnimInfo;

                    //get index of curMotionPose
                    for (indexOfLastBlend = 0; indexOfLastBlend < currentAnimInfo.motionPoses.Length; indexOfLastBlend++) {
                        if (currentAnimInfo.motionPoses[indexOfLastBlend].timestamp == curMotionPose.timestamp) {
                            break;
                        }
                    }
                }


                //selectedAnim = controller.animClipInfoList[clipIndex].animClip;
                //blendSwitcher.BlendToAnim(selectedAnim, Random.Range(0.0f, selectedAnim.length));

                ++clipIndex;
                if (clipIndex >= controller.animClipInfoList.Count) {
                    clipIndex = 0;
                }



                //yield return new WaitForSeconds(waitTime);
                //yield return null;
                yield return new WaitForEndOfFrame();
            }

        }

#if UNITY_EDITOR

        private void DisplayDebugInfo(int windowID)
        {
            GUILayout.BeginVertical();

            //GUILayout.Label("testing");

            if (blendSwitcher == null
                || !blendSwitcher.mixer.IsValid())
            {
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label(GetLayoutName(0, "First Node"));

            GUILayout.Label("Transition Percentage: " + blendSwitcher.transitionPercentage);

            GUILayout.Label(GetLayoutName(1, "Second Node"));

            GUILayout.Label(controller.currentTaskOutput);

            GUILayout.EndVertical();
        }

        private string GetLayoutName(int inputIndex, string displayName = "Node")
        {
            if(blendSwitcher.mixer.GetInput(inputIndex).IsValid()) {

                AnimationClipPlayable node = blendSwitcher.mixer.GetInput(inputIndex).CastTo<AnimationClipPlayable>();
                string nodeName = node.clip.name;
                float nodeTime = (float)node.time;

                return displayName + ": " + nodeName + " - " + nodeTime.ToString();
            }

            else {
                return displayName + " Invalid";
            }


}

#endif


#if UNITY_EDITOR

[SerializeField]
        private bool g_showSkeleton;

        [SerializeField]
        private Color g_skeletonBoneColor = Color.cyan;
        [SerializeField]
        private Color g_skeletonJointColor = Color.yellow;
        [SerializeField]
        private float g_skeletonJointRadius = 0.05f;

        [Space]
        [SerializeField]
        private Color g_bodyOrientaionVecColor = Color.magenta;
        [SerializeField]
        private Color g_hipsOrientationVecColor = Color.blue;

        [Space]
        [SerializeField]
        private Color g_flooredBodyPosColor = Color.red;
        [SerializeField]
        private float g_flooredBodyPosRadius = 0.05f;

        [Space]
        [SerializeField]
        private Transform g_skeletonRoot;

        void OnDrawGizmos() {
            Color originalGizmoColor = Gizmos.color;
            
            if (g_showSkeleton) { Gizmo_DrawSkeletonHierarchy(g_skeletonRoot); }
            Gizmo_DrawBodyOrientation();

            Gizmos.color = originalGizmoColor;
        }

        private void Gizmo_DrawSkeletonHierarchy(Transform root) {

            foreach (Transform tf in root) {
                Gizmo_DrawSkeletonHierarchy(tf);
            }

            Gizmos.color = g_skeletonBoneColor;
            Gizmos.DrawLine(root.position, root.parent.position);

            Gizmos.color = g_skeletonJointColor;
            Gizmos.DrawSphere(root.position, g_skeletonJointRadius);
        }

        private void Gizmo_DrawBodyOrientation() {

            Gizmos.color = g_bodyOrientaionVecColor;

            HumanPoseHandler hPoseHandler = new HumanPoseHandler(cosmeticSkel.avatar, cosmeticSkel.skeletonRoot);
            HumanPose hPose = new HumanPose();
            hPoseHandler.GetHumanPose(ref hPose);

            Gizmos.DrawLine(hPose.bodyPosition + transform.position,
                            hPose.bodyPosition + (hPose.bodyRotation * Vector3.forward * 12.0f));


            //Debug.LogFormat("Body Position: {0}", hPose.bodyPosition);

            Gizmos.color = g_hipsOrientationVecColor;

            Gizmos.DrawLine(cosmeticSkel.skeletonRoot.position,
                            cosmeticSkel.skeletonRoot.position + (cosmeticSkel.skeletonRoot.rotation * Vector3.forward * 12.0f));



            Gizmos.color = g_flooredBodyPosColor;
            Gizmos.DrawLine(new Vector3(hPose.bodyPosition.x,
                                        cosmeticSkel.rootMotionReferencePoint.position.y,
                                        hPose.bodyPosition.z),
                            hPose.bodyRotation * Vector3.forward * 12.0f);

            Gizmos.DrawLine(new Vector3(hPose.bodyPosition.x,
                                        cosmeticSkel.rootMotionReferencePoint.position.y,
                                        hPose.bodyPosition.z),
                            hPose.bodyRotation * Vector3.up * 12.0f);

        }
#endif

    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MotionFieldComponent))]
    public class MotionFieldComponent_Editor : Editor {

        private MotionFieldComponent selfScript;
        private Animator animControl;

        public int selectedAnim = 0;
        public int selectedPose = 0;

        public Color rootMotionColor = Color.red;

        void OnEnable() {

            selfScript = (MotionFieldComponent)target;
            animControl = selfScript.GetComponent<Animator>();
        }

        void OnSceneGUI() {
            Color originalHandleColor = Handles.color;

            DrawRootMotionVectors();
            //DrawBodyOrientation();

            Handles.color = originalHandleColor;

            //Repaint();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            /*
            EditorGUILayout.Space();
            if (GUILayout.Button("Transcribe Bone Poses")) {
                TranscribeBoneLabelsFromControllerTool();
            }
            */

            EditorGUILayout.Space();
            DrawMotionApplicationTool();

            EditorGUILayout.Space();

            EditorGUILayout.Vector3Field("Angular Vel:", animControl.angularVelocity);
            EditorGUILayout.Vector3Field("Vel: ", animControl.velocity);
            EditorGUILayout.Vector3Field("DeltaPos: ", animControl.deltaPosition);
            //EditorGUILayout.Vector3Field("Center of Mass: ", animControl.bodyPosition);
        }

        private void TranscribeBoneLabelsFromControllerTool() {
            if (selfScript.controller == null) { return; }
        //HACK: come back and use a Bone Labels Scriptable Object instead (when it finally exists
            if (selfScript.controller.animClipInfoList.Count == 0) { return; }
            if (selfScript.controller.animClipInfoList[0].motionPoses.Length == 0) { return; }
            /*
            selfScript.cosmeticSkel.cosmeticBones = new List<CosmeticSkeletonBone>();
            
            foreach (string s in selfScript.controller.animClipInfoList[0].motionPoses.Select(x => x.bonePoses[0].boneLabel)) {
                selfScript.cosmeticSkel.cosmeticBones.Add(new CosmeticSkeletonBone() { boneLabel = s });
            }
            */
        }

        private void DrawMotionApplicationTool() {
            if (selfScript.controller == null) { return; }

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Test Your Motion Pose");

            selectedAnim = EditorGUILayout.IntSlider(selectedAnim, 0, selfScript.controller.animClipInfoList.Count - 1);
            selectedPose = EditorGUILayout.IntSlider(selectedPose, 0, selfScript.controller.animClipInfoList[selectedAnim].motionPoses.Length);

            if (GUILayout.Button("ApplyPose")) {
                selfScript.ApplyMotionPoseToSkeleton(selfScript.controller.animClipInfoList[selectedAnim].motionPoses[selectedPose]);
            }

            if (GUILayout.Button("Play thorugh poses")) {
                selfScript.StartCoroutine(selfScript.PlayOutMotionPoseAnim(selectedAnim));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRootMotionVectors() {

            if (selfScript.controller == null) { return; }

            Matrix4x4 originalMatrix = Handles.matrix;
            Handles.matrix = selfScript.transform.localToWorldMatrix;

            Handles.color = rootMotionColor;


            if (selfScript.controller.animClipInfoList.Count > 0) {
                if (selfScript.controller.animClipInfoList[selectedAnim].motionPoses.Length > 0) {
                    Handles.DrawLine(Vector3.zero, //selfScript.transform.position,
                                     selfScript.controller.animClipInfoList[selectedAnim].motionPoses[selectedPose].rootMotionInfo.value.rotation * 
                                     selfScript.controller.animClipInfoList[selectedAnim].motionPoses[selectedPose].rootMotionInfo.value.position *
                                     1000.0f);
                }

            }

            Handles.matrix = originalMatrix;
        }

        private void DrawBodyOrientation() {
            Handles.color = Color.cyan;
            Matrix4x4 originalMatrix = Handles.matrix;

            HumanPoseHandler hPoseHandler = new HumanPoseHandler(selfScript.cosmeticSkel.avatar, selfScript.cosmeticSkel.skeletonRoot);
            HumanPose hPose = new HumanPose();
            hPoseHandler.GetHumanPose(ref hPose);

            //hPose.
            Matrix4x4 poseMat = new Matrix4x4();
            poseMat.SetTRS(hPose.bodyPosition, hPose.bodyRotation, Vector3.one);

            //Handles.matrix = poseMat;

            Handles.DrawLine(hPose.bodyPosition, hPose.bodyRotation * Vector3.forward * 12.0f);

            Handles.matrix = originalMatrix;
        }
    }

#endif

}

