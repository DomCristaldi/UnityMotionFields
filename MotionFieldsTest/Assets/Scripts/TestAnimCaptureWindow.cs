using UnityEngine;
using UnityEditor;
using System.Collections;

public class SampleClipTool : EditorWindow {

    class Styles {
        public Styles() {
        }
    }
    static Styles s_Styles;

    protected GameObject go;
    protected AnimationClip animationClip;
    protected float time = 0.0f;
    protected bool lockSelection = false;
    protected bool animationMode = false;

    [MenuItem("Mecanim/SampleClip", false, 2000)]
    public static void DoWindow() {
        GetWindow<SampleClipTool>();
    }

    public void OnEnable() {
    }

    public void OnSelectionChange() {
        if (!lockSelection) {
            go = Selection.activeGameObject;
            Repaint();
        }
    }

    public void OnGUI() {
        if (s_Styles == null)
            s_Styles = new Styles();

        if (go == null) {
            EditorGUILayout.HelpBox("Please select a GO", MessageType.Info);
            return;
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate", EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck())
            ToggleAnimationMode();

        GUILayout.FlexibleSpace();
        lockSelection = GUILayout.Toggle(lockSelection, "Lock", EditorStyles.toolbarButton);
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
        if (animationClip != null) {
            float startTime = 0.0f;
            float stopTime = animationClip.length;
            time = EditorGUILayout.Slider(time, startTime, stopTime);
        }
        else if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();


        DisplayAnimInfo();

        EditorGUILayout.EndVertical();
    }

    void Update() {

        if (go == null) {
            Debug.Log("Blammo");
            return;
        }

        if (animationClip == null) {
            Debug.Log("boom");
            return;
        }

        // there is a bug in AnimationMode.SampleAnimationClip which crash unity if there is no valid controller attached
        Animator animator = go.GetComponent<Animator>();
        //animator.ApplyBuiltinRootMotion();
        animator.applyRootMotion = true;
        if (animator != null && animator.runtimeAnimatorController == null) {
            Debug.Log("missing something with the Animator");
            return;
        }

        if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode()) {
            Debug.Log("Do the Sampling");

            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(go, animationClip, time);
            AnimationMode.EndSampling();

            Debug.LogFormat("Body Position: {0}\nBody Rotation: {1}", animator.bodyPosition, animator.bodyRotation);
            //animator.

            SceneView.RepaintAll();
        }
        /*
        else {
            Debug.Log("something else broke");
        }
        */
    }

    void ToggleAnimationMode() {
        if (AnimationMode.InAnimationMode())
            AnimationMode.StopAnimationMode();
        else
            AnimationMode.StartAnimationMode();
    }

    private void DisplayAnimInfo() {


        Transform skelRootTf = null;

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();

        //HIPS LOCATION AND ROTATION
        //skelRootTf = EditorGUILayout.ObjectField(skelRootTf, typeof(Transform), true) as Transform;
        skelRootTf = go.GetComponent<AnimationMotionFields.MotionFieldComponent>().cosmeticSkel.skeletonRoot;

        if (skelRootTf == null) { return; }

        EditorGUILayout.Vector3Field("Hip Location: ", skelRootTf.position);
        EditorGUILayout.Vector4Field("Hip Rotatoin: ", skelRootTf.rotation.Ex_VectorValue());

        EditorGUILayout.Space();

        //BODY LOCATION AND ROTATION
        HumanPoseHandler hPoseHandler = new HumanPoseHandler(go.GetComponent<Animator>().avatar,
                                                             skelRootTf);
        HumanPose hPose = new HumanPose();
        hPoseHandler.GetHumanPose(ref hPose);

        EditorGUILayout.Vector3Field("Body Location: ", hPose.bodyPosition);
        EditorGUILayout.Vector4Field("Body Rotation: ", hPose.bodyRotation.Ex_VectorValue());


        //hip position
        //hip rotation
        //body position
        //body rotation





        EditorGUILayout.EndVertical();
    }

    private void DrawHumanPoseInfo() {


    }
}
