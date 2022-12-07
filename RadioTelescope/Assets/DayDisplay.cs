using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayDisplay : MonoBehaviour
{
    public GameObject starfield;
    public TMPro.TextMeshProUGUI dayText;

    // Update is called once per frame
    void Update()
    {
        dayText.text = "day: " + starfield.GetComponent<Starfield>().day.ToString() + "\n";

        if (starfield.GetComponent<Starfield>().dateUnitID == 2)
        {
            dayText.color = Color.red;
            Debug.Log("it is on the Day date unit");
        }
        else {
            dayText.color = Color.white;
        }
    }
}
