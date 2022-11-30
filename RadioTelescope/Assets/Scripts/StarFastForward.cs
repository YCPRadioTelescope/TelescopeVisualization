using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarFastForward : MonoBehaviour
{
    public GameObject starfield;

    private void Start()
    {
    }

    public void dateUnitPicker() {
        if (starfield.GetComponent<Starfield>().dateUnitID == 0)
        {
            yearForward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 1)
        {
            monthForward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 2)
        {
            dayForward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 3)
        {
            hourForward();
        }
        else {
            Debug.Log("this isn't a valid token: " + starfield.GetComponent<Starfield>().dateUnitID);
        }
    
    }


    public void yearForward() {
        Debug.Log("CHANGED THE YEAR");
        starfield.GetComponent<Starfield>().year++;
    }

    public void monthForward() {
        Debug.Log("CHANGED THE MONTH");
        starfield.GetComponent<Starfield>().month++;
    }

    public void dayForward() {
        Debug.Log("CHANGED THE DAY");
        starfield.GetComponent<Starfield>().day++;
    }

    public void hourForward() {
        Debug.Log("CHANGE THE HOUR");
        starfield.GetComponent<Starfield>().hours++;
    }

   
}
