using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections.Generic;

[System.Serializable]
public class MotionSkeletonBonePlayable : Playable {

    public Transform boneTransformRef;
    /*
    public MotionSkeletonBonePlayable() {
        //boneTransformRef = null;
    }
    */
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

    public class MSBoneDeserialized {
        public Transform referencedTransform;
        public List<MSBoneDeserialized> children;
    }

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


    void OnDestroy() {
        //rootBone.Dispose();
    }


    public void OnBeforeSerialize() {
        
        serializedBones.Clear();//wipe the serialization storage

        if (rootBone == null) { return; }
        AddBoneToSerialization(rootBone);//make sure we start by adding the root (assumed it's index 0 when we deserialize)

        //rootBone.Dispose();

        //throw new System.NotImplementedException();
        
    }

    void AddBoneToSerialization(MotionSkeletonBonePlayable rootBone) {

        MSBoneSerialized newSerializedBone = new MSBoneSerialized() {
            //referencedTransform = rootBone.boneTransformRef,
            numInputs = rootBone.GetInputs().Length,
            indexOfSelf = serializedBones.Count,
            indexOfFirstChild = serializedBones.Count + 1,
        };

        serializedBones.Add(newSerializedBone);

        foreach (MotionSkeletonBonePlayable msBone in rootBone.GetInputs()) {
            AddBoneToSerialization(msBone);
        }

        Debug.Log("before serialize: " + serializedBones.Count);


    }


    public void OnAfterDeserialize() {

        Debug.Log("Deserialize");

        //Debug.Log("after serialize");
        rootBone.Dispose();
        rootBone = null;

        if (serializedBones.Count > 0) {

            Debug.Log("We have info to deserialize");

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

        Debug.Log("generate bone");

        MotionSkeletonBonePlayable[] children = new MotionSkeletonBonePlayable[serializedBones[rootIndex].numInputs];
        MSBoneSerialized[] inputBones = new MSBoneSerialized[serializedBones[rootIndex].numInputs];

        for (int i = 0; i < inputBones.Length; ++i) {
            children[i] = new MotionSkeletonBonePlayable(inputBones[i].referencedTransform);
        }
        

        //RECURSIVLEY BUILD ALL CHILDREN OF THE CHILD BONES
        for (int i = 0, j = serializedBones[rootIndex].indexOfFirstChild;//compound for loop b/c these arrays are order-synchronized
                i < children.Length;
                ++i, ++j)
        {
            GenerateMotionSkeleton(i, ref children[j]);
        }

        foreach (MotionSkeletonBonePlayable child in children) {
            Playable.Connect(rootBoneReference, child);
        }

    }
    
    public void Init() {

    }


}