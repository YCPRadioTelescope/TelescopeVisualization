using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonthDisplay : MonoBehaviour
{
    public GameObject starfield;
    public TMPro.TextMeshProUGUI monthText;

    // Update is called once per frame
    void Update()
    {
        monthText.text = "Month: " + starfield.GetComponent<Starfield>().month.ToString() + "\n";
        if (starfield.GetComponent<Starfield>().dateUnitID == 1)
        {
            monthText.color = Color.red;
            Debug.Log("it is on the Month date unit");
        }
        else {
            monthText.color = Color.white;
        }
    }
}
