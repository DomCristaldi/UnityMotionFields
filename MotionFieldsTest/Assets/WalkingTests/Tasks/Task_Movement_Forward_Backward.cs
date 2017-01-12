using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class Task_Movement_Forward_Backward : ATask
{

    public Task_Movement_Forward_Backward()
    {

    }

    override public float CheckCost(MotionPose oldPose, MotionPose newPose, Transform targetLocation, List<AnimClipInfo> animClipInfoList)
    {
        /* TODO
         *
         * this task should work if its the only task. 
         * however, with multiple tasks, care must be taken so that they have equal weighting.
         * IE one task does not overpoer others.
         * 
         *  currently, this task can potentially return float.MaxValue, overshadowing all other tasks in the total cost.
         */

        float taskval = Input.GetAxisRaw("Vertical");

        //Debug.LogFormat("Task Input: MovementForwardBackward: {0}", taskval);

        if (taskval == 0)
        {
            //want deviation from 0 movement to be as small as possible. lower movement = better cost
            float movement = Mathf.Abs(newPose.rootMotionInfo.value.position.z) + Mathf.Abs(newPose.rootMotionInfo.positionNext.position.z);

            return Mathf.Atan(movement);
        }
        else if (taskval < 0)
        {
            //move backwards. lower movement (it can be negative) = better cost
            float movement = newPose.rootMotionInfo.value.position.z + newPose.rootMotionInfo.positionNext.position.z;
            return (Mathf.PI / 2) - Mathf.Atan(-movement);
        }
        else //taskval > 0
        {
            //move forwards. hogher movement (it can be negative) = better cost
            float movement = newPose.rootMotionInfo.value.position.z + newPose.rootMotionInfo.positionNext.position.z;
            return (Mathf.PI / 2) - Mathf.Atan(movement);
        }
    }
}
