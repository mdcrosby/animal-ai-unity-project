using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using ArenasParameters;
using UnityEngineExtensions;//for arena.transform.FindChildWithTag - @TODO check necessary/good practice.

///
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

    private bool _firstReset = true;
    private ArenasConfigurations _arenasConfigurations;
    private TrainingArena[] _arenas;
    private ArenasParametersSideChannel _arenasParametersSideChannel;


    public void Awake()
    {
        //This is used to initialise the ArenaParametersSideChannel wich is a subclass of MLAgents SideChannel
        _arenasParametersSideChannel = new ArenasParametersSideChannel();
        _arenasConfigurations = new ArenasConfigurations();

        _arenasParametersSideChannel.NewArenasParametersReceived += _arenasConfigurations.UpdateWithConfigurationsReceived;

        SideChannelsManager.RegisterSideChannel(_arenasParametersSideChannel);
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;//When ML-Agents Academy resets environment append our method.
    }

    public void EnvironmentReset()
    {
        Debug.Log("Environment Reset");
        if (_firstReset)//On the first reset, set all the parameters that will last throughout training
        {
            Dictionary<string, int> environmentParameters = RetrieveEnvironmentParameters();

            int paramValue;
            playerMode = (environmentParameters.TryGetValue("playerMode", out paramValue) ? paramValue : 1) > 0;
            int numberOfArenas = environmentParameters.TryGetValue("numberOfArenas", out paramValue) ? paramValue : 1;
            int resolution = environmentParameters.TryGetValue("resolution", out paramValue) ? paramValue : defaultResolution;
            bool grayscale = (environmentParameters.TryGetValue("grayscale", out paramValue) ? paramValue : 0) > 0;
            int rays_per_side = environmentParameters.TryGetValue("rays_per_side", out paramValue) ? paramValue : defaultRaysPerSide;
            int ray_max_degrees = environmentParameters.TryGetValue("ray_max_degrees", out paramValue) ? paramValue : defaultRayMaxDegrees;


            if (Application.isEditor)//Default settings for tests in Editor
            {
                numberOfArenas = 1;
                playerMode = true;
                resolution = 512;
                grayscale = false;
            }

            resolution = Math.Max(minimumResolution, Math.Min(maximumResolution, resolution));
            numberOfArenas = playerMode ? 1 : numberOfArenas;
            _arenasConfigurations.numberOfArenas = numberOfArenas;

            _arenas = new TrainingArena[numberOfArenas];//A new training arena loads all objects.
            ChangeResolution(resolution, resolution, grayscale);
            //ChangeRayCasts(rays_per_side, ray_max_degrees);//Number per side
            InstantiateArenas(numberOfArenas);
            ConfigureIfPlayer(playerMode);

            _firstReset = false;

            Debug.Log("Performed first environment reset:\nPlayerMode = " + playerMode + "\nNo. Arenas: " + numberOfArenas + "\nResolution: " + resolution + "\ngrayscale: " + grayscale);
        }
    }

    private void ChangeRayCasts(int no_raycasts, int max_degrees)
    {
        RayPerceptionSensorComponent3D raySensor = arena.transform.Find("AAI3Agent").Find("Agent").GetComponent<RayPerceptionSensorComponent3D>();//@TODO update
        raySensor.RaysPerDirection = no_raycasts;
        raySensor.MaxRayDegrees = max_degrees;
    }

    private void ChangeResolution(int cameraWidth, int cameraHeight, bool grayscale)
    {
        CameraSensorComponent cameraSensor = arena.transform.Find("AAI3Agent").Find("Agent").GetComponent<CameraSensorComponent>();//@TODO update
        //agent.transform.FindChildWithTag("agent").GetComponent<CameraSensorComponent>();//@Todo update
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
            _arenas[i] = arenaInst.GetComponent<TrainingArena>();
            _arenas[i].arenaID = i;
        }

        GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition =
            new Vector3(n * width / 2, 50 * (float)n, (float)n * height / 2);
    }

    private void ConfigureIfPlayer(bool playerMode)
    {
        GameObject.FindGameObjectWithTag("score").SetActive(playerMode);//@TODO not implemented
        if (playerMode)
        {
            playerControls.SetActive(true);
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
            }
        }
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
            SideChannelsManager.UnregisterSideChannel(_arenasParametersSideChannel);
        }
    }

}
