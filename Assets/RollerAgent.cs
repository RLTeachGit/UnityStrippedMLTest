using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RollerAgent : Agent {

    public  Transform ParentPlane;
    Vector3 mStartPostion;

	// Use this for initialization
    Rigidbody rBody;

    private void Awake() {
        brain = FindObjectsOfType<Brain>()[0];  //Find and attach to first Brain
    }

    void Start() {
        rBody = GetComponent<Rigidbody>();
        mStartPostion = transform.position;
    }

    public Transform Target;

    Vector3 NewPosition(float vRange =4.0f) {
        Vector3 tNewPosition = new Vector3(Random.Range(-vRange, vRange), 0.5f, Random.Range(-vRange, vRange));
        tNewPosition += ParentPlane.position;

        return tNewPosition;
    }

    public override void AgentReset() {
        if (this.transform.position.y < (ParentPlane.position.y-1.0f)) {
            // The agent fell
            this.transform.position = mStartPostion;
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
        } else {
            // Move the target to a new spot
            Target.position = NewPosition(); ;
        }
    }

    public override void CollectObservations() {
        // Calculate relative position
        Vector3 relativeTargetPosition = Target.position - this.transform.position;

        // Relative position
        AddVectorObs(relativeTargetPosition.x / 5);
        AddVectorObs(relativeTargetPosition.z / 5);

        // Distance to edges of platform

        Vector3 relativePosition = (transform.position - ParentPlane.position)/5.0f;


        AddVectorObs(-relativePosition.x);
        AddVectorObs(relativePosition.x);
        AddVectorObs(-relativePosition.z);
        AddVectorObs(relativePosition.z);

        // Agent velocity
        AddVectorObs(rBody.velocity.x / 5);
        AddVectorObs(rBody.velocity.z / 5);
    }


    public float speed = 10;
    private float previousDistance = float.MaxValue;

    public override void AgentAction(float[] vectorAction, string textAction) {
        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  Target.position);

        // Reached target
        if (distanceToTarget < 1.42f) {
            AddReward(1.0f);
            Done();
        }

        // Getting closer
        if (distanceToTarget < previousDistance) {
            AddReward(0.1f);
        }

        // Time penalty
        AddReward(-0.05f);

        // Fell off platform
        if (this.transform.position.y < -1.0) {
            AddReward(-1.0f);
            Done();
        }
        previousDistance = distanceToTarget;

        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);
    }
}
