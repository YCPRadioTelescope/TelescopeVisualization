using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zinnia.Extension;

// A script to control the telescope via the VR controlled joystick.
public class MoveTelescopeVR : MonoBehaviour
{
	public TelescopeController tc;
	public GameObject joystick;
	private Vector3 rotation;
	
	// Update is called once per frame
	void Update()
	{
		rotation = joystick.TryGetEulerRotation(true);
		if(rotation.x > 180)
			rotation.x -= 360;

		if(rotation.z > 190)
			rotation.z -= 360;

		tc.ChangeAzimuth(rotation.z * (float) 0.01);
		tc.ChangeElevation(rotation.x * (float) 0.01);
	}
}
