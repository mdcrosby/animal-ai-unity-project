using System.Collections.Generic;
using System;
using UnityEngine;
using Lights;
using AAIOCommunicators;
using System.Text;

namespace ArenasParameters
{
    /// <summary>
    /// The list of prefabs that can be passed as items to spawn in the various arenas 
    /// </summary>
    [System.Serializable]
    public class ListOfPrefabs
    {
        public List<GameObject> allPrefabs;
        public List<GameObject> GetList()
        {
            return allPrefabs;
        }
    }

    /// <summary>
    /// We define a Spawnable item as a GameObject and a list of parameters to spawn it. These 
    /// include whether or not colors and sizes should be randomized, as well as lists of positions
    /// rotations and sizes the user can provide. Any of these parameters left empty by the user
    /// will be randomized at the time we spawn the associated GameObject
    /// </summary>
    public class Spawnable
    {
        public string name = "";
        public GameObject gameObject = null;
        public List<Vector3> positions = null;
        public List<float> rotations = null;
        public List<Vector3> sizes = null;
        public List<Vector3> colors = null;

        public Spawnable(GameObject obj)
        {
            name = obj.name;
            gameObject = obj;
            positions = new List<Vector3>();
            rotations = new List<float>();
            sizes = new List<Vector3>();
            colors = new List<Vector3>();
        }

        internal Spawnable(ItemToSpawnProto proto)
        {
            name = proto.Name;
            positions = new List<Vector3>();
            foreach (VectorProto v in proto.Positions)
            {
                positions.Add(new Vector3(v.X, v.Y, v.Z));
            }
            rotations = new List<float>(proto.Rotations);
            sizes = new List<Vector3>();
            foreach (VectorProto v in proto.Sizes)
            {
                sizes.Add(new Vector3(v.X, v.Y, v.Z));
            }
            colors = new List<Vector3>();
            foreach (VectorProto v in proto.Colors)
            {
                colors.Add(new Vector3(v.X, v.Y, v.Z));
            }
        }
        
        internal Spawnable(YAMLDefs.Item yamlItem)
        {
            name = yamlItem.name;
            positions = yamlItem.positions;
            rotations = yamlItem.rotations;
            sizes = yamlItem.sizes;
            colors = new List<Vector3>();
            foreach (YAMLDefs.RGB c in yamlItem.colors){
                colors.Add(new Vector3(c.r, c.g, c.b));
            }
        }

    }

    /// <summary>
    /// An ArenaConfiguration contains the list of items that can be spawned in the arena, the 
    /// maximum number of steps which can vary from one episode to the next (T) and whether all
    /// sizes and colors can be randomized
    /// </summary>
    public class ArenaConfiguration
    {
        public int T = 1000;
        public List<Spawnable> spawnables = new List<Spawnable>();
        public LightsSwitch lightsSwitch = new LightsSwitch();
        public bool toUpdate = false;
        public string protoString = "";// @TODO Check functionality with new yaml loaders

        public ArenaConfiguration()
        {
        }

        public ArenaConfiguration(ListOfPrefabs listPrefabs)
        {
            foreach (GameObject prefab in listPrefabs.allPrefabs)
            {
                spawnables.Add(new Spawnable(prefab));
            }
            T = 0;
            toUpdate = true;
        }

        internal ArenaConfiguration(ArenaConfigurationProto proto)
        {
            T = proto.T;
            spawnables = new List<Spawnable>();
            foreach (ItemToSpawnProto item in proto.Items)
            {
                spawnables.Add(new Spawnable(item));
            }
            List<int> blackouts = new List<int>();
            foreach (int blackout in proto.Blackouts)
            {
                blackouts.Add(blackout);
            }
            lightsSwitch = new LightsSwitch(T, blackouts);
            toUpdate = true;
            protoString = proto.ToString();
        }

        internal ArenaConfiguration(YAMLDefs.Arena yamlArena)
        {
            T = yamlArena.t;
            spawnables = new List<Spawnable>();
            foreach (YAMLDefs.Item item in yamlArena.items)
            {
                spawnables.Add(new Spawnable(item));
            }
            List<int> blackouts = yamlArena.blackouts;
            lightsSwitch = new LightsSwitch(T, blackouts);
            toUpdate = true;
            protoString = yamlArena.ToString();//This is holdover from dodgy proto check @TODO UDPATE
        }

