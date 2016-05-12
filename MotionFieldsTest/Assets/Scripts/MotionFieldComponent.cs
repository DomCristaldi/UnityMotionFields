using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace AnimationMotionFields {

    [System.Serializable]
    public class CosmeticSkeletonBone {
        public string boneLabel;
        public Transform boneTf;
    }

    [System.Serializable]
    public class CosmeticSkeleton {
        public List<CosmeticSkeletonBone> cosmeticBones;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(CosmeticSkeleton))]
    public class CosmeticSkeleton_PropertyDrawer : PropertyDrawer {

        private ReorderableList reorderList;

        private ReorderableList GetReorderList(SerializedProperty property) {
            if (reorderList == null) {

                reorderList = new ReorderableList(property.serializedObject, property, true, true, true, true);
                reorderList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.width -= 40;
                    rect.x += 20;
                    EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), true);
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
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProp.GetArrayElementAtIndex(i)));
            }
            reorderList.elementHeight = height;
            reorderList.DoList(position);

            EditorGUI.EndProperty();
        }

    }
#endif

    [RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        //private Animator _animatorComponent;

        //public MotionFieldController assignedMotionFieldController;

        //public int numFramesToBlend = 1;

        //private MotionFieldMixerRoot motionFieldMixer;

        public CosmeticSkeleton cosmeticSkel;

        public MotionSkeleton skeleton;
        //public MotionSkeletonBonePlayable testRoot;
        //MotionSkeletonBonePlayable testPlayable;

        void Awake() {
            //skeleton.Init();


            //testPlayable = new MotionSkeletonBonePlayable();

            //if (testRoot != null) { testRoot.Dispose(); }
            //testRoot = skeleton.GenerateSkeleton();
            //testRoot = new MotionSkeletonBonePlayable();

            //_animatorComponent = GetComponent<Animator>();

            //testRoot = new MotionSkeletonBonePlayable();
            /*
            motionFieldMixer = new MotionFieldMixerRoot(assignedMotionFieldController.animClipInfoList
                                                                                        .Where(x => x.useClip)
                                                                                        .Select(x => x.animClip).ToArray(),
                                                        numFramesToBlend
                                                        );
            */

            //motionFieldMixer.SetClipWeight(assignedMotionFieldController.animClipInfoList[0].animClip.name, 0.3f);
            //motionFieldMixer.SetClipWeight(assignedMotionFieldController.animClipInfoList[0].animClip.name, 0.1f);

            //motionFieldMixer.SetClipWeight(assignedMotionFieldController.animClipInfoList[0].animClip.name, 0.6f);


            //_animatorComponent.Play(motionFieldMixer);

            //motionFieldMixer.state = PlayState.Paused;
        }

        // Use this for initialization
        void Start () {

        }

        // Update is called once per frame
        void Update () {
            //GraphVisualizerClient.Show(animMixer, gameObject.name);
            //GraphVisualizerClient.Show(motionFieldMixer, gameObject.name);

            
            if (skeleton != null) {
                if (skeleton.rootBonePlayable != null) {
                    GraphVisualizerClient.Show(skeleton.rootBonePlayable, gameObject.name);
                }
            }
            
	    }

        void OnDestroy() {
            //CURRENTLY CRASHES EDITOR WHEN STOP PLAYING
            //motionFieldMixer.Dispose();//dump all resources that were allocated to the mixer
        }
    }

}
