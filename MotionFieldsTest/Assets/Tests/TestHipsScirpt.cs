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

#if UNITY_EDITOR
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

            Vector3 flooredCenterOfMass = new Vector3(hPose.bodyPosition.x,
                                          selfScript.rootMotionRefPoint.position.y,
                                          hPose.bodyPosition.z);

            //floor out the two rotations to get only Yaw (XZ Plane) component
            Quaternion bodyRot_Floored = Quaternion.LookRotation(Vector3.ProjectOnPlane(hPose.bodyRotation * Vector3.forward, Vector3.up).normalized, Vector3.up);
            Quaternion refRot_Floored = Quaternion.LookRotation(Vector3.ProjectOnPlane(selfScript.rootMotionRefPoint.rotation * Vector3.forward, Vector3.up).normalized, Vector3.up);

            //Quaternion adjRot = refRot_Floored * Quaternion.Inverse(bodyRot_Floored);

            //raw angle between two floored rotatoins (this is always positive)
            float adjustmentAngle = Quaternion.Angle(bodyRot_Floored, refRot_Floored);

            //calculate a plane that uses the floored reference point's rotation's right vector as the normal
            Vector3 rightOfFlooredRefRot = Vector3.Cross(refRot_Floored * Vector3.forward, Vector3.up);
            Plane testPlane = new Plane(rightOfFlooredRefRot, hPose.bodyPosition);

            //debugs for seeing that plane
            Debug.DrawRay(flooredCenterOfMass, testPlane.normal * 20.0f, Color.magenta, 5.0f);
            Debug.DrawRay(flooredCenterOfMass, refRot_Floored * Vector3.forward * 20.0f, Color.green, 5.0f);
            Debug.DrawRay(flooredCenterOfMass, bodyRot_Floored * Vector3.forward * 20.0f, Color.yellow, 5.0f);
            Debug.LogFormat("Positive Side: {0}", testPlane.GetSide(flooredCenterOfMass + (bodyRot_Floored * Vector3.forward)));

            //use plane to determine direction of rotation (if we're oriented to the positive side, we need to rotate left, so we multiply by -1.0f)
            if (!testPlane.GetSide(flooredCenterOfMass + (bodyRot_Floored * Vector3.forward))) { adjustmentAngle *= -1.0f; }

            //final debug for determining that our math is correct
            Debug.LogFormat("Adjustment Angle: {0}", adjustmentAngle);

            //rotate the hips transform around the up axis by the angle we just calculated
            selfScript.transform.RotateAround(selfScript.transform.position, //hpose.bodyPosition -> TODO: DO WE WANT TO USE THIS INSTEAD?
                                              Vector3.up,
                                              adjustmentAngle);

        }
    }

}
#endif
