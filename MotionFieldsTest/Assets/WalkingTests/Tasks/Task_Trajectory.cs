using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Task_Trajectory : ATask
{

    public Task_Trajectory()
    {

    }

    override public float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval)
    {
        return 0;
    }

    override public float DetermineTaskValue()
    {
        return 0;
    }
}
