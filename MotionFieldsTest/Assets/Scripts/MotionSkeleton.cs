using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

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

        public List<MSBoneDeserialized> children;

        public MSBoneDeserialized() {
            children = new List<MSBoneDeserialized>();
        }
    }

    [System.Serializable]
    public struct MSBoneSerialized {
        //public List<> 
        //public int[] boneInputIndexes;
        //public int[] boneOutputIndexes;
        //public Transform referencedTransform;

        public int posX, posY, posZ;
        public int quatX, quatY, quatZ, quatW;
        public int scaleX, scaleY, scaleZ;

        public int numChildren;
        public int indexOfSelf;
        public int indexOfFirstChild;
    }

    public MSBoneDeserialized rootDeserializedBone = new MSBoneDeserialized();
    public List<MSBoneSerialized> serializedBones;

    public void OnBeforeSerialize() {
        serializedBones.Clear();

        if (rootDeserializedBone == null) { return; }//don't bother serializeing, we don't have any data

        SerializeBone(rootDeserializedBone);
        //throw new System.NotImplementedException();
    }
    private void SerializeBone(MSBoneDeserialized boneToAdd) {
        MSBoneSerialized serBone = new MSBoneSerialized() {

            numChildren = boneToAdd.children.Count,
            //indexOfSelf = index,
            indexOfFirstChild = serializedBones.Count + 1
        };

        serializedBones.Add(serBone);
        foreach (MSBoneDeserialized child in boneToAdd.children) {
            SerializeBone(child);
        }
    }


    public void OnAfterDeserialize() {

        if (serializedBones.Count > 0) {

        }
        else { rootDeserializedBone = null; }

        //throw new System.NotImplementedException();
    }

    private MSBoneDeserialized GenerateDeserializedBoneGraph(int index) {
        var serBone = serializedBones[index];
        List<MSBoneDeserialized> children = new List<MSBoneDeserialized>();
        for (int i = 0; i != serBone.numChildren; ++i) {
            children.Add(GenerateDeserializedBoneGraph(serBone.indexOfFirstChild + i));
        }

        return new MSBoneDeserialized() {
            children = children,
        };
    }


    public MotionSkeletonBonePlayable rootBonePlayable;

    public void Init() {
        /*
        if (rootBonePlayable != null) {
            rootBonePlayable.Dispose();
        }
        */
        /*
        if (rootDeserializedBone == null) {
            Debug.LogWarningFormat("Root Bone was not serialized or is null. Setting Skeleton to null");
            rootBonePlayable = null;
            return;
        }
        */

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
    
}