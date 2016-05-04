using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;
using System.Linq;

namespace AnimationMotionFields {



    [RequireComponent(typeof(Animator))]
    public class MotionFieldComponent : MonoBehaviour {

        //private Animator _animatorComponent;

        //public MotionFieldController assignedMotionFieldController;

        //public int numFramesToBlend = 1;

        //private MotionFieldMixerRoot motionFieldMixer;

        public MotionSkeleton skeleton;
        //public MotionSkeletonBonePlayable testRoot;
        //MotionSkeletonBonePlayable testPlayable;

        void Awake() {
            skeleton.Init();


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
