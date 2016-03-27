using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {



    [RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        private Animator _animatorComponent;

        public MotionFieldController assignedMotionFieldController;

        public int numFramesToBlend = 1;

        //private AnimationMixerPlayable animMixer;
        private MotionFieldMixerRoot motionFieldMixer;

        //private Playable root;

        void Awake() {
            _animatorComponent = GetComponent<Animator>();

            //animMixer.SetInputs
        }

	    // Use this for initialization
	    void Start () {
            motionFieldMixer = new MotionFieldMixerRoot(assignedMotionFieldController.animClipInfoList
                                                                                            .Where(x => x.useClip)
                                                                                            .Select(x => x.animClip).ToArray(),
                                                        numFramesToBlend
                                                        );


            //animMixer = new AnimationMixerPlayable(true);
            //motionFieldMixer = new MotionFieldMixerPlayable();
            /*
            foreach (AnimClipInfo clipInfo in assignedMotionFieldController.animClipInfoList) {

                if (!clipInfo.useClip) { continue; }

                //animMixer.AddInput(new AnimationClipPlayable(clipInfo.animClip));
                //motionFieldMixer.AddMotionFieldClip(clipInfo.animClip, 2);
            }
            */

	    }
	
	    // Update is called once per frame
	    void Update () {
            //GraphVisualizerClient.Show(animMixer, gameObject.name);
            GraphVisualizerClient.Show(motionFieldMixer, gameObject.name);
	    }
    }

}
