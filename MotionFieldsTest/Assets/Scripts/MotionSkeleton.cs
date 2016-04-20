using UnityEngine;
using System.Collections;

/*
[System.Serializable]
public class MotionSkeletonBone {

}
*/

[CreateAssetMenu]
public class MotionSkeleton : ScriptableObject {

    public Transform rootTransform;

    //public Transform[] excludedTransforms;

    public void SetRootTransform(Transform root) {

    } 

}
