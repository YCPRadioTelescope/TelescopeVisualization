using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test_Load_Image : MonoBehaviour
{
    private UnityEngine.UI.Image image = null;
    private void Awake()
    {
        if (image != null)
        {
            //image.mainTexture = Resources.Load<Image>("Images/test");
        }
    }
}
