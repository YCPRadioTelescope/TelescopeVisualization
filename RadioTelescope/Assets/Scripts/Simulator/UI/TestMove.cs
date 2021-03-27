using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMove : MonoBehaviour
{
    public TelescopeControllerSim tc;
    public TMP_InputField x;
    public TMP_InputField y;
    public TMP_InputField speed;
    public Button startButton;
    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(TestMovement);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestMovement()
    {
        tc.SetZ(float.Parse(y.text));
        tc.SetY(float.Parse(x.text));
        tc.speed = float.Parse(speed.text);
    }
}
