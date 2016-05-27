using UnityEngine;
using System.Collections;

//[System.Serializable]
[CreateAssetMenu]
public class ATask : ScriptableObject{

	public ATask(){
		
	}

	public float max;

	public float min;

	public int numSamples;
		
	virtual public float CheckReward (float valBefore, float valAfter){
		//get the immediate reward based on how task has changed (high if value changed favorably, low otherwise)
		return -1f;
	}

	virtual public float DetermineTaskValue (MotionPose pose){
		//decides the value of this task element for the given pose. 
        //certain tasks will potentially need to look at things from the world (i.e. is the player near a ledge? Was he just damaged? Is the crouch button held?)
		return -1f;
	}
		
}
