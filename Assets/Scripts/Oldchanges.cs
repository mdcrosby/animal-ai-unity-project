using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AAIOCommunicators;
using Lights;

namespace ArenaParameters
{
    /// <summary>
    /// The list of prefabs that can be passed as items to spawn in the various arenas instantiated 
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

    }

    /// <summary>
    /// An ArenaConfiguration contains the list of items taht can be spawned in the arena, the 
    /// maximum number of steps which can vary from one episode to the next (T) and whether all
    /// sizes and colors can be randomized
    /// </summary>
    public class ArenaConfiguration
    {
        public int T = 1000;
        public List<Spawnable> spawnables = new List<Spawnable>();
        public LightsSwitch lightsSwitch = new LightsSwitch();
        public bool toUpdate = false;
        public string protoString = "";

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

        public void SetGameObject(List<GameObject> listObj)
        {
            foreach (Spawnable spawn in spawnables)
            {
                spawn.gameObject = listObj.Find(x => x.name == spawn.name);
            }
        }
    }

}