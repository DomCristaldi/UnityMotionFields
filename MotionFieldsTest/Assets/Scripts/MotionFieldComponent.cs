using UnityEngine;
//using UnityEngine.Experimental.Director;
using System.Collections.Generic;
//using System.Linq;

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
        public List<CosmeticSkeletonBone> cosmeticBones;

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

            return _reorderList.GetHeight(); //GetReorderList(property.FindPropertyRelative("cosmeticBones")).GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            //base.OnGUI(position, property, label);

            if (_reorderList == null) { _reorderList = GetReorderList(property.FindPropertyRelative("cosmeticBones")); }

            property.serializedObject.Update();

            _reorderList.DoList(position);

            property.serializedObject.ApplyModifiedProperties();
        }

    }
#endif

    [ExecuteInEditMode]
    [RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        Animator anim;

        public MotionFieldController controller;

        public CosmeticSkeleton cosmeticSkel;

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

        void Awake() {
            anim = GetComponent<Animator>();
        }

        // Use this for initialization
        void Start () {
            //anim.Play();
            //transform.LerpTransform(transform, transform, 0.0f, Transform_ExtensionMethods.LerpType.Position | Transform_ExtensionMethods.LerpType.Rotation | Transform_ExtensionMethods.LerpType.Scale);
        }

        // Update is called once per frame
        void Update () {
            //Debug.Log("adf");
            //anim.SetTime(t);

            //transform.LerpTransform(startPos.transform, endPos.transform, lerpVal);

            //lerpVal += lerpDelta * Time.deltaTime;

        }

        //public 

        public void ApplyMotionPoseToSkeleton(MotionPose pose) {
            if (controller == null) {
                Debug.LogErrorFormat("Motion Field Controller for {0} has not been assigned. Cannot apply Motion Pose", gameObject.name);
                return;
            }

            if (cosmeticSkel == null) {
                Debug.LogErrorFormat("Skeleton for {0} has not been assigned. Cannot apply Motion Pose", gameObject.name);
                return;
            }


            cosmeticSkel.ApplyPose(pose);

        }

    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MotionFieldComponent))]
    public class MotionFieldComponent_Editor : Editor {

        private MotionFieldComponent selfScript;
        private Animator animControl;

        public int selectedAnim;
        public int selectedPose;

        void OnEnable() {

            selfScript = (MotionFieldComponent)target;
            animControl = selfScript.GetComponent<Animator>();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            DrawMotionApplicationTool();

            EditorGUILayout.Vector3Field("Angular Vel:", animControl.angularVelocity);
            EditorGUILayout.Vector3Field("Vel: ", animControl.velocity);
            EditorGUILayout.Vector3Field("DeltaPos: ", animControl.deltaPosition);
            EditorGUILayout.Vector3Field("Center of Mass: ", animControl.bodyPosition);
        }


        private void DrawMotionApplicationTool() {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Test Your Motion Pose");

            selectedAnim = EditorGUILayout.IntSlider(selectedAnim, 0, selfScript.controller.animClipInfoList.Count - 1);
            selectedPose = EditorGUILayout.IntSlider(selectedPose, 0, selfScript.controller.animClipInfoList[selectedAnim].motionPoses.Length);

            if (GUILayout.Button("ApplyPose")) {
                selfScript.ApplyMotionPoseToSkeleton(selfScript.controller.animClipInfoList[selectedAnim].motionPoses[selectedPose]);
            }

            EditorGUILayout.EndVertical();
        }

    }

#endif

}

