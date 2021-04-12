using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Class dealing with agent actions.
///Requires agent is rigidbody for push/pull interactions.
///Currently almagamation of previous AAI and UnityFPS demo code.
///@TODO Currently special way methods for dealing with gravity or off ground states. - need to add before 0.0.1
///@TODO Revisit after 0.0.1 completed. Check methods from UnityFPS
///</summary>
public class AgentInput : MonoBehaviour
{
    

    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float maxSpeedOnGround = 20f;    

    [Tooltip("Slowdown factor")]
    public float slowDownFactor = .3f;
    
    [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
    public float movementSharpnessOnGround = 15;

    private Rigidbody _rigidBody;
    public Vector3 characterVelocity { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        bool wDown = Input.GetKey(KeyCode.W);
        bool sDown = Input.GetKey(KeyCode.S);
        bool dDown = Input.GetKey(KeyCode.D);
        bool aDown = Input.GetKey(KeyCode.A);
        
        if (wDown){
            Vector3 worldspaceMoveInput = transform.TransformVector(new Vector3(1f, 0f, 0f));
            Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround;
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);
        }
        if (sDown){
            Vector3 worldspaceMoveInput = transform.TransformVector(new Vector3(-1f, 0f, 0f));
            Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround;
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);
        }
        if(dDown){
            transform.Rotate(new Vector3(0f, .5f, 0f), Space.Self);
        }
        if(aDown){
            transform.Rotate(new Vector3(0f, -.5f, 0f), Space.Self);
        }
        else{
            characterVelocity = Vector3.Lerp(characterVelocity, new Vector3(0f,0f,0f), movementSharpnessOnGround * Time.deltaTime * slowDownFactor);
        }
        _rigidBody.velocity = characterVelocity;//*Time.deltaTime*10f);
    }

}
