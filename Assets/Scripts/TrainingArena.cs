using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using ArenaBuilders;
using UnityEngineExtensions;
using ArenasParameters;
using Holders;

public class TrainingArena : MonoBehaviour
{
    public ListOfPrefabs prefabs = new ListOfPrefabs();
    public GameObject spawnedObjectsHolder;
    public int maxSpawnAttemptsForAgent = 100;
    public int maxSpawnAttemptsForPrefabs = 20;
    public ListOfBlackScreens blackScreens = new ListOfBlackScreens();
    [HideInInspector]
    public int arenaID = -1;
    [HideInInspector]
    public Agent _agent;
    private ArenaBuilder _builder;
    private ArenaConfiguration _arenaConfiguration = new ArenaConfiguration();
    private AAI3EnvironmentManager _environmentManager;
    private List<Fade> _fades = new List<Fade>();
    private bool _lightStatus = true;
    private int _agentDecisionInterval; // To replace with a call to DecisionRequester.DecisionPeriod if possible
    // (not possible at the moment as it's internal and we cannot call GetComponent on internals) 

    internal void Awake()
    {
        _builder = new ArenaBuilder(gameObject,
                                    spawnedObjectsHolder,
                                    maxSpawnAttemptsForPrefabs,
                                    maxSpawnAttemptsForAgent);
        _environmentManager = GameObject.FindObjectOfType<AAI3EnvironmentManager>();
        _agent = FindObjectsOfType<Agent>(true)[0];
        _agentDecisionInterval = _agent.GetComponentInChildren<DecisionPeriod>().decisionPeriod;
        _fades = blackScreens.GetFades();
    }

    public void ResetArena()
    {
        Debug.Log("Resetting Arena");
        foreach (GameObject holder in transform.FindChildrenWithTag("spawnedObjects"))
        {
            holder.SetActive(false);
            Destroy(holder);
        }

        ArenaConfiguration newConfiguration;
        if (!_environmentManager.GetConfiguration(arenaID, out newConfiguration))
        {
            newConfiguration = new ArenaConfiguration(prefabs);
            _environmentManager.AddConfiguration(arenaID, newConfiguration);
        }
        _arenaConfiguration = newConfiguration;
        if (_arenaConfiguration.toUpdate)
        {
            _arenaConfiguration.SetGameObject(prefabs.GetList());
            _builder.Spawnables = _arenaConfiguration.spawnables;
            _arenaConfiguration.toUpdate = false;
            _agent.MaxStep = _arenaConfiguration.T * _agentDecisionInterval;
        }

        _builder.Build();
        _arenaConfiguration.lightsSwitch.Reset();
    }

    public void UpdateLigthStatus()
    {
        int stepCount = _agent.StepCount;
        bool newLight = _arenaConfiguration.lightsSwitch.LightStatus(stepCount, _agentDecisionInterval);
        if (newLight != _lightStatus)
        {
            _lightStatus = newLight;
            foreach (Fade fade in _fades)
            {
                fade.StartFade();
            }
        }
    }

    void FixedUpdate()
    {
        UpdateLigthStatus();
    }
}
