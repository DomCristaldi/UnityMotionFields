using UnityEngine;
using System.Linq;

public static class Quaternion_ExtensionMethods
{
    public static Quaternion AverageQuaternions(Quaternion[] quats, float[] weights)
    {
        if (quats.Length != weights.Length) { Debug.LogError("number of quaternions to average does not equal th number of weights."); return Quaternion.identity; }
        Quaternion avgRot = new Quaternion(0, 0, 0, 0);
        Quaternion first = quats[0];

        for (int i = 0; i < quats.Length; ++i)
        {
            //if the dot product is negaticve, negate quats[i] so that it exists on the same half-sphere. 
            //This is allowed because q = -q, and is nessesary because the error of aproximation increases the farther apart the quaternions are.
            if (Quaternion.Dot(quats[i], first) > 0.0f) 
            {
                avgRot.w += (quats[i].w * weights[i]);
                avgRot.x += (quats[i].x * weights[i]);
                avgRot.y += (quats[i].y * weights[i]);
                avgRot.z += (quats[i].z * weights[i]);
            }
            else
            {
                avgRot.w -= (quats[i].w * weights[i]);
                avgRot.x -= (quats[i].x * weights[i]);
                avgRot.y -= (quats[i].y * weights[i]);
                avgRot.z -= (quats[i].z * weights[i]);
            }
        }

        //Normalize the result to a unit Quaternion
        avgRot = avgRot.Normalize();

        return avgRot;
    }

    /// <summary>
    /// Returns a Vector4 representation of the quaternion (w, x, y, z)
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Vector4(w, x, y, z)</returns>
    public static Vector4 Ex_VectorValue(this Quaternion value) {
        return new Vector4(value.w, value.x, value.y, value.z);
    }

    /// <summary>
    /// Does NOT modify the Quaternion passed in, but returns the Normalized Quaternion
    /// </summary>
    public static Quaternion Normalize(this Quaternion value)
    {
        float invMag = 1.0f / Mathf.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w);
        value.x *= invMag;
        value.y *= invMag;
        value.z *= invMag;
        value.w *= invMag;
        return value;
    }

    /// <summary>
    /// Gives full precision representation of a quaternion formatted as "(x, y, z, w)"
    /// </summary>
    public static string debugString(this Quaternion value)
    {
        return "(" + value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString() + ")";
    } 
}

public static class Transform_ExtensionMethods {

    [System.Flags]
    public enum LerpType {
        Position = 0x01,
        Rotation = 0x02,
        Scale = 0x04,
    }

    public static Transform LerpTransform(this Transform thisTf, Transform tfFrom, Transform tfTo, float delta, LerpType type = LerpType.Position) {


        /*
        if (type == (LerpType.Position & type)) {
            Debug.Log("pos");
        }

        if () {
            Debug.Log("rot");
        }

        if (type == (LerpType.Scale & type)) {
            Debug.Log("scl");
        }
        */

        thisTf.localPosition = Vector3.Lerp(tfFrom.localPosition, tfTo.localPosition, delta);
        thisTf.localRotation = Quaternion.Slerp(tfFrom.localRotation, tfTo.localRotation, delta);
        thisTf.localScale = Vector3.Lerp(tfFrom.localScale, tfTo.localScale, delta);

        return thisTf;
    }


    //TEST ME!!!!!!!!!!!!!!!!!!!!!1
    public static Vector3 AveragePosition_Recursive(this Transform thisTf) {

        //TODO: come back and do this with Linq so it's easier to understand (if optimizations not needed)

        Transform[] aggregateTransforms = thisTf.GetComponentsInChildren<Transform>();

        Vector3[] aggregatePositions = (Vector3[])(from tf in aggregateTransforms
                                                   select tf.position);

        Vector3 avgPos;
        avgPos.x = (float)(from pos in aggregatePositions
                           select pos.x).Average();

        avgPos.y = (float)(from pos in aggregatePositions
                           select pos.y).Average();

        avgPos.z = (float)(from pos in aggregatePositions
                           select pos.z).Average();

        return avgPos;
        //Transform[] childrenTF = thisTf.GetComponentsInChildren<Transform>();

        //Vector3 avgVec = thisTf.position; //new Vector3(0.0f, 0.0f, 0.0f);

        //Vector3[] aggregatePositions = from ()
        /*
        foreach(Transform tf in thisTf) {
            avgVec += tf.AveragePosition_Recursive();
        }

        return avgVec;
        */
    }
}

