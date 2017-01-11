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

    private List<BoneTransform> GenerateSplinePoints(BoneTransform start, BoneTransform startvel, BoneTransform end, BoneTransform endvel, int numpoints)
    {
        //TODO: this is only generating a trajectory of the root motion of the character. to also consider the characters facing direction, we would have to look at skeleton root (i think).

        //startvel and endvel are distance from previous frame to start and previous frame to end, respectively.
        Vector3 sAnchor = start.position;
        Vector3 sControl = start.position + startvel.position;
        Vector3 eAnchor = end.position;
        Vector3 eControl = end.position + endvel.position;

        List<Vector3> splinePoints = new List<Vector3>(numpoints + 1);
        List<Vector3> splineSlopes = new List<Vector3>(numpoints + 1);
        for (int dim = 0; dim < 3; dim++) { //loop through x,y,z dimensions of the position vectors
            //spline formula from www.moshplant.com/direct-or/bezier/math.html
            float c = 3.0f * (sControl[dim] - sAnchor[dim]);
            float b = 3.0f * (eControl[dim] - sControl[dim]) - c;
            float a = eAnchor[dim] - sAnchor[dim] - c - b;
            for (int i = 0; i < numpoints + 1; i++) {
                float t = (float)i / (float)(numpoints);

                Vector3 v = splinePoints[i];
                v[dim] = (a * Mathf.Pow(t, 3)) + (b * Mathf.Pow(t, 2)) + (c * t) + sAnchor[dim];
                splinePoints[i] = v;

                Vector3 v2 = splineSlopes[i];
                v2[dim] = (3 * a * Mathf.Pow(t, 2)) + (2 * b * t) + c; //slope formula is just derivate of position formula
                splineSlopes[i] = v2;
            }
        }


        //position in GoalTrajectory is difference between previous and current splinePoints
        //rotation in GoalTrajectory is difference between previous and current splineSlopes
        //TODO: position of points will be from rotational orientation of first point, not the previous point.... (have to rotate the vector by some quaternion... diff between starting slope and previous slope?)
        List<BoneTransform> GoalTrajectory = new List<BoneTransform>(numpoints);
        for (int i = 0; i < numpoints; i++) {
            Vector3 positionDiff = splinePoints[i + 1] - splinePoints[i];
            Quaternion rotationDiff = Quaternion.LookRotation(splineSlopes[i + 1]) * Quaternion.Inverse(Quaternion.LookRotation(splineSlopes[i]));
            GoalTrajectory[i] = new BoneTransform(splinePoints[i + 1] - splinePoints[i], Quaternion.identity);
        }
        return GoalTrajectory;
    }

    private List<Vector3> GenerateSplinePositions(Vector3 sAnchor, Vector3 sControl, Vector3 eAnchor, Vector3 eControl, int numpoints)
    {
        List<Vector3> splinePoints = new List<Vector3>(numpoints + 1);
        for(int dim = 0; dim < 3; dim++) { //loop through x,y,z dimensions of the position vectors
            //x0 is sA, x1 is sC x2 is eC, x3 is eA
            float c = 3.0f * (sControl[dim] - sAnchor[dim]);
            float b = 3.0f * (eControl[dim] - sControl[dim]) - c;
            float a = eAnchor[dim] - sAnchor[dim] - c - b;
            for(int i = 0; i < numpoints+1; i++) {
                float t = (float)i/(float)(numpoints);
                Vector3 v = splinePoints[i];
                v[dim] = a*Mathf.Pow(t, 3) + b*Mathf.Pow(t,2) + c*t + sAnchor[dim];
                splinePoints[i] = v;
            }
        }
        for(int i = 0; i < numpoints; i++) {
            Vector3 v = splinePoints[i+1] - splinePoints[i];
            splinePoints[i] = v;
        }
        splinePoints.RemoveAt(numpoints);
        return splinePoints;
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
