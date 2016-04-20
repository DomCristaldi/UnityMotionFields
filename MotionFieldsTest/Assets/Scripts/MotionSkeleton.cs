using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Collections;




[System.Serializable]
public class MotionSkeletonBone : Playable {

    public Transform boneTransformRef;

    public MotionSkeletonBone(Transform boneTransformRef) {
        this.boneTransformRef = boneTransformRef;
    }

    public override void ProcessFrame(FrameData info, object playerData) {
        base.ProcessFrame(info, playerData);
    }

}

/*
[System.Serializable]
public class MotionSkeletonBoneBinder {

    public MotionSkeletonBone parentBone;

    public MotionSkeletonBone[] childBones;
}
*/

[CreateAssetMenu]
public class MotionSkeleton : ScriptableObject {

    [SerializeField]
    public MotionSkeletonBone rootBone;
    

}