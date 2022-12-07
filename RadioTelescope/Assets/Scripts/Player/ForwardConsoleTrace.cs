using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardConsoleTrace : MonoBehaviour
{
    // Start and end are two invisible cubes, one on top of the player
    // and one off in the distance, between which the ray is cast.
    public GameObject start;
    public GameObject end;

    // The object that the raycast hit.
    private RaycastHit hitInfo;
    // Update is called once per frame
    void Update()
    {
        var dir = start.transform.forward * 10000;
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {

            if (hitInfo.transform.GetComponent<StarFastForward>())
            {

                if (Input.GetMouseButtonUp(0))
                {
                    hitInfo.transform.GetComponent<StarFastForward>().dateUnitPicker();
                }
            }

        }
    }
}


