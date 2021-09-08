using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YAMLDefs{
public class ArenaConfig{
    public IDictionary<int, Arena> arenas { get; set;}
}

public class Arena{
    public int t {get; set;} = 0;
    public List<Item> items {get; set;} = new List<Item>();
    public float pass_mark {get; set;} = 0;
    public List<int> blackouts {get; set;} = new List<int>();
}

public class Item{
    public string name {get; set;} = "";
    public List<Vector3> positions { get; set;} = new List<Vector3>();
    public List<float> rotations { get; set;} = new List<float>();
    public List<Vector3> sizes { get; set;} = new List<Vector3>();
    public List<RGB> colors { get; set;} = new List<RGB>();
}

//Not needed - can just use the Unity Vector3 implementation directly
// class Vector3{
//     public float x {get; set;}    
//     public float y {get; set;}
//     public float z {get; set;}
// }

public class RGB{
    public float r {get; set;} = 0;
    public float g {get; set;} = 0;
    public float b {get; set;} = 0;
}

}