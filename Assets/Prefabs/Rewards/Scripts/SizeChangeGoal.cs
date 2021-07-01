using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SizeChangeGoal : BallGoal
{
    [Header("Size Change Params")]
    public float initialSize = 5;
    public float finalSize = 2;
    private bool isShrinking;
    private bool freeToGrow = true;
    //private Collider[] immovableColliders;
    //private List<Tuple<GameObject,Vector3>> immoveableObstacles = new List<Tuple<GameObject, Vector3>>();
    private bool allowSizeChanges = false;
    public float sizeChangeRate; // can be either a constant amount, or an interpolation proportion
    private float sizeProportion = 0; // used only if linear interpolation
    public enum InterpolationMethod{Constant, Linear}
    public InterpolationMethod interpolationMethod;

    [Header("Reward Params")]
    public bool rewardSizeTracking = true;
    public float rewardOverride = 0;

    [Header("Grow/Shrink Timing Params")]
    public int fixedFrameDelay = 150; // controls extent of delay before size change commences
    private int delayCounter;
    private bool finishedSizeChange = false;

    // 'reward' and 'isMulti' carried over from inherited Goal class

    public override void SetSize(Vector3 size)
    {
        // bypasses random size assignment (used e.g. by ArenaBuilder) from parent BallGoal class, fixing to initialSize if this is used
        // otherwise just changes size as usual
        base.SetSize(((size.x < 0 || size.y < 0 || size.z < 0) ? initialSize * Vector3.one: size));
        // retains original reward disconnected from size
        if (!rewardSizeTracking) { reward = rewardOverride; }
    }

    void Awake()
    {
        isShrinking = (finalSize <= initialSize);
        delayCounter = fixedFrameDelay;

        sizeMin = Mathf.Min(initialSize, finalSize) * Vector3.one;
        sizeMax = Mathf.Max(initialSize, finalSize) * Vector3.one;

        // flip sizeChangeRate to align with direction of size-change (grow/shrink)
        if (isShrinking == (sizeChangeRate >= 0)) { sizeChangeRate*=-1; }

        SetSize(initialSize * Vector3.one);

        //if ((int)interpolationMethod > 0) { sizeProportion = (isShrinking ? 1 : 0); } // start at 1 if shrinking, 0 if growing
    }

    override public void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.gameObject.tag == "Immovable")
        {
            //immoveableObstacles.Add(new Tuple<GameObject, Vector3>(collision.gameObject, collision.GetContact(0).point));
        }
    }

    void FixedUpdate()
    {
        /*=== RAY COLLISIONS FOR IMMOVEABLES GO HERE ===*/
        if (!isShrinking)
        {
            Vector3 rayDir; string debug = "";
            /*
            foreach (Tuple<GameObject, Vector3> immoveableTuple in immoveableObstacles)
            {
                debug += immoveableTuple.Item1.name + " @ " + immoveableTuple.Item2 + ". ";
                /*
                for (int c = 0; c < immoveable.contactCount; c++)
                {
                    // get direction of ray towards collision contact point
                    rayDir = immoveable.GetContact(c).point - this.transform.position;


                }/
                
            }
            Debug.Log(debug);/

            immovableColliders = Physics.OverlapSphere(transform.position, transform.localScale.x);
            foreach (Collider c in immovableColliders)
            {
                debug += c.gameObject.name+", ";
            }
            Debug.Log(debug);*/

            freeToGrow = !Physics.Raycast(transform.position + new Vector3(0, transform.localScale.y/2, 0), Vector3.up, Mathf.Abs(sizeChangeRate));
            //Debug.DrawLine(Vector3.zero, new Vector3(10, 10, 10), Color.green, 0.1f, false);
            //Debug.DrawRay(transform.position + new Vector3(0, transform.localScale.y/2, 0), Vector3.up, Color.green, 0.1f, true);
            //Debug.Log(freeToGrow);
        }


        /*=== delay and size change operations here ===*/
        if (delayCounter > 0) { delayCounter--; }
        else
        {
            if (delayCounter == 1) { Debug.Log("delayCounter will now hit zero. Starting size change!"); }

            if (!finishedSizeChange && freeToGrow)
            {
                if ((int)interpolationMethod == 0/*Constant*/) { SetSize((_height + sizeChangeRate) * Vector3.one); }
                else if ((int)interpolationMethod > 0)/*Polynomial*/ { PolyInterpolationUpdate(); }

                if (isShrinking ? _height <= finalSize : _height >= finalSize) { SetSize(finalSize * Vector3.one); finishedSizeChange = true; }
            }
        }
    }

    private void PolyInterpolationUpdate()
    {
        sizeProportion += sizeChangeRate;
        // assume linear interpolation for now
        SetSize((sizeProportion * finalSize + (1 - sizeProportion) * initialSize) * Vector3.one);
    }
}
