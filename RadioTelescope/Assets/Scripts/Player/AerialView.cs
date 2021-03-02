using System;
using UnityEngine;
using System.Collections;

// A script attached to a secondary camera that causes it to rotate about the radio
// telescope. Disables the player when active, disallowing the player from moving
// around or highlighting parts of the telescope.
public class AerialView : MonoBehaviour
{
	// The aerial camera used for the overhead shot that gets enabled when this
	// script is active, and the player that gets disabled when this script is active.
	public Camera aerialCam;
	public GameObject player;
	
	// Whether the aerial view is active and how fast it rotates in degrees per second.
	public bool active = false;
	public float speed = 10.0f;
	
	// The target object to look at and rotate around.
	public GameObject target;
	private Vector3 point;
	
	// Every focusTimer seconds the camera zooms in or out to keep things looking fancy.
	public double focusTimer;
	private double elapsedTime = 0.0;
	private bool focus = false;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Get the target's position as a point. This is where the camera will look at
		// and rotate around.
		point = target.transform.position;
	}
	
	// Update is called once per frame.
	void Update()
	{
		// When T is pressed, toggle the aerial camera's activity.
		if(Input.GetKeyDown(KeyCode.T))
		{
			active = !active;
			aerialCam.enabled = active;
			player.SetActive(!active);
		}
		
		if(active)
		{
			// Track how much time has elapsed. If focusTimer seconds have elapsed,
			// change the focus.
			elapsedTime += Time.deltaTime;
			if(elapsedTime > focusTimer)
			{
				elapsedTime -= focusTimer;
				focus = !focus;
			}
			
			// While focusing, the camera zooms in. While not focusing, it zooms out.
			if(focus && aerialCam.fieldOfView >= 20)
				aerialCam.fieldOfView = aerialCam.fieldOfView - 0.5f;
			else if(!focus && aerialCam.fieldOfView < 65)
				aerialCam.fieldOfView = aerialCam.fieldOfView + 0.5f;
			
			// Point the aerial camera toward the target and rotate around it.
			aerialCam.transform.LookAt(point);
			aerialCam.transform.RotateAround(point, new Vector3(0.0f, 1.0f, 0.0f), Time.deltaTime * speed);
		}
	}
}

