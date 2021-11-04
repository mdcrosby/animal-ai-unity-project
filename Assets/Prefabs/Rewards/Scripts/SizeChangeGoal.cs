using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SizeChangeGoal : BallGoal
{
    [Header("Size Change Params")]
    public float initialSize = 5;
    public override void SetInitialValue(float v)
    {
        initialSize = v;
        if (delayCounter > 0) reward = initialSize;
    }
    public float finalSize = 1;
    public override void SetFinalValue(float v)
    {
        finalSize = v;
    }

    private bool isShrinking;
    private void CheckIfNeedToFlip()
    {
        isShrinking = (finalSize <= initialSize);
        if (isShrinking == (sizeChangeRate >= 0)) { sizeChangeRate *= -1; }
    }
    private bool freeToGrow = true;
    
    //private bool allowSizeChanges = false;
    public float sizeChangeRate; // can be either a constant amount, or an interpolation proportion
    public override void SetChangeRate(float v) { sizeChangeRate = v; CheckIfNeedToFlip(); }

    private float sizeProportion = 0; // used only if linear interpolation
    public enum InterpolationMethod{Constant, Linear}
    public InterpolationMethod interpolationMethod;

    [Header("Reward Params")]
    public bool rewardSizeTracking = true;
    public float rewardOverride = 0;

    [Header("Grow/Shrink Timing Params")]
    public int fixedFrameDelay = 150; // controls extent of delay before size change commences
    public override void SetDelay(float v)
    {
        fixedFrameDelay = Mathf.RoundToInt(v);
        delayCounter = fixedFrameDelay; // not sure if need to reset here or not
    }
    private int delayCounter;
    private bool finishedSizeChange = false;

    // 'reward' and 'isMulti' carried over from inherited Goal class

    // list of current collision objects
    //private CollisionImpulseTracker colImpTracker;
    private Collider[] sphereOverlap;

    public override void SetSize(Vector3 size)
    {
        // bypasses random size assignment (used e.g. by ArenaBuilder) from parent BallGoal class, fixing to initialSize if this is used
        // otherwise just changes size as usual
        base.SetSize((size.x < 0 || size.y < 0 || size.z < 0 || delayCounter > 0) ? initialSize * Vector3.one: size);
        // retains original reward disconnected from size
        if (!rewardSizeTracking) { reward = rewardOverride; }
    }

    new void Start()
    {
        base.Start(); // e.g.for ETK

        sizeMax = 8 * Vector3Int.one;
        initialSize = Mathf.Clamp(initialSize, 0, sizeMax.x);
        finalSize = Mathf.Clamp(finalSize, 0, sizeMax.x);

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
            freeToGrow = !Physics.Raycast(transform.position + new Vector3(0, transform.localScale.y/2, 0), Vector3.up, Mathf.Abs(sizeChangeRate));
            //Debug.DrawLine(Vector3.zero, new Vector3(10, 10, 10), Color.green, 0.1f, false);
            //Debug.DrawRay(transform.position + new Vector3(0, transform.localScale.y/2, 0), Vector3.up, Color.green, 0.1f, true);
            //Debug.Log(freeToGrow);

            sphereOverlap = Physics.OverlapSphere(this.transform.position, this.transform.localScale.x/2 - 0.05f);
            List<Collider> overlapList = new List<Collider>();
            foreach (Collider c in sphereOverlap) {
                if (c.gameObject.tag == "arena" || c.gameObject.tag == "Immovable") {
                    overlapList.Add(c);
                }
            }
            //print(overlapList.Count);
            //foreach (Collider c in overlapList) { print(c.gameObject.name); }
            if (overlapList.Count > 0)
            {
                freeToGrow = false;
            }
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
