//using UnityEngine;
//using UnityEngine.Experimental.Director;
//using System.Collections.Generic;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class MotionSkeletonBonePlayable : Playable {

//    //public Transform boneTransformRef;
//    /*
//    public MotionSkeletonBonePlayable() {
//        //boneTransformRef = null;
//    }
//    */
//    /*
//    public MotionSkeletonBonePlayable(Transform boneTransformRef) {
//        this.boneTransformRef = boneTransformRef;
//    }
//    */
//    /*
//    public override void ProcessFrame(FrameData info, object playerData) {
//        base.ProcessFrame(info, playerData);
//    }
//    */
//}

//[CreateAssetMenu]
//public class MotionSkeleton : ScriptableObject, ISerializationCallbackReceiver {

//    //UNSERIALIZED
//    public class MSBoneDeserialized {
//        //public Transform referencedTransform;
//        public int posX, posY, posZ;
//        public int quatX, quatY, quatZ, quatW;
//        public int scaleX, scaleY, scaleZ;

//        public string boneLabel;

//        public List<MSBoneDeserialized> children;
//        /*
//        public MSBoneDeserialized() {
//            this.children = new List<MSBoneDeserialized>();
//        }
//        */
//        public MSBoneDeserialized() {
//            //this.boneLabel = label;

//            this.children = new List<MSBoneDeserialized>();
//        }

//#if UNITY_EDITOR
//        public void DisplayValues_Editor() {
//            EditorGUILayout.BeginHorizontal();

//            boneLabel = EditorGUILayout.TextField(boneLabel);

//            EditorGUILayout.EndHorizontal();
//        }
//#endif
//    }

//    [System.Serializable]
//    public struct MSBoneSerialized {
//        //public List<> 
//        //public int[] boneInputIndexes;
//        //public int[] boneOutputIndexes;
//        //public Transform referencedTransform;

//        [SerializeField]
//        public string boneLabel;
//        /*
//        public int posX, posY, posZ;
//        public int quatX, quatY, quatZ, quatW;
//        public int scaleX, scaleY, scaleZ;
//        */

//        public int numChildren;
//        //public int indexOfSelf;
//        public int indexOfFirstChild;
//    }

//    public MSBoneDeserialized rootDeserializedBone = new MSBoneDeserialized();
//    public List<MSBoneSerialized> serializedBones;

////SERIALIZATION
//    public void OnBeforeSerialize() {
//        serializedBones.Clear();

//        if (rootDeserializedBone == null) { return; }//don't bother serializeing, we don't have any data

//        SerializeBone(rootDeserializedBone);
//    }
//    private void SerializeBone(MSBoneDeserialized boneToAdd) {

//        //Debug.Log("Serialize: " + boneToAdd.boneLabel);

//        //create the serializable struct to hold the info we want to save
//        MSBoneSerialized serBone = new MSBoneSerialized() {

//            boneLabel = boneToAdd.boneLabel,

//            numChildren = boneToAdd.children.Count,
//            //indexOfSelf = index,
//            indexOfFirstChild = serializedBones.Count + 1
//        };

//        //add it to the serializable list
//        serializedBones.Add(serBone);

//        //recurse and do the same for its children
//        foreach (MSBoneDeserialized child in boneToAdd.children) {
//            SerializeBone(child);
//        }
//    }

////DESERIALIZATION
//    public void OnAfterDeserialize() {

//        //we have info to serialize
//        if (serializedBones.Count > 0) {
//            /*
//            for (int i = 0; i < serializedBones.Count; ++i) {
//                Debug.Log(serializedBones[i].boneLabel);
//            }
//            */

//            rootDeserializedBone = GenerateDeserializedBoneGraph(0);
//        }
//        //nothing to serialize, set the root ot null
//        else { rootDeserializedBone = null; }

//    }
//    private MSBoneDeserialized GenerateDeserializedBoneGraph(int index) {

//        //Debug.Log("index: " + index);

