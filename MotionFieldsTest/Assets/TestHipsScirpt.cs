using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestHipsScirpt : MonoBehaviour {

    public Avatar avatar;
    public Transform rootMotionRefPoint;


    void Awake() {

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}


[CustomEditor(typeof(TestHipsScirpt))]
public class TestHipsScirpt_Editor : Editor{

    TestHipsScirpt selfScript;

    public HumanPoseHandler hPoseHandler;
    public HumanPose hPose;


    void OnEnable() {
        selfScript = (TestHipsScirpt)target;

        hPoseHandler = new HumanPoseHandler(selfScript.avatar, selfScript.transform);
        hPose = new HumanPose();
        UpdateAvatar();
    }

    private void UpdateAvatar() {
        hPoseHandler.GetHumanPose(ref hPose);
    }


    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        UpdateAvatar();

        Inspector_DrawRotationTools();

    }


    private void Inspector_DrawRotationTools() {

        if (GUILayout.Button("Adjust For Root Offset")) {
            Vector3 flooredCenterOfMass = new Vector3(hPose.bodyPosition.x,
                                                      selfScript.rootMotionRefPoint.position.y,
                                                      hPose.bodyPosition.z);
            Vector3 adjustmentDirec = selfScript.rootMotionRefPoint.position - flooredCenterOfMass;

            selfScript.transform.position += adjustmentDirec;
        }

        if(GUILayout.Button("Adjust Rotation")) {
            Quaternion flooredBodyRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(hPose.bodyRotation * Vector3.forward, Vector3.up).normalized, Vector3.up);
            Quaternion flooredRefRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(selfScript.rootMotionRefPoint.rotation * Vector3.forward, Vector3.up).normalized, Vector3.up);

            Quaternion adjRot = flooredRefRot * Quaternion.Inverse(flooredBodyRot);

            //selfScript.transform.rotation = adjRot;


            //selfScript.transform.rotation = flooredBodyRot;


            //Quaternion newHipsRot = selfScript.transform.rotation * Quaternion.Inverse(flooredBodyRot);
            //Quaternion newHipsRot = flooredBodyRot * Quaternion.Inverse(selfScript.transform.rotation);

            //selfScript.transform.rotation = newHipsRot;

            //Quaternion originalBodyRot

            float adjustmentAngle = Quaternion.Angle(flooredBodyRot, flooredRefRot);
            Plane testPlane = new Plane(hPose.bodyPosition, selfScript.rootMotionRefPoint.position);

            if(testPlane.GetSide(flooredBodyRot * Vector3.forward)) { adjustmentAngle *= -1.0f; }

            selfScript.transform.RotateAround(selfScript.transform.position,
                                              Vector3.up,
                                              adjustmentAngle);

        }
    }

}
