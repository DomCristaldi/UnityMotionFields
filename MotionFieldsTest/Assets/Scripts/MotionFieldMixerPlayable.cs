using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Director;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif


namespace AnimationMotionFields {

    public class BlendSwitcherPlayable : CustomAnimationPlayable
    {
        //TODO: check if we should check the time of the From Node if it has more weight in the blending
        // you can do the math to set an int instead of hard coding a 0 or 1 in the 
        // mixer funcitons for getting and setting

        public AnimationMixerPlayable mixer;

        public string targetClipName {
            get {
                Assert.IsTrue(mixer.IsValid());
                Assert.IsTrue(mixer.inputCount == 2);
                if (mixer.GetInput(1).IsValid()) {
                    Debug.Log("fetching clip name");
                    return mixer.GetInput(1).CastTo<AnimationClipPlayable>().clip.name;
                }
                else {
                    Debug.LogWarning("failed to fethc name");
                    return "";
                }
            }
        }

        public float targetClipTime {
            get {
                Assert.IsTrue(mixer.IsValid());
                Assert.IsTrue(mixer.inputCount == 2);

                if (mixer.GetInput(1).IsValid()) {
                    return (float) mixer.GetInput(1).CastTo<AnimationClipPlayable>().time;
                }
                else {
                    return -1.0f;
                }
            }
        }

        public AnimationClipPlayable fromClip;
        public AnimationClipPlayable toClip;

        private float transitionDuration;
        private float timeSpentTransitioning;
        public float transitionPercentage {get { return timeSpentTransitioning / transitionDuration; }}

        public BlendSwitcherPlayable()
        {
            //initialize timer so we don't calculate NaN (fucking IEEE)
            timeSpentTransitioning = -1.0f;
            transitionDuration = -1.0f;

            this.mixer = AnimationMixerPlayable.Create();
            AddInput(mixer);
        }

        //CALL THIS TO SET UP THE BLEND SWICHER AFTER IT'S CREATED
        public void InitBlendSwitcher(AnimationClip startingClip, float timestamp) {

            fromClip = AnimationClipPlayable.Create(startingClip);
            toClip = AnimationClipPlayable.Create(startingClip);
            fromClip.time = timestamp;
            toClip.time = timestamp;

            mixer.AddInput(fromClip);
            mixer.AddInput(toClip);

            mixer.SetInput(fromClip, 0);
            mixer.SetInput(toClip, 1);

            mixer.SetInputWeight(0, 0.0f);
            mixer.SetInputWeight(1, 1.0f);


            return;


            AnimationClipPlayable startingClipPlayable = AnimationClipPlayable.Create(startingClip);
            startingClipPlayable.time = timestamp;

            AnimationClipPlayable endingClipPlayable = AnimationClipPlayable.Create(startingClip);
            endingClipPlayable.time = timestamp;

            //allocate the starting clip to the proper index (needs to be in index 1 b/c it's starting at its destination, and we transition from 0 -> 1)
            //mixer.AddInput(AnimationPlayable.Null);//index 0
            mixer.AddInput(startingClipPlayable);//index 0
            mixer.AddInput(endingClipPlayable);//index 1

            mixer.SetInput(startingClipPlayable, 0);
            mixer.SetInput(endingClipPlayable, 1);

            mixer.SetInputWeight(0, 1.0f);
            mixer.SetInputWeight(1, 0.0f);

            //mixer.SetInputWeight(0, 0.0f);
            //mixer.SetInputWeight(1, 1.0f);
        }

        public override void PrepareFrame(FrameData info)
        {

            Assert.IsTrue(mixer.inputCount == 2);
            Assert.IsTrue(mixer.GetInput(0).IsValid());
            Assert.IsTrue(mixer.GetInput(1).IsValid());
            /*
            if (mixer.inputCount != 2
                || !mixer.GetInput(0).IsValid()
                || !mixer.GetInput(1).IsValid()) {

                Debug.Log("invalid shit in Prepare Frame of Blend Switcher");
                return;
            }
            */
            /*
            timeSpentTransitioning = Mathf.Clamp(timeSpentTransitioning += info.deltaTime,
                                                 0.0f,
                                                 transitionDuration);
            */
            
            timeSpentTransitioning = Mathf.MoveTowards(timeSpentTransitioning,
                                                       transitionDuration,
                                                       info.deltaTime);

            mixer.SetInputWeight(0, 1.0f - transitionPercentage);
            mixer.SetInputWeight(1, transitionPercentage);

        }

        //COME BACK HERE <<<----------------
        //IMPLEMENT THE SWITCH BLENDER
        //HAVE IT ALWAYS RUN, JUST SWAP OUT WHEN YOU WANNA BLEND AGAIN

