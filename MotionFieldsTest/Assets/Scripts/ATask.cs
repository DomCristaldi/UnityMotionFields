using UnityEngine;
using System.Collections;

//[System.Serializable]
[CreateAssetMenu]
public class ATask : ScriptableObject{

	public ATask(){
		
	}

	public string name;

	public float max;

	public float min;

	public int numSamples;
		
	virtual public float CheckReward (float valBefore, float valAfter){
		try{
			throw new UnityException();
		}
		catch (UnityException ex){
			Debug.LogError ("Define dis function yo!");
		}
		return -1f;
	}

	//determine task value takes in a motion state (position + velocity) and decides the value of this task at this state. It is NOT calculating reward/value func
	virtual public float DetermineTaskValue (){
		try{
			throw new UnityException();
		}
		catch (UnityException ex){
			Debug.LogError ("Define dis function yo!");
		}
		return -1f;
	}
		
}
