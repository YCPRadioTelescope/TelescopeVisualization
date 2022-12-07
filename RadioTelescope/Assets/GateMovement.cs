using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateMovement : MonoBehaviour
{
   // public Transform gate;
    bool openGate = false;
    bool closeGate = false;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void activateGate() {
        if (transform.rotation.eulerAngles.y >= 350)
        {
            closeGate = true;
        }
        else if (transform.rotation.eulerAngles.y <= 182 && transform.rotation.eulerAngles.y >= 180 )
        {
            openGate = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
        if (transform.rotation.eulerAngles.y > 350)
        {
            openGate = false;
        }

        if (transform.rotation.eulerAngles.y <= 182 && transform.rotation.eulerAngles.y >= 180)
        {
            closeGate = false;
        }
        

        if (openGate)
        {
            transform.Rotate( 0, 0, 1);
        }

        if (closeGate)
        { 
            transform.Rotate( 0, 0, -1);
        }
    }
}
