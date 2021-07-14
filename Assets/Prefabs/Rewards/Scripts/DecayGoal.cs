using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecayGoal : BallGoal
{
    [Header("Reward Params")]
    public float initialReward;
    public float finalReward;

    [Header("Colour Params")]
    [ColorUsage(true, true)]
    public Color initialColour;
    [ColorUsage(true, true)]
    public Color finalColour;

    [Header("Decay Params")]
    public float decayRate = -0.001f;
    public bool flipDecayDirection = false;

    private Material _basemat;
    private Material _radialmat;
    private bool isDecaying = false;
    private float decayWidth;
    private float loAlpha = 0.11f;
    private float hiAlpha = 0.35f;

    public int fixedFrameDelay = 150; // controls extent of delay before (anti-)decay commences
    private int delayCounter;

    void Awake()
    {
        _basemat = this.gameObject.GetComponent<MeshRenderer>().material;
        _basemat.EnableKeyword("_EMISSION");
        _basemat.SetColor("_EmissionColor", flipDecayDirection ? finalColour : initialColour);

        _radialmat = this.gameObject.GetComponent<MeshRenderer>().materials[2];
        _radialmat.SetFloat("_Cutoff", isDecaying ? loAlpha : hiAlpha);

        canRandomizeColor = false;
        SetEpisodeEnds(false);

        // if (negative) decay but rate is positive, or (positive) 'anti-decaying' but rate is negative, then flip the rate value provided
        if (flipDecayDirection ? decayRate < 0 : decayRate > 0) { decayRate *= -1; Debug.Log("Had to flip decay rate"); }
        delayCounter = fixedFrameDelay;

        this.gameObject.tag = "goodGoalMulti";
        Debug.Log("intitialReward: "+initialReward);
        reward = initialReward;
        sizeMax = sizeMin = (flipDecayDirection ? finalReward : initialReward) * Vector3.one;

        decayWidth = Mathf.Abs(initialReward - finalReward);

        UpdateColour(flipDecayDirection?0:1);
    }

    public override void SetSize(Vector3 size)
    {
        base.SetSize(size);
    }

    // StartDecay()/StopDecay() functions by default do not change reward value,
    // since we might want to use these to pause/restart decay without resetting!
    void StartDecay(bool reset = false) { isDecaying = true; if (reset) { reward = initialReward; } }
    void StopDecay(bool reset = false) { isDecaying = false; if (reset) { reward = finalReward; } }

    // assumes linear decay (for now - @TO-DO could maybe add other decay functions?)
    void FixedUpdate()
    {
        if (StillInInitDecayState() && !isDecaying)
        {
            if (delayCounter > 0) { delayCounter--; }
            else { StartDecay(); UpdateGoal(0); }
        }

        // if still decaying
        if (isDecaying)
        {
            UpdateGoal(decayRate);
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

        _basemat.SetColor("_EmissionColor", p * initialColour + (1 - p) * finalColour
            + (0.5f - Mathf.Abs(p - 0.5f)) * Color.white * 0.1f /*last component is constant for aesthetics*/);

        _radialmat.SetFloat("_Cutoff", Mathf.Clamp(p*loAlpha + (1-p)*hiAlpha, loAlpha, hiAlpha));
    }
    float getProportion(float r) { return (r - Mathf.Min(initialReward, finalReward)) / decayWidth; }
}

