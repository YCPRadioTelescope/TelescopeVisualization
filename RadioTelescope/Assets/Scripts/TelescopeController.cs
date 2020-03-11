using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeController : MonoBehaviour
{
    public GameObject xRotation;
    public GameObject yRotation;
    private float rotateX;

    /**
     * Takes in a speed and moves the telescope
     * Returns the current rotation
     * RotateX returns X rotation
     * RotateY returns Y roataion
     */
    public float RotateZ(float speed)
    {
        
        //xRotation.transform.Rotate(0,0,-speed);

        rotateX = xRotation.transform.eulerAngles.z;
        rotateX += speed;
        rotateX = Mathf.Clamp(rotateX, 0, 100);

        xRotation.transform.localRotation = Quaternion.Euler(0, 0, rotateX);

        Debug.Log("rotating X on telescope");

        return xRotation.transform.eulerAngles.z;
        
    }
    
    public float RotateY(float speed)
    {
        yRotation.transform.Rotate(0,speed,0);

        return yRotation.transform.eulerAngles.y;

    }
}