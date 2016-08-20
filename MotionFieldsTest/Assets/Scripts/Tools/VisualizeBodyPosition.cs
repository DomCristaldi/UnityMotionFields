using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class VisualizeBodyPosition : MonoBehaviour {

    public Avatar avatar;
    public Transform skeletonRoot;

    public Transform referencePoint;

    HumanPoseHandler poseHanlder;
    HumanPose hPose;

    void Awake() {
        hPose = new HumanPose();
        poseHanlder = new HumanPoseHandler(avatar, skeletonRoot);

    }

    // Use this for initialization
    void Start () {

        UpdateHumanPose();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateHumanPose();
	}

    private void UpdateHumanPose() {
        poseHanlder = new HumanPoseHandler(avatar, skeletonRoot);
        hPose = new HumanPose();

        poseHanlder.GetHumanPose(ref hPose);
    }

#if UNITY_EDITOR

    [SerializeField]
    private Color g_rootColor = Color.green;
    [SerializeField]
    private float g_rootSize = 0.01f;

    [SerializeField]
    private Color g_refColor = Color.red;
    [SerializeField]
    private float g_refSize = 0.01f;

    [SerializeField]
    private Color g_connectionColor = Color.yellow;

    void OnDrawGizmos() {
        Color originalGizmoColor = Gizmos.color;

        Gizmo_DrawPoseConnection();

        Gizmos.color = originalGizmoColor;
    }

    private void Gizmo_DrawPoseConnection() {
        Gizmos.color = g_rootColor;
        Gizmos.DrawSphere(hPose.bodyPosition + referencePoint.position, g_rootSize);

        Gizmos.color = g_refColor;
        Gizmos.DrawSphere(referencePoint.position, g_refSize);

        Gizmos.color = g_connectionColor;
        Gizmos.DrawLine(hPose.bodyPosition + referencePoint.position, referencePoint.transform.position);

    }

#endif
}
