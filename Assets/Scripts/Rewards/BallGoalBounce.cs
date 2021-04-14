// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class BallGoalBounce : BallGoal
{

    public float maximumVelocity = 20;
    public float forceToApply = 5;

    private Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();

        rBody.AddForce(forceToApply * transform.forward * Time.fixedDeltaTime,
                            ForceMode.VelocityChange);
    }

    // void FixedUpdate()
    // {
        // Vector3 velocity = rBody.velocity;
        // Vector3 direction = velocity.normalized;
        // Vector3 forceVector = (maximumVelocity - velocity.magnitude) * forceToApply * direction;
        // forceVector.y = 0;

        // rBody.AddForce(forceVector * Time.fixedDeltaTime, ForceMode.VelocityChange);
    // }
}
