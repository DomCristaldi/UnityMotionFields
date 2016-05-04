using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MotionSkeletonBonePlayable : Playable {

    //public Transform boneTransformRef;
    /*
    public MotionSkeletonBonePlayable() {
        //boneTransformRef = null;
    }
    */
    /*
    public MotionSkeletonBonePlayable(Transform boneTransformRef) {
        this.boneTransformRef = boneTransformRef;
    }
    */
    /*
    public override void ProcessFrame(FrameData info, object playerData) {
        base.ProcessFrame(info, playerData);
    }
    */
}

[CreateAssetMenu]
public class MotionSkeleton : ScriptableObject, ISerializationCallbackReceiver {

    public class MSBoneDeserialized {
        //public Transform referencedTransform;
        public int posX, posY, posZ;
        public int quatX, quatY, quatZ, quatW;
        public int scaleX, scaleY, scaleZ;

        public string boneLabel;

        public List<MSBoneDeserialized> children;
        /*
        public MSBoneDeserialized() {
            this.children = new List<MSBoneDeserialized>();
        }
        */
        public MSBoneDeserialized() {
            //this.boneLabel = label;

            this.children = new List<MSBoneDeserialized>();
        }
    }

    [System.Serializable]
    public struct MSBoneSerialized {
        //public List<> 
        //public int[] boneInputIndexes;
        //public int[] boneOutputIndexes;
        //public Transform referencedTransform;

        [SerializeField]
        public string boneLabel;

        public int posX, posY, posZ;
        public int quatX, quatY, quatZ, quatW;
        public int scaleX, scaleY, scaleZ;

        public int numChildren;
        public int indexOfSelf;
        public int indexOfFirstChild;
    }

    public MSBoneDeserialized rootDeserializedBone = new MSBoneDeserialized();
    public List<MSBoneSerialized> serializedBones;

//SERIALIZATION
    public void OnBeforeSerialize() {
        serializedBones.Clear();

        if (rootDeserializedBone == null) { return; }//don't bother serializeing, we don't have any data

        SerializeBone(rootDeserializedBone);
    }
    private void SerializeBone(MSBoneDeserialized boneToAdd) {

        //create the serializable struct to hold the info we want to save
        MSBoneSerialized serBone = new MSBoneSerialized() {

            boneLabel = boneToAdd.boneLabel,

            numChildren = boneToAdd.children.Count,
            //indexOfSelf = index,
            indexOfFirstChild = serializedBones.Count + 1
        };

        //add it to the serializable list
        serializedBones.Add(serBone);

        //recurse and do the same for its children
        foreach (MSBoneDeserialized child in boneToAdd.children) {
            SerializeBone(child);
        }
    }

//DESERIALIZATION
    public void OnAfterDeserialize() {

        //we have info to serialize
        if (serializedBones.Count > 0) {
            rootDeserializedBone = GenerateDeserializedBoneGraph(0);
        }
        //nothing to serialize, set the root ot null
        else { rootDeserializedBone = null; }

    }
    private MSBoneDeserialized GenerateDeserializedBoneGraph(int index) {
        //record reference for easy access
        MSBoneSerialized serBone = serializedBones[index];
        
        //generate all children before we can link them b/c they need to link their children
        List<MSBoneDeserialized> children = new List<MSBoneDeserialized>();
        for (int i = 0; i != serBone.numChildren; ++i) {
            children.Add(GenerateDeserializedBoneGraph(serBone.indexOfFirstChild + i));//link children after recursive generation call
        }

        //set values that we saved from serialization
        return new MSBoneDeserialized() {
            boneLabel = serBone.boneLabel,
            children = children,
        };
    }


    public MotionSkeletonBonePlayable rootBonePlayable;

    public void Init() {
        
        //clear any graph that may have been sitting in here before
        if (rootBonePlayable != null) {
            rootBonePlayable.Dispose();
        }
        
        //if the root is null we can't build a Playable graph
        if (rootDeserializedBone == null) {
            Debug.LogWarningFormat("Root Bone was not serialized or is null. Setting Skeleton to null");
            rootBonePlayable = null;
            return;
        }
        
        //actual creation of skeleton as a Playable graph
        rootBonePlayable = RecursivelyGenerateSkeleton(rootDeserializedBone);
    }
    
    public MotionSkeletonBonePlayable GenerateSkeleton() {
        return RecursivelyGenerateSkeleton(rootDeserializedBone);
    }

    private MotionSkeletonBonePlayable RecursivelyGenerateSkeleton(MSBoneDeserialized rootBone) {

        MotionSkeletonBonePlayable newRoot = new MotionSkeletonBonePlayable();

        
        foreach (MSBoneDeserialized child in rootBone.children) {
            Playable.Connect(newRoot, RecursivelyGenerateSkeleton(child));
        }
        

        return newRoot;
    }
    

    void OnDestroy() {
        //clean up the Playable using built-in disposal function
        if (rootBonePlayable != null) {
            rootBonePlayable.Dispose();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(MotionSkeleton))]
public class MotionSkeleton_Editor : Editor {

}
#endif
