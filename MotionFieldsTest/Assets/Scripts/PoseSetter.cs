using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PoseSetter : MonoBehaviour {

    //Animation anim;
    Animator animator;
    public AnimationClip clip;

    //public float timeStamp;

    void Awake() {
        //anim = GetComponent<Animation>();
        animator = GetComponent<Animator>();
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ApplyClipAtTimestamp(float timeStamp) {
        //anim[anim.clip.name].time = timeStamp;
        //animator.
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(PoseSetter))]
[CanEditMultipleObjects]
public class PoseSetter_Editor : Editor {

    PoseSetter selfScript;

    float timestamp;

    void OnEnable() {
        selfScript = (PoseSetter)target;

        AnimationMode.StartAnimationMode();
    }

    void OnDisable() {
        AnimationMode.StopAnimationMode();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();


        EditorGUILayout.BeginHorizontal();

        timestamp = EditorGUILayout.FloatField("timestamp", timestamp);

        if (GUILayout.Button("Apply Pose At Timestamp")) {
            //selfScript.ApplyClipAtTimestamp(timestamp);
            SampleAnimation();
        }


        EditorGUILayout.EndHorizontal();


    }

    public void SampleAnimation() {

        Debug.Log("Sample");

        if (!AnimationMode.InAnimationMode()) {
            Debug.Log("problem");
            return;
        }

        AnimationMode.BeginSampling();

        AnimationMode.SampleAnimationClip(selfScript.gameObject,
                                          selfScript.clip,
                                          timestamp);

        //AnimationMode.EndSampling();

        SceneView.RepaintAll();

    }

}
#endif
