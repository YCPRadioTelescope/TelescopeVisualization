﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDateRayCast : MonoBehaviour
{
    // Start and end are two invisible cubes, one on top of the player
    // and one off in the distance, between which the ray is cast.
    public GameObject start;
    public GameObject end;
    public bool isVr;
    public bool timeout = true;
    public VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input.UnityAxis1DAction rightTrigger;

    // The object that the raycast hit.
    private RaycastHit hitInfo;
    // Update is called once per frame]

    void Update()
    {

        var dir = start.transform.forward * 10000;
        if (isVr) { dir = dir * -1; }
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {

            if (hitInfo.transform.GetComponent<ChangeDateUnitUpward>())
            {
                Debug.Log("the object has the desried script");
                if ((Input.GetMouseButtonUp(0) || rightTrigger.IsActivated) && timeout)
                {
                    Debug.Log("updateRaycast works!!");
                    hitInfo.transform.GetComponent<ChangeDateUnitUpward>().increaseDateUnit();
                    timeout = false;
                    StartCoroutine(delay());
                }
            }

            IEnumerator delay()
            {
                yield return new WaitForSeconds(.2f);
                timeout = true;
            }
        }
    }
}