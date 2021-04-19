using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject followObj;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = followObj.transform.position;
        transform.rotation = followObj.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = followObj.transform.position;
        transform.rotation = followObj.transform.rotation;
    }
}
