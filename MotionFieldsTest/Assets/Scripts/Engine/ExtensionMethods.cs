using UnityEngine;

public static class Transform_ExtensionMethods {

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