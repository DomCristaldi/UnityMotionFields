using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu]
public class Task_Trajectory : ATask
{
    float trajectoryDist = 0.2f; // trajectory is constructed this far into the future. Not measured in number of poses, since different animations can have different sampling rates.

    public Task_Trajectory()
    {

    }

    override public float CheckCost(MotionPose oldPose, MotionPose newPose, Transform targetLocation, List<AnimClipInfo> animClipInfoList)
    {
        //create candidate trajectory. note: if you reach the end of the anim, then trajectory stays in place for remaining time.
        AnimClipInfo clipInfo = animClipInfoList
                                        .Where(c => c.animClip.name == newPose.animName)
                                        .First();

        int numframes = (int)Mathf.Ceil(trajectoryDist / clipInfo.frameStep);

        int index = clipInfo.motionPoses.Select((Value, Index) => new { Value, Index })
                 .Single(p => p.Value.timestamp== newPose.timestamp).Index;

        Trajectory candidate = new Trajectory(clipInfo.motionPoses, index, numframes);



        //create goal trajectory

        //compare them to generate cost

        return 0;
    }
}

public class Trajectory
{
    /*
     * list of points that make up frames of the trajectory.
     * each point represents the positional and rotational displacement from the previous point.
     */
    
    public List<BoneTransform> points;

    public Trajectory(int numpoints)
    {
        points = new List<BoneTransform>(numpoints);
    }

    public Trajectory(MotionPose[] poses, int startIndex, int length)
    {
        startIndex += 1; //the candidate trajectory of pose x should not include root motion of x, just x+1, x+2, ect. 

        points = new List<BoneTransform>();

        for(int i = startIndex; i < startIndex + length; i++) {
            if(i >= poses.Length) { //trajectory has gone past end of anim data
                points.Add(new BoneTransform()); //identity bontransform, to signify no movement. (position is Vector3.zero, rotation is Quaternion.Identity)
            }
            else {
                points.Add(new BoneTransform(poses[i].rootMotionInfo.value));
            }
        }
    }
}
