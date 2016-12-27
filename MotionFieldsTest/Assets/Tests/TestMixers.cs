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


    //public PoseMixerPlayable poseMixer;
    public BlendSwitcherPlayable blendSwitcher;

    public AnimationClip clip1;
    public float time1;

    [Space]
    public float time2;
    public AnimationClip clip2;

    [Space]
    public float tranTime = 1.0f;


    void Awake() {

        animControl = GetComponent<Animator>();

        /*
        AnimationClipPlayable clipPlayable1 = AnimationClipPlayable.Create(clip1);
        clipPlayable1.time = time1;
        poseMixer = Playable.Create<PoseMixerPlayable>();
        poseMixer.InitPlayable(clipPlayable1);
        */

        blendSwitcher = Playable.Create<BlendSwitcherPlayable>();

        //animControl.Play(poseMixer);
    }


	// Use this for initialization
	void Start () {
        blendSwitcher.InitBlendSwitcher(clip1, 0.0f);


        animControl.Play(blendSwitcher);



    }

    void OnDestroy()
    {

    }

    // Update is called once per frame
    void Update () {

        GraphVisualizerClient.Show(blendSwitcher, "Blend Switcher");

        /*
        if (poseMixer != null) {
            GraphVisualizerClient.Show(poseMixer, "Pose Mixer");
        }
        else {
            Debug.Log("Pose Matcher is Null");
        }
        */
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(TestMixers))]
public class TestMixers_Editor : Editor
{

    TestMixers selfScript;

    bool flipClip = true;

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

                flipClip = !flipClip;

                AnimationClipPlayable newNode = AnimationClipPlayable.Create(selfScript.clip1);
                //selfScript.poseMixer.BlendToAnim(newNode, selfScript.tranTime);
                AnimationClip targetClip = null;
                if (flipClip) { targetClip = selfScript.clip1; }
                else { targetClip = selfScript.clip2; }
                selfScript.blendSwitcher.BlendToAnim(targetClip, 0.0f, selfScript.tranTime);
            }

            if (GUILayout.Button("Destroy")) {
                //MUST STOP PLAYER TO AVOID CRASH
                selfScript.animControl.Stop();
                //selfScript.poseMixer.TearDown();
            }
        }
    }
}

#endif
