using System;
using UnityEngine;
using System.Collections;
 
public class CameraClass : MonoBehaviour {
   
    public GameObject target;//the target object
    public GameObject target2;//the target object
    public GameObject target3;//the target object
    public Transform start;
    public float speedMod = 10.0f;//a speed modifier
    private Vector3 point;//the coord to the point where the camera looks at
    private Vector3 point2;//the coord to the point where the camera looks at
    private int rotate = 0;
   
    void Start () {//Set up things on the start method
        point = target.transform.position;//get target's coords
        point2 = target2.transform.position;//get target's coords
        StartCoroutine(ExecuteAfterTime(36));
        start = this.transform;
    }
   
    void Update () {//makes the camera rotate around "point" coords, rotating around its Y axis, 20 degrees per second times the speed modifier
        if (rotate == 0)
        {
            transform.LookAt(point);//makes the camera look to it
            transform.RotateAround (point,new Vector3(0.0f,1.0f,0.0f),20 * Time.deltaTime * speedMod);
        }
        else if(rotate == 1)
        {
            transform.LookAt(point2);//makes the camera look to it
            transform.RotateAround (point2,new Vector3(0.0f,1.0f,0.0f),20 * Time.deltaTime * speedMod);
        }
        else
        {
            
        }
            

    }
    IEnumerator ExecuteAfterTime(float time)
    {
        
        yield return new WaitForSeconds(time);
        Debug.Log("yeet");
        this.transform.position = Vector3.Lerp(this.transform.position, target3.transform.position, speedMod * Time.deltaTime * 500);
        rotate = 1;
        StartCoroutine(ExecuteAfterTime2(36));
    }
    
    IEnumerator ExecuteAfterTime2(float time)
    {
        
        yield return new WaitForSeconds(time);
        Debug.Log("yeet");
        this.transform.position = Vector3.Lerp(this.transform.position, start.position, speedMod * Time.deltaTime * 500);
        rotate = 2;
    }
}
