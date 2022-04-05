using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_Load_Image : MonoBehaviour
{
    private Texture2D new_tex;
    private void Start()
    {
        byte[] pngBytes = System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Sky_Interaction_Data/Skrek.jpg");
        Debug.Log(pngBytes);
        new_tex = new Texture2D(128, 128);
        new_tex.LoadImage(pngBytes);
        transform.GetComponent<RawImage>().texture = new_tex;
    }
}
