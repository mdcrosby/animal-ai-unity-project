// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class DecayGoal : BallGoal
{
    public float initialReward;
    public float middleReward; // used to calculate where the 'neutralColour' should be, for colour interpolation
    public float finalReward;
    // if reward above goodEndingThreshold when agent reaches it, end episode like GoodGoal
    public float goodRewardEndThreshold;
    // if reward below badEndingThreshold when agent reaches it, end episode like BadGoal
    public float badRewardEndThreshold;

    [ColorUsage(true, true)]
    public Color goodColour;
    [ColorUsage(true, true)]
    public Color neutralColour;
    [ColorUsage(true, true)]
    public Color badColour;

    public float decayRate = -0.001f;
    public bool flipDecayDirection = false;

    private Material _mat;
    private bool isDecaying = false;
    private float middleDecayProportion;
    private float decayWidth;

    void Awake()
    {
        _mat = this.gameObject.GetComponent<MeshRenderer>().material;
        _mat.EnableKeyword("_EMISSION");
        _mat.SetColor("_EmissionColor", flipDecayDirection ? badColour : goodColour);

        // if (negative) decay but rate is positive, or (positive) 'anti-decaying' but rate is negative, then flip the rate value provided
        if (flipDecayDirection ? decayRate < 0 : decayRate > 0) { decayRate *= -1; Debug.Log("Had to flip decay rate"); }

        // check middle value (for yellow)
        if (middleReward < Mathf.Min(initialReward, finalReward) || middleReward > Mathf.Max(initialReward, finalReward)) {
            Debug.Log("middleReward not in expected range. Clamping . . .");
            middleReward = Mathf.Clamp(middleReward, Mathf.Min(initialReward, finalReward), Mathf.Max(initialReward, finalReward));
        }
        decayWidth = Mathf.Abs(initialReward - finalReward);
        middleDecayProportion = (flipDecayDirection ? (middleReward - initialReward) : (middleReward - finalReward))/decayWidth;

        reward = initialReward;
        SetTag();

        UpdateColour(1);
        StartDecay();
    }

    // StartDecay()/StopDecay() functions by default do not change reward value,
    // since we might want to use these to pause/restart decay without resetting!
    void StartDecay(bool reset = false) { isDecaying = true; if (reset) { reward = initialReward; } }
    void StopDecay(bool reset = false) { isDecaying = false; if (reset) { reward = finalReward; } }

    void SetTag()
    {
        if (IsGoodGoal()) { this.gameObject.tag = "goodGoal"; }
        else if (IsBadGoal()) { this.gameObject.tag = "badGoal"; }
        else { this.gameObject.tag = "goodGoalMulti"; }
    }

    bool IsGoodGoal() { return reward >= goodRewardEndThreshold; }
    bool IsBadGoal() { return reward <= badRewardEndThreshold; }

    // assumes linear decay (for now - @TO-DO could maybe add other decay functions?)
    void FixedUpdate()
    {
        // if goal reward value sufficiently good/bad, then it could be episode-ending, like GoodGoal or BadGoal
        if (reward >= goodRewardEndThreshold || reward <= badRewardEndThreshold)
        {
            SetEpisodeEnds(true);
        }
        else { SetEpisodeEnds(false); }

        // if still decaying
        if (isDecaying)
        {
            UpdateGoal(decayRate);
            SetTag();
        }

        // if ball has reached end of its decay but we haven't stopped decay yet . . .
        if (HasFinalDecayBeenReached() &&/*but*/ isDecaying)
        {
            StopDecay();
            UpdateGoal(0);
        }
    }

    void SetEpisodeEnds(bool shouldEpisodeEnd) { isMulti = !shouldEpisodeEnd; }

    private bool HasFinalDecayBeenReached() { return flipDecayDirection ? reward >= finalReward : reward <= finalReward; }
    private bool StillInInitDecayState() { return flipDecayDirection ? reward <= initialReward : reward >= initialReward; }

    void UpdateGoal(float rate = -0.001f /*i.e. decayRate*/)
    {
        UpdateValue(rate);
        UpdateColour(getProportion(reward));
    }
    
    private void UpdateValue(float rate)
    {
        // apply step-change to reward value w/given rate, but clamp at extremes
        reward = Mathf.Clamp(reward + rate, Mathf.Min(initialReward, finalReward), Mathf.Max(initialReward, finalReward));
    }
    // (assume we have just updated value before executing UpdateColour(), i.e. just as in UpdateGoal())
    private void UpdateColour(float p)
    {
        //Debug.Log("p is: " + p + ", and middleDecayProportion is: " + middleDecayProportion);
        if (p != Mathf.Clamp(p, 0, 1)) { Debug.Log("UpdateColour passed a bad proprtion! Clamping . . ."); p = Mathf.Clamp(p, 0, 1); }
        // if within 'bad -> neutral' range, interpolates between red (bad) and yellow (neutral)
        if (p < middleDecayProportion)
        {
            p = (p / middleDecayProportion); // treat as linear interpolation from 0 to 1 even though actually from 0 to PASSMARK
            _mat.SetColor("_EmissionColor", p * neutralColour + (1 - p) * badColour
                + (0.5f - Mathf.Abs(p - 0.5f)) * Color.white * 0.1f /*last component is constant for aesthetics*/);
        }
        // if within 'neutral -> good' range, interpolates between yellow (neutral) and green (good)
        else
        {
            p = ((p - middleDecayProportion) / (1 - middleDecayProportion)); // treat as linear interpolation from 0 to 1 even though actually from PASSMARK to 1
            _mat.SetColor("_EmissionColor", p * goodColour + (1 - p) * neutralColour
                + (0.5f - Mathf.Abs(p - 0.5f)) * Color.white * 0.1f /*last component is constant for aesthetics*/);
        }
    }
    float getProportion(float r) { return (r - Mathf.Min(initialReward, finalReward)) / decayWidth; }
}
