using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ATask : ScriptableObject{

    //[Range(0.0f, 1.0f)]
    //public float contributionScale = 0.5f;

    public const float maxTaskValue = 10000000.0f;

    public float max;

	public float min;

	public int numSamples;

    abstract public float CheckCost(MotionPose oldPose, MotionPose newPose, Transform targetLocation, List<AnimClipInfo> animClipInfoList);		
}
