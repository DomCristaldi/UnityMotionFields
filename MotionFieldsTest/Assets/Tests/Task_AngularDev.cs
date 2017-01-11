using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class Task_AngularDev : ATask
{

    public Task_AngularDev()
    {

    }

    //holds value from -pi to pi indicating deviation from desired player direction. deviation of 0 means it is moving in the correct direction

    override public float CheckCost(MotionPose oldPose, MotionPose newPose, Transform targetLocation, List<AnimClipInfo> animClipInfoList)
    {
        float goalAngle = targetLocation.rotation.x - oldPose.rootMotionInfo.value.rotX; //pseudocode. difference in rads of rotation between character and goal. wont work because oldPose is not in same coordinate space. unsure if rotX is correct val to check.

        float candidateAngle = newPose.rootMotionInfo.value.rotX - oldPose.rootMotionInfo.value.rotX; //psuedocode. difference in rads of rotation between character and candidate. unsure if rotX is correct val to check.

        //the more similar goalAngle and candidateAngle are, the better
        return Mathf.Abs(candidateAngle - goalAngle); //2 PI is highest possible difference, lower difference yields better cost.
    }

}