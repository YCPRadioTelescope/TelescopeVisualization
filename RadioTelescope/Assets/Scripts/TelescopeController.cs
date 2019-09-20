using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelescopeController : MonoBehaviour
{
    public GameObject xRotation;
    public GameObject yRotation;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(RotateX(1));
       // Debug.Log(RotateY(0.2f));
    }

    public float RotateX(float speed)
    {
        xRotation.transform.Rotate(speed,0,0);

        return xRotation.transform.rotation.x;
        
    }
    
   public float RotateY(float speed)
    {
        yRotation.transform.Rotate(0,speed,0);

        return yRotation.transform.rotation.y;

    }
}
