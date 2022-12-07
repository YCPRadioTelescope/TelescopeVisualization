using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class hourDisplay : MonoBehaviour
{
    public GameObject starfield;
    public TMPro.TextMeshProUGUI hourText;

    // Update is called once per frame
    void Update()
    {
        hourText.text = "hour: " + starfield.GetComponent<Starfield>().hours.ToString();

        if (starfield.GetComponent<Starfield>().dateUnitID == 3)
        {
            hourText.color = Color.red;
            Debug.Log("it is on the Hour date unit");
        }
        else {
            hourText.color = Color.white;
        }
    }
}
