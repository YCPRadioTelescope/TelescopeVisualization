using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTelescope : MonoBehaviour
{
    public GameObject telescope;

    public TelescopeController tc;
    // Start is called before the first frame update
    void Start()
    {
        tc = telescope.GetComponent<TelescopeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("up"))
        {
            tc.RotateX(0.5f);
        }
        
        if (Input.GetKey("down"))
        {
            tc.RotateX(-0.5f);
        }
        
        if (Input.GetKey("right"))
        {
            tc.RotateY(0.5f);
        }
        
        if (Input.GetKey("left"))
        {
            tc.RotateY(-0.5f);
        }
    }
}
