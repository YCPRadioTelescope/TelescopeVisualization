﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky_ray : MonoBehaviour
{

    private RaycastHit hitInfo;
    private LineRenderer lr;
    public bool vrActive;

    public GameObject start;
    public GameObject end;
    public GameObject currObj;

    public GameObject StarCanvus;

    bool ishit = false;

    private void Update()
    { 
        var dir = start.transform.forward * 10000;

        // Cast a ray between the start object and end object. If a part of the telescope
        // is hit, hitInfo is changed.
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {

            if (hitInfo.transform.tag == "sky")
            {
                ishit = true;
                currObj = hitInfo.transform.gameObject;
                currObj.GetComponent<Star_Object>().is_hovered = true;
                if (Input.GetMouseButtonDown(0))
                {
                    StarCanvus.active = true;
                }
            }
            else
            {
                if(currObj != null)
                {
                    ishit = false;
                    currObj.GetComponent<Star_Object>().is_hovered = false;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    StarCanvus.active = false;
                }
            }
        }
        else
        {
            if (currObj != null)
            {
                ishit = false;
                currObj.GetComponent<Star_Object>().is_hovered = false;
            }
            if (Input.GetMouseButtonDown(0))
            {
                StarCanvus.active = false;
            }
        }
    }
}