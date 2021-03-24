using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTele : MonoBehaviour
{
    public GameObject target;

    public Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera.transform.LookAt(target.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
