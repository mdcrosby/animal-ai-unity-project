using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using ArenasParameters;
using UnityEngineExtensions;//for arena.transform.FindChildWithTag - @TODO check necessary/good practice.

/// Training scene must start automatically on launch by training process
/// Academy must reset the scene to a valid starting point for each episode
/// Training episode must have a definite end (MaxSteps or Agent.EndEpisode)
///
/// Scene requires:
///     GameObject with tag "agent" and component CameraSensorComponent
///     GameObject with tag MainCamera which views the full scene in Unity
///
public class AAI3EnvironmentManager : MonoBehaviour
{
    public GameObject arena; // A prefab for the training arena setup
    public int maximumResolution = 512;
    public int minimumResolution = 4;
    public int defaultResolution = 84;
    public int defaultRaysPerSide = 2;
    public int defaultRayMaxDegrees = 60;
    public GameObject playerControls; //Just for camera and reset controls ...@TODO Don't think this should be a GameObject in the scene and linked there (carried over from v2.0)

    [HideInInspector]
    public bool playerMode;

    private ArenasConfigurations _arenasConfigurations;
    private TrainingArena[] _instantiatedArenas;
    private ArenasParametersSideChannel _arenasParametersSideChannel;


    public void Awake()
    {
        //This is used to initialise the ArenaParametersSideChannel wich is a subclass of MLAgents SideChannel
        _arenasParametersSideChannel = new ArenasParametersSideChannel();
        _arenasConfigurations = new ArenasConfigurations();

        _arenasParametersSideChannel.NewArenasParametersReceived += _arenasConfigurations.UpdateWithConfigurationsReceived;

        SideChannelManager.RegisterSideChannel(_arenasParametersSideChannel);
        
        //Get all commandline arguments and update starting parameters
        Dictionary<string, int> environmentParameters = RetrieveEnvironmentParameters();

        int paramValue;
        playerMode = (environmentParameters.TryGetValue("playerMode", out paramValue) ? paramValue : 0) > 0;
        int numberOfArenas = environmentParameters.TryGetValue("numberOfArenas", out paramValue) ? paramValue : 1;
        int resolution = environmentParameters.TryGetValue("resolution", out paramValue) ? paramValue : defaultResolution;
        bool grayscale = (environmentParameters.TryGetValue("grayscale", out paramValue) ? paramValue : 0) > 0;
        int rays_per_side = environmentParameters.TryGetValue("rays_per_side", out paramValue) ? paramValue : defaultRaysPerSide;
        int ray_max_degrees = environmentParameters.TryGetValue("ray_max_degrees", out paramValue) ? paramValue : defaultRayMaxDegrees;
        bool useRayCasts = (environmentParameters.TryGetValue("useRayCasts", out paramValue) ? paramValue : 0) > 0;

        if (Application.isEditor)//Default settings for tests in Editor @TODO replace this with custom config settings in editor window for easier testing.
        {
            numberOfArenas = 1;
            playerMode = true;
            resolution = 500;
            grayscale = false;
        }

        resolution = Math.Max(minimumResolution, Math.Min(maximumResolution, resolution));
        numberOfArenas = playerMode ? 1 : numberOfArenas;//Only ever use 1 arena in playerMode
        _arenasConfigurations.numberOfArenas = numberOfArenas;

        _instantiatedArenas = new TrainingArena[numberOfArenas];
        InstantiateArenas(numberOfArenas);//Instantiate every new arena with agent and objects. Agents are currently deactivated until we set the sensors.
        ConfigureIfPlayer(playerMode);
        
        //Destroy the sensor-type that is not being used.
        if(!useRayCasts){
            foreach(Agent a in FindObjectsOfType<Agent>(true)){
                DestroyImmediate(a.GetComponentInChildren<RayPerceptionSensorComponent3D>());        
            }
        }
        else{
            foreach(Agent a in FindObjectsOfType<Agent>(true)){
                DestroyImmediate(a.GetComponentInChildren<CameraSensorComponent>());        
            }
        }

        //Enable all the agents now that their sensors have been set.
        foreach (TrainingArena arena in _instantiatedArenas){
            arena._agent.transform.Find("Agent").gameObject.SetActive(true);
        }   

        // if(useRayCasts){
        //     Debug.Log("using Ray Casts");
        //     ChangeRayCasts(rays_per_side, ray_max_degrees);//Number per side
        //     CameraSensorComponent cameraSensor = arena.transform.Find("AAI3Agent").Find("Agent").GetComponent<CameraSensorComponent>();//@TODO update
        //     cameraSensor.enabled = false;//@TODO even disabled seems to still be used as part of the observation space.
        //     Debug.Log(cameraSensor.isActiveAndEnabled);
        // }
        // else{
        Debug.Log("using camera with res " + resolution);   
        // ChangeResolution(resolution, resolution, grayscale);
        // RayPerceptionSensorComponent3D raySensor = _arenas[0]._agent.GetComponentInChildren<RayPerceptionSensorComponent3D>();
        // RayPerceptionSensorComponent3D raySensor = _arenas[0].transform.Find("AAI3Agent").Find("Agent").GetComponent<RayPerceptionSensorComponent3D>();//@TODO update
        // Destroy(raySensor);//Note disabling rayperception does not work.
        // CameraSensorComponent cameraSensor = _arenas[0]._agent.GetComponentInChildren<CameraSensorComponent>();
        // Destroy(cameraSensor);
        // }
        Debug.Log("Performed first environment reset:\nPlayerMode = " + playerMode + "\nNo. Arenas: " + numberOfArenas + "\nResolution: " + resolution + "\ngrayscale: " + grayscale);
    }

