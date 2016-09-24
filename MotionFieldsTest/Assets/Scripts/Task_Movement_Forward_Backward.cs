using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Task_Movement_Forward_Backward : ATask {

    public Task_Movement_Forward_Backward()
    {

    }

    override public float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval)
    {
        /* TODO
         *
         * this task should work if its the only task. 
         * however, with multiple tasks, care must be taken so that they have equal weighting.
         * IE one task does not overpoer others.
         * 
         *  currently, this task can potentially return float.MaxValue, overshadowing all other tasks in the total reward.
         */

        //Debug.LogFormat("Task Input: MovementForwardBackward: {0}", taskval);

        if(taskval == 0)
        {
            //want deviation from 0 movement to be as small as possible. lower movement = higher reward
            float movement = Mathf.Abs(newPose.rootMotionInfo.value.posZ) + Mathf.Abs(newPose.rootMotionInfo.positionNext.posZ);
            
            //yes you MUST check for both, 0.0f != -0.0f. i know i know, its wierd.
            if(movement == 0.0f || movement == -0.0f)//TODO: Would this work with Mathf.Approximately?
            {
                //return Mathf.Infinity;
                return maxTaskValue;
            }
            else
            {
                return 1.0f / movement;
            }
        }
        else if(taskval < 0)
        {
            //move backwards. lower movement (it can be negative) = better reward
            float movement = newPose.rootMotionInfo.value.posZ + newPose.rootMotionInfo.positionNext.posZ;
            return -movement;
        }
        else //taskval > 0
        {
            //move forwards. hogher movement (it can be negative) = better reward
            float movement = newPose.rootMotionInfo.value.posZ + newPose.rootMotionInfo.positionNext.posZ;
            return movement;
        }
    }

    override public float DetermineTaskValue()
    {
        //holds value from -1 to 1. value of zero means stand still, 1 is move forward, 0 is move backwards. 
        //determine task by checking input. if no input, task = 0. if up arrow, task = 1. if down arrow, task = -1
        return Input.GetAxisRaw("Vertical");
    }
}
