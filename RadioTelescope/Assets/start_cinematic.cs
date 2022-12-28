using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using UnityEngine.SceneManagement;

public class start_cinematic : MonoBehaviour
{
    private RaycastHit hitInfo;
    public GameObject start;
    public GameObject end;
    public UnityAxis1DAction rightTrigger;
    public bool isVR;

    private void Update()
    {
        var dir = start.transform.forward * 10000;

        if (isVR)
        {
            dir = start.transform.forward * -10000;
        }
        else
        {
            dir = start.transform.forward * 10000;
        }

        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            if (Input.GetMouseButtonDown(0) || rightTrigger.IsActivated)
            {
                if (hitInfo.transform.name == transform.name)
                {
                    if (isVR)
                    {
                        //vr scene
                        SceneManager.LoadScene(1);
                    }
                    else
                    {
                        SceneManager.LoadScene(2);
                    }
                }
            }
        }

    }
}