//        MSBoneDeserialized newRoot = new MSBoneDeserialized() {
//            boneLabel = serializedBones[index].boneLabel,
//        };

//        MSBoneSerialized serBone = serializedBones[index];

//        //Debug.Log("Deserialize Bone: " + serBone.boneLabel);
        
//        for (int i = 0; i < serBone.numChildren; ++i) {
//            newRoot.children.Add(GenerateDeserializedBoneGraph(i + serBone.indexOfFirstChild));
//        }
        
//        /*
//        List<MSBoneDeserialized> childNodes = new List<MSBoneDeserialized>();
//        for (int i = 0; i < serBone.numChildren; ++i) {

//        }
//        */
//        return newRoot;

//        /*
//        //record reference for easy access
//        MSBoneSerialized serBone = serializedBones[index];
        
//        //generate all children before we can link them b/c they need to link their children
//        List<MSBoneDeserialized> children = new List<MSBoneDeserialized>();


//        for (int i = 0; i < serBone.numChildren; ++i) {

//            if (serBone.numChildren == 0) { Debug.Log(serBone.boneLabel); }

//            children.Add(GenerateDeserializedBoneGraph(serBone.indexOfFirstChild + i));//link children after recursive generation call
//        }

//        //set values that we saved from serialization
//        return new MSBoneDeserialized() {
//            boneLabel = serBone.boneLabel,
//            children = children,
//        };
//        */
//    }


//    public MotionSkeletonBonePlayable rootBonePlayable;

//    public void Init() {
        
//        //clear any graph that may have been sitting in here before
//        if (rootBonePlayable != null) {
//            rootBonePlayable.Dispose();
//        }
        
//        //if the root is null we can't build a Playable graph
//        if (rootDeserializedBone == null) {
//            Debug.LogWarningFormat("Root Bone was not serialized or is null. Setting Skeleton to null");
//            rootBonePlayable = null;
//            return;
//        }
        
//        //actual creation of skeleton as a Playable graph
//        rootBonePlayable = RecursivelyGenerateSkeleton(rootDeserializedBone);
//    }
    
//    public MotionSkeletonBonePlayable GenerateSkeleton() {
//        return RecursivelyGenerateSkeleton(rootDeserializedBone);
//    }

//    private MotionSkeletonBonePlayable RecursivelyGenerateSkeleton(MSBoneDeserialized rootBone) {

//        MotionSkeletonBonePlayable newRoot = new MotionSkeletonBonePlayable();

        
//        foreach (MSBoneDeserialized child in rootBone.children) {
//            Playable.Connect(newRoot, RecursivelyGenerateSkeleton(child));
//        }
        

//        return newRoot;
//    }

//    public MSBoneDeserialized CreateSkeletonFromSuppliedHierarchy_Recursive(Transform hierarchyRoot) {
//        MSBoneDeserialized newRoot = new MSBoneDeserialized() { boneLabel = hierarchyRoot.name };

//        //MSBoneDeserialized[] children = new MSBoneDeserialized[hierarchyRoot.childCount];
//        /*
//        List<MSBoneDeserialized> children = new List<MSBoneDeserialized>(hierarchyRoot.childCount);
//        for (int i = 0; i < hierarchyRoot.childCount; ++i) {
//            children[i] = CreateSkeletonFromSuppliedHierarchy_Recursive(hierarchyRoot.GetChild(i));
//        }
//        */
//        List<MSBoneDeserialized> children = new List<MSBoneDeserialized>();
//        foreach (Transform childTf in hierarchyRoot) {
//            children.Add(CreateSkeletonFromSuppliedHierarchy_Recursive(childTf));
//        }

//        newRoot.children = children;

//        return newRoot;
//    }

//    void OnDestroy() {
//        //clean up the Playable using built-in disposal function
//        if (rootBonePlayable != null) {
//            rootBonePlayable.Dispose();
//        }
//    }
//}


//#if UNITY_EDITOR
//[CustomEditor(typeof(MotionSkeleton))]
//public class MotionSkeleton_Editor : Editor {

//}


//#endif