        public void SetGameObject(List<GameObject> listObj)
        {
            foreach (Spawnable spawn in spawnables)
            {
                spawn.gameObject = listObj.Find(x => x.name == spawn.name);
            }
        }
    }

    /// <summary>
    /// ArenaConfigurations is a dictionary of configurations for each arena
    /// </summary>
    public class ArenasConfigurations
    {
        public Dictionary<int, ArenaConfiguration> configurations;
        public int numberOfArenas = 1;
        public int seed;

        public ArenasConfigurations()
        {
            configurations = new Dictionary<int, ArenaConfiguration>();
        }

        internal void Add(int k, ArenaConfigurationProto arenaConfigurationProto)
        {
            if (!configurations.ContainsKey(k))
            {
                configurations.Add(k, new ArenaConfiguration(arenaConfigurationProto));
            }
            else
            {
                if (arenaConfigurationProto.ToString() != configurations[k].protoString)
                {
                    configurations[k] = new ArenaConfiguration(arenaConfigurationProto);
                }
            }
        }
    
        internal void Add(int k, YAMLDefs.Arena yamlConfig)
        {
            if (!configurations.ContainsKey(k))
            {
                configurations.Add(k, new ArenaConfiguration(yamlConfig));
            }
            else
            {
                if (yamlConfig.ToString() != configurations[k].protoString)
                {
                    configurations[k] = new ArenaConfiguration(yamlConfig);
                }
            }
        }

        public void UpdateWithYAML(YAMLDefs.ArenaConfig yamlArenaConfig){
            if (yamlArenaConfig.arenas.ContainsKey(-1)){
                Debug.Log("We only have one arena key");
                for(int i = 0; i < numberOfArenas; i++){
                    Add(i, yamlArenaConfig.arenas[-1]);
                }
            }
            else{
                Debug.Log("We have multiple arena keys");
                foreach (KeyValuePair<int, YAMLDefs.Arena> arenaConfiguration in yamlArenaConfig.arenas){
                    Add(arenaConfiguration.Key, arenaConfiguration.Value);
                }
            }
        }

        public void UpdateWithConfigurationsReceived(object sender, ArenasParametersEventArgs arenasParametersEvent)
        {
            byte[] arenas = arenasParametersEvent.Proto;
            // ArenasConfigurationsProto arenasConfigurationsProto = ArenasConfigurationsProto.Parser.ParseFrom(arenas);
            // Debug.Log(arenasConfigurationsProto);
            var YAMLReader = new YAMLDefs.YAMLReader();
            // string bitString = BitConverter.ToString(arenas);
            string utfString = Encoding.UTF8.GetString(arenas, 0, arenas.Length);   
            Debug.Log("Converted Message to bitString");
            Debug.Log(utfString);
            var parsed = YAMLReader.deserializer.Deserialize<YAMLDefs.ArenaConfig>(utfString);
            UpdateWithYAML(parsed);
            
            // if (arenasConfigurationsProto.Arenas.ContainsKey(-1))
            // {
            //     // In case we have only a single configuration for all arenas we copy this configuration
            //     // to all arenas
            //     Debug.Log("We only have one arena key");
            //     for (int i = 0; i < numberOfArenas; i++)
            //     {
            //         Add(i, arenasConfigurationsProto.Arenas[-1]);
            //     }
            // }
            // else
            // {

            //     Debug.Log("We have multiple arena keys");
            //     foreach (KeyValuePair<int, ArenaConfigurationProto> arenaConfiguration in arenasConfigurationsProto.Arenas)
            //     {
            //         if (configurations.ContainsKey(arenaConfiguration.Key))
            //         {
            //             // we only update the arenas for which a new configuration was received
            //             Add(arenaConfiguration.Key, arenaConfiguration.Value);
            //         }
            //         else
            //         {
            //             // need to check what to do if we don t have the key already
            //             Add(arenaConfiguration.Key, arenaConfiguration.Value);
            //         }
            //     }
            // }
        }

        public void SetAllToUpdated()
        {
            foreach (KeyValuePair<int, ArenaConfiguration> configuration in configurations)
            {
                configuration.Value.toUpdate = false;
            }
        }

        public void Clear()
        {
            configurations.Clear();
        }
    }
}