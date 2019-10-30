using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControlStationMoveDrive : MonoBehaviour
{
    public GameObject joystick;
    public UnityEvent ungrab;

    void Start()
    {
        if (ungrab != null)
        {
            ungrab.AddListener(MoveToCenter);
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {

    }

    void MoveToCenter()
    {
        Debug.Log("Ungrabbed");
    }
    
    
}
