using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawTheLine : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer lr;
    public GameObject end;
    void Start()
    {
        lr = this.transform.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = this.transform.position;
        newPos.y = newPos.y + 0.05f;
        lr.SetPosition(0, newPos);
        lr.SetPosition(1, end.transform.position);
        //lr.SetPosition(0, (this.transform.forward.normalized * 1000));
    }
}
