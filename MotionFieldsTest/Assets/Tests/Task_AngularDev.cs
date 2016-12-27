using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Task_AngularDev : ATask
{

    public Task_AngularDev()
    {

    }

    //holds value from -pi to pi indicating deviation from desired player direction. deviation of 0 means it is moving in the correct direction

    override public float CheckReward(MotionPose oldPose, MotionPose newPose, Transform targetLocation)
    {
        float goalAngle = targetLocation.rotation.x - oldPose.rootMotionInfo.value.rotX; //pseudocode. difference in rads of rotation between character and goal. wont work because oldPose is not in same coordinate space. unsure if rotX is correct val to check.

        float candidateAngle = newPose.rootMotionInfo.value.rotX - oldPose.rootMotionInfo.value.rotX; //psuedocode. difference in rads of rotation between character and candidate. unsure if rotX is correct val to check.

        //the more similar goalAngle and candidateAngle are, the better
        return (2 * Mathf.PI) - Mathf.Abs(candidateAngle - goalAngle); //2 PI is highest possible difference, difference subtracted from 2PI so that a lower difference yields higher reward.
    }

}