public static class Matrix4x4_ExtensionMethods {

    public static Vector3 ExtractTranslation(this Matrix4x4 thisMatrix) {
        return thisMatrix.GetColumn(3);
    }

    public static Quaternion ExtractRotation(this Matrix4x4 thisMatrix) {
        return Quaternion.LookRotation(thisMatrix.GetColumn(2), thisMatrix.GetColumn(1));
    }


    public static Vector3 ExtractScale(this Matrix4x4 thisMatrix) {

        /*
        Vector4 column0 = thisMatrix.GetColumn(0);
        Vector4 column1 = thisMatrix.GetColumn(1);
        Vector4 column2 = thisMatrix.GetColumn(2);

        float sign0 = Mathf.Sign(column0[0]) * Mathf.Sign(column0[1]) * Mathf.Sign(column0[2]) * Mathf.Sign(column0[3]);
        float sign1 = Mathf.Sign(column1[0]) * Mathf.Sign(column1[1]) * Mathf.Sign(column1[2]) * Mathf.Sign(column1[3]);
        float sign2 = Mathf.Sign(column2[0]) * Mathf.Sign(column2[1]) * Mathf.Sign(column2[2]) * Mathf.Sign(column2[3]);


        return new Vector3(column0.magnitude * sign0,
                           column1.magnitude * sign1,
                           column2.magnitude * sign2);
        */

        return new Vector3(thisMatrix.GetColumn(0).magnitude,
                           thisMatrix.GetColumn(1).magnitude,
                           thisMatrix.GetColumn(2).magnitude);
    }


    public static Matrix4x4 Slerp(this Matrix4x4 thisMatrix, Matrix4x4 startMatrix, Matrix4x4 endMatrix, float delta) {

        return Matrix4x4.TRS(Vector3.Lerp(startMatrix.ExtractTranslation(),
                                          endMatrix.ExtractTranslation(),
                                          delta),

                             Quaternion.Slerp(startMatrix.ExtractRotation(),
                                              endMatrix.ExtractRotation(),
                                              delta),

                             Vector3.Lerp(startMatrix.ExtractScale(),
                                          endMatrix.ExtractScale(),
                                          delta)
                             );
    }

}

public static class BoneTransform_ExtensionMethods {


    //TEST ME!!!!!!!!!!!!!!!!!!!!!!!!!!
    //OPTIMIZE (this is bad use of Linq, come back and do it right
    //this function is deprecated, use BoneTransform.BlendTransforms(BoneTransform[], float[]) instead
    public static BoneTransform AvgPoseValue(this BoneTransform[] thisBoneTransformArray) {

        BoneTransform avgBoneTf = new BoneTransform();


        avgBoneTf.posX = (from tf in thisBoneTransformArray
                          select tf.posX).Average();

        avgBoneTf.posY = (from tf in thisBoneTransformArray
                          select tf.posY).Average();

        avgBoneTf.posZ = (from tf in thisBoneTransformArray
                          select tf.posZ).Average();


        avgBoneTf.rotW = (from tf in thisBoneTransformArray
                          select tf.rotW).Average();

        avgBoneTf.rotX = (from tf in thisBoneTransformArray
                          select tf.rotX).Average();

        avgBoneTf.rotY = (from tf in thisBoneTransformArray
                          select tf.rotY).Average();

        avgBoneTf.rotZ = (from tf in thisBoneTransformArray
                          select tf.rotZ).Average();

        return avgBoneTf;
    }

}