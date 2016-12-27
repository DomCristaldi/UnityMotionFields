//using UnityEngine;
//using UnityEngine.Experimental.Director;
//using System.Collections.Generic;

//namespace AnimationMotionFields {


//    [System.Serializable]
//    public class MotionFieldClipPlayableBinding {

//        public AnimationClipPlayable animClipPlayable;
//        public float relativeWeight;
//        public int index;

//        public MotionFieldClipPlayableBinding(AnimationClip animClip) {
//            animClipPlayable = AnimationClipPlayable.Create(animClip); //new AnimationClipPlayable(animClip);
//            relativeWeight = 0.0f;
//        }
//    }




////MIXER FOR CLIPS
//    public class MotionFieldClipMixer : AnimationMixerPlayable {

//        private string _clipName;
//        public string clipName {
//            get { return _clipName; }
//        }

//        public float clipMixerWeight = 0.0f;

//        private MotionFieldClipPlayableBinding[] animClipPlayableBindings;

////CONSTRUCTOR
//        public MotionFieldClipMixer(AnimationClip animClip, int numDuplicates) {

//            _clipName = animClip.name;//record the name of the clip we supplied
//            animClipPlayableBindings = new MotionFieldClipPlayableBinding[numDuplicates];

//            for (int i = 0; i < numDuplicates; ++i) {//add appropriate number of duplicates
//                animClipPlayableBindings[i] = new MotionFieldClipPlayableBinding((animClip));

//                //record index it was set to for consistent access
//                int clipPlayableIndex = AddInput(animClipPlayableBindings[i].animClipPlayable);
//                animClipPlayableBindings[i].index = clipPlayableIndex;
//            }

//            clipMixerWeight = 0.0f;
//            SetWeightAcrossMixer(0.0f);//initialize all weights to 0 b/c we dial them in when needed
//        }


////SET THE WEIGHT OF ALL CLIPS ACROSS THE MIXER
//        public void SetWeightAcrossMixer(float weight) {

//            //Corner case for 0.0f weight input
//            if (weight == 0.0f) {
//                clipMixerWeight = 0.0f;
//                for (int i = 0; i < GetInputs().Length; ++i) {
//                    animClipPlayableBindings[i].relativeWeight = 0.0f;
//                    SetInputWeight(animClipPlayableBindings[i].index, 0.0f);
//                }
//                return;
//            }


//            //Regular Execution
//            clipMixerWeight = weight;
//            int numClipPlayables = GetInputs().Length;

//            for (int i = 0; i < numClipPlayables; ++i) {
//                animClipPlayableBindings[i].relativeWeight = weight / numClipPlayables;
//                SetInputWeight(animClipPlayableBindings[i].index, weight / numClipPlayables);
//            }
            
//        }


////SET THE WIEGHT OF THE NEXT PLAYABLE CLIP THAT IS ALREADY SET TO 0.0f
//        public void SetNextAvailableWeight(float weight) {

//            clipMixerWeight += weight;

//            //find the next clip with a relative weight of 0
//            for (int i = 0; i < animClipPlayableBindings.Length; ++i) {
//                if (animClipPlayableBindings[i].relativeWeight == 0.0f) {
//                    animClipPlayableBindings[i].relativeWeight = weight;
//                    break;
//                }
//            }

//            //apply the normalized clip weights
//            foreach (MotionFieldClipPlayableBinding binding in animClipPlayableBindings) {
//                if (binding.relativeWeight != 0.0f) {
//                    SetInputWeight(binding.index, binding.relativeWeight / clipMixerWeight);
//                }
//            }

//            /*
//            //set the first weight that's found to the supplied weight, then stop
//            for (int i = 0; i < GetInputs().Length; ++i) {
//                if (GetInputWeight(i) == 0.0f) {
//                    SetInputWeight(i, weight);
//                    break;
//                }
//            }
//            */
//        }
//    }


////MIXER FOR CLIP MIXERS
//    public class MotionFieldMixerRoot : AnimationMixerPlayable {

//        private Dictionary<string, int> mixerMappings;

//        public MotionFieldMixerRoot(AnimationClip[] animClips, int numDuplicateClips) {

//            //turn off playing animations by default (we just want this for poses)
//            //state = PlayState.Paused;

//            //mixerMappings = new Dictionary<string, AnimationMixerPlayable>();
//            mixerMappings = new Dictionary<string, int>();

//            foreach (AnimationClip clip in animClips) {

//                //createc Clip Mixer, complete with duplicates, and add to Root Mixer, record the index so we can access it later
//                int mixerIndex = AddInput(new MotionFieldClipMixer(clip, numDuplicateClips));

//                //store mapping between index and animation clip for quick lookup
//                mixerMappings.Add(clip.name, mixerIndex);
//            }
//        }

//        public void SetClipWeight(string clipName, float weight) {
//            (GetInput(mixerMappings[clipName]) as MotionFieldClipMixer).SetNextAvailableWeight(weight);
//        }
//    }
//}
