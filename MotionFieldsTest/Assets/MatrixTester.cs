using UnityEngine;
using System.Collections;

public class MatrixTester : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ApplyToTransformHierarcyRecursive(transform);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ApplyToTransformHierarcyRecursive(Transform rootTransform) {

        //Matrix4x4 m = Matrix4x4.identity;
        Matrix4x4 m = Matrix4x4.TRS(new Vector3(5, 3, 2),
                                    Quaternion.LookRotation(new Vector3(2, 6, 7).normalized, Vector3.up), 
                                    new Vector3(2,8,3));

        /*
        Debug.LogFormat("translation: {0}\nroation: {1}", m.ExtractTranslation(),
                                                          m.ExtractRotation());
        */

        rootTransform.localRotation = m.ExtractRotation();
        
        foreach (Transform tf in rootTransform) {
            ApplyToTransformHierarcyRecursive(tf);
        } 
        
    }


}
