using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class AnimatorPlaybackSetter : MonoBehaviour {

    public string clipName;

    private Animator animControl;

    public Transform referencePoint;
    public Transform skeletonRoot;

    private HumanPoseHandler hPoseHandler;
    public HumanPose hPose;

    public float playbackTime;

    void Awake() {
        animControl = GetComponent<Animator>();

        hPoseHandler = new HumanPoseHandler(animControl.avatar, skeletonRoot);
        hPose = new HumanPose();

        //animControl.StartPlayback();
        //animControl.playbackTime = playbackTime;

        //animControl.SetTimeUpdateMode(UnityEngine.Experimental.Director.DirectorUpdateMode.Manual);

    }

    // Use this for initialization
    void Start () {
        hPoseHandler.GetHumanPose(ref hPose);
    }

    // Update is called once per frame
    void Update () {

        //animControl.playbackTime = playbackTime;
        //animControl.SetTime(playbackTime);

        float clipLength = 0.0f;
        foreach (AnimationClip c in animControl.runtimeAnimatorController.animationClips) {
            if (c.name == clipName) {
                clipLength = c.length;
                break;
            }
        }

        hPoseHandler.GetHumanPose(ref hPose);

        animControl.Play(clipName, 0, playbackTime / clipLength);

        Debug.LogFormat("Ref  Pos: {0}\nPose Pos: {1}", referencePoint.localPosition,
                                                        new Vector3(hPose.bodyPosition.x,
                                                                    referencePoint.position.y,
                                                                    hPose.bodyPosition.z));

    }


#if UNITY_EDITOR
    void OnDrawGizmos() {

    }
#endif

}


#if UNITY_EDITOR
[CustomEditor(typeof(AnimatorPlaybackSetter))]
public class AnimatorPlaybackSetter_Editor : Editor {

    AnimatorPlaybackSetter selfScript;

    void OnEnable() {
        selfScript = (AnimatorPlaybackSetter)target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        DrawPoseInfo();

    }

    private void DrawPoseInfo() {

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();

        if (selfScript.skeletonRoot != null) {
            PrintPoseInfo("Hips", selfScript.skeletonRoot.position, selfScript.skeletonRoot.rotation);
        }

        EditorGUILayout.Space();

        if (selfScript.referencePoint != null) {
            PrintPoseInfo("Ref", selfScript.referencePoint.position, selfScript.referencePoint.rotation);
        }

        EditorGUILayout.Space();

        PrintPoseInfo("HPose Stuff", selfScript.hPose.bodyPosition, selfScript.hPose.bodyRotation);

        EditorGUILayout.EndVertical();
    }

    private void PrintPoseInfo(string label, Vector3 pos, Quaternion rot) {
        EditorGUILayout.Vector3Field(label + "Position: ", pos);
        EditorGUILayout.Vector4Field(label + "Rotation: ", rot.Ex_VectorValue());
    }

}
#endif
