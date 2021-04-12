using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goals : MonoBehaviour
{
    Rigidbody _rigidBody;
    Collider _collider;
    public float rotatingSpeed = 360f;
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        //Make it rotate
        // transform.Rotate(Vector3.up, rotatingSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision other)
    {
        //Destroy only if the playing agent entered the collision zone.
        AgentInput triggerObject = other.collider.GetComponent<AgentInput>();
        // Debug.Log("DAFA");
        if(triggerObject != null){

            Destroy(gameObject);
        }
    }

}
