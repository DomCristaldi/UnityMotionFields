using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

namespace AnimationMotionFields {


    public class MotionFieldClipMixer : AnimationMixerPlayable {

        private string _clipName;
        public string clipName {
            get { return _clipName; }
        }


        public MotionFieldClipMixer(AnimationClip animClip, int numDuplicates) {

            _clipName = animClip.name;//record the name of the clip we supplied

            for (int i = 0; i < numDuplicates; ++i) {//add appropriate number of duplicates
                AddInput(new AnimationClipPlayable(animClip));
            }

            SetAllInputlWeights(0.0f);//initialize all weights to 0 b/c we dial them in when needed
        }

        public void SetAllInputlWeights(float weight) {
            for (int i = 0; i < GetInputs().Length; ++i) {
                SetInputWeight(i, weight);
            }
        }

        public void SetNextAvailableWeight(float weight) {

            //set the first weight that's found to the supplied weight, then stop
            for (int i = 0; i < GetInputs().Length; ++i) {
                if (GetInputWeight(i) == 0.0f) {
                    SetInputWeight(i, weight);
                    break;
                }
            }
        }
    }

    public class MotionFieldMixerRoot : AnimationMixerPlayable {

        private Dictionary<string, MotionFieldClipMixer> mixerMappings;

        public MotionFieldMixerRoot(AnimationClip[] animClips, int numDuplicateClips) {

            //mixerMappings = new Dictionary<string, AnimationMixerPlayable>();
            mixerMappings = new Dictionary<string, MotionFieldClipMixer>();

            foreach (AnimationClip clip in animClips) {

                MotionFieldClipMixer mfClipMixer = new MotionFieldClipMixer(clip, numDuplicateClips);

                mixerMappings.Add(clip.name, mfClipMixer);//adds to dictoinary for easy lookup
                AddInput(mfClipMixer);//adds to actual mixer so it can be used

            }

        }
    }
}
