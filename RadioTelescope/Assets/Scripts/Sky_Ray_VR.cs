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

    bool ishit = false;

    private void Update()
    {
        var dir = start.transform.forward * -10000;


        // If this script has vrActive set to true, a line is drawn between the start
        // and end positions.
        if (vrActive)
        {
            dir *= -1;
            lr.SetPosition(0, start.transform.position);
            lr.SetPosition(1, end.transform.position);
        }

        // Cast a ray between the start object and end object. If a part of the telescope
        // is hit, hitInfo is changed.
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            if (hitInfo.transform.tag == "sky")
            {
                ishit = true;
                currObj = hitInfo.transform.gameObject;
                currObj.GetComponent<Star_Object>().is_hovered = true;
                if (rightTrigger.Value > 0.2f)
                {
                    StarCanvus.active = true;
                }
            }
            else
            {
                if (currObj != null)
                {
                    ishit = false;
                    currObj.GetComponent<Star_Object>().is_hovered = false;
                }
                if (rightTrigger.Value > 0.2f)
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
            if (rightTrigger.Value > 0.2f)
            {
                StarCanvus.active = false;
            }
        }
    }
}
