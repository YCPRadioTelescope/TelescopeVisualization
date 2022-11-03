using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateMovement : MonoBehaviour
{
    public Transform gate;
    bool openGate = false;
    bool closeGate = false;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void activateGate() {
        Debug.Log("gate activated!");

        Debug.Log(transform.rotation.eulerAngles.y);
        if (transform.rotation.eulerAngles.y >= 350)
        {
            Debug.Log("gate close");
            Debug.Log(transform.rotation.eulerAngles.z);
            closeGate = true;
        }
        else if (transform.rotation.eulerAngles.y <= 182 && transform.rotation.eulerAngles.y >= 180 )
        {
            Debug.Log(transform.rotation.eulerAngles.z);
            Debug.Log("gate open");
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
            Debug.Log("Opening");
           Debug.Log(transform.rotation.eulerAngles.y);
        }

        if (closeGate)
        {
           
            transform.Rotate( 0, 0, -1);
            Debug.Log("Closing");
            Debug.Log(transform.rotation.eulerAngles.y);
        }
    }
}
