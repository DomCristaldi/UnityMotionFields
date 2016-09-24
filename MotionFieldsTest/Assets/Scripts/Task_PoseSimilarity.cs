using UnityEngine;
using System.Collections;
using System.Linq;

[CreateAssetMenu]
public class Task_PoseSimilarity : ATask
{


    public override float CheckReward(MotionPose oldPose, MotionPose newPose, float taskval)
    {
        //throw new NotImplementedException();

        float[] poseSimilarityArray = new float[oldPose.bonePoses.Length * 7];//3 position vals, 4 rotation vals

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

    }

    public override float DetermineTaskValue()
    {
        //throw new NotImplementedException();
        return 1.0f;
    }
}
