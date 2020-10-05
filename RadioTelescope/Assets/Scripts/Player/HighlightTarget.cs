using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HighlightTarget : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject end;
    public GameObject start;
    private RaycastHit hitInfo;
    private Material origMat, tempMat;
    public Color highlightColor;
    public Shader shader;
    private Renderer rend = null;
    private Renderer currRend;
    public Text text;
    void Start()
    {
        //origMat = rend.sharedMaterial;
    }

    private void OnEnable()
    {
        //Debug.Log("Enabling HighlightTarget");
    }

    private void OnDisable()
    {
        rend.sharedMaterial = origMat;
        //Debug.Log("Disabling HighlightTarget");
        rend = null;
        text.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        var dir = start.transform.forward * 10000;
        if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            //Debug.Log("we Hit this: " + hitInfo.transform.ToString());
            text.text = hitInfo.transform.GetComponent<ObjectDesc>().Name + ": " + hitInfo.transform.GetComponent<ObjectDesc>().Description;
            Debug.DrawRay(start.transform.position, dir);

            currRend = hitInfo.collider.gameObject.GetComponent<Renderer>();

            if (currRend == rend)
                return;

            if (currRend && currRend != rend)
            {
                if (rend)
                {
                    rend.sharedMaterial = origMat;
                }

            }

            if (currRend)
                rend = currRend;
            else
                return;

            origMat = rend.sharedMaterial;

            tempMat = new Material(origMat);
            rend.material = tempMat;
            rend.material.shader = shader;
        }
        else
        {
            if (rend)
            {
                rend.sharedMaterial = origMat;
                rend = null;
                text.text = "";
            }
        }
    }
}
