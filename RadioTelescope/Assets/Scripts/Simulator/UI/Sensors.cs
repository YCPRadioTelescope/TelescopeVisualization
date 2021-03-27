using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sensors : MonoBehaviour
{
    public TMP_Text elevation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateElSensor(string res)
    {
        elevation.text = "Elevation Limit: " + res;
    }
}
