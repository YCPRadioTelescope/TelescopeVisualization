using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

public class TriggerValue : MonoBehaviour
{
    public UnityAxis1DAction rightTrigger;
    public GameObject rightLine;

    public GameObject controller;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        Debug.Log(rightTrigger.Value);
        if (rightTrigger.Value > 0.2f)
        {
            rightLine.SetActive(true);
        }
        else
        {
            rightLine.SetActive(false);
        }
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
       
        
    }
}
