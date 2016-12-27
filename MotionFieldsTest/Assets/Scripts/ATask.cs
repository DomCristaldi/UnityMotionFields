using UnityEngine;
using System.Collections;

public abstract class ATask : ScriptableObject{

	public float max;

	public float min;

	public int numSamples;

    abstract public float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval);

    abstract public float DetermineTaskValue();
		
}
