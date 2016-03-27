using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;
//using System.Linq;

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

        //float[] clipWeights;

        //public Dictionary
        /*
        public override void PrepareFrame(FrameData info) {
            Playable[] inputs = GetInputs();
        }
        */

        //private Dictionary<string, AnimationMixerPlayable> mixerMappings;
        private Dictionary<string, MotionFieldClipMixer> mixerMappings;

        //public MotionFieldMixerRoot() {}
        
        //public MotionFieldMixerRoot(MotionFieldController motionFieldController) {
        public MotionFieldMixerRoot(AnimationClip[] animClips, int numDuplicateClips) {

            //mixerMappings = new Dictionary<string, AnimationMixerPlayable>();
            mixerMappings = new Dictionary<string, MotionFieldClipMixer>();

            //foreach (AnimationClip clip in motionFieldController.animClipInfoList.Select(x => x.animClip)) {
            foreach (AnimationClip clip in animClips) {

                MotionFieldClipMixer mfClipMixer = new MotionFieldClipMixer(clip, numDuplicateClips);

                mixerMappings.Add(clip.name, mfClipMixer);//adds to dictoinary for easy lookup
                AddInput(mfClipMixer);//adds to actual mixer so it can be used

                //AnimationMixerPlayable duplicatesMixer = new AnimationMixerPlayable();

                /*
                for (int i = 0; i < )

                mixerMappings.Add(clip.name,
                                  new AnimationClipPlayable(clip));
                */

            }

        }


        /*
        public void AddMotionFieldClip(AnimationClip clip, int numDuplicates) {

            //clipWeights = new float[numDuplicates];

            AnimationMixerPlayable duplicatesMixer = new AnimationMixerPlayable();

            for (int i = 0; i < numDuplicates; ++i) {//create the duplicate anim mixer clips (allows use of multiple frames from same clip)
                int index = duplicatesMixer.AddInput(new AnimationClipPlayable(clip));//add the clip to the mixer, record it's index
                //clipWeights[index] = 0.0f;//intialize weight to 0.0f
                duplicatesMixer.SetInputWeight(i, 0.0f);
            }

            //********
            //AddInput(duplicatesMixer);

            //UpdateInputWeights();
        }
        */
        /*
    //UPDATE ALL WEIGHTS TO MATCH THE CLIP WEIGHTS ARRAY
        public void UpdateInputWeights() {
            for (int i = 0; i < clipWeights.Length; ++i) {
                SetInputWeight(i, clipWeights[i]);
            }
        }
        */
    }
}
