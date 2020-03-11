using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleKeybindWindow : MonoBehaviour
{
    public GameObject KeybindCollapse;
    public GameObject KeybindExpand;
    private bool toggle;

    // Start is called before the first frame update
    void Start()
    {
        toggle = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            toggle = !toggle;
        }
            if (toggle)
        {
            KeybindCollapse.SetActive(false);
            KeybindExpand.SetActive(true);
        } else
        {
            KeybindCollapse.SetActive(true);
            KeybindExpand.SetActive(false);
        }
    }
}
