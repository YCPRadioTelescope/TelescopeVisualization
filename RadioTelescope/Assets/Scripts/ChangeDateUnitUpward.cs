using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDateUnitUpward : MonoBehaviour
{
    public GameObject starfield;

    // Update is called once per frame
    public void increaseDateUnit() {
        if (starfield.GetComponent<Starfield>().dateUnitID == 3)
        {
            starfield.GetComponent<Starfield>().dateUnitID = 0;
        }
        else {
            starfield.GetComponent<Starfield>().dateUnitID++;
            
        }
    }
}
