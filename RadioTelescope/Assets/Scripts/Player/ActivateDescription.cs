using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDescription : MonoBehaviour
{
    public GameObject highlight;

    // Start is called before the first frame update
    void Start()
    {
        if (Input.GetMouseButtonDown(0))
        {
            highlight.SetActive(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            highlight.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (Input.GetMouseButtonDown(0))
        {
            highlight.SetActive(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            highlight.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            highlight.SetActive(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            highlight.SetActive(false);
        }
    }
}