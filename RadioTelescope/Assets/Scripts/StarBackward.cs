using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBackward : MonoBehaviour
{
    // Update is called once per frame
    public GameObject starfield;

    public void dateUnitPicker()
    {
        if (starfield.GetComponent<Starfield>().dateUnitID == 0)
        {
            yearBackward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 1)
        {
            monthBackward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 2)
        {
            dayBackward();
        }
        else if (starfield.GetComponent<Starfield>().dateUnitID == 3)
        {
            hourBackward();
        }
        else
        {
            Debug.Log("this isn't a valid token: " + starfield.GetComponent<Starfield>().dateUnitID);
        }

    }

    public void yearBackward() {
        Debug.Log("Changed year backward");
        starfield.GetComponent<Starfield>().year--;
    }
    public void monthBackward()
    {
        Debug.Log("CHANGED THE MONTH");
        starfield.GetComponent<Starfield>().month--;
    }

    public void dayBackward()
    {
        Debug.Log("CHANGED THE DAY");
        starfield.GetComponent<Starfield>().day--;
    }

    public void hourBackward()
    {
        Debug.Log("CHANGE THE HOUR");
        starfield.GetComponent<Starfield>().hours--;
    }

}
