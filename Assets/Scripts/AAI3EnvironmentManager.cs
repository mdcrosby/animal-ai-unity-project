using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Policies;
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
    public EditorSettings editorSettings;
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
        bool useCamera = (environmentParameters.TryGetValue("useCamera", out paramValue) ? paramValue: 0) > 0;
        int resolution = environmentParameters.TryGetValue("resolution", out paramValue) ? paramValue : defaultResolution;
        bool grayscale = (environmentParameters.TryGetValue("grayscale", out paramValue) ? paramValue : 0) > 0;
        bool useRayCasts = (environmentParameters.TryGetValue("useRayCasts", out paramValue) ? paramValue : 0) > 0;
        int raysPerSide = environmentParameters.TryGetValue("raysPerSide", out paramValue) ? paramValue : defaultRaysPerSide;
        int rayMaxDegrees = environmentParameters.TryGetValue("rayMaxDegrees", out paramValue) ? paramValue : defaultRayMaxDegrees;

        if (Application.isEditor)//Default settings for tests in Editor @TODO replace this with custom config settings in editor window for easier testing.
        {
            playerMode = editorSettings.playerMode;
            numberOfArenas = editorSettings.numberOfArenas;
         
            useCamera = editorSettings.useCamera;
            resolution = editorSettings.cameraResolution;
            grayscale = editorSettings.grayscale;

            useRayCasts = editorSettings.useRayCasts;
            raysPerSide = editorSettings.raysPerSide;

            string configPath = editorSettings.configPath;        
        }

        resolution = Math.Max(minimumResolution, Math.Min(maximumResolution, resolution));
        _arenasConfigurations.numberOfArenas = numberOfArenas;

        _instantiatedArenas = new TrainingArena[numberOfArenas];
        InstantiateArenas(numberOfArenas);//Instantiate every new arena with agent and objects. Agents are currently deactivated until we set the sensors.
        ConfigureIfPlayer(playerMode);
        
        //Destroy the sensors that aren't being used and update the values of those that are
        //HACK - mlagents automatically registers cameras when the agent script is initialised so have to:
        //  1) use FindObjectsOfType as this returns deactivated objects
        //  2) start with agent deactivated and then set active after editing sensors
        //  3) use DestroyImmediate so that it is destroyed before agent is initialised
        foreach(Agent a in FindObjectsOfType<Agent>(true)){
            if(!useRayCasts){
                DestroyImmediate(a.GetComponentInChildren<RayPerceptionSensorComponent3D>());
            }
            else{
                ChangeRayCasts(a.GetComponentInChildren<RayPerceptionSensorComponent3D>(), raysPerSide, rayMaxDegrees);
            }
            if(!useCamera){
                DestroyImmediate(a.GetComponentInChildren<CameraSensorComponent>());
            }
            else{
                ChangeResolution(a.GetComponentInChildren<CameraSensorComponent>(), resolution, resolution, grayscale);
            }
            if(playerMode){
                //The following does nothing under normal execution - but when loading the built version
                //with the play script it sets the BehaviorType back to Heursitic 
                //from default as loading this autotamically attaches Academy for training (since mlagents 0.16.0)
                //@TODO must be a better way to do this.
                a.GetComponentInChildren<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
            }
        }

        //Enable all the agents now that their sensors have been set.
        foreach (TrainingArena arena in _instantiatedArenas){
            arena._agent.gameObject.SetActive(true);
        }

        Debug.Log("Environment loaded with options:" + 
            "\nPlayerMode: " + playerMode + 
            "\nNo. Arenas: " + numberOfArenas + 
            "\nuseCamera: " + useCamera + 
            "\nResolution: " + resolution + 
            "\ngrayscale: " + grayscale +
            "\nuseRayCasts: " + useRayCasts +
            "\nraysPerSide: " + raysPerSide +
            "\nrayMaxDegrees: " + rayMaxDegrees            
            );
    }

    private void ChangeRayCasts(RayPerceptionSensorComponent3D raySensor, int no_raycasts, int max_degrees)
    {
        raySensor.RaysPerDirection = no_raycasts;
        raySensor.MaxRayDegrees = max_degrees;
    }

    private void ChangeResolution(CameraSensorComponent cameraSensor, int cameraWidth, int cameraHeight, bool grayscale)
    {
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
        }
    }

    ///<summary>
    ///Parses command line arguments for:
    ///--playerMode: if true then can change camera angles and have control of agent
    ///--numberOfArenas - the number of Arenas to spawn (always set to 1 in playerMode)
    ///--useCamera - if true adds camera obseravations
    ///--resolution - the resolution for camera observations (default 84, min4, max 512)
    ///--grayscale 
    ///--useRayCasts - if true adds raycast observations
    ///--raysPerSide - sets the number of rays per side (total = 2n+1)
    ///--rayAngle - sets the maximum angle of the rays (defaults to 60)
    /// ///</summary>
    private Dictionary<string, int> RetrieveEnvironmentParameters()
    {
        Dictionary<string, int> environmentParameters = new Dictionary<string, int>();

        string[] args = System.Environment.GetCommandLineArgs();
        // Debug.Log("Command Line Args: " + args);
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--playerMode":
                    int playerMode = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 1;
                    environmentParameters.Add("playerMode", playerMode);
                    break;
                case "--numberOfArenas":
                    int nArenas = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 1;
                    environmentParameters.Add("numberOfArenas", nArenas);
                    break;
                case "--useCamera":
                    environmentParameters.Add("useCamera", 1);
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
                case "--raysPerSide":
                    int rps = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 2;
                    environmentParameters.Add("raysPerSide", rps);
                    break;
                case "--rayMaxDegrees":
                    int rmd = (i < args.Length - 1) ? Int32.Parse(args[i + 1]) : 60;
                    environmentParameters.Add("rayMaxDegrees", rmd);
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
