using UnityEngine;
using System.Collections;

public class Task_AngularDev : ATask {

	public Task_AngularDev(){
		
	}

	//holds value from -pi to pi indicating deviation from desired player direction. deviation of 0 means it is moving in the correct direction

	override public float CheckReward(float valBefore, float valAfter){
		//returns value from -1 to 1, negative means it moved away from desired direction, positive means it moved towards desired direction.
		return (Mathf.Abs (valBefore) - Mathf.Abs (valAfter))/Mathf.PI;
	}

	override public float DetermineTaskValue (float[] currentPos){
		//placeholder
		return 0.0f;
	}
}


