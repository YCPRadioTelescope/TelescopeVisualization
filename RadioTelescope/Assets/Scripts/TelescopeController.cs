using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class TelescopeController : MonoBehaviour
{
    public GameObject xRotation;
    public GameObject yRotation;
    public float targetZ = 0.0f;
    public float targetY = 0.0f;
    public float currentZ = 0.0f;
    public float currentY = 0.0f;
    public float speed = 1.0f;

    /**
     * Takes in a speed and moves the telescope
     * Returns the current rotation
     * RotateX returns X rotation
     * RotateY returns Y rotation
     */
    public void Start()
    {
        //SetY(359.0f);
        //SetZ(-11.0f);
    }

    public void Update()
    {
        if (targetY < 0)
        {
            targetY += 360.0f;
        }
        if (targetZ < 0)
        {
            targetZ += 360.0f;
        }
        Debug.Log("ZTarget: " + targetZ);
        Debug.Log("YTarget: " + targetY);
        //Debug.Log(yRotation.transform.eulerAngles.z);
        if ((int)yRotation.transform.localEulerAngles.y != (int)targetY)
        {
            if (targetY >= yRotation.transform.localEulerAngles.y)
            {
                
                currentY = RotateY(speed);
                //Debug.Log("Current Y: " + currentY + ", Target Y: " + targetY);
            }
            else
            {

                currentY = RotateY(-speed);
                //Debug.Log("Current Y: " + currentY + ", Target Y: " + targetY);
            }

        }

        if ((int) xRotation.transform.localEulerAngles.z != (int) targetZ)
        {
            if (targetZ <= 105.0f && targetZ >= 0)
            {
                if (targetZ >= xRotation.transform.localEulerAngles.z)
                {
                    currentZ = RotateZ(-speed);
                    Debug.Log("rotateZ +");
                    //Debug.Log("Current Z: " + currentZ + ", Target Z: " + targetZ);
                }
                else
                {
                    currentZ = RotateZ(speed);
                    Debug.Log("rotateZ -");
                    //Debug.Log("Current Z: " + currentZ + ", Target Z: " + targetZ);

                }
            }
            else
            {
                Debug.Log("TargetZ is out of bounds: " + targetZ);
            }

        }
        
    }

    public float RotateZ(float speed)
    {
        xRotation.transform.Rotate(0,0,-speed);

        
        return xRotation.transform.eulerAngles.z;
    }
    
   public float RotateY(float speed)
    {
        yRotation.transform.Rotate(0,speed,0);

        return yRotation.transform.localEulerAngles.y;

    }

   public void SetZ(float z)
   {
       targetZ = targetZ + z;
   }
   public void SetY(float y)
   {
       targetY = targetY + y;
   }
   
}
