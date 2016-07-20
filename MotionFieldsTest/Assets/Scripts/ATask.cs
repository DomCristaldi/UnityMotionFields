using UnityEngine;
using System.Collections;

public class ATask : ScriptableObject{

	public ATask(){
		
	}

	public float max;

	public float min;

	public int numSamples;
		
	virtual public float CheckReward (MotionPose oldPose, MotionPose newPose, float taskval){
		//get the immediate reward based on how beneficial the change in pose is for the taskval
		return -1f;
	}

	virtual public float DetermineTaskValue (){
		//decides the value of this task element. 
        //task values determined by parameters in the world.
		return -1f;
	}
		
}
