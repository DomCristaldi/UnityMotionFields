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

        private MotionFieldMixerRoot motionFieldMixer;

        void Awake() {
            _animatorComponent = GetComponent<Animator>();

        }

	    // Use this for initialization
	    void Start () {
            motionFieldMixer = new MotionFieldMixerRoot(assignedMotionFieldController.animClipInfoList
                                                                                            .Where(x => x.useClip)
                                                                                            .Select(x => x.animClip).ToArray(),
                                                        numFramesToBlend
                                                        );
	    }
	
	    // Update is called once per frame
	    void Update () {
            //GraphVisualizerClient.Show(animMixer, gameObject.name);
            GraphVisualizerClient.Show(motionFieldMixer, gameObject.name);
	    }
    }

}
