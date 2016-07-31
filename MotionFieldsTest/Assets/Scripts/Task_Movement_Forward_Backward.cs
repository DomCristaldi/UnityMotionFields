using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Task_Movement_Forward_Backward : ATask {

    public Task_Movement_Forward_Backward()
    {

    }

    //holds value from -pi to pi indicating deviation from desired player direction. deviation of 0 means it is moving in the correct direction

    override public float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval)
    {
        return 0.0f;
    }

    override public float DetermineTaskValue()
    {
        return 0.0f;
    }
}
