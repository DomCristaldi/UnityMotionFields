using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

using AnimationMotionFields;

[RequireComponent(typeof(Animator))]
public class TestMixers : MonoBehaviour {

    public Animator animControl;

    public PoseMixerPlayable testClip;

    BlendFromToPlayable blender;
    BlendFromToPlayable blender2;

    public PoseMixerPlayable poseMixer;

    public AnimationClip clip1;
    public float time1;

    [Space]
    public float time2;
    public AnimationClip clip2;

    [Space]
    public float tranTime = 1.0f;


    void Awake() {

        animControl = GetComponent<Animator>();

        //testClip = Playable.Create<PoseMixerPlayable>();

        AnimationClipPlayable clipPlayable1 = AnimationClipPlayable.Create(clip1);
        clipPlayable1.time = time1;
        /*
        AnimationClipPlayable clipPlayable2 = AnimationClipPlayable.Create(clip2);
        clipPlayable2.time = time2;

        AnimationClipPlayable clipPlayable3 = AnimationClipPlayable.Create(clip2);
        clipPlayable3.time = time2;
        */
        poseMixer = Playable.Create<PoseMixerPlayable>();
        poseMixer.InitPlayable(clipPlayable1);
        //poseMixer.BlendToAnim(clipPlayable2, tranTime);
        //poseMixer.BlendToAnim(clipPlayable3, tranTime);

        animControl.Play(poseMixer);
    }


	// Use this for initialization
	void Start () {

    }

    void OnDestroy()
    {

    }

    // Update is called once per frame
    void Update () {
        if (poseMixer != null) {
            GraphVisualizerClient.Show(poseMixer, "Pose Mixer");
        }
        else {
            Debug.Log("Pose Matcher is Null");
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TestMixers))]
public class TestMixers_Editor : Editor
{

    TestMixers selfScript;

    void OnEnable()
    {
        selfScript = (TestMixers)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DrawAddButtons();
    }


    private void DrawAddButtons()
    {
        using(EditorGUILayout.HorizontalScope hScope = new EditorGUILayout.HorizontalScope()) {

            if (GUILayout.Button("Add Node")) {

                AnimationClipPlayable newNode = AnimationClipPlayable.Create(selfScript.clip1);
                selfScript.poseMixer.BlendToAnim(newNode, selfScript.tranTime);
            }

            if (GUILayout.Button("Destroy")) {
                selfScript.animControl.Stop();
                selfScript.poseMixer.TearDown();
                //selfScript.testClip.Destroy();
            }
        }
    }
}

#endif
