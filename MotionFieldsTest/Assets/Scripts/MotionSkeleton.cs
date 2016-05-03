using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

[System.Serializable]
public class MotionSkeletonBonePlayable : Playable {

    public Transform boneTransformRef;

    public MotionSkeletonBonePlayable(Transform boneTransformRef) {
        this.boneTransformRef = boneTransformRef;
    }

    public override void ProcessFrame(FrameData info, object playerData) {
        base.ProcessFrame(info, playerData);
    }

}

public class MotionSkeletonBone : ScriptableObject {
    //public
    public MotionSkeletonBoneJoint inJoint;
    public MotionSkeletonBoneJoint outJoint;
}

public class MotionSkeletonBoneJoint : ScriptableObject {
    public List<MotionSkeletonBone> inBones;
    public List<MotionSkeletonBone> outBones;
}

[CreateAssetMenu]
public class MotionSkeleton : ScriptableObject, ISerializationCallbackReceiver {

    [System.Serializable]
    public struct MSBoneSerialized {
        //public List<> 
        //public int[] boneInputIndexes;
        //public int[] boneOutputIndexes;
        public Transform referencedTransform;
        public int numInputs;
        public int indexOfSelf;
        public int indexOfFirstChild;
    }

    //[SerializeField]
    public List<MSBoneSerialized> serializedBones;


    [SerializeField]
    public MotionSkeletonBonePlayable rootBone;


    public void OnBeforeSerialize() {
        serializedBones.Clear();//wipe the serialization storage

        AddBoneToSerialization(rootBone);//make sure we start by adding the root (assumed it's index 0 when we deserialize)

        //throw new System.NotImplementedException();
    }

    void AddBoneToSerialization(MotionSkeletonBonePlayable rootBone) {

        MSBoneSerialized newSerializedBone = new MSBoneSerialized() {
            referencedTransform = rootBone.boneTransformRef,
            numInputs = rootBone.GetInputs().Length,
            indexOfSelf = serializedBones.Count,
            indexOfFirstChild = serializedBones.Count + 1,
        };

        serializedBones.Add(newSerializedBone);

        foreach (MotionSkeletonBonePlayable msBone in rootBone.GetInputs()) {
            AddBoneToSerialization(msBone);
        }

    }


    public void OnAfterDeserialize() {

        if (serializedBones.Count > 0) {

            rootBone = new MotionSkeletonBonePlayable(serializedBones[0].referencedTransform);
            GenerateMotionSkeleton(0, ref rootBone);
            //GenerateDeserializedTree(ref root);
        }
        else {
            Debug.LogWarning("No serialized bones found. Setting root to null");
            rootBone = null;
        }
        //throw new System.NotImplementedException();
    }

    //Recursively generate the Motion Skeleton from root bone's index
    private void GenerateMotionSkeleton(int rootIndex, ref MotionSkeletonBonePlayable rootBoneReference) {//NOTE TO SELF: THE AMOUNT OF RECURSIVE NESTED FOR LOOPS IS SCARY. CONSIDER REFACTORING
        MotionSkeletonBonePlayable[] children = new MotionSkeletonBonePlayable[serializedBones[rootIndex].numInputs];
        MSBoneSerialized[] inputBones = new MSBoneSerialized[serializedBones[rootIndex].numInputs];

        for (int i = 0; i < inputBones.Length; ++i) {
            children[i] = new MotionSkeletonBonePlayable(inputBones[i].referencedTransform);
        }
        
        //compound for loop b/c these arrays are order-synchronized
        for (int i = 0, j = serializedBones[rootIndex].indexOfFirstChild;
                i < children.Length;
                ++i, ++j)
        {
            GenerateMotionSkeleton(i, ref children[j]);
        }

        foreach (MotionSkeletonBonePlayable child in children) {
            Playable.Connect(rootBoneReference, child);
        }


        /*
        for (int i = 0; i < serializedBones[rootIndex].numInputs; ++i) {

            children[i] = new MotionSkeletonBonePlayable(serializedBones[rootIndex].referencedTransform);
        }

        for (int i = 0; i < serializedBones[rootIndex].numInputs; ++i) {
            MotionSkeletonBonePlayable childSkeletalBone = new MotionSkeletonBonePlayable(children[i].boneTransformRef);

            for (int childIndex = )

            
        }
        */

        //return _root;
    }

    /*
    private void GenerateDeserializedTree(ref MotionSkeletonBonePlayable rootBone) {
        for (int i = 0; i < serializedBones.Count; ++i) {

        }
    }
    */
    

    public void Init() {

    }


}