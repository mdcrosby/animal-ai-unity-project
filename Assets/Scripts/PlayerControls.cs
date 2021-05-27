using Unity.MLAgents;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{

    private Camera _cameraAbove;
    private Camera _cameraAgent;
    private Camera _cameraFollow;
    private TrainingAgent _agent;
    private Text _score;
    private int _numActive = 0;
    private Dictionary<int, Camera> _cameras;
    public float prevScore = 0;

    void Start()
    {
        _agent = GameObject.FindGameObjectWithTag("agent").GetComponent<TrainingAgent>();
        _cameraAbove = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _cameraAgent = _agent.transform.Find("AgentCamMid").GetComponent<Camera>();
        _cameraFollow = GameObject.FindGameObjectWithTag("camBase").GetComponent<Camera>();
        _score = GameObject.FindObjectOfType<Text>();//@TODO ewww

        _cameraAbove.enabled = true;
        _cameraAgent.enabled = false;
        _cameraFollow.enabled = false;

        _cameras = new Dictionary<int, Camera>();
        _cameras.Add(0, _cameraAbove);
        _cameras.Add(1, _cameraAgent);
        _cameras.Add(2, _cameraFollow);
        _numActive = 0;
        Debug.Log("Initializing Player Controls");
    }

    void Update()
    {
        bool cDown = Input.GetKeyDown(KeyCode.C);
        if (cDown)
        {
            _cameras[_numActive].enabled = false;
            _numActive = (_numActive + 1) % 3;
            _cameras[_numActive].enabled = true;
        }   
        if (Input.GetKeyDown(KeyCode.R))
        {
            _agent.EndEpisode();
        }

        _score.text = "Prev reward: " + _agent.GetPreviousScore().ToString("0.000") + "\n"
                        + "Reward: " + _agent.GetCumulativeReward().ToString("0.000");
    }
}
