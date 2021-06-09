using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using PrefabInterface;
using Unity.MLAgents.Sensors;

/// Actions are currently discrete. 2 branches of 0,1,2, 0,1,2
///
public class TrainingAgent : Agent, IPrefab
{
    public void SetColor(Vector3 color) { }
    public void SetSize(Vector3 scale) { }

    /// <summary>
    /// Returns a random position within the range for the object.
    /// </summary>
    public virtual Vector3 GetPosition(Vector3 position,
                                        Vector3 boundingBox,
                                        float rangeX,
                                        float rangeZ)
    {
        float xBound = boundingBox.x;
        float zBound = boundingBox.z;
        float xOut = position.x < 0 ? Random.Range(xBound, rangeX - xBound)
                                    : Math.Max(0, Math.Min(position.x, rangeX));
        float yOut = Math.Max(position.y, 0) + transform.localScale.y / 2 + 0.01f;
        float zOut = position.z < 0 ? Random.Range(zBound, rangeZ - zBound)
                                    : Math.Max(0, Math.Min(position.z, rangeZ));

        return new Vector3(xOut, yOut, zOut);
    }

    ///<summary>
    /// If rotationY set to < 0 change to random rotation.
    ///</summary>
    public virtual Vector3 GetRotation(float rotationY)
    {
        return new Vector3(0,
                        rotationY < 0 ? Random.Range(0f, 360f) : rotationY,
                        0);
    }

    public float speed = 30f;
    public float rotationSpeed = 100f;
    public float rotationAngle = 0.25f;
    [HideInInspector]
    public int numberOfGoalsCollected = 0;
    public ProgressBar progBar;

    private Rigidbody _rigidBody;
    private bool _isGrounded;
    private ContactPoint _lastContactPoint;
    private TrainingArena _arena;
    private float _rewardPerStep;
    private float _previousScore = 0;
    private float _currentScore = 0;
    private float _maxHealth = 100;

    public override void Initialize()
    {
        //Because mlagents (2.0) finds the attached sensors during OnEnable->LazyInitialize we have to make sure they are already setup correctly.
        //This is the latest point when it is possible to remove them.
        // DestroyImmediate(GetComponent<CameraSensorComponent>());//This destroys in the editor but it still returns part of the observations??
        // DestroyImmediate(GetComponent<RayPerceptionSensorComponent3D>());
        _arena = GetComponentInParent<TrainingArena>();
        _rigidBody = GetComponent<Rigidbody>();
        _rewardPerStep = MaxStep > 0 ? -1f / MaxStep : 0; // No step reward for infinite episode by default
        progBar = GameObject.Find("UI ProgressBar").GetComponent<ProgressBar>();
        progBar.AssignAgent(this);
    }

    //@TODO check if switching these fields to add the [Observable] attribute to fields and properties on the Agent is better practice for these obs.
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localVel = transform.InverseTransformDirection(_rigidBody.velocity);
        sensor.AddObservation(localVel);
        Vector3 localPos = transform.position;
        sensor.AddObservation(localPos);
    }

    public override void OnActionReceived(ActionBuffers action)
    {
        int actionForward = Mathf.FloorToInt(action.DiscreteActions[0]);
        int actionRotate = Mathf.FloorToInt(action.DiscreteActions[1]);
        
        MoveAgent(actionForward, actionRotate);

        AddReward(_rewardPerStep);
        _currentScore = GetCumulativeReward();
    }

    private void MoveAgent(int actionForward, int actionRotate)
    {
        Vector3 directionToGo = Vector3.zero;
        Vector3 rotateDirection = Vector3.zero;

        if (_isGrounded)
        {
            switch (actionForward)
            {
                case 1:
                    directionToGo = transform.forward * 1f;
                    break;
                case 2:
                    directionToGo = transform.forward * -1f;
                    break;
            }
        }
        switch (actionRotate)
        {
            case 1:
                rotateDirection = transform.up * 1f;
                break;
            case 2:
                rotateDirection = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDirection, Time.fixedDeltaTime * rotationSpeed);
        _rigidBody.AddForce(directionToGo * speed * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut) 
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 2;
        }
   }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        _previousScore = _currentScore;
        numberOfGoalsCollected = 0;
        _arena.ResetArena();
        _rewardPerStep = MaxStep > 0 ? -1f / MaxStep : 0;
        _isGrounded = false;
        progBar.BarValue = _maxHealth;
    }


    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0)
            {
                _isGrounded = true;
            }
        }
        _lastContactPoint = collision.contacts.Last();
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0)
            {
                _isGrounded = true;
            }
        }
        _lastContactPoint = collision.contacts.Last();
    }

    void OnCollisionExit(Collision collision)
    {
        if (_lastContactPoint.normal.y > 0)
        {
            _isGrounded = false;
        }
    }

    public void AgentDeath(float reward)
    {
        Debug.Log("Agent Died");
        AddReward(reward);
        _currentScore = GetCumulativeReward();
        EndEpisode();
    }

    public void AddExtraReward(float rewardFactor)
    {
        AddReward(Math.Min(rewardFactor * _rewardPerStep, -0.00001f));
        if (progBar != null)
        {
            progBar.ReduceHealth(0.05f);
        }
    }

    public float GetPreviousScore()
    {
        return _previousScore;
    }
}
