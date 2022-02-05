using ArenasParameters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorManager : MonoBehaviour
{
    public GameObject arena;
    public GameObject uiCanvas;
    // for orbit camera
    public GameObject camController;
    // most recently imported/exported config file
    public string savedConfigFile = "";

    private ArenasConfigurations _arenasConfigurations;
    private TrainingArena _instantiatedArena;

    // Start is called before the first frame update
    void Awake()
    {
        _arenasConfigurations = new ArenasConfigurations();
        InstantiateArena();
        AlterArenaForLevelEditor();
        // AlterX() must be called in Awake() so changes are made before first frame
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AlterArenaForLevelEditor()
    {
        if (_instantiatedArena == null) { throw new Exception("No arena instantiated... can't alter for level editing!!"); }
        else {
            // find and destroy the training agent that comes with the Arena packed scene
            Destroy(_instantiatedArena.transform.Find("AAI3Agent").gameObject);
        }
    }

    private void InstantiateArena()
    {
        GameObject arenaInst = Instantiate(arena, new Vector3(0f, 0f, 0f), Quaternion.identity);
        _instantiatedArena = arenaInst.GetComponent<TrainingArena>();
        _instantiatedArena.arenaID = 0;
    }
}
