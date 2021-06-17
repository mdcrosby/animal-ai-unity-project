using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSettings : MonoBehaviour
{
    public bool playerMode = true;
    public int numberOfArenas = 1;

    public bool useCamera = true;
    public int cameraResolution = 84;
    public bool grayscale = false;

    public bool useRayCasts = true;
    public int raysPerSide = 2;
    public int rayMaxDegrees = 60;

    public string configPath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
