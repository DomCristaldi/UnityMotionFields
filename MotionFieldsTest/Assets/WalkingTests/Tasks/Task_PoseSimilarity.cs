using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu]
public class Task_PoseSimilarity : ATask
{


    public override float CheckCost(MotionPose oldPose, MotionPose newPose, Transform targetLocation, List<AnimClipInfo> animClipInfoList)
    {
        /*
        float[] poseSimilarityArray = new float[oldPose.bonePoses.Length * 7];//3 position values, 4 rotation values

        //compare every bone from Old Pose to New Pose
        for (int i = 0; i < oldPose.bonePoses.Length; ++i) {

            float[] oldFlatPositionNext = oldPose.bonePoses[i].flattenedPositionNext();
            float[] newFlatPosition = newPose.bonePoses[i].flattenedValue();

            //compare the indivisual components of the bone from Old Pose to New Pose
            for (int j = 0; j < oldFlatPositionNext.Length; ++j) {
                poseSimilarityArray[i + j] = Mathf.Abs(oldFlatPositionNext[j] - newFlatPosition[j]);
            }
        }

        return maxTaskValue - (poseSimilarityArray.Average() * 100.0f);
        */

        float[] oldPoseFootNext = oldPose.GetBonePose("LeftFoot").flattenedPositionNext();
        //oldPoseFootNext = oldPoseFootNext.Concat<float>(oldPose.GetBonePose("RightFoot").flattenedPositionNext()).ToArray<float>();

        float[] newPoseFoot = newPose.GetBonePose("LeftFoot").flattenedValue();
        //newPoseFoot = newPoseFoot.Concat<float>(newPose.GetBonePose("RightFoot").flattenedValue()).ToArray<float>();

        float[] poseSimilarityArray = new float[oldPoseFootNext.Length];

        for (int i = 0; i < oldPoseFootNext.Length; ++i) {
            poseSimilarityArray[i] = Mathf.Abs(oldPoseFootNext[i] - newPoseFoot[i]);
        }

        return poseSimilarityArray.Average();
    }
}
