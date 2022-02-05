using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE_CamController : MonoBehaviour
{
    private Camera _cameraTopDown;
    private Camera _camera1stPerson;
    private bool _topDown = true;
    public bool usingTopDownCam() { return _topDown; }
    public int getActiveCamID() { return _topDown ? 1 : 0; }
    public int getCamID(bool isTopDown) { return isTopDown ? 1 : 0; }
    public void swapActiveCam() {
        _topDown = !_topDown;
        _cameraTopDown.enabled = usingTopDownCam();
        _camera1stPerson.enabled = !usingTopDownCam();
    }

    private float fwd_const = 0.05f;     // z-axis movement const
    private float upd_const = 0.03125f;  // y-axis movement const
    private float rot_const = 0.5f;  // xz-plane rotate const

    void Start()
    {
        _camera1stPerson = this.GetComponentsInChildren<Camera>()[getCamID(!_topDown)]; // index==0
        _cameraTopDown = this.GetComponentsInChildren<Camera>()[getCamID(_topDown)]; // index==1

        _cameraTopDown.enabled = true;
        _camera1stPerson.enabled = false;
        _topDown = true;
    }

    void Update()
    {
        bool toggleCam = Input.GetKeyDown(KeyCode.C);
        bool moveForwards = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        bool moveBackwards = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        bool spinLeft = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool spinRight = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        bool ascend = Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.Q);
        bool descend = Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.E);

        if (toggleCam)
        {
            swapActiveCam();
        }
        else if (!usingTopDownCam())
        {
            //Debug.Log(_camera1stPerson.transform.forward);
            _camera1stPerson.transform.Translate(_camera1stPerson.transform.forward * ((moveForwards? fwd_const:0) - (moveBackwards? fwd_const:0)), Space.World);
            _camera1stPerson.transform.Translate(_camera1stPerson.transform.up * ((ascend? upd_const:0) - (descend? upd_const:0)));
            _camera1stPerson.transform.Rotate(_camera1stPerson.transform.up, ((spinRight? rot_const:0) - (spinLeft? rot_const:0)));
        }
    }

}
