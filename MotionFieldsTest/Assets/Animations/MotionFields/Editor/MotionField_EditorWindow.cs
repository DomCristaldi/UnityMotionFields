using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;


public class MotionField_EditorWindow : EditorWindow {

    [SerializeField]
    public SO_MotionField selectedMotionField;

    //[SerializeField]
    //public List<AnimationClip> animClips;

    private ReorderableList reorderableAnimClips;

    //[SerializeField]

    

    [MenuItem("MotionFields/Motion Field Author")]
    static void Init() {
        MotionField_EditorWindow window = (MotionField_EditorWindow)EditorWindow.GetWindow(typeof(MotionField_EditorWindow));
        window.Show();
    }


    void OnEnable() {
        Debug.Log("Bloop");

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
            selectedMotionField = (SO_MotionField) EditorGUILayout.ObjectField("Motion Field: ", selectedMotionField, typeof(SO_MotionField), false);


        EditorGUILayout.EndHorizontal();

        if (selectedMotionField != null) {
            //reorderableAnimClips.DoLayoutList();

            if (GUILayout.Button("Create Test Clip")) {

            }


            if (GUILayout.Button("Analyze Keyframes")) {
                AnimationCurve extractedCurve = null;
                List<AnimationCurve> embeddedCurves = new List<AnimationCurve>();


                try {
                    //AnimationCurve[] embeddedCurves =
                    EditorCurveBinding[] embeddedCuveBindings = AnimationUtility.GetCurveBindings(selectedMotionField.animClipInfoList[0].animClip);

                    foreach (EditorCurveBinding eCB in embeddedCuveBindings) {
                        embeddedCurves.Add(AnimationUtility.GetEditorCurve(selectedMotionField.animClipInfoList[0].animClip,
                                                                           eCB));
                    }


                    /*
                    List<AnimationCurve> embeddedCurves = embeddedCuveBindings.ToList<EditorCurveBinding>().SelectMany(x => AnimationUtility.GetEditorCurve(selectedMotionField.animClipInfoList[0].animClip,
                                                                                                                                                            x.)
                    */

                    extractedCurve = AnimationUtility.GetEditorCurve(selectedMotionField.animClipInfoList[0].animClip,
                                                                                AnimationUtility.GetCurveBindings(selectedMotionField.animClipInfoList[0].animClip)[0]);


                }
                catch {
                    Debug.LogErrorFormat("Motion Field Author: Error with retrieving Animation Clips from supplied Motion Field {0}", selectedMotionField.name);
                }
                finally {
                    if (extractedCurve != null) {
                        //Debug.Log(extractedCurve.keys.Length);
                        Debug.Log(embeddedCurves.Count);
     
                    }
                }


            }

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
