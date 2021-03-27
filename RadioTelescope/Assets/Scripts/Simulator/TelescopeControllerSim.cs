using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental;
using UnityStandardAssets.Vehicles.Car;

public class TelescopeControllerSim : MonoBehaviour
{
    public GameObject xRotation;
    public GameObject yRotation;
    public float targetZ = 0.0f;
    public float targetY = 0.0f;
    public float currentZ = 0.0f;
    public float currentY = 0.0f;
    public float speed = 1.0f;
    public Sensors sen;

    public TMP_Text ZPos;
    public TMP_Text YPos;
    public TMP_Text ElPos;
    public TMP_Text AzPos;
    public TMP_Text SpeedTxt;
    public TMP_Text TargetYY;
    public TMP_Text TargetXX;

    public float yRemainder = 0;

    public bool isNeg = false;

    public bool isMovingY = false;
    
    public bool isMovingZ = false;

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
        Debug.Log(isMovingY);
        /*if (targetY < 0)
        {
            targetY += 360.0f;
        }*/
        
        //Debug.Log("y rem: " + yRemainder);
       

        if (targetZ < 0)
        {
            targetZ += 360.0f;
        }
        
        //Debug.Log("ZTarget: " + targetZ);
        //Debug.Log("YTarget: " + targetY)
        //Debug.Log(yRotation.transform.eulerAngles.z);
        
        if (Math.Round(currentY, 1) != Math.Round(targetY, 1))
        {
            //Debug.Log(targetY);
            //Debug.Log(currentY);

            if (isNeg == false)
            {
                currentY = RotateY(speed);  
            }
            else
            {
                currentY = RotateY(-speed);
            }
        }
        else
        {
            
            targetY = currentY;
        }

        if (targetY == currentY && yRemainder > 0)
        {
            if (isNeg == true)
            {
                targetY = (targetY + yRemainder) - 360;
                yRemainder = 0;
            }
            else
            {
                targetY = (targetY + yRemainder) - 360;
                yRemainder = 0;
            }
           
        }
        else if (targetY == currentY && yRemainder == 0)
        {
            isMovingY = false;
        }
        
        

        //currentY = RotateY(speed);

        if ((int) xRotation.transform.localEulerAngles.z != (int) targetZ)
        {
            if (targetZ <= 105.0f && targetZ >= 0)
            {
                if (targetZ >= xRotation.transform.localEulerAngles.z)
                {
                    currentZ = RotateZ(-speed);
                    //Debug.Log("rotateZ +");
                    //Debug.Log("Current Z: " + currentZ + ", Target Z: " + targetZ);
                }
                else
                {
                    currentZ = RotateZ(speed);
                    //Debug.Log("rotateZ -");
                    //Debug.Log("Current Z: " + currentZ + ", Target Z: " + targetZ);

                }
                sen.updateElSensor("Good");
            }
            else
            {
                //Debug.Log("TargetZ is out of bounds: " + targetZ);
                if (targetZ <= 105.0f)
                {
                    targetZ = 1;
                }
                else
                {
                    targetZ = 0;
                }
                sen.updateElSensor("Hit");
                //targetZ = currentZ;
            }

        }
        else
        {
            isMovingZ = false;
        }
        
        YPos.text = "Unity Y Position: " + System.Math.Round(currentY, 0);
        ZPos.text = "Unity Z Position: " + System.Math.Round(currentZ, 0);
        if (Math.Round(currentY, 0) == 359)
        {
            AzPos.text = "Y Degrees: " + (System.Math.Round(currentY,0) + 1);
        }
        else
        {
            AzPos.text = "Y Degrees: " + System.Math.Round(currentY, 2);
        }
        ElPos.text = "X Degrees: " + System.Math.Round((currentZ - 16.0),0);
        
        SpeedTxt.text = "Speed : " + System.Math.Round(speed, 2);

    }

    public float RotateZ(float speed)
    {
        xRotation.transform.Rotate(0,0,-speed);
        
        
        return xRotation.transform.eulerAngles.z;
    }
    
    public float RotateY(float speed)
    {
        yRotation.transform.Rotate(0,speed,0);
        //yRotation.transform.Rotate(0,targetY,0);
        
        //yRotation.transform.rotation = Quaternion.Slerp(yRotation.transform.rotation, Quaternion.Euler(0, targetY, 0), Time.deltaTime * speed);
        float newY;
        if (isNeg)
        {
            newY = currentY = (currentY + 360) +  speed;
        }
        else
        {
            newY = currentY += speed;
        }
        
        if (newY >= 360)
        {
            newY -= 360;
        }

        return newY;

    }
    
  /* public float RotateY(float speed)
   {
       //yRotation.transform.rotation = Quaternion.RotateTowards(yRotation.transform.rotation, Quaternion.AngleAxis(targetY, transform.up), Time.deltaTime * (speed*100)); 
       
       yRotation.transform.rotation = Quaternion.Lerp(yRotation.transform.rotation, Quaternion.AngleAxis(targetY, transform.up), (speed ) * Time.deltaTime);

       return yRotation.transform.localEulerAngles.y;

   }*/
   
   public void SetZ(float z)
   {
       if (isMovingZ == false)
       {
           TargetXX.text = "Target X: " + z;
           targetZ = targetZ + z;
           isMovingZ = true;
       }
       
   }
   public void SetY(float y)
   {
       if (isMovingY == false)
       {
           TargetYY.text = "Target Y: " + y;
           isNeg = false;
           if (y < 0)
           {
               isNeg = true;
               y = y * -1;
           }

           if (isNeg == true)
           {
               if (currentY == 0)
               {
                   targetY = 360 - y;
               }
               else
               {
                   targetY = currentY - y;
               }

           }
           else
           {
               targetY = currentY + y;
           }


           if (isNeg == true)
           {
               if (targetY <= 0)
               {
                   yRemainder = targetY + 359;
                   targetY = 1;
               }
           }
           else
           {
               if (targetY >= 360)
               {
                   yRemainder = targetY - 359;
                   targetY = 359;
               }
           }

           isMovingY = true;
       }


   }

   public float getCurrentZ()
   {
       return currentZ;
   }

   public float getCurrentY()
   {
       return currentY;
   }
   
   IEnumerator RotateMe(Vector3 byAngles, float inTime) {
       var fromAngle = transform.rotation;
       var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
       for(var t = 0f; t < 1; t += Time.deltaTime/inTime) {
           transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
           yield return null;
       }
   }
}
