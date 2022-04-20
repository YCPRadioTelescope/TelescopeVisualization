using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VROverride_VR : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.XR.XRSettings.enabled = true;
    }
}
