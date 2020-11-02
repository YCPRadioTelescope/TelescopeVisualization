using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zinnia.Extension;

// A script to control the telescope via the VR controlled joystick.
public class MoveTelescopeVR : MonoBehaviour
{
	public GameObject telescope;
	public TelescopeController tc;
	public GameObject joystick;
	private Vector3 rotation;

	// Start is called before the first frame update
	void Start()
	{
		tc = telescope.GetComponent<TelescopeController>();
	}

	// Update is called once per frame
	void Update()
	{
		rotation = joystick.TryGetEulerRotation(true);
		if(rotation.x > 180)
			rotation.x -= 360;

		if(rotation.z > 190)
			rotation.z -= 360;

		tc.RotateZ(rotation.x * (float) 0.01);
		tc.RotateY(rotation.z * (float) 0.01);
	}
}