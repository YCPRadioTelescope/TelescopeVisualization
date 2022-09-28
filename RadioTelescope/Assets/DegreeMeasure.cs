using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DegreeMeasure : MonoBehaviour
{
    public GameObject Starfield;
    private GameObject target;
    private bool set = false;

    // Update is called once per frame


    private void Start()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(1);
            foreach (Transform child in Starfield.transform)
            {
                Debug.Log("CHILD: " + child.name);
                if (child.GetComponent<Star_Object>() != null)
                {
                    if (child.GetComponent<Star_Object>().Label.Contains("Merak")) ;
                    {
                        target = child.gameObject;
                        set = true;
                    }
                }
            }
            if (set == false)
            {
                Debug.Log("No object was found for the measure cube");
            }

        }
    }
    void Update()
    {
        if (set)
        {
            transform.LookAt(target.transform, Vector3.up);
        }
    }
}
