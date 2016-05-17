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

    [System.Serializable]
    public class CosmeticSkeleton {
        public List<CosmeticSkeletonBone> cosmeticBones;

        public void ApplyPose(MotionPose pose) {

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

        private ReorderableList reorderList;
        private float elementPadding = 0.5f;

        private ReorderableList GetReorderList(SerializedProperty property) {
            if (reorderList == null) {

                reorderList = new ReorderableList(property.serializedObject, property, true, true, true, true);
                reorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.width -= 40;
                    rect.x += 20;

                    //if (isFocused) { Debug.Log(property.GetArrayElementAtIndex(index).FindPropertyRelative("boneLabel").stringValue); }


                    EditorGUI.PropertyField(rect, 
                                            property.GetArrayElementAtIndex(index), 
                                            new GUIContent(property.GetArrayElementAtIndex(index).FindPropertyRelative("boneLabel").stringValue), 
                                            true);
                };

                reorderList.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "Bone Lookup Table", EditorStyles.boldLabel);
                };

            }
            return reorderList;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

            return GetReorderList(property.FindPropertyRelative("cosmeticBones")).GetHeight();

            /*
            if (reorderList != null) {

                //return reorderList.elementHeight * reorderList.count;
                return reorderList.GetHeight();
            }
            else { return base.GetPropertyHeight(property, label); }
            */
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            //base.OnGUI(position, property, label);

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty listProp = property.FindPropertyRelative("cosmeticBones");
            reorderList = GetReorderList(listProp);
            float height = 0.0f;
            for (int i = 0; i < listProp.arraySize; ++i) {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProp.GetArrayElementAtIndex(i))) + elementPadding;
            }
            reorderList.elementHeight = height;
            reorderList.DoList(position);

            EditorGUI.EndProperty();
        }

    }
#endif

    //[RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        public MotionFieldController controller;

        public CosmeticSkeleton cosmeticSkel;

        //public MotionSkeleton skeleton;

        private CosmeticSkeleton curPose;
        private CosmeticSkeleton nextPose;

        public GameObject startPos;
        public GameObject endPos;

        float lerpVal = 0.0f;
        public float lerpDelta = 0.2f;

        void Awake() {

        }

        // Use this for initialization
        void Start () {
            //transform.LerpTransform(transform, transform, 0.0f, Transform_ExtensionMethods.LerpType.Position | Transform_ExtensionMethods.LerpType.Rotation | Transform_ExtensionMethods.LerpType.Scale);
        }

        // Update is called once per frame
        void Update () {


            transform.LerpTransform(startPos.transform, endPos.transform, lerpVal);

            //lerpVal += lerpDelta * Time.deltaTime;

	    }

        //public 

    }



}
