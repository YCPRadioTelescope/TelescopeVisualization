using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star_collection : MonoBehaviour
{
    public string description;
    public string RA;
    public string DEC;
    public string Label;
    public Texture2D image;
    public DateTime Date;
    public string Date_text;

    public void constructor(string ra, string dec, string label, string desc, Texture2D tex, DateTime date)
    {
        RA = ra;
        DEC = dec;
        Label = label;
        description = desc;
        image = tex;
        Date = date;
        Date_text = date.ToString();
    }
}