        public void BlendToAnim(AnimationClip clip, float timestamp, float transitionDuration = 0.25f)
        {

            Debug.Log("do blend");

            Debug.AssertFormat(mixer.IsValid(), "mixer is invalid. Make sure Blend Switcher is fully initialized");
            Debug.AssertFormat(mixer.inputCount != 2, "Not enough mixer inputs. Currently has {0} Inputs", mixer.inputCount);

            Playable oldFromNode = mixer.GetInput(0);
            mixer.RemoveInput(0);
            oldFromNode.Destroy();

            Playable oldToNode = mixer.GetInput(1);
            mixer.RemoveInput(1);
            mixer.SetInput(oldToNode, 0);

            AnimationClipPlayable newToNode = AnimationClipPlayable.Create(clip);
            newToNode.time = timestamp;
            mixer.SetInput(newToNode, 1);

            Debug.LogFormat("Blend Info\nFrom Clip: {0} - {1}\nToClip : {2} - {3}",
                            mixer.GetInput(0).CastTo<AnimationClipPlayable>().clip.name,
                            mixer.GetInput(0).CastTo<AnimationClipPlayable>().time,
                            mixer.GetInput(1).CastTo<AnimationClipPlayable>().clip.name,
                            mixer.GetInput(1).CastTo<AnimationClipPlayable>().time);

            /*
            mixer.SetInput(mixer.GetInput(0),
                           1);
            */
            /*
            mixer.SetInput(,
                           0);
            */
            mixer.SetInputWeight(0, 1.0f);
            mixer.SetInputWeight(1, 0.0f);

            this.transitionDuration = transitionDuration;
            this.timeSpentTransitioning = 0.0f;


            /*
            //fromClip.clip = toClip.clip;
            //fromClip.Destroy();
            Playable indexZero = Playable.Null;
            if (mixer.inputCount > 0) { indexZero = mixer.GetInput(0); }
            mixer.RemoveInput(0);
            if (indexZero.IsValid()) {
                indexZero.Destroy();
            }
            AnimationClipPlayable newfromClip = AnimationClipPlayable.Create(toClip.clip);
            newfromClip.time = toClip.time;
            mixer.SetInput(newfromClip, 0);



            toClip = AnimationClipPlayable.Create(clip);
            toClip.time = timestamp;
            */
            /*
            Playable prevPlayable = mixer.GetInput(0);
            Playable currentPlayable = mixer.GetInput(1);

            Debug.Log("playables logged");
            Debug.Break();

            AnimationClipPlayable nextClipPlayable = AnimationClipPlayable.Create(clip);
            nextClipPlayable.time = timestamp;

            Debug.Log("new playable created");
            Debug.Break();


            
            mixer.RemoveAllInputs();
            if (prevPlayable.IsValid()) {
                prevPlayable.Destroy();
            }

            Debug.Log("destruction");
            Debug.Break();
            */
            /*
            mixer.SetInput(currentPlayable, 0);
            mixer.SetInput(nextClipPlayable, 1);
            */
            //mixer.SetInputWeight(0, 1.0f);
            //mixer.SetInputWeight(1, 0.0f);

            //this.transitionDuration = transitionDuration;
            //timeSpentTransitioning = 0.0f;

        }
    }


    [System.Serializable]
    //public class MotionFieldClipPlayableBinding
    public class AnimationPlayableBinding
    {
        public AnimationPlayable animPlayable;
        public int index;

        public AnimationPlayableBinding(AnimationPlayable animPlayable)
        {
            this.animPlayable = animPlayable;
        }
    }

    public class AnimationClipPlayableBinding
    {
        public AnimationClipPlayable animClipPlayable;
        public int index;   

        public AnimationClipPlayableBinding(AnimationClip animClip, float timestamp = 0.0f)
        {
            animClipPlayable = AnimationClipPlayable.Create(animClip);
            animClipPlayable.time = timestamp;
        }
    }


    public class BlendFromToPlayable : CustomAnimationPlayable
    {
        public AnimationMixerPlayable mixer;

        public Playable fromClip { get { return mixer.GetInput(0); } }
        public Playable toClip { get { return mixer.GetInput(1); } }

        public float transitionTime;
        private float timeSpentTransitioning;
        public float transitionPercentage { get { return timeSpentTransitioning / transitionTime; } }

        bool lockOut = false;

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
            if (lockOut) { return; }

            //calculate a clamped (0 - 1) weight
            timeSpentTransitioning += info.deltaTime;
            timeSpentTransitioning = Mathf.Clamp(timeSpentTransitioning, 0.0f, transitionTime);

            //IF WE'RE FULLY TRANSITIONED
            if (Mathf.Approximately(transitionPercentage, 1.0f)) {
                Prune();
                return;
            }

            //set the weights on the mixer so they transition from one to the other
            mixer.SetInputWeight(0, 1.0f - transitionPercentage);
            mixer.SetInputWeight(1, transitionPercentage);
        }

        public void Prune()
        {
            lockOut = true;

            Playable outputNodeRef = GetOutput(0);
            //for(int i = 0; i < outputNodeRef.outputCount; ++i) {
            //    if (outputNodeRef.GetOutput(i) == this) { Debug.Log("we good"); }
            //}
            Debug.Log(outputNodeRef.inputCount);

            Playable toNode = toClip;

            mixer.RemoveAllInputs();

            

            //outputNodeRef.SetInput(toClip, 0);
            //Playable.Disconnect(outputNodeRef, 0);

            //Playable.Disconnect(mixer, 1);
            
            //outputNodeRef.

            Playable.Connect(toNode, outputNodeRef,
                             0, 0);

            //Playable.Disconnect(outputNodeRef, 1);
            mixer.RemoveAllInputs();
            //mixer.Destroy();
            //Destroy();
            Debug.Log(outputNodeRef.inputCount);

            //mixer.Destroy();

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
