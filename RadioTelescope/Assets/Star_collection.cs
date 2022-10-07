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

    public void contstructor(string ra, string dec, string label, string desc, Texture2D tex)
    {
        RA = ra;
        DEC = dec;
        Label = label;
        description = desc;
        image = tex;
    }
}
