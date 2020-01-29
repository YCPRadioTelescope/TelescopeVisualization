using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeController : MonoBehaviour
{
    public GameObject xRotation;
    public GameObject yRotation;

    /**
     * Takes in a speed and moves the telescope
     * Returns the current rotation
     * RotateX returns X rotation
     * RotateY returns Y roataion
     */
    public float RotateZ(float speed)
    {
        xRotation.transform.Rotate(0,0,-speed);

        return xRotation.transform.eulerAngles.x;
        
    }
    
    public float RotateY(float speed)
    {
        yRotation.transform.Rotate(0,speed,0);

        return yRotation.transform.eulerAngles.y;

    }
}