    private void ChangeRayCasts(int no_raycasts, int max_degrees)
    {
        RayPerceptionSensorComponent3D raySensor = _instantiatedArenas[0].agent.GetComponent<RayPerceptionSensorComponent3D>();//@TODO update
        raySensor.RaysPerDirection = no_raycasts;
        raySensor.MaxRayDegrees = max_degrees;
    }

    private void ChangeResolution(int cameraWidth, int cameraHeight, bool grayscale)
    {
        CameraSensorComponent cameraSensor = _instantiatedArenas[0].agent.GetComponent<CameraSensorComponent>();//@TODO update
        cameraSensor.Width = cameraWidth;
        cameraSensor.Height = cameraHeight;
        cameraSensor.Grayscale = grayscale;
    }

    ///<summary>
    /// We organize the arenas in a grid and position the main camera at the center, high enough
    /// to see all arenas at once.
    ///</summary>
    private void InstantiateArenas(int numberOfArenas)
    {
        Vector3 boundingBox = arena.GetBoundsWithChildren().extents;
        float width = 2 * boundingBox.x + 5f;
        float height = 2 * boundingBox.z + 5f;
        int n = (int)Math.Round(Math.Sqrt(numberOfArenas));

        for (int i = 0; i < numberOfArenas; i++)
        {
            float x = (i % n) * width;
            float y = (i / n) * height;
            GameObject arenaInst = Instantiate(arena, new Vector3(x, 0f, y), Quaternion.identity);
            _instantiatedArenas[i] = arenaInst.GetComponent<TrainingArena>();
            _instantiatedArenas[i].arenaID = i;
        }

        GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition =
            new Vector3(n * width / 2, 50 * (float)n, (float)n * height / 2);
    }

    private void ConfigureIfPlayer(bool playerMode)
    {
        Debug.Log("Setting playerMode: " + playerMode);
        GameObject.FindGameObjectWithTag("score").SetActive(playerMode);//@TODO not implemented
        if (playerMode)
        {
            playerControls.SetActive(true);
            //The following does nothing under normal execution - but when loading the built version
            //with the load_config_and_play script it sets the BehaviorType back to Heursitic 
            //from default as loading this autotamically attaches Academy for training (since mlagents 0.16.0)
            //@TODO must be a better way to do this.
            // _arenas[0].agent.GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;//@TODO update
        }
    }

    ///<summary>
    ///Parses command line arguments for:
    ///--playerMode
    ///--receiveConfiguration
    ///--numberOfArenas
    ///--resolution
    ///--grayscale
    ///</summary>
    private Dictionary<string, int> RetrieveEnvironmentParameters()
    {
        Dictionary<string, int> environmentParameters = new Dictionary<string, int>();

        string[] args = System.Environment.GetCommandLineArgs();
        Debug.Log("Command Line Args: " + args);
        // if(Application.isEditor){//Used for debugging in Editor
        //     environmentParameters.Add("resolution", 36);
        //     environmentParameters.Add("useRayCasts", 1);
        // }
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--playerMode":
                    int playerMode = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 1;
                    environmentParameters.Add("playerMode", playerMode);
                    break;
                case "--receiveConfiguration":
                    environmentParameters.Add("receiveConfiguration", 0);
                    break;
                case "--numberOfArenas":
                    int nArenas = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 1;
                    environmentParameters.Add("numberOfArenas", nArenas);
                    break;
                case "--resolution":
                    int camW = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : defaultResolution;
                    environmentParameters.Add("resolution", camW);
                    break;
                case "--grayscale":
                    environmentParameters.Add("grayscale", 1);
                    break;
                case "--useRayCasts":
                    environmentParameters.Add("useRayCasts", 1);
                    break;
            }
        }
        Debug.Log("Environment Parameters: " + string.Join(Environment.NewLine, environmentParameters));
        return environmentParameters;
    }

    public bool GetConfiguration(int arenaID, out ArenaConfiguration arenaConfiguration)
    {
        return _arenasConfigurations.configurations.TryGetValue(arenaID, out arenaConfiguration);
    }

    public void AddConfiguration(int arenaID, ArenaConfiguration arenaConfiguration)
    {
        _arenasConfigurations.configurations.Add(arenaID, arenaConfiguration);
    }


    public void OnDestroy()
    {
        if (Academy.IsInitialized)
        {
            SideChannelManager.UnregisterSideChannel(_arenasParametersSideChannel);
        }
    }

}
