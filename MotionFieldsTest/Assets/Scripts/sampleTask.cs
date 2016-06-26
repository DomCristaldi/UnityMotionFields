using UnityEngine;
using System.Collections;

public class Task_AngularDev : ATask
{

    public Task_AngularDev()
    {

    }

    //holds value from -pi to pi indicating deviation from desired player direction. deviation of 0 means it is moving in the correct direction

    override public float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval)
    {
        //returns difference between angle of oldpose-to-newpose and taskval angle. lower difference is better.
        float candidateDifference = newPose.bonePoses[0].value.rotX - oldPose.bonePoses[0].value.rotX; //psuedocode. bonePoses[0] needs to be hipBone (ideally root) and rotX is most likely not the correct rotation param to sheck

        return (2 * Mathf.PI) - Mathf.Abs(candidateDifference - taskval); //2 PI is highest possible difference, difference subtracted from 2PI so that a lower difference yields higher reward.
    }

    override public float DetermineTaskValue()
    {
        //task value is angle (in rads) difference between characters current facing direction and direction they SHOULD be facing in (they wont be the same when char needs to turn)
        float dirCurrent_paceholder = 0.0f;
        float dirGoal_placeholder = 0.0f;

        return dirGoal_placeholder - dirCurrent_paceholder;
    }
}

