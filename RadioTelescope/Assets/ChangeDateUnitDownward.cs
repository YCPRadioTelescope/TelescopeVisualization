using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDateUnitDownward : MonoBehaviour
{
    public GameObject starfield;

    // Update is called once per frame
    public void decreaseDateUnit()
    {
        Debug.Log("made it to the method");
        if (starfield.GetComponent<Starfield>().dateUnitID == 0)
        {
            starfield.GetComponent<Starfield>().dateUnitID = 3;
            Debug.Log(starfield.GetComponent<Starfield>().dateUnitID);
        }
        else {
            starfield.GetComponent<Starfield>().dateUnitID--;
            Debug.Log(starfield.GetComponent<Starfield>().dateUnitID);
        }
    }
}
