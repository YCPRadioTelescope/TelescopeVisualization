using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

// A script for moving the VR joystick according to the player grabbing it.
// This drives the MoveTelescopeVR script.
public class MoveJoystick : MonoBehaviour
{
    /*
     * how to setup this script
     * joystick is the joystick that moves the telescope
     * speed is how fast it returns to 0
     * target is where the final rotation should be.
     * In the radio telescope scene,
     * navigate to OptionsMenu/ControlStation/Top/JointContainer/Joystick/Interactable.Primary_Grab.Secondary_swap/InteractionLogic/Interactable.GrabLogic/Interactable.GrabEvent.Stack/InputRecievers/
     * Select InputGameobjectGrab
     * In the Inspector, in the Game Object Event Proxy Emitter, Add another game object to the Emitted script so that it looks like this... 
     */
    public GameObject joystick;
    public float speed;
    public Transform target;
    
    // Update is called once per frame
    void Update()
    {
        var step = speed * Time.deltaTime;
        joystick.transform.rotation = Quaternion.RotateTowards(joystick.transform.rotation, target.rotation, step);
        Debug.Log("Ungrabbed");
    }

    void End()
    {    
        Debug.Log("Grabbed");
    }
}
