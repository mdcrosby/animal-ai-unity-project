using UnityEngine;
using System.Collections;
using System.Collections.Generic;    // Don't forget to add this if using a List.

public class CollisionImpulseTracker : MonoBehaviour
{

    // public current impulse parameter
    public float impulseMagnitude;

    private void FixedUpdate()
    {
        impulseMagnitude = 0;
    }

    void OnCollisionEnter(Collision col)
    {
        print("OnCollisionEnter activated");
        impulseMagnitude += col.impulse.magnitude;
    }

    void OnCollisionStay(Collision col)
    {
        impulseMagnitude -= col.impulse.magnitude;
    }
}