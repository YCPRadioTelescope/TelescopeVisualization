using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Switcher : MonoBehaviour
{
    public Button buttonSim;
    public Button buttonVr;

    public GameObject simCamera;

    public GameObject vrCamera;
    // Start is called before the first frame update
    void Start()
    {
        buttonSim.onClick.AddListener(TaskOnClick);
        buttonVr.onClick.AddListener(TaskOnClick2);
    }

    void TaskOnClick(){
        if (simCamera.activeInHierarchy == true)
        {
            simCamera.SetActive(false);
            vrCamera.SetActive(true);
        }

    }
    
    void TaskOnClick2(){
        if (vrCamera.activeInHierarchy == true)
        {
            vrCamera.SetActive(false);
            simCamera.SetActive(true);
        }

    }
}
