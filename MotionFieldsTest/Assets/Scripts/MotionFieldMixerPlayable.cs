using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

namespace AnimationMotionFields {

    public class BlendFromToPlayable : CustomAnimationPlayable
    {
        public AnimationMixerPlayable mixer;

        public AnimationPlayable fromClip { get { return mixer.GetInput(0).CastTo<AnimationPlayable>(); } }
        public AnimationPlayable toClip { get { return mixer.GetInput(1).CastTo<AnimationPlayable>(); } }

        public float transitionTime;
        private float timeSpentTransitioning;
        public float transitionPercentage { get { return timeSpentTransitioning / transitionTime; } }

    //CONSTRUCTOR
        public BlendFromToPlayable()
        {
            //initialize the mixer
            this.mixer = AnimationMixerPlayable.Create();

            //add it as an input to this custom playable
            AddInput(mixer);

        }

        public void SetTransitionInputs(AnimationPlayable from,
                                        AnimationPlayable to,
                                        float transitionTime)
        {
            mixer.AddInput(from);
            mixer.SetInputWeight(0, 1.0f);

            mixer.AddInput(to);
            mixer.SetInputWeight(1, 0.0f);


            //setup timer
            this.transitionTime = transitionTime;
            this.timeSpentTransitioning = 0.0f;
        }

    //UPDATE
        public override void PrepareFrame(FrameData info)
        {
            //calculate a clamped (0 - 1) weight
            timeSpentTransitioning += info.deltaTime;
            timeSpentTransitioning = Mathf.Clamp(timeSpentTransitioning, 0.0f, transitionTime);

            //IF WE'RE FULLY TRANSITIONED
            if (Mathf.Approximately(transitionPercentage, 1.0f)) {
                //Prune();
                return;
            }

            //set the weights on the mixer so they transition from one to the other
            mixer.SetInputWeight(0, 1.0f - transitionPercentage);
            mixer.SetInputWeight(1, transitionPercentage);
        }

        public void Prune()
        {
            Playable outputNodeRef = GetOutput(0);
            //for(int i = 0; i < outputNodeRef.outputCount; ++i) {
            //    if (outputNodeRef.GetOutput(i) == this) { Debug.Log("we good"); }
            //}

            AnimationPlayable toNode = toClip;

            //outputNodeRef.SetInput(toClip, 0);
            Playable.Disconnect(outputNodeRef, 0);
            Playable.Disconnect(mixer, 1);
            
            Playable.Connect(toNode, outputNodeRef,
                             0, 0);

            
            //mixer.RemoveAllInputs();
            //mixer.Destroy();
            //Destroy();
        }
    }


    public class PoseMixerPlayable : CustomAnimationPlayable
    {

        AnimationPlayable root;

        public PoseMixerPlayable()
        {
            this.root = AnimationPlayable.Null;
            AddInput(root);
        }

        public void InitPlayable(AnimationClipPlayable startingClip) {
            RemoveAllInputs();
            root = startingClip;
            AddInput(startingClip);

            /*
            if (root != AnimationPlayable.Null) { root.Destroy(); }
            root = AnimationPlayable.Null;
            root = startingClip;
            */
            //root.AddInput(startingClip);
        }

        public void TearDown()
        {

            RemoveAllInputs();
            root.Destroy();
            Destroy();
        }

        public void BlendToAnim(AnimationClipPlayable blendToClip,
                                float blendInTime)
        {

            //AnimationPlayable rootNode = root.GetInput(0).CastTo<AnimationPlayable>();
            RemoveAllInputs();

            BlendFromToPlayable newBlending = Playable.Create<BlendFromToPlayable>();

            AnimationPlayable oldRoot = root;
            root = AnimationPlayable.Null;

            newBlending.SetTransitionInputs(oldRoot,
                                            blendToClip,
                                            blendInTime);
            
            root = newBlending;
            AddInput(newBlending);
        }


    }

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
}
