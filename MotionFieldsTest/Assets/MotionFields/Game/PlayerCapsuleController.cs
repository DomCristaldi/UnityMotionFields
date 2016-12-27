using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCapsuleController : MonoBehaviour {

    public enum MovementState
    {
        Idle = 0,
        Moving = 1,
    }

    public MovementState currentMovementState;

    public KeyCode moveFoward = KeyCode.W;
    public KeyCode moveBackward = KeyCode.S;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode moveLeft = KeyCode.A;

    [Space]
    public float movementVelocity = 0.5f;
    public float movementAcceleration = 1.0f;

    Vector2 desiredMoveDirec;
    Vector2 currentMoveDirec;

	// Use this for initialization
	void Start () {
        currentMoveDirec = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        UpdateLatestDesiredInput();
        MoveCurrentMovementTowardsDesired();

        transform.position += new Vector3(currentMoveDirec.x * movementVelocity * Time.deltaTime,
                                          0.0f,
                                          currentMoveDirec.y * movementVelocity * Time.deltaTime);

        //Debug.LogFormat("Desired Movement Input: {0}", GetMovementComponentFromInput(moveFoward, moveBackward));
        Debug.LogFormat("Desired Movement Input: {0}", currentMoveDirec);

    }

    private float GetMovementComponentFromInput(KeyCode positiveInput,
                                                KeyCode negativeInput)
    {
        bool posInput = Input.GetKey(positiveInput);
        bool negINput = Input.GetKey(negativeInput);

        if (posInput == negINput) { return 0.0f; }
        else if (posInput) { return 1.0f;  }
        else { return -1.0f; }
    }

    private void UpdateLatestDesiredInput()
    {
        //if (Input.GetKeyDown(moveFoward))
        desiredMoveDirec.x = GetMovementComponentFromInput(moveRight, moveLeft);
        desiredMoveDirec.y = GetMovementComponentFromInput(moveFoward, moveBackward);
        desiredMoveDirec.Normalize();
    }

    private void MoveCurrentMovementTowardsDesired()
    {
        currentMoveDirec = Vector2.MoveTowards(currentMoveDirec,
                                               desiredMoveDirec,
                                               movementAcceleration * Time.deltaTime);
    }

    /*
    private void MoveGameobject()
    {

    }
    */
}
