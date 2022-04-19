using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

public class Sky_Ray_VR : MonoBehaviour
{

    private RaycastHit hitInfo;
    private LineRenderer lr;
    public bool vrActive;

    public GameObject start;
    public GameObject end;
    public GameObject currObj;
    public UnityAxis1DAction rightTrigger;

    public GameObject StarCanvus;

    bool activated = false;

    bool ishit = false;


    private void Update()
    {
        if (rightTrigger.IsActivated)
        {
            if (activated == false)
            {
                TriggerPressed();
            }
            activated = true;
        }

        if (activated == true)
        {
            if (!rightTrigger.IsActivated)
            {
                activated = false;
            }
        }

        var dir = start.transform.forward * -10000;


        // Cast a ray between the start object and end object. If a part of the telescope
        // is hit, hitInfo is changed.
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            if (hitInfo.transform.tag == "sky")
            {
                ishit = true;
                currObj = hitInfo.transform.gameObject;
                currObj.GetComponent<Star_Object>().is_hovered = true;
            }
            else
            {
                if (currObj != null)
                {
                    ishit = false;
                    currObj.GetComponent<Star_Object>().is_hovered = false;
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
        }
    }
    private void TriggerPressed()
    {
        var dir = start.transform.forward * -10000;


        // Cast a ray between the start object and end object. If a part of the telescope
        // is hit, hitInfo is changed.
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            if (hitInfo.transform.tag == "sky")
            {
                StarCanvus.active = true;
            }
            else
            {
                StarCanvus.active = false;
            }
        }
        else
        {
            StarCanvus.active = false;
        }
    }
}
