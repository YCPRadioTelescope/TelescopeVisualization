using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConsoleDisplay : MonoBehaviour
{
    public GameObject starfield;
    //public TextMeshProUGUI text;
    public TMPro.TextMeshProUGUI yearText;

    // Update is called once per frame
    void Update()
    {
       yearText.text = "year: " + starfield.GetComponent<Starfield>().year.ToString() + "\n";

        if (starfield.GetComponent<Starfield>().dateUnitID == 0)
        {
            yearText.color = Color.red;
            Debug.Log("it is on the year date unit");
        }
        else {
            yearText.color = Color.white;
        }
    }
}
